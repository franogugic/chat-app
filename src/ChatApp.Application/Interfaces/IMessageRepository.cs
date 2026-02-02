using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IMessageRepository
{
    Task<Message> SendMessage(Guid senderId, SendMessageRequestDTO request, CancellationToken cancellationToken = default);
    Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
}