using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WhalesIslandReef.Editor.Q03A21
{
    /// <summary>
    /// Builds the Q03-A21 visible D20 formal Player.
    ///
    /// This is a QA deliverable so the admitted Q03-A19 Humanoid action set can
    /// be seen in a real window. It does not change the default Player route:
    /// the D20 formal presentation is installed at runtime by
    /// Q03A21VisibleFormalPlayerRunner only.
    /// </summary>
    public static class Q03A21VisibleFormalPlayerBuilder
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string HarborScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string OutputPath =
            "tools/results/LOCAL-QUALITY/Q03-A21/Q03_A21_D20_FORMAL_ACTION_PLAYER_R01.exe";
        private const string BuildLogPath =
            "tools/results/LOCAL-QUALITY/Q03-A21/Q03_A21_D20_FORMAL_ACTION_PLAYER_R01_BUILD.log";

        [MenuItem("Tools/WhalesIslandReef/Q03-A21/Build Visible D20 Formal Action Player")]
        public static void BuildFromMenu() => Build();

        /// <summary>
        /// Compile-time anchor for the QA runner type. Referencing it here keeps
        /// the component's assembly reachable from the editor build path, so the
        /// managed linker cannot strip the runtime entry point.
        /// </summary>
        private static readonly Type[] QaRunnerAnchor =
        {
            typeof(WhalesIslandReef.QA.Q03A21.Q03A21VisibleFormalPlayerRunner),
        };

        public static void Build()
        {
            if (QaRunnerAnchor.Length == 0)
                throw new InvalidOperationException("Q03-A21 QA runner anchor is empty.");

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var output = Path.Combine(projectRoot, OutputPath).Replace('\\', '/');
            var log = Path.Combine(projectRoot, BuildLogPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { MainMenuScenePath, HarborScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                // Development + AllowDebugging keeps the Runtime Pipeline
                // control surface available for the QA probe loop.
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
            };

            Debug.Log("[Q03-A21] Building visible formal-action Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            File.WriteAllText(log,
                "Q03-A21 visible D20 formal action Player\n" +
                "result=" + summary.result + "\n" +
                "scenes=" + MainMenuScenePath + ";" + HarborScenePath + "\n" +
                "output=" + output + "\n" +
                "totalErrors=" + summary.totalErrors + "\n" +
                "totalWarnings=" + summary.totalWarnings + "\n" +
                "totalSize=" + summary.totalSize + "\n" +
                "buildTimeMs=" + summary.totalTime.TotalMilliseconds.ToString("F0") + "\n" +
                "timestampUtc=" + DateTime.UtcNow.ToString("O") + "\n");

            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    "Q03-A21 Player build failed: " + summary.result + "; errors=" + summary.totalErrors);

            Debug.Log("[Q03-A21] Build succeeded: errors=" + summary.totalErrors +
                      " warnings=" + summary.totalWarnings + " size=" + summary.totalSize);
        }
    }
}
