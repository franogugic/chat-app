using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Interfaces;

public interface IMessageService
{
    Task<MessageDTO> SendMessage(Guid senderId, SendMessageRequestDTO request, CancellationToken cancellationToken = default);
}