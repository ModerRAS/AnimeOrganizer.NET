using System.IO;
using Xunit;

namespace AnimeOrganizer.Tests;

public class FileParserTests
{
    [Theory]
    [InlineData("[ANi] 妖怪旅館營業中 貳 - 07 [1080P][Baha][WEB-DL][AAC AVC][CHT].mp4", "ANi", "妖怪旅館營業中 貳", "07", "[1080P][Baha][WEB-DL][AAC AVC][CHT]", ".mp4")]
    [InlineData("[SubsPlease] 间谍过家家 - 12 [1080p].mkv", "SubsPlease", "间谍过家家", "12", "[1080p]", ".mkv")]
    [InlineData("[EMBER] 进击的巨人 The Final Season - 01 [1080p][Multiple Subtitle].avi", "EMBER", "进击的巨人 The Final Season", "01", "[1080p][Multiple Subtitle]", ".avi")]
    public void Parse_ValidFilename_ReturnsCorrectInfo(string filename, string expectedPublisher, string expectedAnime, string expectedEpisode, string expectedTags, string expectedExt)
    {
        // Arrange
        var testPath = Path.Combine("test", filename);
        
        // Act
        var result = FilenameParser.Parse(testPath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPublisher, result.Publisher);
        Assert.Equal(expectedAnime, result.AnimeName);
        Assert.Equal(expectedEpisode, result.Episode);
        Assert.Equal(expectedTags, result.Tags);
        Assert.Equal(expectedExt, result.Extension);
        Assert.Equal(testPath, result.OriginalPath);
    }

    [Theory]
    [InlineData("[ANi] 测试 - 1 [Tag].mp4", "01")]
    [InlineData("[ANi] 测试 - 5 [Tag].mp4", "05")]
    [InlineData("[ANi] 测试 - 9 [Tag].mp4", "09")]
    [InlineData("[ANi] 测试 - 10 [Tag].mp4", "10")]
    public void Parse_SingleDigitEpisode_PadsWithZero(string filename, string expectedEpisode)
    {
        // Arrange
        var testPath = Path.Combine("test", filename);
        
        // Act
        var result = FilenameParser.Parse(testPath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEpisode, result.Episode);
    }

    [Theory]
    [InlineData("测试 - 01.mp4")]
    [InlineData("[ANi] 测试.mp4")]
    [InlineData("测试 - 01 [Tag].mp4")]
    [InlineData("[ANi] 测试 - 01 Tag.mp4")]
    [InlineData("")]
    [InlineData("random_file.txt")]
    public void Parse_InvalidFilename_ReturnsNull(string filename)
    {
        // Arrange
        var testPath = Path.Combine("test", filename);
        
        // Act
        var result = FilenameParser.Parse(testPath);
        
        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("[ANi]  妖怪旅館營業中 貳  -  07  [1080P].mp4", "ANi", "妖怪旅館營業中 貳", "07", "[1080P]", ".mp4")]
    public void Parse_FilenameWithExtraWhitespace_TrimmedCorrectly(string filename, string expectedPublisher, string expectedAnime, string expectedEpisode, string expectedTags, string expectedExt)
    {
        // Arrange
        var testPath = Path.Combine("test", filename);
        
        // Act
        var result = FilenameParser.Parse(testPath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPublisher, result.Publisher);
        Assert.Equal(expectedAnime, result.AnimeName);
        Assert.Equal(expectedEpisode, result.Episode);
        Assert.Equal(expectedTags, result.Tags);
        Assert.Equal(expectedExt, result.Extension);
    }

    [Theory]
    [InlineData("[ANi] 测试 - 01 [Tag].MP4", ".mp4")]
    [InlineData("[ANi] 测试 - 01 [Tag].Mp4", ".mp4")]
    [InlineData("[ANi] 测试 - 01 [Tag].MKV", ".mkv")]
    public void Parse_DifferentExtensionCases_NormalizedToLowercase(string filename, string expectedExt)
    {
        // Arrange
        var testPath = Path.Combine("test", filename);
        
        // Act
        var result = FilenameParser.Parse(testPath);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedExt, result.Extension);
    }
}