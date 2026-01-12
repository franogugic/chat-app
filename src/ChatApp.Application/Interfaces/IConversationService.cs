using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IConversationService
{
    Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    
    Task<Conversation> CreatePrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
}