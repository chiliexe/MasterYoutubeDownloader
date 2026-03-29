using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MasterYoutubeDownloader.App.Resources.Enums;
using Spectre.Console;
using YoutubeExplode.Playlists;

namespace MasterYoutubeDownloader.App.Resources;

public class YtdlpHandler
{
    private ResourcesHandler _resourcesHandler { get; set; }
    public YtdlpHandler(ResourcesHandler resourcesHandler)
    {
        _resourcesHandler = resourcesHandler;
    }

    public async Task DownloadVideo
    (string url, FileType fileType, VideoResolution videoResolution = VideoResolution.HD)
    {
        if (!_resourcesHandler.CheckIfBinariesExists())
        {
            AnsiConsole.MarkupLine("Recurso necessário não encontrado, favor instalar novamente.");
            return;
        }
        bool downloadRegistrado = false;
        bool conversaoRegistrada = false;
        url += url.Split('&')[0];

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots14)
            .StartAsync("[yellow][[Info]] Analisando Link... [/]", async ctx =>
            {
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                string command = string.Empty;

                if(fileType == FileType.Video)
                    command = YTCommandBuilder.BuildCommandVideoDownload(url, false, videoResolution);
                else
                    command = YTCommandBuilder.BuildCommandAudioDownload(url, false);

                var startInfo = new ProcessStartInfo
                {
                    FileName = isWindows ? "cmd.exe" : "/bin/bash",
                    Arguments = isWindows ? $"/S /C \"{command}\"" : $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    // RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = startInfo };


                // process.ErrorDataReceived += (sender, e) =>
                // {
                //     if (!string.IsNullOrEmpty(e.Data))
                //         AnsiConsole.MarkupLine($"[red]Erro yt-dlp:[/] {e.Data.EscapeMarkup()}");
                // };


                process.OutputDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;

                    if (e.Data.Contains("has already been downloaded"))
                    {
                        AnsiConsole.MarkupLine("[red][[Info]][/] O vídeo já existe na pasta de Downloads");
                        return;
                    }

                    var match = Regex.Match(e.Data, @"(\d+(\.\d+)?)%");

                    if (e.Data.Contains("[ffmpeg]") || e.Data.Contains("Merging") || e.Data.Contains("Converting"))
                    {
                        ctx.Status("[bold blue][[Status]][/] Formatando/Convertendo Vídeo...");

                        if (!conversaoRegistrada)
                            conversaoRegistrada = true;

                    }
                    else if (match.Success)
                    {
                        var percent = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        ctx.Status($"[bold blue][[Status]][/] Baixando Vídeo - {percent}%");

                        if (percent >= 100 && !downloadRegistrado)
                        {
                            AnsiConsole.MarkupLine("[yellow][[Info]][/] Download do arquivo concluído!");
                            downloadRegistrado = true;
                        }
                    }
                    else if (!downloadRegistrado)
                    {
                        ctx.Status("[bold blue][[Status]][/] Analisando e iniciando download...");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                // process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            });
        if (downloadRegistrado)
            AnsiConsole.MarkupLine("[green]Download concluído! - Arquivo salvo na Pasta de Downloads[/]");
    }

    public async Task DownloadPlaylist
    (string url, FileType fileType, VideoResolution videoResolution = VideoResolution.HD)
    {
        if (!_resourcesHandler.CheckIfBinariesExists())
        {
            AnsiConsole.MarkupLine("Recurso necessário não encontrado, favor instalar novamente.");
            return;
        }

        bool downloadRegistrado = false;
        // bool conversaoRegistrada = false;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots14)
            .StartAsync("[yellow][[Info]] Analisando Link... [/]", async ctx =>
            {
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                string command = string.Empty;

                if(fileType == FileType.Video)
                    command = YTCommandBuilder.BuildCommandVideoDownload(url, true, videoResolution);
                else
                    command = YTCommandBuilder.BuildCommandAudioDownload(url, true);

                var startInfo = new ProcessStartInfo
                {
                    FileName = isWindows ? "cmd.exe" : "/bin/bash",
                    Arguments = isWindows ? $"/S /C \"{command}\"" : $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    // RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = startInfo };


                // process.ErrorDataReceived += (sender, e) =>
                // {
                //     if (!string.IsNullOrEmpty(e.Data))
                //         AnsiConsole.MarkupLine($"[red]Erro yt-dlp:[/] {e.Data.EscapeMarkup()}");
                // };


                string totalItens = "0";
                string itemAtual = "0";
                string tituloAtual = "Analisando...";

                process.OutputDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;

                    // 1. Pega o progresso da playlist (1 of 4)
                    var playlistMatch = Regex.Match(e.Data, @"Downloading item (\d+) of (\d+)");
                    if (playlistMatch.Success)
                    {
                        itemAtual = playlistMatch.Groups[1].Value;
                        totalItens = playlistMatch.Groups[2].Value;
                    }

                    // 2. Pega o Título do vídeo pelo caminho do arquivo
                    if (e.Data.Contains("Destination:"))
                    {
                        var parts = e.Data.Split('\\');
                        var fileName = parts[^1]; // Pega o nome do arquivo
                        // Brilhante - Curta Metragem de Anima��o (2021).f136.mp4
                        // 
                        tituloAtual = Regex.Replace(fileName, @"\.f\d+", "");
                    }

                    var match = Regex.Match(e.Data, @"(\d+(\.\d+)?)%");
                    if (match.Success)
                    {
                        var percent = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

                        // Exemplo: [Status] Baixando 01/04 - Nome do Vídeo (45%)
                        ctx.Status($"[yellow][[Status]][/] Baixando {itemAtual.PadLeft(2, '0')}/{totalItens.PadLeft(2, '0')} - [cyan]{tituloAtual}[/] ({percent}%)");

                        if (percent >= 100 && !downloadRegistrado)
                        {
                            AnsiConsole.MarkupLine($"[green]✔ [[Finalizado {itemAtual.PadLeft(2, '0')}/{totalItens.PadLeft(2, '0')}]][/] {tituloAtual}");
                            downloadRegistrado = true;
                        }
                    }

                    // Resetar a flag de download quando o item mudar
                    if (e.Data.Contains("Downloading item"))
                    {
                        downloadRegistrado = false;
                    }

                    // 4. Mensagem Final da Playlist
                    if (e.Data.Contains("Finished downloading playlist"))
                    {
                        AnsiConsole.Write(new Rule("[bold green]Playlist Concluída![/]").RuleStyle("grey"));
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                // process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            });


    }


}
