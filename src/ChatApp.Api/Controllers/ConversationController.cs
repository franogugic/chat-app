using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly ILogger<ConversationController> _logger;
    
    public ConversationController(IConversationService conversationService, ILogger<ConversationController> logger)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    [Authorize]
    [HttpGet("private/{userId2:guid}")]
    public async Task<IActionResult> GetPrivateConversation(Guid userId2)
    {
        
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(nameIdentifier))
            return Unauthorized();
        var userId1 = Guid.Parse(nameIdentifier);     
        
        if (userId1 == userId2)
            return BadRequest("Cannot get a private conversation with oneself.");
        
        var conversation = await _conversationService.GetPrivateConversationAsync(userId1, userId2);
        
        _logger.LogInformation("Conversation between {UserId1} and {UserId2} found: {Conversation}", userId1, userId2, conversation);
        
        if (conversation is null)
            return Ok(null);
        
        return Ok(conversation);
    }

    [Authorize]
    [HttpPost("private/create")]
    public async Task<IActionResult> CreatePrivateConversation([FromBody] Guid userId2)
    {
        if(userId2 == Guid.Empty)
            return BadRequest("Invalid user ID.");
        
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var userId1))
        {
            return Unauthorized("User identity not found in token.");
        }
        
        if(userId1 == userId2)
            return BadRequest("Cannot create a private conversation with oneself.");
        
        var conversation = await _conversationService.CreatePrivateConversationAsync(userId1, userId2);
        
        if(conversation is null)
            return BadRequest("Failed to create private conversation.");
        
        return Ok(conversation);
    }
    
    
    [Authorize]
    [HttpGet("user/conversations")]
    public async Task<IActionResult> GetUserConversations()
    {
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                             ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var userId))
        {
            return Unauthorized("User identity not found in token.");
        }
        
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        
        return Ok(conversations);
    }
    
}