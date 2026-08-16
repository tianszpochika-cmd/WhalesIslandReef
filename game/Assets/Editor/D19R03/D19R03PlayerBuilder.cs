#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Editor.D19R03
{
    /// <summary>
    /// Builds a task-owned D19 R03 validation Player from InitialHarbor.
    /// This is a validation build only; it does not alter build settings or
    /// the formal release configuration.
    /// </summary>
    public static class D19R03PlayerBuilder
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string OutputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN5.exe";
        private const string LogPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN5.log";

        [MenuItem("Whalebone/D19 R03/Build Validation Player Rerun 5")]
        public static void BuildFromMenu()
        {
            BuildPlayer();
        }

        [MenuItem("Whalebone/D19 R03/Build MainMenu Route Player Rerun 7")]
        public static void BuildMainMenuRoutePlayerRerun7FromMenu()
        {
            BuildMainMenuRoutePlayerRerun7();
        }

        public static void BuildMainMenuRoutePlayerRerun7()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN7.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN7.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log("[D19-R03] Building MainMenu route validation Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 MainMenu route Player build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 MainMenu route validation Player build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] MainMenu route validation Player build succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun8()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN8.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN8.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log("[D19-R03] Building MainMenu route validation Player RERUN8: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 MainMenu route Player RERUN8 build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 MainMenu route validation Player RERUN8 build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] MainMenu route validation Player RERUN8 build succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun9()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN9.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN9.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log("[D19-R03] Building MainMenu route validation Player RERUN9: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 MainMenu route Player RERUN9 build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 MainMenu route validation Player RERUN9 build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] MainMenu route validation Player RERUN9 build succeeded: " + output);
        }

        /// <summary>
        /// Builds a non-development local validation Player. The earlier
        /// development builds opened a Windows firewall prompt for Unity's
        /// PlayerConnection, which made genuine menu input impossible. This
        /// build keeps the route probe and runtime logs but does not enable
        /// PlayerConnection/debug networking. It is still a validation build,
        /// not a release build and does not touch project build settings.
        /// </summary>
        public static void BuildMainMenuRoutePlayerRerun10()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN10.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN10.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route validation Player RERUN10: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 MainMenu route Player RERUN10 build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route validation Player build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route validation Player RERUN10 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun20()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN20.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN20.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route validation Player RERUN20: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN20 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route validation Player RERUN20 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route validation Player RERUN20 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun26()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN26.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN26.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route validation Player RERUN26: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN26 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route validation Player RERUN26 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route validation Player RERUN26 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun29()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN29.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN29.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route validation Player RERUN29: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN29 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route validation Player RERUN29 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route validation Player RERUN29 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build MainMenu Route Player Rerun 41")]
        public static void BuildMainMenuRoutePlayerRerun41()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN41.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN41.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route validation Player RERUN41: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN41 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route validation Player RERUN41 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route validation Player RERUN41 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build MainMenu Route Player Rerun 42")]
        public static void BuildMainMenuRoutePlayerRerun42()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN42.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN42.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route validation Player RERUN42: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN42 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route validation Player RERUN42 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route validation Player RERUN42 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun43()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN43.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN43.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route diagnostic Player RERUN43: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN43 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route diagnostic Player RERUN43 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route diagnostic Player RERUN43 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun44()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN44.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN44.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN44: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN44 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN44 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN44 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun45()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN45.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN45.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN45: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN45 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN45 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN45 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun46()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN46.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN46.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN46: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN46 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN46 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN46 succeeded: " + output);
        }

        public static void BuildFixedShotPlayerRerun15()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A02/D19_R03_A02_Player_RERUN15.exe";
            const string logPath = "tools/results/D19/D19-R03-A02/D19_R03_A02_PLAYER_BUILD_RERUN15.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03-A02] Building non-development fixed-shot Player RERUN15: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 fixed-shot Player RERUN15 build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 fixed-shot Player build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03-A02] Fixed-shot Player RERUN15 succeeded: " + output);
        }

        public static void BuildFixedShotPlayerRerun16()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A02/D19_R03_A02_Player_RERUN16.exe";
            const string logPath = "tools/results/D19/D19-R03-A02/D19_R03_A02_PLAYER_BUILD_RERUN16.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03-A02] Building non-development fixed-shot Player RERUN16: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 fixed-shot Player RERUN16 build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 fixed-shot Player build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03-A02] Fixed-shot Player RERUN16 succeeded: " + output);
        }

        public static void BuildFixedShotPlayerRerun17()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A02/D19_R03_A02_Player_RERUN17.exe";
            const string logPath = "tools/results/D19/D19-R03-A02/D19_R03_A02_PLAYER_BUILD_RERUN17.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03-A02] Building non-development fixed-shot Player RERUN17: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 fixed-shot Player RERUN17 build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 fixed-shot Player build succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03-A02] Fixed-shot Player RERUN17 succeeded: " + output);
        }

        public static void CleanupValidationHost()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var host = GameObject.Find("D19_R03_ValidationProbeHost");
            if (host != null) UnityEngine.Object.DestroyImmediate(host);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[D19-R03] Validation probe host removed from formal scene");
        }

        public static void BuildPlayer()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var output = Path.Combine(projectRoot, OutputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log("[D19-R03] Building validation Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "D19 R03 validation Player build failed: " + report.summary.result +
                    "; errors=" + report.summary.totalErrors);
            }

            File.WriteAllText(Path.Combine(projectRoot, LogPath),
                "D19 R03 validation Player build succeeded\n" +
                "scene=" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Validation Player build succeeded: " + output);
        }
    }
}
#endif
