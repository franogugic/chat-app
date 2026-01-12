using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    
    public ConversationService(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
    }
    
    public async Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _conversationRepository.GetPrivateConversationAsync(userId1, userId2, cancellationToken);
    }
    
    public async Task<Conversation> CreatePrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        var conversation = Conversation.Create(null, false);
        conversation.AddParticipant(userId1);
        conversation.AddParticipant(userId2);

        var createdConversation = await _conversationRepository.AddAsync(conversation, cancellationToken);
        
        return createdConversation;
    }
}