using Microsoft.Extensions.Logging;
using TargCC.CLI.Services;

namespace TargCC.CLI.Tests.Services;

/// <summary>
/// Tests for <see cref="OutputService"/>.
/// </summary>
public class OutputServiceTests
{
    private readonly OutputService _service;
    private readonly Mock<ILogger<OutputService>> _loggerMock;

    public OutputServiceTests()
    {
        _loggerMock = new Mock<ILogger<OutputService>>();
        _service = new OutputService(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsException_WhenLoggerIsNull()
    {
        // Arrange & Act
        var act = () => new OutputService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Success_LogsInformation()
    {
        // Arrange
        var message = "Test success message";

        // Act
        _service.Success(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Error_LogsError()
    {
        // Arrange
        var message = "Test error message";

        // Act
        _service.Error(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Warning_LogsWarning()
    {
        // Arrange
        var message = "Test warning message";

        // Act
        _service.Warning(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Info_LogsInformation()
    {
        // Arrange
        var message = "Test info message";

        // Act
        _service.Info(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Debug_LogsDebug()
    {
        // Arrange
        var message = "Test debug message";

        // Act
        _service.Debug(message);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Heading_LogsInformation()
    {
        // Arrange
        var heading = "Test Heading";

        // Act
        _service.Heading(heading);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Heading")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void BlankLine_DoesNotThrow()
    {
        // Arrange & Act
        var act = () => _service.BlankLine();

        // Assert
        act.Should().NotThrow();
    }
}
