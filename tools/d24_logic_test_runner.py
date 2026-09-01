#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24-L17—L22 的独立纯 C# 测试入口。

测试项目在临时目录生成，引用已存在的纯逻辑程序集并链接必要源码；不会
生成 Unity 工程文件、场景、Prefab 或任何持久化运行时产物。
"""

from __future__ import annotations

import argparse
import html
import json
import subprocess
import sys
import tempfile
from pathlib import Path
from typing import Dict, Iterable, List, Sequence


TASK_FILES = {
    "D24-L17": "game/Assets/Tests/EditMode/D24/D24SeedReplayTests.cs",
    "D24-L18": "game/Assets/Tests/EditMode/D24/D24StateCoverageTests.cs",
    "D24-L19": "game/Assets/Tests/EditMode/D24/D24SchemaRoundTripTests.cs",
    "D24-L20": "game/Assets/Tests/EditMode/D24/D24NoNetworkInSystemsTests.cs",
    "D24-L21": "game/Assets/Tests/EditMode/D24/D24NoUnityInAgentCoreTests.cs",
    "D24-L22": "game/Assets/Tests/EditMode/D24/D24CrossEndContractTests.cs",
}
ALL_TASKS = tuple(TASK_FILES)


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Run D24 pure C# logic tests")
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--task", choices=("all",) + ALL_TASKS, default="all")
    parser.add_argument("--configuration", choices=("Debug", "Release"), default="Release")
    parser.add_argument("--json-out", type=Path, default=None)
    return parser.parse_args(argv)


def _xml(value: str) -> str:
    return html.escape(value, quote=True)


def _compile_items(paths: Iterable[Path]) -> str:
    return "\n".join(f'    <Compile Include="{_xml(str(path.resolve()))}" />' for path in paths)


def _project_text(
    base: Path,
    temp_dir: Path,
    tasks: Sequence[str],
) -> str:
    test_paths = [base / TASK_FILES[task] for task in tasks]
    source_paths: List[Path] = []
    if any(task in tasks for task in ("D24-L17", "D24-L18", "D24-L19")):
        for chapter in ("D20", "D21", "D22"):
            source_paths.extend(sorted((base / "game" / "Assets" / "Scripts" / "Systems" / chapter).glob("*.cs")))
    if any(task in tasks for task in ("D24-L17", "D24-L18")):
        source_paths.append(base / "game" / "Assets" / "Tests" / "EditMode" / "D23" / "D23ScenarioFixtureFactory.cs")
    all_paths = test_paths + source_paths
    missing = [str(path) for path in all_paths if not path.exists()]
    if missing:
        raise FileNotFoundError("missing C# input: " + ", ".join(missing))

    references = []
    systems = base / "game" / "Temp" / "bin" / "Debug" / "Whalebone.Systems.dll"
    agent_runtime = base / "game" / "Temp" / "bin" / "Debug" / "Whalebone.Agent.Runtime.dll"
    if any(task in tasks for task in ("D24-L17", "D24-L18", "D24-L19")):
        if not systems.exists():
            raise FileNotFoundError("required reference is missing: " + str(systems))
        references.append(f'    <Reference Include="Whalebone.Systems"><HintPath>{_xml(str(systems.resolve()))}</HintPath></Reference>')
        if not agent_runtime.exists():
            raise FileNotFoundError("required reference is missing: " + str(agent_runtime))
        references.append(f'    <Reference Include="Whalebone.Agent.Runtime"><HintPath>{_xml(str(agent_runtime.resolve()))}</HintPath></Reference>')

    program = temp_dir / "Program.cs"
    program.write_text(
        "using System;\n"
        "namespace D24StandaloneRunner\n"
        "{\n"
        "    public static class Program\n"
        "    {\n"
        "        public static int Main(string[] args)\n"
        "        {\n"
        "            int failed = 0;\n"
        + "            failed += " + "; failed += ".join(
            f"Whalebone.Tests.D24.{_class_name(task)}.RunAll()" for task in tasks
        )
        + ";\n"
        "            Console.WriteLine(\"D24 standalone failed=\" + failed);\n"
        "            return failed == 0 ? 0 : 1;\n"
        "        }\n"
        "    }\n"
        "}\n",
        encoding="utf-8",
    )
    all_paths.append(program)
    return (
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n"
        "  <PropertyGroup>\n"
        "    <OutputType>Exe</OutputType>\n"
        "    <TargetFramework>net9.0</TargetFramework>\n"
        "    <LangVersion>9.0</LangVersion>\n"
        "    <Nullable>disable</Nullable>\n"
        "    <ImplicitUsings>disable</ImplicitUsings>\n"
        "    <EnableDefaultItems>false</EnableDefaultItems>\n"
        "    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>\n"
        "  </PropertyGroup>\n"
        "  <ItemGroup>\n"
        + _compile_items(all_paths)
        + "\n"
        + "\n".join(references)
        + "\n  </ItemGroup>\n"
        "</Project>\n"
    )


def _class_name(task: str) -> str:
    return {
        "D24-L17": "D24SeedReplayTests",
        "D24-L18": "D24StateCoverageTests",
        "D24-L19": "D24SchemaRoundTripTests",
        "D24-L20": "D24NoNetworkInSystemsTests",
        "D24-L21": "D24NoUnityInAgentCoreTests",
        "D24-L22": "D24CrossEndContractTests",
    }[task]


def _run(command: Sequence[str], cwd: Path) -> Dict[str, object]:
    completed = subprocess.run(
        list(command),
        cwd=str(cwd),
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
        check=False,
    )
    return {
        "command": " ".join(command),
        "exit_code": completed.returncode,
        "stdout": completed.stdout[-12000:],
        "stderr": completed.stderr[-12000:],
    }


def run_task(base: Path, task: str, configuration: str) -> Dict[str, object]:
    with tempfile.TemporaryDirectory(prefix="d24-csharp-") as directory:
        temp_dir = Path(directory)
        project = temp_dir / "D24Standalone.csproj"
        try:
            project.write_text(_project_text(base, temp_dir, (task,)), encoding="utf-8")
        except Exception as error:
            return {"task": task, "passed": False, "status": "BLOCKED", "error": str(error)}
        build = _run(("dotnet", "build", str(project), "--configuration", configuration, "--nologo", "-warnaserror"), base)
        if int(build["exit_code"]) != 0:
            return {"task": task, "passed": False, "status": "BUILD_FAILED", "build": build}
        run = _run(("dotnet", "run", "--project", str(project), "--configuration", configuration, "--no-build", "--no-restore", "--nologo"), base)
        return {
            "task": task,
            "passed": int(run["exit_code"]) == 0,
            "status": "PASS" if int(run["exit_code"]) == 0 else ("LOGIC_BLOCKED_OR_FAILED" if task == "D24-L22" else "TEST_FAILED"),
            "build": build,
            "run": run,
        }


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    base = args.root.expanduser().resolve()
    tasks = ALL_TASKS if args.task == "all" else (args.task,)
    task_results = [run_task(base, task, args.configuration) for task in tasks]
    payload = {
        "schema_version": "d24.standalone-csharp.v1",
        "tasks": task_results,
        "status": "PASS" if all(bool(item.get("passed")) for item in task_results) else "FAIL_OR_BLOCKED",
        "passed": all(bool(item.get("passed")) for item in task_results),
        "runtime_deferred": [
            "仅执行纯 C# 逻辑编译/测试；Unity Player/PlayMode、场景、Prefab、Canvas、Input、物理、视觉、性能和网络未执行。"
        ],
    }
    encoded = json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=True)
    print(encoded)
    if args.json_out is not None:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(encoded + "\n", encoding="utf-8")
    return 0 if payload["passed"] else 1


if __name__ == "__main__":
    sys.exit(main())
