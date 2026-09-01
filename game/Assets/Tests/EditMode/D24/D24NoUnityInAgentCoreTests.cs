// D24-L21：AgentCore 工程闭包独立性检查（纯 C#；不扫描未编译遗留文件）。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Whalebone.Tests.D24
{
    public static class D24NoUnityInAgentCoreTests
    {
        public static int Run() { return RunAll(); }

        public static int RunAll()
        {
            try
            {
                string root = FindRepositoryRoot();
                string project = Path.Combine(root, "game", "AgentCore", "AgentCore.csproj");
                if (!File.Exists(project)) return Fail("AgentCore.csproj is missing");
                XDocument document = XDocument.Load(project);
                IEnumerable<XElement> elements = document.Descendants();
                string targetFramework = elements.FirstOrDefault(item => item.Name.LocalName == "TargetFramework")?.Value ?? string.Empty;
                int packageRefs = elements.Count(item => item.Name.LocalName == "PackageReference");
                int projectRefs = elements.Count(item => item.Name.LocalName == "ProjectReference");
                var compileFiles = new List<string>();
                foreach (XElement compile in elements.Where(item => item.Name.LocalName == "Compile"))
                {
                    string include = (string)compile.Attribute("Include");
                    if (string.IsNullOrWhiteSpace(include)) continue;
                    string file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project), include));
                    if (!File.Exists(file)) return Fail("linked source is missing: " + include);
                    compileFiles.Add(file);
                }
                // SDK-style projects include local *.cs by default. Include
                // that evaluated closure as well as explicit linked sources;
                // otherwise an excluded network/helper file could hide a
                // Unity dependency from this audit.
                string projectDirectory = Path.GetDirectoryName(project);
                compileFiles.AddRange(Directory.GetFiles(projectDirectory, "*.cs", SearchOption.TopDirectoryOnly));
                compileFiles = compileFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var forbidden = new[] { "UnityEngine", "UnityEditor", "MonoBehaviour", "UnityObject", "UnityEvent", "Unity.", "PackageReference", "ProjectReference" };
                int violations = 0;
                foreach (string file in compileFiles.Concat(new[] { project }))
                {
                    string text = File.ReadAllText(file);
                    foreach (string term in forbidden)
                        if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            violations++;
                            Console.WriteLine("[FAIL] D24-L21 " + Path.GetRelativePath(root, file) + " contains " + term);
                        }
                }
                Console.WriteLine("    target_framework=" + targetFramework + "; linked_sources=" + compileFiles.Count + "; package_refs=" + packageRefs + "; project_refs=" + projectRefs);
                return targetFramework == "net9.0" && compileFiles.Count > 0 && packageRefs == 0 && projectRefs == 0 && violations == 0 ? 0 : 1;
            }
            catch (Exception error)
            {
                Console.WriteLine("[EX] D24-L21 " + error.Message);
                return 1;
            }
        }

        private static int Fail(string message)
        {
            Console.WriteLine("[FAIL] D24-L21 " + message);
            return 1;
        }

        private static string FindRepositoryRoot()
        {
            string current = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "AGENTS.md"))) return current;
                current = Directory.GetParent(current)?.FullName;
            }
            throw new DirectoryNotFoundException("repository root with AGENTS.md was not found");
        }
    }
}
