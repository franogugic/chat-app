using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<Conversation?> AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
}