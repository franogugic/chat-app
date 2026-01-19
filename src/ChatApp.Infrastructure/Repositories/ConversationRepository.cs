using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    public readonly AppDbContext _dbContext;
    
    public ConversationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    public async Task<Conversation?> GetPrivateConversationAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var conversation = await _dbContext.Conversations
            .Include(c => c.Participants)
                .ThenInclude(u => u.User)
            .FirstOrDefaultAsync( c =>
                !c.IsGroup &&
                c.Participants.Count == 2 &&
                c.Participants.Any(p => p.UserId == userId1) &&
                c.Participants.Any(p => p.UserId == userId2), cancellationToken
                );
        
        return conversation;
    }
    
    public async Task<Conversation?> GetConversationByIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations.Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);    
    }
    
    public async Task<Conversation?> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _dbContext.AddAsync(conversation, cancellationToken);
        await  _dbContext.SaveChangesAsync(cancellationToken);
        return conversation;
    }
}