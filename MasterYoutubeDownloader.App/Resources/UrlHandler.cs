using System;
using MasterYoutubeDownloader.App.Resources.Enums;

namespace MasterYoutubeDownloader.App.Resources;

public struct BinaryUrl 
{
    public string FFMpegUrl { get; set; }
    public string YtdlpUrl { get; set; }
}

public class UrlHandler
{
    public static UrlType HandleUrlType(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return UrlType.Invalid;

        if (url.Contains("/playlist") || url.Contains("&list=") || url.Contains("?list="))
        {
            return UrlType.Playlist;
        }
        
        if (url.Contains("/watch") || url.Contains("youtu.be/") || url.Contains("/v/"))
        {
            return UrlType.Video;
        }

        return UrlType.Invalid;
    }

    public static BinaryUrl GetWinBinaryUrl() => new()
    {
        FFMpegUrl = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.4.1/ffmpeg-4.4.1-win-64.zip",
        YtdlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
    };

    public static BinaryUrl GetLinuxBinaryUrl() => new()
    {
        FFMpegUrl = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.4.1/ffmpeg-4.4.1-linux-64.zip",
        YtdlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp"
    };

    public static BinaryUrl GetMacBinaryUrl() => new()
    {
        FFMpegUrl = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.4.1/ffmpeg-4.4.1-osx-64.zip",
        YtdlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_macos"
    };
}
