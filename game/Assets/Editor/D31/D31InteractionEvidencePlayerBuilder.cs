#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Whalebone.Editor.D31
{
    /// <summary>
    /// Builds the short, opt-in D31 interaction evidence Player.
    /// This is a validation artifact only and does not change the release build.
    /// </summary>
    public static class D31InteractionEvidencePlayerBuilder
    {
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";
        private const string HarborScene = "Assets/Scenes/InitialHarbor.unity";
        private const string OutputRelativePath = "tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_R45.exe";
        private const string LogRelativePath = "tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_BUILD_R45.log";

        [MenuItem("Whalebone/D31/Build Interaction Evidence Player R45")]
        public static void Build()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var output = Path.Combine(projectRoot, OutputRelativePath);
            var log = Path.Combine(projectRoot, LogRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { MainMenuScene, HarborScene },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };

            Debug.Log("[D31-S05] Building interaction evidence Player R45: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D31 interaction evidence Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(log,
                "D31 interaction evidence Player R45 build succeeded\n" +
                "scenes=" + MainMenuScene + ";" + HarborScene + "\n" +
                "output=" + output.Replace('\\', '/') + "\n" +
                "build_options=BuildOptions.None\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D31-S05] Interaction evidence Player R45 build succeeded: " + output);
        }
    }
}
#endif
