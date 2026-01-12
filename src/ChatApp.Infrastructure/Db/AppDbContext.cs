using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!; 
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Mail).IsUnique();
            entity.Property(e => e.Mail).IsRequired().HasMaxLength(256);             
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        { 
            entity.HasKey(u => u.Id);
            
            entity.HasOne(u => u.Conversation)
                .WithMany(u => u.Messages)
                .HasForeignKey(u => u.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(u => u.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(u => u.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
        });
        
        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            //priamrni kljuc je kombinacija UserId in ConversationId
            entity.HasKey(cp => new { cp.UserId, cp.ConversationId });

            entity.HasOne(cp => cp.User)
                .WithMany(u => u.ConversationParticipants)
                .HasForeignKey(cp => cp.UserId);

            entity.HasOne(cp => cp.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ConversationId);
        });
        
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.LastMessage)
                .WithMany()
                .HasForeignKey(c => c.LastMessageId)
                .OnDelete(DeleteBehavior.Restrict); 
        });
    }
}