using System;
using MasterYoutubeDownloader.App.Resources;
using MasterYoutubeDownloader.App.Resources.Enums;

namespace MasterYoutubeDownloader.Tests;

public class UrlHandlerTests
{
    [Theory]
    [InlineData("https://www.youtube.com/playlist?list=123", UrlType.Playlist)]
    [InlineData("https://www.youtube.com/watch?v=123&list=ABC", UrlType.Playlist)]
    [InlineData("https://www.youtube.com/watch?v=123", UrlType.Video)]
    [InlineData("https://youtu.be/123", UrlType.Video)]
    [InlineData("https://google.com", UrlType.Invalid)]
    public void UrlHandlerType_Should_Return_Correct_Type(string url, UrlType expectedUrlType)
    {
        // Act
        var result = UrlHandler.HandleUrlType(url);

        // Assert
        Assert.Equal(result, expectedUrlType);
    }
}
