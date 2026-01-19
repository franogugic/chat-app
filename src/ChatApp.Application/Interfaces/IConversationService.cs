using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IConversationService
{
    Task<ConversationDto?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    
    Task<ConversationDto?> CreatePrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
}