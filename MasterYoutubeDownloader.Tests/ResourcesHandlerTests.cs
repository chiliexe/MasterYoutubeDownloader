using System;
using MasterYoutubeDownloader.App.Resources;

namespace MasterYoutubeDownloader.Tests;

public class ResourcesHandlerTests
{
    [Fact]
    public void ResourcesBinFile_Should_HaveCorrectExtensionForCurrentOS()
    {
        // Arrange
        var isWindows = OperatingSystem.IsWindows();
        var resourceHandler = new ResourcesHandler();

        // Assert
        if (isWindows)
        {
            Assert.EndsWith(".exe", resourceHandler.YtdlpBinPath);
            Assert.EndsWith(".exe", resourceHandler.FFmpegBinPath);
        }
        else
        {
            Assert.False(resourceHandler.YtdlpBinPath.EndsWith(".exe"));
            Assert.False(resourceHandler.FFmpegBinPath.EndsWith(".exe"));
        }
    }


}
