using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    public readonly AppDbContext _dbContext;
    
    public MessageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task<Message> SendMessage(Guid senderId, SendMessageRequestDTO request, CancellationToken cancellationToken = default)
    {
        var message = Message.Create(senderId, request.ConversationId, request.Content, request.MessageType);
        
        await _dbContext.Messages.AddAsync(message, cancellationToken);
        var conversation = await _dbContext.Conversations.FindAsync(request.ConversationId, cancellationToken);
        if (conversation != null)
        {
            conversation.UpdateLastMessage(message.Id);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        return message;
    }
    
    public async Task MarkMessagesAsReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.Messages
            .Where(m => m.ConversationId == conversationId 
                        && m.SenderId != userId 
                        && !m.IsRead)
            .ToListAsync(cancellationToken);

        if (messages.Any())
        {
            foreach (var msg in messages)
            {
                msg.MarkAsRead(); 
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }}