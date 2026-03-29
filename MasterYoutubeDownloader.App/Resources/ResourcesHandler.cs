using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using MasterYoutubeDownloader.App.Resources.Enums;
using Spectre.Console;

namespace MasterYoutubeDownloader.App.Resources;

public class ResourcesHandler
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    public readonly bool IsOsUnknown;
    public readonly string BasePath;
    public readonly string YtdlpBinPath;
    public readonly string FFmpegBinPath;
    private Dictionary<OSType, string> _ytdlpBinUrlDict = new();
    private Dictionary<OSType, string> _ffmpegBinUrlDict = new();

    public ResourcesHandler()
    {
        BasePath = AppDomain.CurrentDomain.BaseDirectory;
        var os = CheckOS();
        IsOsUnknown = os is OSType.Unknown;

        var win = UrlHandler.GetWinBinaryUrl();
        var lin = UrlHandler.GetLinuxBinaryUrl();
        var mac = UrlHandler.GetMacBinaryUrl();

        _ytdlpBinUrlDict = new Dictionary<OSType, string> { { OSType.Win, win.YtdlpUrl }, { OSType.Linux, lin.YtdlpUrl }, { OSType.MacOs, mac.YtdlpUrl } };
        _ffmpegBinUrlDict = new Dictionary<OSType, string> { { OSType.Win, win.FFMpegUrl }, { OSType.Linux, lin.FFMpegUrl }, { OSType.MacOs, mac.FFMpegUrl } };

        
        string ext = (os is OSType.Win) ? ".exe" : "";
        YtdlpBinPath = Path.Combine(BasePath, "bin", $"ytdlp{ext}"); 
        FFmpegBinPath = Path.Combine(BasePath, "bin", $"ffmpeg{ext}");
    }

    public async Task DonwloadYTdlpResource()
    {
        if (File.Exists(YtdlpBinPath)) return;

        Directory.CreateDirectory(Path.GetDirectoryName(YtdlpBinPath)!);
        var downloadUrl = _ytdlpBinUrlDict[CheckOS()];

        await AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[yellow]Baixando Ferramenta de Download...[/]", async ctx =>
        {
            var response = await _httpClient.GetAsync(downloadUrl);
            using var fs = new FileStream(YtdlpBinPath, FileMode.Create);
            await response.Content.CopyToAsync(fs);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start("chmod", $"+x \"{YtdlpBinPath}\"");
        });
        AnsiConsole.MarkupLine("[green]✔ Ferramenta de Download Pronta![/]");
    }

    public async Task DonwloadFFMpegResource()
    {
        if (File.Exists(FFmpegBinPath)) return;

        var binDir = Path.GetDirectoryName(FFmpegBinPath)!;
        Directory.CreateDirectory(binDir);
        
        var downloadUrl = _ffmpegBinUrlDict[CheckOS()];
        var zipPath = Path.Combine(binDir, "ffmpeg.zip");

        await AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("[yellow]Baixando Ferramenta de Conversão de vídeo...[/]", async ctx =>
        {
            using var response = await _httpClient.GetStreamAsync(downloadUrl);
            using var fs = new FileStream(zipPath, FileMode.Create);
            await response.CopyToAsync(fs);
            fs.Close(); 

            ctx.Status("[green]Extraindo binários...[/]");
            ZipFile.ExtractToDirectory(zipPath, binDir, overwriteFiles: true);
            File.Delete(zipPath);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start("chmod", $"+x \"{FFmpegBinPath}\"");
        });
        AnsiConsole.MarkupLine("[green]✔ Ferramenta de Conversão de Vídeo Pronta![/]");
    }

    private OSType CheckOS()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSType.Win;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSType.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSType.MacOs;
        return OSType.Unknown;
    }

    public bool CheckIfBinariesExists()
    {
        return File.Exists(FFmpegBinPath) && File.Exists(YtdlpBinPath);
    }
}