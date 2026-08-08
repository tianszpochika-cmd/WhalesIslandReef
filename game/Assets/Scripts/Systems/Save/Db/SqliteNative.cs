// ============================================================
// SqliteNative —— sqlite3 原生库 P/Invoke 声明（Windows x64）
//
// 原生库来源：sqlite.org 官方预编译 sqlite-dll-win-x64（MIT/public domain）
//   安装位置：Assets/Plugins/x86_64/sqlite3.dll（编辑器 EditMode 测试与
//   Windows 构建均加载此插件）。
//
// 红线自查：本文件仅 DllImport 本地原生库，无任何网络 / LLM 调用。
// ============================================================

using System;
using System.Runtime.InteropServices;

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// sqlite3 C API 的最小 P/Invoke 面（仅本项目需要的调用）。
    /// 常量与返回值定义见 https://www.sqlite.org/c3ref/c_ok.html
    /// </summary>
    internal static class SqliteNative
    {
        // 返回码
        internal const int SQLITE_OK = 0;
        internal const int SQLITE_ROW = 100;
        internal const int SQLITE_DONE = 101;

        // 打开标志
        internal const int SQLITE_OPEN_READWRITE = 0x00000002;
        internal const int SQLITE_OPEN_CREATE = 0x00000004;
        internal const int SQLITE_OPEN_FULLMUTEX = 0x00010000;

        // bind_text 的析构器：-1 = 让 sqlite 复制文本
        internal const int SQLITE_TRANSIENT = -1;

        // 列类型（sqlite3_column_type）
        internal const int SQLITE_INTEGER = 1;
        internal const int SQLITE_FLOAT = 2;
        internal const int SQLITE_TEXT = 3;
        internal const int SQLITE_BLOB = 4;
        internal const int SQLITE_NULL = 5;

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int sqlite3_open_v2(string filename, out IntPtr db, int flags, string vfs);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_close(IntPtr db);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr sqlite3_errmsg(IntPtr db);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr userData, out IntPtr errMsg);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int sqlite3_prepare_v2(IntPtr db, string sql, int nByte, out IntPtr stmt, out IntPtr tail);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_step(IntPtr stmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_finalize(IntPtr stmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern int sqlite3_bind_text(IntPtr stmt, int index, string value, int n, IntPtr destructor);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_int(IntPtr stmt, int index, int value);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_bind_null(IntPtr stmt, int index);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_count(IntPtr stmt);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr sqlite3_column_name(IntPtr stmt, int col);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_type(IntPtr stmt, int col);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_column_text(IntPtr stmt, int col);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_column_int(IntPtr stmt, int col);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double sqlite3_column_double(IntPtr stmt, int col);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern long sqlite3_last_insert_rowid(IntPtr db);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_busy_timeout(IntPtr db, int ms);

        [DllImport("sqlite3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_changes(IntPtr db);

        internal static string PtrToString(IntPtr ptr)
        {
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }
    }
}
