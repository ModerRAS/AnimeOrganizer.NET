using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AnimeOrganizer;

public record AnimeFileInfo(
    string Publisher,
    string AnimeName,
    string Episode,
    string Tags,
    string Extension,
    string OriginalPath
);

public static class FilenameParser
{
    private static readonly Regex AnimeFileRegex = new(
        @"^\[(?<publisher>[^\]]+)\]\s+(?<anime>.+?)\s+-\s+(?<episode>\d+)\s+(?<tags>\[.+\])(?<ext>\.\w+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static AnimeFileInfo? Parse(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var match = AnimeFileRegex.Match(fileName);
            
            if (!match.Success)
                return null;

            var publisher = match.Groups["publisher"].Value.Trim();
            var animeName = match.Groups["anime"].Value.Trim();
            var episode = match.Groups["episode"].Value.PadLeft(2, '0');
            var tags = match.Groups["tags"].Value.Trim();
            var extension = match.Groups["ext"].Value.ToLower();

            return new AnimeFileInfo(publisher, animeName, episode, tags, extension, filePath);
        }
        catch
        {
            return null;
        }
    }

    private static void CreateHardLinkInternal(string targetPath, string sourcePath)
    {
        try
        {
            // 尝试使用.NET 6+的File.CreateHardLink方法
            var method = typeof(File).GetMethod("CreateHardLink", new[] { typeof(string), typeof(string) });
            if (method != null)
            {
                method.Invoke(null, new object[] { targetPath, sourcePath });
                return;
            }
        }
        catch
        {
            // 如果反射调用失败，回退到平台特定实现
        }

        // 平台特定的硬链接实现
        if (OperatingSystem.IsWindows())
        {
            CreateWindowsHardLink(targetPath, sourcePath);
        }
        else
        {
            CreateUnixHardLink(targetPath, sourcePath);
        }
    }

    private static void CreateWindowsHardLink(string targetPath, string sourcePath)
    {
        const int ERROR_NOT_SAME_DEVICE = 17;
        
        try
        {
            // 使用Windows API创建硬链接
            if (!CreateHardLinkW(targetPath, sourcePath, IntPtr.Zero))
            {
                var error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                if (error == ERROR_NOT_SAME_DEVICE)
                {
                    throw new IOException("跨设备硬链接不支持", ERROR_NOT_SAME_DEVICE);
                }
                throw new IOException($"创建硬链接失败，错误码: {error}");
            }
        }
        catch (EntryPointNotFoundException)
        {
            // 如果API不可用，回退到复制
            throw new NotSupportedException("当前系统不支持硬链接");
        }
    }

    private static void CreateUnixHardLink(string targetPath, string sourcePath)
    {
        try
        {
            // 使用系统调用创建硬链接
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ln",
                    Arguments = $"\"{sourcePath}\" \"{targetPath}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode != 0)
            {
                if (error.Contains("Invalid cross-device link") || error.Contains("different device"))
                {
                    throw new IOException("跨设备硬链接不支持", 17);
                }
                throw new IOException($"创建硬链接失败: {error}");
            }
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new NotSupportedException("ln 命令不可用");
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLinkW(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
}

public enum OperationMode
{
    Move,
    Copy,
    Link
}

public static class FileOrganizer
{
    public static bool Organize(AnimeFileInfo animeFile, string targetRoot, OperationMode mode, bool dryRun = false)
    {
        try
        {
            var targetDir = Path.Combine(targetRoot, animeFile.AnimeName);
            var targetFileName = $"{animeFile.Episode} {animeFile.Tags}{animeFile.Extension}";
            var targetPath = Path.Combine(targetDir, targetFileName);

            if (dryRun)
            {
                Console.WriteLine($"[DRY-RUN] {animeFile.OriginalPath} -> {targetPath}");
                return true;
            }

            Directory.CreateDirectory(targetDir);

            switch (mode)
            {
                case OperationMode.Move:
                    File.Move(animeFile.OriginalPath, targetPath, overwrite: true);
                    break;
                case OperationMode.Copy:
                    File.Copy(animeFile.OriginalPath, targetPath, overwrite: true);
                    break;
                case OperationMode.Link:
                    try
                    {
                        // 使用平台特定的硬链接创建方法
                        CreateHardLinkInternal(targetPath, animeFile.OriginalPath);
                    }
                    catch (IOException ex) when (ex.HResult == 17 || ex.Message.Contains("cross-device") || ex.Message.Contains("different device"))
                    {
                        throw new InvalidOperationException("硬链接失败：源文件和目标必须在同一文件系统", ex);
                    }
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"处理文件失败 {animeFile.OriginalPath}: {ex.Message}");
            return false;
        }
    }

    private static void CreateHardLinkInternal(string targetPath, string sourcePath)
    {
        try
        {
            // 尝试使用.NET 6+的File.CreateHardLink方法
            var method = typeof(File).GetMethod("CreateHardLink", new[] { typeof(string), typeof(string) });
            if (method != null)
            {
                method.Invoke(null, new object[] { targetPath, sourcePath });
                return;
            }
        }
        catch
        {
            // 如果反射调用失败，回退到平台特定实现
        }

        // 平台特定的硬链接实现
        if (OperatingSystem.IsWindows())
        {
            CreateWindowsHardLink(targetPath, sourcePath);
        }
        else
        {
            CreateUnixHardLink(targetPath, sourcePath);
        }
    }

    private static void CreateWindowsHardLink(string targetPath, string sourcePath)
    {
        const int ERROR_NOT_SAME_DEVICE = 17;
        
        try
        {
            // 使用Windows API创建硬链接
            if (!CreateHardLinkW(targetPath, sourcePath, IntPtr.Zero))
            {
                var error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                if (error == ERROR_NOT_SAME_DEVICE)
                {
                    throw new IOException("跨设备硬链接不支持", ERROR_NOT_SAME_DEVICE);
                }
                throw new IOException($"创建硬链接失败，错误码: {error}");
            }
        }
        catch (EntryPointNotFoundException)
        {
            // 如果API不可用，回退到复制
            throw new NotSupportedException("当前系统不支持硬链接");
        }
    }

    private static void CreateUnixHardLink(string targetPath, string sourcePath)
    {
        try
        {
            // 使用系统调用创建硬链接
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ln",
                    Arguments = $"\"{sourcePath}\" \"{targetPath}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode != 0)
            {
                if (error.Contains("Invalid cross-device link") || error.Contains("different device"))
                {
                    throw new IOException("跨设备硬链接不支持", 17);
                }
                throw new IOException($"创建硬链接失败: {error}");
            }
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new NotSupportedException("ln 命令不可用");
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLinkW(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
}

public class Program
{
    private static readonly HashSet<string> DefaultExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".rmvb"
    };

    public static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            ShowHelp();
            return;
        }

        try
        {
            var options = ParseArguments(args);
            RunOrganizer(options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine(@"AnimeOrganizer.NET v1.0.0 - 跨平台动漫文件整理工具

用法: aniorg --source=<路径> [选项]

参数:
  --source <路径>       必填：源目录路径
  --target <路径>       目标根目录（默认：与源目录相同）
  --mode <move|copy|link> 操作模式（默认：move）
  --dry-run            仅预览不执行
  --include-ext <ext>  包含扩展名（默认：mp4,mkv,avi,mov,wmv,flv,rmvb）
  --verbose            显示详细日志
  --help               显示帮助

硬链接说明：
  使用 --mode=link 可创建硬链接，几乎不占用额外空间，但要求源和目标在同一文件系统。

示例:
  aniorg --source=""D:\Downloads"" --mode=link --target=""E:\Anime""
  aniorg --source=""/media/下载"" --dry-run --verbose");
    }

    private static Options ParseArguments(string[] args)
    {
        var options = new Options
        {
            Target = null,
            Mode = OperationMode.Move,
            DryRun = false,
            IncludeExt = DefaultExtensions,
            Verbose = false
        };

        foreach (var arg in args)
        {
            if (arg.StartsWith("--source="))
            {
                options.Source = arg.Substring("--source=".Length);
            }
            else if (arg.StartsWith("--target="))
            {
                options.Target = arg.Substring("--target=".Length);
            }
            else if (arg.StartsWith("--mode="))
            {
                var modeStr = arg.Substring("--mode=".Length).ToLower();
                options.Mode = modeStr switch
                {
                    "move" => OperationMode.Move,
                    "copy" => OperationMode.Copy,
                    "link" => OperationMode.Link,
                    _ => throw new ArgumentException($"无效的操作模式: {modeStr}")
                };
            }
            else if (arg == "--dry-run")
            {
                options.DryRun = true;
            }
            else if (arg.StartsWith("--include-ext="))
            {
                var extStr = arg.Substring("--include-ext=".Length);
                options.IncludeExt = new HashSet<string>(
                    extStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(ext => ext.StartsWith('.') ? ext : $".{ext}"),
                    StringComparer.OrdinalIgnoreCase
                );
            }
            else if (arg == "--verbose")
            {
                options.Verbose = true;
            }
        }

        if (string.IsNullOrEmpty(options.Source))
            throw new ArgumentException("--source 参数是必填的");

        if (!Directory.Exists(options.Source))
            throw new ArgumentException($"源目录不存在: {options.Source}");

        options.Target ??= options.Source;

        if (!Directory.Exists(options.Target))
            throw new ArgumentException($"目标目录不存在: {options.Target}");

        return options;
    }

    private static void RunOrganizer(Options options)
    {
        var files = Directory.GetFiles(options.Source, "*", SearchOption.AllDirectories);
        var processed = 0;
        var succeeded = 0;
        var failed = 0;

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            if (!options.IncludeExt.Contains(extension))
                continue;

            var animeFile = FilenameParser.Parse(file);
            if (animeFile == null)
            {
                if (options.Verbose)
                    Console.WriteLine($"跳过：无法解析文件名 {Path.GetFileName(file)}");
                continue;
            }

            processed++;
            
            if (FileOrganizer.Organize(animeFile, options.Target!, options.Mode, options.DryRun))
                succeeded++;
            else
                failed++;
        }

        Console.WriteLine($"处理完成：总计{processed}个文件，成功{succeeded}个，失败{failed}个");
    }

    private class Options
    {
        public string Source { get; set; } = null!;
        public string? Target { get; set; }
        public OperationMode Mode { get; set; }
        public bool DryRun { get; set; }
        public HashSet<string> IncludeExt { get; set; } = null!;
        public bool Verbose { get; set; }
    }
}