using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ChatApp.Infrastructure.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, byte> OnlineUsers = new ConcurrentDictionary<string, byte>();
    private readonly ILogger<ChatHub> _logger;
    private readonly IMessageService _messageService;
    
    public ChatHub(ILogger<ChatHub> logger, IMessageService messageService)
    { 
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier; 
        _logger.LogCritical(userId);
        if (userId != null)
        {
            OnlineUsers.TryAdd(userId, 0);
            await Clients.All.SendAsync("UserStatusChanged", userId, true);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        
        if (userId != null)
        {
            OnlineUsers.TryRemove(userId, out _);
            await Clients.All.SendAsync("UserStatusChanged", userId, false);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }
    
    public List<string> GetOnlineUsers()
    {
        return OnlineUsers.Keys.ToList();
    }
    
    public bool IsThisUserOnline(string userId)
    {
        return OnlineUsers.ContainsKey(userId);
    }
    
    [HubMethodName("MarkAsRead")]
    public async Task MarkAsRead(string conversationId)
    {
        var userIdString = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(conversationId, out Guid convId))
            return;

        var userId = Guid.Parse(userIdString);

        await _messageService.MarkMessagesAsReadAsync(convId, userId);

        await Clients.Group(conversationId.ToLowerInvariant())
            .SendAsync("MessagesRead", conversationId);
    }
}