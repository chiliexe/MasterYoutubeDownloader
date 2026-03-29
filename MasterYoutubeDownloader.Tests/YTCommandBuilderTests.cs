using System;
using MasterYoutubeDownloader.App.Resources;
using MasterYoutubeDownloader.App.Resources.Enums;

namespace MasterYoutubeDownloader.Tests;

public class YTCommandBuilderTests
{
    [Fact]
    public void BuildCommandVideoDownload_Should_Return_Correct_VideoResolution()
    {
        // Arrange
        var url = "https://youtube.com/watch?v=123";
        var resolution = VideoResolution.FullHD;

        // Atc
        var command = YTCommandBuilder.BuildCommandVideoDownload(url, false, resolution);

        // Assert
        Assert.Contains("[height<=1080]", command);
    }

    [Fact]
    public void BuildCommandVideoDownload_Should_Return_PlaylistFlag()
    {
        // arrange
        var url = "https://youtube.com/playlist?list=123";

        // act
        var command = YTCommandBuilder.BuildCommandVideoDownload(url, true, VideoResolution.HD);

        //assert
        Assert.Contains("--yes-playlist", command);
        Assert.Contains("%(playlist_title)s", command);
    }

    [Fact]
    public void BuildCommandVideoDownload_Should_Return_Url_With_Quotes()
    {
        // arrange
        var url = "https://youtube.com/playlist?list=123";

        // act
        var command = YTCommandBuilder.BuildCommandVideoDownload(url, true, VideoResolution.HD);

        //assert
        Assert.Contains($"\"{url}\"", command);
    }

    [Fact]
    public void BuildCommandVideoDownload_Should_Return_FlagWindowsFilenames()
    {
        // Act
        var command = YTCommandBuilder.BuildCommandVideoDownload("url");

        // Assert
        Assert.Contains("--windows-filenames", command);
    }
}
