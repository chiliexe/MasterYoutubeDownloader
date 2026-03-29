using AngleSharp.Text;
using MasterYoutubeDownloader.App.Resources;
using MasterYoutubeDownloader.App.Resources.Enums;
using Spectre.Console;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

internal class Program
{
    private static ResourcesHandler _resourceHandler = new ResourcesHandler();
    private static readonly YoutubeClient _ytClient = new YoutubeClient();
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            AnsiConsole.Clear();
            DrawHeader();

            if (_resourceHandler.IsOsUnknown)
            {
                AnsiConsole.MarkupLine("[red]Sistema Operacional não suportado[/]");
                return;
            }

            await _resourceHandler.DonwloadYTdlpResource();
            await _resourceHandler.DonwloadFFMpegResource();


            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold blue]Menu Principal - Use as Setas do Teclado Para Selecionar[/]")
                    .AddChoices(new[] {
                        " -> Baixar Videos do Youtube",
                        " -> Baixar Musicas do Youtube",
                        " -> Sobre o Projeto ❤",
                        " -> Sair da Aplicação"
                    }));

            if (choice == " -> Sair da Aplicação") break;

            switch (choice)
            {
                case " -> Baixar Videos do Youtube":
                    await StartVideoFlow(isAudio: false);
                    break;
                case " -> Baixar Musicas do Youtube":
                    await StartVideoFlow(isAudio: true);
                    break;
                case " -> Sobre o Projeto ❤":
                    ShowAbout();
                    break;
            }

            AnsiConsole.MarkupLine("[yellow]Pressione Qualquer Tecla para Voltar ao Menu Inicial[/]");
            Console.ReadKey();
        }
    }
    private static void ShowAbout()
    {
        AnsiConsole.Clear();
        DrawHeader();

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);

        table.AddColumn("[yellow]Informação[/]");
        table.AddColumn("[yellow]Detalhes[/]");

        table.AddRow("Projeto", "[bold cyan1]Master Youtube Downloader[/]");
        table.AddRow("Versão", "1.0.0");
        table.AddRow("Desenvolvedor", "Chiliexe");
        table.AddRow("Linguagem", "C# / .NET 8.0");
        table.AddRow("Licença", "[green]MIT License[/]");
        table.AddRow("Repositório", "[link]https://github.com/chiliexe/MasterYoutubeDownloader[/]");

        var aboutPanel = new Panel(
            new Rows(
                table,
                new Padder(new Markup("\nEste projeto foi desenvolvido com foco em [bold]performance[/] e [bold]portabilidade[/]. " +
                                      "O objetivo é fornecer uma interface CLI moderna para manipulação de mídias do YouTube " +
                                      "de forma simples e eficiente."), new Padding(1, 1, 1, 1))
            )
        );

        aboutPanel.Header = new PanelHeader("[bold magenta] SOBRE O PROJETO [/]");
        aboutPanel.Padding = new Padding(2, 1, 2, 1);
        aboutPanel.Expand = true;

        AnsiConsole.Write(aboutPanel);
    }
    private static void DrawHeader()
    {
        // 2. Criando um Grid para organizar o topo
        var headerGrid = new Grid().AddColumn();

        // 3. Título Principal (Painel com Gradiente Cyan para Blue)
        headerGrid.AddRow(
            new Panel(
                Align.Center(
                    new Markup("[bold cyan1]❤ MASTER[/] [bold white]YOUTUBE[/] [bold red]DOWNLOADER[/] [bold cyan1]❤[/]"),
                    VerticalAlignment.Middle
                )
            ).Border(BoxBorder.Double).BorderColor(Color.Cyan1).Expand()
        );

        headerGrid.AddRow(
            new Padder(
                Align.Center(
                    new Markup( 
                        "[grey] By Chiliexe | v1.0.0 | Github: [/][link=https://github.com/chiliexe/MasterYoutubeDownloader]master-downloader[/] [green]✔[/]")
                    ),
                new Padding(0, 0, 0, 1)
            )
        );

        AnsiConsole.Write(headerGrid);

        
        AnsiConsole.Write(new Rule("[yellow]ENTRADA DE DADOS[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();
    }

    private static async Task StartVideoFlow(bool isAudio)
    {
        string promptText = !isAudio ?
            "[white]➜[/] Digite a [bold cyan]URL[/] do Vídeo ou Playlist:" :
            "[white]➜[/] Digite a [bold cyan]URL[/] da Musica (Vídeo ou Playlist):";
        var url = AnsiConsole.Prompt(
            new TextPrompt<string>(promptText)
            .PromptStyle("bold yellow")
            .ValidationErrorMessage("[red]URL inválida![/]")
        );
        var urlType = UrlHandler.HandleUrlType(url);

        if (!isAudio) // video flow
        {
            switch (urlType)
            {
                case UrlType.Video:
                    await StartVideoOption(url, FileType.Video); break;
                case UrlType.Playlist:
                    await StartPlayListOption(url, FileType.Video); break;
            }

        }
        else // audio flow
        {
            switch (urlType)
            {
                case UrlType.Video:
                    await StartVideoOption(url, FileType.Audio); break;
                case UrlType.Playlist:
                    await StartPlayListOption(url, FileType.Audio); break;
            }
        }
    }
    private static async Task StartVideoOption(string url, FileType fileType)
    {
        var ytdlp = new YtdlpHandler(_resourceHandler);
        AnsiConsole.MarkupLine("[yellow][[Info]][/] Analisando Link...");
        var video = await GetVideoMetaData(url);
        if (video is null)
        { AnsiConsole.MarkupLine("Desculpa Vídeo Não encontrado"); return; }

        AnsiConsole.MarkupLine($"[yellow][[Info]][/] Título: {video.Title} ");
        AnsiConsole.MarkupLine($"[yellow][[Info]][/] Duração: {video.Duration} ");

        if (fileType is FileType.Video)
        {
            var result = await AnsiConsole.PromptAsync(
            new SelectionPrompt<string>()
            .Title("[bold blue]Selecione a Resolução do Vídeo:[/]")
            .AddChoices(new[] { "720p ( HD )", "1080p ( FULL HD )" }));

            if (result is "720p ( HD )")
                await ytdlp.DownloadVideo(url, fileType, VideoResolution.HD);
            else
                await ytdlp.DownloadVideo(url, fileType, VideoResolution.FullHD);
        }
        else
        {
            await ytdlp.DownloadVideo(url, fileType);
        }
    }
    private static async Task StartPlayListOption(string url, FileType fileType)
    {
        var ytdlp = new YtdlpHandler(_resourceHandler);
        var videoList = await GetPlaylistMetaData(url);

        if (videoList is null or { Count: 0 }) return;

        var options = new List<string> { " --------> Baixar Todos <-------- " };
        options.AddRange(videoList.Select((v, i) => $"#{i + 1:00} - {(v.Title.Length > 60 ? v.Title[..60] + "..." : v.Title)}"));

        var choiceMenu = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Listando Toda a Playlist:").PageSize(15).AddChoices(options));
        if (fileType is FileType.Video)
        {
            var resMenu = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Selecione a Resolução Desejada:").AddChoices("720p ( HD )", "1080p ( FULL HD )"));
            var res = resMenu == "720p ( HD )" ? VideoResolution.HD : VideoResolution.FullHD;

            if (choiceMenu == options[0])
            {
                await ytdlp.DownloadPlaylist(url, fileType, res);
            }
            else
            {
                var index = int.Parse(choiceMenu[1..3]) - 1;
                var cleanUrl = videoList[index].Url.Split('&')[0];
                await ytdlp.DownloadVideo(cleanUrl, fileType, res);
            }
        }
        else
        {
            if (choiceMenu == options[0])
            {
                await ytdlp.DownloadPlaylist(url, fileType);
            }
            else
            {
                var index = int.Parse(choiceMenu[1..3]) - 1;
                var cleanUrl = videoList[index].Url.Split('&')[0];
                await ytdlp.DownloadVideo(cleanUrl, fileType);
            }
        }
    }
    private static async Task<Video?> GetVideoMetaData(string url)
    {
        try { return await _ytClient.Videos.GetAsync(url); }
        catch { return null; }
    }

    private static async Task<List<PlaylistVideo>> GetPlaylistMetaData(string url)
    {
        using var ytClient = new YoutubeClient();
        try
        {
            var playlistData = await ytClient.Playlists.GetAsync(url);
            var videos = ytClient.Playlists.GetVideosAsync(playlistData.Id);
            List<PlaylistVideo> list = new();
            await foreach (var video in videos)
            {
                list.Add(video);
            }
            return list;
        }
        catch { return null!; }
    }
}