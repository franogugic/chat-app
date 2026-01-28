using AutoMapper;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConversationService> _logger;
    
    public ConversationService(IConversationRepository conversationRepository, IMapper mapper, ILogger<ConversationService> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<ConversationDto?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching private conversation between {UserId1} and {UserId2}", userId1, userId2);
        
        var conversation = await _conversationRepository.GetPrivateConversationAsync(userId1, userId2, cancellationToken);
        if (conversation is null)
        {
            _logger.LogInformation("No private conversation found between {UserId1} and {UserId2} BY USER ID", userId1, userId2);
            conversation = await _conversationRepository.GetConversationByIdAsync(userId2, cancellationToken);
        }
        
        _logger.LogInformation("Private conversation fetched: {Conversation}", conversation);
        
        return _mapper.Map<ConversationDto>(conversation);
    }
    
    public async Task<ConversationDto?> CreatePrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        var getPrivConv = await GetPrivateConversationAsync(userId1, userId2, cancellationToken);
        if (getPrivConv is not null)
        {
            _logger.LogInformation("!!!Private conversation already exists between {UserId1} and {UserId2}: {exist}", userId1, userId2, getPrivConv is not null);
            return getPrivConv;
        }
        
        var conversation = Conversation.Create(null, false);
        conversation.AddParticipant(userId1);
        conversation.AddParticipant(userId2);

        var persistedConversation = await _conversationRepository.AddAsync(conversation, cancellationToken);
        
        if (persistedConversation is null)
        {
            _logger.LogError("Failed to persist conversation to the database.");
            throw new InvalidOperationException("Conversation could not be created.");
        }
        
        var conversationWithDetails = await _conversationRepository.GetConversationByIdAsync(persistedConversation.Id, cancellationToken);
        return _mapper.Map<ConversationDto>(conversationWithDetails);
    }
    
    public async Task<List<UserConversationsResponseDTO>> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(userId, cancellationToken);
        return _mapper.Map<List<UserConversationsResponseDTO>>(conversations, opt => 
        {
            opt.Items["CurrentUserId"] = userId; 
        });    
    }
}