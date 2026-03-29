using System;
using System.Runtime.InteropServices;
using System.Text;
using MasterYoutubeDownloader.App.Resources.Enums;

namespace MasterYoutubeDownloader.App.Resources;


/**
    Resolução 1080p:
    "bin/yt-dlp.exe" --ffmpeg-location "bin/ffmpeg.exe" -f "bv[height<=1080][ext=mp4]+ba[ext=m4a]/b[height<=1080][ext=mp4]/b" --merge-output-format mp4 -o "C:/Users/SeuUsuario/Downloads/%(title)s.%(ext)s" "URL_AQUI"
    Resolução 720p:
    "bin/yt-dlp.exe" --ffmpeg-location "bin/ffmpeg.exe" -f "bv[height<=720][ext=mp4]+ba[ext=m4a]/b[height<=720][ext=mp4]/b" --merge-output-format mp4 -o "C:/Users/SeuUsuario/Downloads/%(title)s.%(ext)s" "URL_AQUI"

    ##########################################

    somente audio - video
    "bin/yt-dlp.exe" --ffmpeg-location "bin/ffmpeg.exe" -x --audio-format mp3 --audio-quality 0 -o "C:/Users/SeuUsuario/Downloads/%(title)s.%(ext)s" "URL_AQUI"
    somente audio - playlist
    "bin/ytdlp" --ffmpeg-location "bin/ffmpeg.exe" -x --audio-format mp3 --audio-quality 0 --yes-playlist -o "C:/Users/SeuUsuario/Downloads/%(playlist_title)s/%(playlist_index)s - %(title)s.%(ext)s" "URL_DA_PLAYLIST"

    ##########################################

    playlist
    Resolução 1080
    "bin/yt-dlp.exe" --ffmpeg-location "bin/ffmpeg.exe" -f "bv[height<=1080][ext=mp4]+ba[ext=m4a]/b" --yes-playlist --merge-output-format mp4 -o "C:/Users/SeuUsuario/Downloads/%(playlist_title)s/%(playlist_index)s - %(title)s.%(ext)s" "URL_DA_PLAYLIST"

    Resolução 720
    "bin/ytdlp" --ffmpeg-location "bin/ffmpeg.exe" -f "bv[height<=720][ext=mp4]+ba[ext=m4a]/b[height<=720][ext=mp4]/b" --yes-playlist --merge-output-format mp4 -o "C:/Users/SeuUsuario/Downloads/%(playlist_title)s/%(playlist_index)s - %(title)s.%(ext)s" "URL_DA_PLAYLIST"

*/
public class YTCommandBuilder
{
    private static readonly string BasePath = AppContext.BaseDirectory;
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static string BuildCommandVideoDownload
        (string url, bool isPlaylist = false, VideoResolution videoResolution = VideoResolution.HD)
    {
        var ytFullPath = Path.Combine(BasePath, "bin", IsWindows ? "ytdlp.exe" : "ytdlp");
        var fmFullPath = Path.Combine(BasePath, "bin", IsWindows ? "ffmpeg.exe" : "ffmpeg");
        
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        var command = new StringBuilder();

        command.Append($"\"{ytFullPath}\" --ffmpeg-location \"{fmFullPath}\" ");
        command.Append("--windows-filenames "); 
        command.Append($"-f \"bv[height<={(int)videoResolution}][vcodec^=avc]+ba[ext=m4a]/b[height<={(int)videoResolution}]\" ");
        command.Append(isPlaylist ? "--yes-playlist " : "--no-playlist ");
        command.Append("--merge-output-format mp4 ");

        if (isPlaylist)
            command.Append($"-o \"{downloadsPath}{Path.DirectorySeparatorChar}%(playlist_title)s{Path.DirectorySeparatorChar}%(playlist_index)s - %(title)s.%(ext)s\" ");
        else
            command.Append($"-o \"{downloadsPath}{Path.DirectorySeparatorChar}%(title)s.%(ext)s\" ");

        command.Append($"\"{url}\"");

        return command.ToString();
    }

    public static string BuildCommandAudioDownload(string url, bool isPlaylist = false)
    {
        var ytFullPath = Path.Combine(BasePath, "bin", IsWindows ? "ytdlp.exe" : "ytdlp");
        var fmFullPath = Path.Combine(BasePath, "bin", IsWindows ? "ffmpeg.exe" : "ffmpeg");
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        var command = new StringBuilder();

        command.Append($"\"{ytFullPath}\" --ffmpeg-location \"{fmFullPath}\" ");
        
        command.Append("--windows-filenames "); 
        command.Append("-x --audio-format mp3 --audio-quality 0 ");

        if (isPlaylist)
        {
            command.Append("--yes-playlist ");
            command.Append($"-o \"{downloadsPath}{Path.DirectorySeparatorChar}Musics-%(playlist_title)s{Path.DirectorySeparatorChar}%(playlist_index)s - %(title)s.%(ext)s\" ");
        }
        else
        {
            command.Append($"-o \"{downloadsPath}{Path.DirectorySeparatorChar}%(title)s.%(ext)s\" ");
        }

        command.Append($"\"{url}\"");

        return command.ToString();
    }
}