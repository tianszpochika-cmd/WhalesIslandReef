// ============================================================
// SqliteDb —— 极简 SQLite 封装（本项目专用）
//
// 定位：强逻辑层数据访问。只暴露本项目需要的操作：
//   - Execute：无参数 DDL / 写语句
//   - Execute(sql, args)：参数化写语句（防注入）
//   - Query(sql, args)：参数化查询，行 = 列名 → 值 的字典
// 值类型统一：string / long / double / null（SQLite 原生类型）。
//
// 说明：单连接 + 调用方同步使用；M0 单机存档无需连接池。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// SQLite 连接封装（IDisposable；using 语句确保关闭）。
    /// </summary>
    internal sealed class SqliteDb : IDisposable
    {
        private IntPtr _db;
        public string Path { get; }

        public SqliteDb(string path)
        {
            Path = path;
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            int rc = SqliteNative.sqlite3_open_v2(
                path, out _db,
                SqliteNative.SQLITE_OPEN_READWRITE | SqliteNative.SQLITE_OPEN_CREATE | SqliteNative.SQLITE_OPEN_FULLMUTEX,
                null);
            if (rc != SqliteNative.SQLITE_OK)
                throw new InvalidOperationException("无法打开数据库 " + path + "：" + LastError());
            SqliteNative.sqlite3_busy_timeout(_db, 5000); // 5s 忙等待，避免并发写瞬时锁冲突
        }

        private string LastError()
        {
            return SqliteNative.PtrToString(SqliteNative.sqlite3_errmsg(_db)) ?? "unknown";
        }

        private void EnsureOk(int rc, string action)
        {
            if (rc != SqliteNative.SQLITE_OK)
                throw new InvalidOperationException("SQLite " + action + " 失败(" + rc + ")：" + LastError());
        }

        /// <summary>执行无参数 SQL（建表 / 插入 / 删除等）。</summary>
        public void Execute(string sql)
        {
            EnsureOk(SqliteNative.sqlite3_exec(_db, sql, IntPtr.Zero, IntPtr.Zero, out IntPtr err), "exec");
        }

        /// <summary>执行参数化写语句（INSERT / UPDATE / DELETE）。</summary>
        public void Execute(string sql, params object[] args)
        {
            RunStatement(sql, args, null);
        }

        /// <summary>执行参数化查询，返回行集合。</summary>
        public List<Dictionary<string, object>> Query(string sql, params object[] args)
        {
            var rows = new List<Dictionary<string, object>>();
            RunStatement(sql, args, stmt =>
            {
                int nCols = SqliteNative.sqlite3_column_count(stmt);
                var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < nCols; i++)
                {
                    string name = SqliteNative.PtrToString(SqliteNative.sqlite3_column_name(stmt, i));
                    row[name] = ReadColumn(stmt, i);
                }
                rows.Add(row);
            });
            return rows;
        }

        private static object ReadColumn(IntPtr stmt, int col)
        {
            switch (SqliteNative.sqlite3_column_type(stmt, col))
            {
                case SqliteNative.SQLITE_INTEGER:
                    return (long)SqliteNative.sqlite3_column_int(stmt, col);
                case SqliteNative.SQLITE_FLOAT:
                    return SqliteNative.sqlite3_column_double(stmt, col);
                case SqliteNative.SQLITE_TEXT:
                    return SqliteNative.PtrToString(SqliteNative.sqlite3_column_text(stmt, col));
                default:
                    return null; // NULL / BLOB
            }
        }

        private void RunStatement(string sql, object[] args, Action<IntPtr> onRow)
        {
            IntPtr stmt;
            EnsureOk(SqliteNative.sqlite3_prepare_v2(_db, sql, -1, out stmt, out IntPtr tail), "prepare");
            try
            {
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        object a = args[i];
                        int idx = i + 1; // sqlite 参数从 1 开始
                        if (a == null) SqliteNative.sqlite3_bind_null(stmt, idx);
                        else if (a is int) SqliteNative.sqlite3_bind_int(stmt, idx, (int)a);
                        else if (a is long) SqliteNative.sqlite3_bind_int(stmt, idx, (int)(long)a);
                        else if (a is double) SqliteNative.sqlite3_bind_double(stmt, idx, (double)a);
                        else if (a is float) SqliteNative.sqlite3_bind_double(stmt, idx, (double)(float)a);
                        else SqliteNative.sqlite3_bind_text(stmt, idx, a.ToString(), -1, new IntPtr(SqliteNative.SQLITE_TRANSIENT));
                    }
                }
                int rc;
                while ((rc = SqliteNative.sqlite3_step(stmt)) == SqliteNative.SQLITE_ROW)
                {
                    onRow?.Invoke(stmt);
                }
                if (rc != SqliteNative.SQLITE_DONE) EnsureOk(rc, "step");
            }
            finally
            {
                SqliteNative.sqlite3_finalize(stmt);
            }
        }

        public long LastInsertRowId() => SqliteNative.sqlite3_last_insert_rowid(_db);

        public void Dispose()
        {
            if (_db != IntPtr.Zero)
            {
                SqliteNative.sqlite3_close(_db);
                _db = IntPtr.Zero;
            }
        }
    }
}
