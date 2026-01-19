using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using ChatApp.Application.Mapping;
using ChatApp.Application.Services;
using ChatApp.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChatApp.UnitTests;

public class ConversationServiceTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<ILogger<ConversationService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly ConversationService _sut;

    public ConversationServiceTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _loggerMock = new Mock<ILogger<ConversationService>>();

        var mappingConfig = new MapperConfiguration(cfg => 
        {
            cfg.AddProfile<AuthMappingProfile>();
        }, NullLoggerFactory.Instance);
        
        _mapper = mappingConfig.CreateMapper();

        _sut = new ConversationService(
            _conversationRepositoryMock.Object, 
            _mapper, 
            _loggerMock.Object);
    }

    #region GetPrivateConversationAsync

    [Fact]
    public async Task GetPrivateConversationAsync_ShouldReturnMappedDto_WhenConversationExists()
    {
        // ARRANGE
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var conversation = Conversation.Create(null, false);
        
        _conversationRepositoryMock
            .Setup(x => x.GetPrivateConversationAsync(userId1, userId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // ACT
        var result = await _sut.GetPrivateConversationAsync(userId1, userId2);

        // ASSERT
        result.Should().NotBeNull();
        result.Should().BeOfType<ConversationDto>();
        _conversationRepositoryMock.Verify(x => x.GetPrivateConversationAsync(userId1, userId2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPrivateConversationAsync_ShouldReturnNull_WhenConversationDoesNotExist()
    {
        // ARRANGE
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        _conversationRepositoryMock
            .Setup(x => x.GetPrivateConversationAsync(userId1, userId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // ACT
        var result = await _sut.GetPrivateConversationAsync(userId1, userId2);

        // ASSERT
        result.Should().BeNull();
    }

    #endregion

    #region CreatePrivateConversationAsync

    [Fact]
    public async Task CreatePrivateConversationAsync_ShouldThrowInvalidOperationException_WhenPersistenceFails()
    {
        // ARRANGE
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        _conversationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        // ACT
        var act = () => _sut.CreatePrivateConversationAsync(userId1, userId2);

        // ASSERT
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Conversation could not be created.");
        
        // Provjera da je greška logirana
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to persist")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePrivateConversationAsync_ShouldReturnDto_WhenSuccessfullyCreated()
    {
        // ARRANGE
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var conversation = Conversation.Create(null, false);
        
        _conversationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
            
        _conversationRepositoryMock
            .Setup(x => x.GetConversationByIdAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        // ACT
        var result = await _sut.CreatePrivateConversationAsync(userId1, userId2);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().Be(conversation.Id);
        _conversationRepositoryMock.Verify(x => x.AddAsync(It.Is<Conversation>(c => c.Participants.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Resilience & Cancellation

    [Fact]
    public async Task GetPrivateConversationAsync_ShouldPropagateCancellation_WhenTokenIsCancelled()
    {
        // ARRANGE
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        _conversationRepositoryMock
            .Setup(x => x.GetPrivateConversationAsync(userId1, userId2, cts.Token))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // ACT
        var act = () => _sut.GetPrivateConversationAsync(userId1, userId2, cts.Token);

        // ASSERT
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}