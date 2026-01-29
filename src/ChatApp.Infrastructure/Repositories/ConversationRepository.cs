using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatApp.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    public readonly AppDbContext _dbContext;
    public readonly ILogger<ConversationRepository> _logger;
    
    public ConversationRepository(AppDbContext dbContext, ILogger<ConversationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var conversation = await _dbContext.Conversations
            .Include(c => c.Participants)
                .ThenInclude(u => u.User)
            .Include(m => m.Messages.OrderBy(mess => mess.SentAt))
            .FirstOrDefaultAsync( c =>
                !c.IsGroup &&
                c.Participants.Count == 2 &&
                c.Participants.Any(p => p.UserId == userId1) &&
                c.Participants.Any(p => p.UserId == userId2), cancellationToken
                );
        
        _logger.LogInformation("Conversation exist between {UserId1} and {UserId2}: {Exists}", userId1, userId2, conversation is not null);
        return conversation;
    }
    
    public async Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations.Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.Messages.OrderBy(mess => mess.SentAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);    
    }
    
    public async Task<Conversation?> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(conversation, cancellationToken);
        await  _dbContext.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task<List<Conversation?>?> GetUserConversationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _dbContext.Conversations
            .Include(c => c.Participants)
                .ThenInclude(u => u.User)
            .Include(c => c.LastMessage)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.LastMessage != null ? c.LastMessage.SentAt : c.CreatedAt) 
            .ToListAsync(cancellationToken);
        
        return conversations;
        
    }
}