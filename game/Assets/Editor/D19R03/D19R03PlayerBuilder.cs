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

        [MenuItem("Whalebone/D19 R03/Build MainMenu Route Player Rerun 47")]
        public static void BuildMainMenuRoutePlayerRerun47()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN47.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN47.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN47: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN47 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN47 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN47 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build MainMenu Route Player Rerun 48")]
        public static void BuildMainMenuRoutePlayerRerun48()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN48: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN48 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN48 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN48 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun49()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN49.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN49.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN49: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN49 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN49 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN49 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun50()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN50.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN50.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN50: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN50 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN50 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN50 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun51()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN51.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN51.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN51: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN51 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN51 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN51 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun52()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN52.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN52.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN52: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN52 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN52 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN52 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun53()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN53.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN53.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN53: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN53 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN53 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN53 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun54()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN54.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN54.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN54: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN54 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN54 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN54 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun55()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN55.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN55.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN55: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN55 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN55 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN55 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun56()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN56.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN56.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN56: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN56 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN56 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN56 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun57()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN57.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN57.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN57: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN57 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN57 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN57 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerRerun58()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN58.exe";
            const string logPath = "tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN58.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[D19-R03] Building non-development MainMenu route Player RERUN58: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D19 R03 MainMenu route Player RERUN58 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "D19 R03 non-development MainMenu route Player RERUN58 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D19-R03] Non-development MainMenu route Player RERUN58 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerQ26PerfR1()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q26/Q26_PERF_PLAYER_R1.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q26/Q26_PERF_PLAYER_R1_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q26-PERF] Building non-development performance Player Q26-PERF-R1: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q26 performance Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q26 performance Player R1 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q26-PERF] Non-development performance Player Q26-PERF-R1 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerQ27ProfileR1()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q27/Q27_D16_PROFILE_PLAYER_R1.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q27/Q27_D16_PROFILE_PLAYER_R1_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q27-PROFILE] Building non-development D16 profile Player Q27-D16-PROFILE-R1: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q27 D16 profile Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q27 D16 profile Player R1 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q27-PROFILE] Non-development D16 profile Player Q27-D16-PROFILE-R1 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q28 Candidate R1")]
        public static void BuildMainMenuRoutePlayerQ28CandidateR1()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R1.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R1_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q28] Building non-development candidate Player Q28-CANDIDATE-R1: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q28 candidate Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q28 candidate Player R1 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q28] Non-development candidate Player Q28-CANDIDATE-R1 succeeded: " + output);
        }

        public static void BuildMainMenuRoutePlayerQ28CandidateR2()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R2.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R2_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q28] Building non-development candidate Player Q28-CANDIDATE-R2: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q28 candidate Player R2 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q28 candidate Player R2 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q28] Non-development candidate Player Q28-CANDIDATE-R2 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q28 Candidate R3")]
        public static void BuildMainMenuRoutePlayerQ28CandidateR3()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R3.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R3_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q28] Building non-development candidate Player Q28-CANDIDATE-R3: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q28 candidate Player R3 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q28 candidate Player R3 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q28] Non-development candidate Player Q28-CANDIDATE-R3 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q29 UI Navigation Candidate R4")]
        public static void BuildMainMenuRoutePlayerQ29UiNavigationCandidateR4()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q29/Q29_UI_NAVIGATION_CANDIDATE_R4.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q29/Q29_UI_NAVIGATION_CANDIDATE_R4_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q29] Building non-development UI-navigation candidate Player R4: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q29 UI-navigation candidate Player R4 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q29 UI-navigation candidate Player R4 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q29] Non-development UI-navigation candidate Player R4 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q26 D6 Foam Cleanup Candidate R1")]
        public static void BuildMainMenuRoutePlayerQ26D6FoamCleanupCandidateR1()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q26/Q26_D6_FOAM_CLEANUP_CANDIDATE_R1.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q26/Q26_D6_FOAM_CLEANUP_CANDIDATE_R1_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q26] Building non-development D6 foam cleanup candidate Player R1: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q26 D6 foam cleanup candidate Player R1 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q26 D6 foam cleanup candidate Player R1 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q26] Non-development D6 foam cleanup candidate Player R1 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q26 Spatial Cleanup Candidate R2")]
        public static void BuildMainMenuRoutePlayerQ26SpatialCleanupCandidateR2()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q26/Q26_SPATIAL_CLEANUP_CANDIDATE_R2.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q26/Q26_SPATIAL_CLEANUP_CANDIDATE_R2_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q26] Building non-development spatial cleanup candidate Player R2: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q26 spatial cleanup candidate Player R2 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q26 spatial cleanup candidate Player R2 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q26] Non-development spatial cleanup candidate Player R2 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q26 Pirate Port Grounding Candidate R3")]
        public static void BuildMainMenuRoutePlayerQ26PiratePortGroundingCandidateR3()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q26/Q26_PIRATE_PORT_GROUNDING_CANDIDATE_R3.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q26/Q26_PIRATE_PORT_GROUNDING_CANDIDATE_R3_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q26] Building non-development pirate port grounding candidate Player R3: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q26 pirate port grounding candidate Player R3 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q26 pirate port grounding candidate Player R3 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q26] Non-development pirate port grounding candidate Player R3 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q03-A04 Latest Candidate")]
        public static void BuildMainMenuRoutePlayerQ03A04LatestCandidate()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q03-A04/Q03_A04_LATEST_CANDIDATE_PLAYER.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q03-A04/Q03_A04_LATEST_CANDIDATE_PLAYER_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q03-A04] Building latest opt-in candidate Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q03-A04 latest candidate Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q03-A04 latest opt-in candidate Player succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "runtime_flag=-d20Candidate\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q03-A04] Latest opt-in candidate Player succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q03-A05 Fish Pose Candidate")]
        public static void BuildMainMenuRoutePlayerQ03A05FishPoseCandidate()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q03-A05/Q03_A05_FISH_POSE_CANDIDATE_PLAYER.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q03-A05/Q03_A05_FISH_POSE_CANDIDATE_PLAYER_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q03-A05] Building Fish pose candidate Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q03-A05 Fish pose candidate Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q03-A05 Fish pose candidate Player succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "runtime_flag=-d20Candidate\n" +
                "fish_arm_excursion_degrees=8\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q03-A05] Fish pose candidate Player succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q03-A06 Fish Orientation Candidate")]
        public static void BuildMainMenuRoutePlayerQ03A06FishOrientationCandidate()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q03-A06/Q03_A06_FISH_ORIENTATION_CANDIDATE_PLAYER.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q03-A06/Q03_A06_FISH_ORIENTATION_CANDIDATE_PLAYER_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q03-A06] Building Fish orientation candidate Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q03-A06 Fish orientation candidate Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q03-A06 Fish orientation candidate Player succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "runtime_flag=-d20Candidate\n" +
                "fish_root_pitch_degrees=32\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q03-A06] Fish orientation candidate Player succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q03-A07 Fish Rebind Candidate")]
        public static void BuildMainMenuRoutePlayerQ03A07FishRebindCandidate()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q03-A07/Q03_A07_FISH_REBIND_CANDIDATE_PLAYER.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q03-A07/Q03_A07_FISH_REBIND_CANDIDATE_PLAYER_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q03-A07] Building Fish rebind candidate Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q03-A07 Fish rebind candidate Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q03-A07 Fish rebind candidate Player succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "runtime_flag=-d20Candidate\n" +
                "candidate_generic_animator_rebind=true\n" +
                "fish_root_pitch_degrees=90\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q03-A07] Fish rebind candidate Player succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R1")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR1()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R1.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R1_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R1: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R1 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R1 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R2")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR2()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R2.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R2_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R2: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R2 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R2 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R2 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R3")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR3()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R3.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R3_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R3: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R3 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R3 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R3 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R4")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR4()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R4.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R4_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R4: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R4 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R4 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R4 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R5")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR5()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R5.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R5_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R5: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R5 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R5 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R5 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R6")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR6()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R6.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R6_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R6: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R6 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R6 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R6 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R7")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR7()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R7.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R7_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R7: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R7 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R7 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R7 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R8")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR8()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R8.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R8_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R8: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R8 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R8 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R8 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R9")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR9()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R9.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R9_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R9: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R9 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R9 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R9 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R10")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR10()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R10.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R10_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R10: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R10 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R10 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R10 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R11")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR11()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R11.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R11_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R11: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R11 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R11 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R11 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R12")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR12()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R12.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R12_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R12: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R12 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R12 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R12 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R13")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR13()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R13.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R13_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R13: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R13 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R13 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R13 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R14")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR14()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R14.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R14_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R14: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R14 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R14 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R14 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R15")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR15()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R15.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R15_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R15: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R15 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R15 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R15 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q30 Candidate R16")]
        public static void BuildMainMenuRoutePlayerQ30CandidateR16()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R16.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R16_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q30] Building non-development candidate Player Q30-CANDIDATE-R16: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q30 candidate Player R16 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q30 candidate Player R16 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q30] Non-development candidate Player Q30-CANDIDATE-R16 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A01 Candidate R17")]
        public static void BuildMainMenuRoutePlayerQ21A01CandidateR17()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A01/Q21_A01_CANDIDATE_PLAYER_R17.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A01/Q21_A01_CANDIDATE_PLAYER_R17_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A01] Building non-development candidate Player R17: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A01 candidate Player R17 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A01 candidate Player R17 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A01] Non-development candidate Player R17 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A02 Candidate R18")]
        public static void BuildMainMenuRoutePlayerQ21A02CandidateR18()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A02/Q21_A02_CANDIDATE_PLAYER_R18.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A02/Q21_A02_CANDIDATE_PLAYER_R18_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A02] Building non-development candidate Player R18: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A02 candidate Player R18 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A02 candidate Player R18 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A02] Non-development candidate Player R18 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A03 Candidate R19")]
        public static void BuildMainMenuRoutePlayerQ21A03CandidateR19()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A03/Q21_A03_CANDIDATE_PLAYER_R19.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A03/Q21_A03_CANDIDATE_PLAYER_R19_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A03] Building non-development candidate Player R19: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A03 candidate Player R19 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A03 candidate Player R19 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A03] Non-development candidate Player R19 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A03 Candidate R20")]
        public static void BuildMainMenuRoutePlayerQ21A03CandidateR20()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A03/Q21_A03_CANDIDATE_PLAYER_R20.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A03/Q21_A03_CANDIDATE_PLAYER_R20_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A03] Building non-development candidate Player R20: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A03 candidate Player R20 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A03 candidate Player R20 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A03] Non-development candidate Player R20 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A04 Candidate R21")]
        public static void BuildMainMenuRoutePlayerQ21A04CandidateR21()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R21.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R21_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A04] Building non-development candidate Player R21: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A04 candidate Player R21 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A04 candidate Player R21 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A04] Non-development candidate Player R21 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A04 Candidate R22")]
        public static void BuildMainMenuRoutePlayerQ21A04CandidateR22()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R22.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R22_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A04] Building non-development candidate Player R22: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A04 candidate Player R22 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A04 candidate Player R22 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A04] Non-development candidate Player R22 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A04 Candidate R23")]
        public static void BuildMainMenuRoutePlayerQ21A04CandidateR23()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R23.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R23_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A04] Building non-development candidate Player R23: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A04 candidate Player R23 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A04 candidate Player R23 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A04] Non-development candidate Player R23 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A04 Candidate R24")]
        public static void BuildMainMenuRoutePlayerQ21A04CandidateR24()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R24.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R24_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A04] Building non-development candidate Player R24: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A04 candidate Player R24 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A04 candidate Player R24 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A04] Non-development candidate Player R24 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q03-A01 Candidate R25")]
        public static void BuildMainMenuRoutePlayerQ03A01CandidateR25()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q03-A01/Q03_A01_CANDIDATE_PLAYER_R25.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q03-A01/Q03_A01_CANDIDATE_PLAYER_R25_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q03-A01] Building non-development candidate Player R25: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q03-A01 candidate Player R25 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q03-A01 candidate Player R25 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q03-A01] Non-development candidate Player R25 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q20-A01 Candidate R26")]
        public static void BuildMainMenuRoutePlayerQ20A01CandidateR26()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q20-A01/Q20_A01_CANDIDATE_PLAYER_R26.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q20-A01/Q20_A01_CANDIDATE_PLAYER_R26_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q20-A01] Building non-development candidate Player R26: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q20-A01 candidate Player R26 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q20-A01 candidate Player R26 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q20-A01] Non-development candidate Player R26 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q21-A05 Candidate R27")]
        public static void BuildMainMenuRoutePlayerQ21A05CandidateR27()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q21-A05/Q21_A05_CANDIDATE_PLAYER_R27.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q21-A05/Q21_A05_CANDIDATE_PLAYER_R27_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q21-A05] Building non-development candidate Player R27: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q21-A05 candidate Player R27 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q21-A05 candidate Player R27 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q21-A05] Non-development candidate Player R27 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q05-A01 Candidate R28")]
        public static void BuildMainMenuRoutePlayerQ05A01CandidateR28()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q05-A01/Q05_A01_CANDIDATE_PLAYER_R28.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q05-A01/Q05_A01_CANDIDATE_PLAYER_R28_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q05-A01] Building non-development candidate Player R28: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q05-A01 candidate Player R28 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q05-A01 candidate Player R28 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q05-A01] Non-development candidate Player R28 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q05-A02 Candidate R29")]
        public static void BuildMainMenuRoutePlayerQ05A02CandidateR29()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q05-A02/Q05_A02_CANDIDATE_PLAYER_R29.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q05-A02/Q05_A02_CANDIDATE_PLAYER_R29_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q05-A02] Building non-development candidate Player R29: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q05-A02 candidate Player R29 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q05-A02 candidate Player R29 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q05-A02] Non-development candidate Player R29 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q05-A03 Candidate R30")]
        public static void BuildMainMenuRoutePlayerQ05A03CandidateR30()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q05-A03/Q05_A03_CANDIDATE_PLAYER_R30.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q05-A03/Q05_A03_CANDIDATE_PLAYER_R30_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q05-A03] Building non-development candidate Player R30: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q05-A03 candidate Player R30 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q05-A03 candidate Player R30 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q05-A03] Non-development candidate Player R30 succeeded: " + output);
        }

        [MenuItem("Whalebone/D19 R03/Build LOCAL QUALITY Q05-A03 Candidate R31")]
        public static void BuildMainMenuRoutePlayerQ05A03CandidateR31()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            const string mainMenuScene = "Assets/Scenes/MainMenu.unity";
            const string outputPath = "tools/results/LOCAL-QUALITY/Q05-A03/Q05_A03_CANDIDATE_PLAYER_R31.exe";
            const string logPath = "tools/results/LOCAL-QUALITY/Q05-A03/Q05_A03_CANDIDATE_PLAYER_R31_BUILD.log";
            var output = Path.Combine(projectRoot, outputPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { mainMenuScene, ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Q05-A03] Building non-development candidate Player R31: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Q05-A03 candidate Player R31 build failed: " + report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(Path.Combine(projectRoot, logPath),
                "Q05-A03 candidate Player R31 succeeded\n" +
                "scenes=" + mainMenuScene + ";" + ScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None (no Development/AllowDebugging PlayerConnection)\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[Q05-A03] Non-development candidate Player R31 succeeded: " + output);
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
