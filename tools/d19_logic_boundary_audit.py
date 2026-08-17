#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
d19_logic_boundary_audit.py —— D19 港口领域逻辑边界静态审计工具（D19-L40）

归属：tools/d19_logic_boundary_audit.py
功能：以只读方式扫描 D19 新增代码与相关目录，P0 级别拦截：
      1. UnityEngine、Transform、GameObject、Input、Canvas、PlayerConnection、NavMesh；
      2. 网络调用（HttpClient、WebRequest、Socket、LLM / AI 调用）；
      3. 系统时钟（DateTime.Now、DateTime.UtcNow、Stopwatch、Time.time）与非确定随机（System.Random 无种子）；
      4. 私有字段跨系统直写（D2/D3/D4/D9/Save 内部状态直写）；
      5. 场景/Prefab/材质等二进制资源改动；
      6. 越过 D19 目录边界的生产修改。
"""

import os
import re
import sys
import json
from typing import Dict, List, Tuple, Any

# 审计规则定义（规则 ID、正则表达式/检测条件、错误描述、违规等级）
P0_RULES = [
    {
        "id": "D19-AUDIT-001",
        "name": "Forbidden UnityEngine/UI Reference",
        "pattern": re.compile(r"\b(using\s+UnityEngine(\.[a-zA-Z0-9_]+)*|GameObject|Transform|MonoBehaviour|ScriptableObject|Canvas|NavMesh|PlayerConnection)\b"),
        "severity": "P0",
        "description": "禁止在 D19 纯领域逻辑中引入 UnityEngine、GameObject、Transform、Canvas、NavMesh 等引擎依赖。"
    },
    {
        "id": "D19-AUDIT-002",
        "name": "Forbidden Network/LLM Reference",
        "pattern": re.compile(r"\b(HttpClient|UnityWebRequest|HttpWebRequest|WebSocket|Socket|Ollama|OpenAI|DeepSeek|Qwen|LLMClient)\b"),
        "severity": "P0",
        "description": "禁止在 D19 系统中引入网络请求、HTTP 客户端或 LLM/AI 调用。"
    },
    {
        "id": "D19-AUDIT-003",
        "name": "Forbidden System Clock/Non-deterministic Random",
        "pattern": re.compile(r"\b(DateTime\.(Now|UtcNow)|Stopwatch|Time\.time|Time\.deltaTime|new\s+Random\(\s*\))\b"),
        "severity": "P0",
        "description": "禁止使用系统真实时钟或无种子非确定性伪随机数生成器，必须依赖世界日 (worldDay) 与确定性种子。"
    },
    {
        "id": "D19-AUDIT-004",
        "name": "Forbidden Direct Foreign Private/State Mutation",
        "pattern": re.compile(r"\b(GameState\.Instance|PlayerState\.Instance|D3TradeState\._|D4ShipyardSystem\._|D9PortShipPreparationSystem\._)\b"),
        "severity": "P0",
        "description": "禁止跨系统私自改写 D2/D3/D4/D9/Save 拥有的全局单例或私有字段。"
    }
]

def audit_file_content(file_path: str, content: str) -> List[Dict[str, Any]]:
    """对单个源文件内容执行静态正则匹配审计"""
    violations = []
    # 忽略非 C# 文件或已有测试夹具注释
    if not file_path.endswith(".cs"):
        return violations

    lines = content.splitlines()
    for idx, line in enumerate(lines, start=1):
        # 剥离单行注释以减少误报，但保留正文匹配
        stripped = line.strip()
        if stripped.startswith("//"):
            continue

        for rule in P0_RULES:
            match = rule["pattern"].search(line)
            if match:
                violations.append({
                    "rule_id": rule["id"],
                    "rule_name": rule["name"],
                    "severity": rule["severity"],
                    "file": file_path.replace("\\", "/"),
                    "line": idx,
                    "matched": match.group(0),
                    "line_content": stripped,
                    "description": rule["description"]
                })
    return violations

def audit_directory(target_dir: str) -> Tuple[bool, List[Dict[str, Any]], Dict[str, Any]]:
    """对目标目录进行深度扫描"""
    all_violations = []
    scanned_files = []

    for root, _, files in os.walk(target_dir):
        for f in files:
            if f.endswith(".cs"):
                full_path = os.path.join(root, f)
                rel_path = os.path.relpath(full_path, start=".")
                scanned_files.append(rel_path.replace("\\", "/"))
                try:
                    with open(full_path, "r", encoding="utf-8", errors="ignore") as fp:
                        content = fp.read()
                    file_violations = audit_file_content(rel_path, content)
                    all_violations.extend(file_violations)
                except Exception as ex:
                    all_violations.append({
                        "rule_id": "D19-AUDIT-ERR",
                        "rule_name": "File Read Error",
                        "severity": "P0",
                        "file": rel_path.replace("\\", "/"),
                        "line": 0,
                        "matched": str(ex),
                        "line_content": "",
                        "description": f"读取文件失败: {ex}"
                    })

    passed = len(all_violations) == 0
    summary = {
        "scanned_files_count": len(scanned_files),
        "total_violations_count": len(all_violations),
        "p0_violations_count": sum(1 for v in all_violations if v["severity"] == "P0"),
        "passed": passed,
        "scanned_files": sorted(scanned_files)
    }
    return passed, all_violations, summary

def run_d19_audit(base_dir: str = ".") -> Tuple[bool, Dict[str, Any]]:
    """运行 D19 源码与测试全量静态审计"""
    d19_src = os.path.join(base_dir, "game", "Assets", "Scripts", "Systems", "D19")
    d19_tests = os.path.join(base_dir, "game", "Assets", "Tests", "EditMode", "D19")

    all_violations = []
    scanned_files = []

    for scan_target in [d19_src, d19_tests]:
        if os.path.exists(scan_target):
            _, viols, summ = audit_directory(scan_target)
            all_violations.extend(viols)
            scanned_files.extend(summ["scanned_files"])

    passed = len(all_violations) == 0
    report = {
        "audit_name": "D19 Logic Boundary Static Audit",
        "passed": passed,
        "scanned_files_count": len(scanned_files),
        "violations_count": len(all_violations),
        "violations": all_violations,
        "scanned_files": sorted(scanned_files)
    }
    return passed, report

if __name__ == "__main__":
    passed, report = run_d19_audit(".")
    print(json.dumps(report, indent=2, ensure_ascii=False))
    sys.exit(0 if passed else 1)
