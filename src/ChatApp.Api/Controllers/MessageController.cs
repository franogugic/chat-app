using System.Security.Claims;
using ChatApp.Application.DTO_s;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    
    
    public MessageController(ILogger<MessageController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Authorize]
    [HttpPost("sendMessage")]
    public async Task<IActionResult> SendMessages([FromBody] SendMessageRequestDTO request)
    {
        _logger.LogInformation("SendMessages called.");
        
        if(string.IsNullOrWhiteSpace(request.Content) || request.ConversationId == Guid.Empty)
            return new BadRequestResult();
        
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Name indentifivator from token: {SenderId}", nameIdentifier);

        if (string.IsNullOrEmpty(nameIdentifier))
            return Unauthorized();
        var senderId = Guid.Parse(nameIdentifier); 
        _logger.LogInformation("SenderId from token: {SenderId}", senderId);
        
        return Ok(senderId); 
    }
    
}