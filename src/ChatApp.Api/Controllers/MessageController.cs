using System.Security.Claims;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;
    
    public MessageController(ILogger<MessageController> logger, IMessageService messageService, IHubContext<ChatHub> hubContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    [Authorize]
    [HttpPost("sendMessage")]
    public async Task<IActionResult> SendMessages([FromBody] SendMessageRequestDTO request)
    {
        _logger.LogInformation("SendMessages called.");
        
        if(string.IsNullOrWhiteSpace(request.Content))
            return new BadRequestResult();
        
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier))
            return Unauthorized();
        
        var senderId = Guid.Parse(nameIdentifier); 
        
        var message = await _messageService.SendMessage(senderId, request, cancellationToken: default);
        
        _logger.LogInformation("Šaljem poruku {MsgId} u grupu konverzacije: {ConvId}", message.Id, request.ConversationId);        
        
        await _hubContext.Clients.Group(request.ConversationId.ToString().ToLowerInvariant())
            .SendAsync("ReceiveMessage", message);
        
        _logger.LogInformation("Message {MessageContent} sent and broadcasted via SignalR.", message.Content);
        
        return Ok(message); 
    }
    
}