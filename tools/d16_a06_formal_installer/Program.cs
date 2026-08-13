using System.Text.Json;

internal static class Program
{
    private const string PayloadExecutableName = "WhalesIslandReef.exe";

    private static int Main(string[] args)
    {
        try
        {
            var options = ParseOptions(args);
            if (options.ShowHelp)
            {
                PrintUsage();
                return 0;
            }

            var payload = RequirePath(options, "payload");
            var install = RequirePath(options, "install-dir");
            var userData = RequirePath(options, "user-data-dir");
            ValidatePaths(payload, install, userData);

            var result = options.Operation switch
            {
                "install" => Install(payload, install, userData),
                "update" => Update(payload, install, userData),
                "uninstall" => Uninstall(install, userData),
                _ => throw new InstallerException("operation 必须是 install、update 或 uninstall。"),
            };

            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
            }));
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("Whalebone installer failed: " + error.Message);
            return 1;
        }
    }

    private static InstallerResult Install(string payload, string install, string userData)
    {
        if (Directory.Exists(install))
            throw new InstallerException("安装目标已存在，请使用 update，拒绝覆盖现有安装。" );

        var staging = install + ".staging";
        var previous = install + ".previous";
        RemoveScopedDirectory(staging);
        RemoveScopedDirectory(previous);
        try
        {
            CopyPayload(payload, staging);
            Directory.Move(staging, install);
            Directory.CreateDirectory(userData);
        }
        catch
        {
            RemoveScopedDirectory(staging);
            RemoveScopedDirectory(install);
            throw;
        }

        return Result("install", install, userData);
    }

    private static InstallerResult Update(string payload, string install, string userData)
    {
        if (!Directory.Exists(install))
            throw new InstallerException("更新目标不存在，拒绝把 update 当作首次安装。" );

        var staging = install + ".staging";
        var previous = install + ".previous";
        RemoveScopedDirectory(staging);
        RemoveScopedDirectory(previous);
        var oldInstallMoved = false;
        try
        {
            CopyPayload(payload, staging);
            Directory.Move(install, previous);
            oldInstallMoved = true;
            Directory.Move(staging, install);
            RemoveScopedDirectory(previous);
            Directory.CreateDirectory(userData);
        }
        catch
        {
            RemoveScopedDirectory(staging);
            if (oldInstallMoved && Directory.Exists(previous))
            {
                RemoveScopedDirectory(install);
                Directory.Move(previous, install);
            }
            throw;
        }

        return Result("update", install, userData);
    }

    private static InstallerResult Uninstall(string install, string userData)
    {
        RemoveScopedDirectory(install + ".staging");
        RemoveScopedDirectory(install + ".previous");
        RemoveScopedDirectory(install);
        return Result("uninstall", install, userData);
    }

    private static void CopyPayload(string payload, string destination)
    {
        var executable = Path.Combine(payload, PayloadExecutableName);
        if (!File.Exists(executable))
            throw new InstallerException("构建目录缺少 " + PayloadExecutableName + "。" );

        CopyDirectory(payload, destination);
    }

    private static void CopyDirectory(string source, string destination)
    {
        var attributes = File.GetAttributes(source);
        if ((attributes & FileAttributes.ReparsePoint) != 0)
            throw new InstallerException("拒绝跟随符号链接或 reparse point: " + source);

        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source))
        {
            if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                throw new InstallerException("拒绝复制符号链接或 reparse point: " + file);
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        }

        foreach (var directory in Directory.EnumerateDirectories(source))
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
    }

    private static void RemoveScopedDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    private static void ValidatePaths(string payload, string install, string userData)
    {
        if (!Directory.Exists(payload))
            throw new InstallerException("构建目录不存在: " + payload);
        if (SameOrChild(install, payload) || SameOrChild(payload, install))
            throw new InstallerException("构建目录与安装目录必须互斥。" );
        if (SameOrChild(userData, install) || SameOrChild(install, userData))
            throw new InstallerException("用户数据目录必须独立于安装目录。" );
    }

    private static bool SameOrChild(string candidate, string parent)
    {
        var left = Normalize(candidate);
        var right = Normalize(parent);
        return left.Equals(right, StringComparison.OrdinalIgnoreCase) ||
               left.StartsWith(right + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string RequirePath(Options options, string name) =>
        options.Values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? Normalize(value)
            : throw new InstallerException("缺少参数 --" + name + "。" );

    private static Options ParseOptions(string[] args)
    {
        var options = new Options();
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase))
            {
                options.ShowHelp = true;
                continue;
            }
            if (!arg.StartsWith("--", StringComparison.Ordinal) || index + 1 >= args.Length)
                throw new InstallerException("无效参数: " + arg);
            options.Values[arg[2..]] = args[++index];
        }

        options.Operation = options.Values.TryGetValue("operation", out var operation)
            ? operation.ToLowerInvariant()
            : string.Empty;
        return options;
    }

    private static InstallerResult Result(string operation, string install, string userData) => new()
    {
        Operation = operation,
        Status = "PASS",
        InstallExists = Directory.Exists(install),
        UserDataPreserved = Directory.Exists(userData),
        Scope = "formal-portable-windows-installer",
        ExternalPublishPerformed = false,
    };

    private static void PrintUsage()
    {
        Console.WriteLine("WhaleboneIslandReef setup");
        Console.WriteLine("--operation install|update|uninstall --payload <dir> --install-dir <dir> --user-data-dir <dir>");
    }

    private sealed class Options
    {
        public string Operation { get; set; } = string.Empty;
        public bool ShowHelp { get; set; }
        public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class InstallerResult
    {
        public string Operation { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public bool InstallExists { get; init; }
        public bool UserDataPreserved { get; init; }
        public string Scope { get; init; } = string.Empty;
        public bool ExternalPublishPerformed { get; init; }
    }

    private sealed class InstallerException : Exception
    {
        public InstallerException(string message) : base(message) { }
    }
}
