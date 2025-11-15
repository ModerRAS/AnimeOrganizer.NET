using System;
using System.IO;
using Xunit;

namespace AnimeOrganizer.Tests;

public class FileOrganizerTests : IDisposable
{
    private readonly string _testSourceDir;
    private readonly string _testTargetDir;
    private readonly string _testFilePath;
    private readonly AnimeFileInfo _testAnimeFile;

    public FileOrganizerTests()
    {
        _testSourceDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testTargetDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testSourceDir);
        Directory.CreateDirectory(_testTargetDir);

        _testFilePath = Path.Combine(_testSourceDir, "[ANi] 测试 - 01 [1080P].mp4");
        File.WriteAllText(_testFilePath, "test content");

        _testAnimeFile = new AnimeFileInfo(
            "ANi",
            "测试",
            "01",
            "[1080P]",
            ".mp4",
            _testFilePath
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSourceDir))
            Directory.Delete(_testSourceDir, true);
        if (Directory.Exists(_testTargetDir))
            Directory.Delete(_testTargetDir, true);
    }

    [Fact]
    public void Organize_MoveMode_MovesFileToCorrectLocation()
    {
        // Act
        var result = FileOrganizer.Organize(_testAnimeFile, _testTargetDir, OperationMode.Move);

        // Assert
        Assert.True(result);
        var expectedPath = Path.Combine(_testTargetDir, "测试", "01 [1080P].mp4");
        Assert.True(File.Exists(expectedPath));
        Assert.False(File.Exists(_testFilePath));
        Assert.Equal("test content", File.ReadAllText(expectedPath));
    }

    [Fact]
    public void Organize_CopyMode_CopiesFileToCorrectLocation()
    {
        // Act
        var result = FileOrganizer.Organize(_testAnimeFile, _testTargetDir, OperationMode.Copy);

        // Assert
        Assert.True(result);
        var expectedPath = Path.Combine(_testTargetDir, "测试", "01 [1080P].mp4");
        Assert.True(File.Exists(expectedPath));
        Assert.True(File.Exists(_testFilePath));
        Assert.Equal("test content", File.ReadAllText(expectedPath));
        Assert.Equal("test content", File.ReadAllText(_testFilePath));
    }

    [Fact]
    public void Organize_DryRunMode_DoesNotModifyFiles()
    {
        // Act
        var result = FileOrganizer.Organize(_testAnimeFile, _testTargetDir, OperationMode.Move, dryRun: true);

        // Assert
        Assert.True(result);
        Assert.True(File.Exists(_testFilePath));
        Assert.False(Directory.Exists(Path.Combine(_testTargetDir, "测试")));
    }

    [Fact]
    public void Organize_TargetDirectoryDoesNotExist_CreatesDirectory()
    {
        // Arrange
        var nonExistentTarget = Path.Combine(_testTargetDir, "sub", "dir");

        // Act
        var result = FileOrganizer.Organize(_testAnimeFile, nonExistentTarget, OperationMode.Copy);

        // Assert
        Assert.True(result);
        Assert.True(Directory.Exists(nonExistentTarget));
        var expectedPath = Path.Combine(nonExistentTarget, "测试", "01 [1080P].mp4");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void Organize_TargetFileExists_OverwritesExistingFile()
    {
        // Arrange
        var targetDir = Path.Combine(_testTargetDir, "测试");
        Directory.CreateDirectory(targetDir);
        var targetFile = Path.Combine(targetDir, "01 [1080P].mp4");
        File.WriteAllText(targetFile, "existing content");

        // Act
        var result = FileOrganizer.Organize(_testAnimeFile, _testTargetDir, OperationMode.Copy);

        // Assert
        Assert.True(result);
        Assert.Equal("test content", File.ReadAllText(targetFile));
    }
}