// D24-L20：D18-D23 活跃逻辑源的禁用依赖静态检查（纯 C#）。

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Whalebone.Tests.D24
{
    public static class D24NoNetworkInSystemsTests
    {
        private static readonly string[] Forbidden =
        {
            "UnityEngine", "UnityEditor", "MonoBehaviour", "HttpClient", "System.Net.Http",
            "TcpClient", "UdpClient", "Socket", "WebRequest", "WebClient", "OpenAI", "DeepSeek",
            "Ollama", "LLM", "DateTime.Now", "DateTime.UtcNow", "Guid.NewGuid", "Task.Delay",
            "Thread.Sleep", "Environment.TickCount",
        };

        public static int Run() { return RunAll(); }

        public static int RunAll()
        {
            try
            {
                string root = FindRepositoryRoot();
                if (!ContainsForbidden("LLM", "LLM") || ContainsForbidden("HullMax", "LLM"))
                {
                    Console.WriteLine("[FAIL] D24-L20 LLM token boundary self-test failed");
                    return 1;
                }
                var files = new List<string>();
                foreach (string chapter in new[] { "D18", "D19", "D20", "D21", "D22", "D23" })
                {
                    string directory = Path.Combine(root, "game", "Assets", "Scripts", "Systems", chapter);
                    if (!Directory.Exists(directory)) continue;
                    files.AddRange(Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories));
                }
                int hits = 0;
                foreach (string file in files)
                {
                    string masked = Mask(File.ReadAllText(file, Encoding.UTF8));
                    foreach (string term in Forbidden)
                        if (ContainsForbidden(masked, term))
                        {
                            hits++;
                            Console.WriteLine("[FAIL] D24-L20 " + Path.GetRelativePath(root, file) + " contains " + term);
                        }
                }
                Console.WriteLine("    systems_files=" + files.Count + "; forbidden_hits=" + hits + "; llm_token_boundary_checks=pass; optional_missing_D18_D23_are_not_runtime_evidence");
                return hits == 0 && files.Count > 0 ? 0 : 1;
            }
            catch (Exception error)
            {
                Console.WriteLine("[EX] D24-L20 " + error.Message);
                return 1;
            }
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

        private static string Mask(string source)
        {
            char[] output = source.ToCharArray();
            int index = 0;
            while (index < source.Length)
            {
                char current = source[index];
                char next = index + 1 < source.Length ? source[index + 1] : '\0';
                if (current == '/' && next == '/')
                {
                    int end = source.IndexOf('\n', index);
                    MaskRange(output, index, end < 0 ? source.Length : end);
                    index = end < 0 ? source.Length : end;
                    continue;
                }
                if (current == '/' && next == '*')
                {
                    int end = source.IndexOf("*/", index + 2, StringComparison.Ordinal);
                    end = end < 0 ? source.Length : end + 2;
                    MaskRange(output, index, end);
                    index = end;
                    continue;
                }
                if (current != '"' && current != '\'') { index++; continue; }

                char quote = current;
                int rawWidth = 0;
                if (quote == '"')
                {
                    int width = 0;
                    while (index + width < source.Length && source[index + width] == '"') width++;
                    if (width >= 3) rawWidth = width;
                }
                bool verbatim = quote == '"' &&
                    ((index > 0 && source[index - 1] == '@') ||
                     (index > 1 && ((source[index - 2] == '$' && source[index - 1] == '@') ||
                                    (source[index - 2] == '@' && source[index - 1] == '$'))));
                int cursor = index + (rawWidth == 0 ? 1 : rawWidth);
                if (rawWidth > 0)
                {
                    string closing = new string('"', rawWidth);
                    int end = source.IndexOf(closing, cursor, StringComparison.Ordinal);
                    cursor = end < 0 ? source.Length : end + rawWidth;
                }
                else
                {
                    while (cursor < source.Length)
                    {
                        char value = source[cursor];
                        if (value == '\n' && !verbatim) { cursor++; break; }
                        if (value == quote)
                        {
                            if (verbatim && cursor + 1 < source.Length && source[cursor + 1] == quote) { cursor += 2; continue; }
                            cursor++;
                            break;
                        }
                        if (!verbatim && value == '\\') cursor += 2;
                        else cursor++;
                    }
                }
                MaskRange(output, index, cursor);
                index = cursor;
            }
            return new string(output);
        }

        private static bool ContainsForbidden(string source, string term)
        {
            if (!string.Equals(term, "LLM", StringComparison.Ordinal))
                return source.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

            int searchStart = 0;
            while (searchStart < source.Length)
            {
                int index = source.IndexOf(term, searchStart, StringComparison.OrdinalIgnoreCase);
                if (index < 0) return false;
                bool leftBoundary = index == 0 || !IsIdentifierChar(source[index - 1]);
                int end = index + term.Length;
                bool rightBoundary = end >= source.Length || !IsIdentifierChar(source[end]);
                if (leftBoundary && rightBoundary) return true;
                searchStart = end;
            }
            return false;
        }

        private static bool IsIdentifierChar(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }

        private static void MaskRange(char[] output, int start, int end)
        {
            for (int index = start; index < end && index < output.Length; index++)
                if (output[index] != '\n' && output[index] != '\r') output[index] = ' ';
        }
    }
}
