using System.Security.Claims;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
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
    private readonly IMessageService _messageService;
    
    public MessageController(ILogger<MessageController> logger, IMessageService messageService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    [Authorize]
    [HttpPost("sendMessage")]
    public async Task<IActionResult> SendMessages([FromBody] SendMessageRequestDTO request)
    {
        _logger.LogInformation("SendMessages called.");
        if(string.IsNullOrWhiteSpace(request.Content))
            return new BadRequestResult();
        
        var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Name indentifivator from token: {SenderId}", nameIdentifier);

        if (string.IsNullOrEmpty(nameIdentifier))
            return Unauthorized();
        var senderId = Guid.Parse(nameIdentifier); 
        _logger.LogInformation("SenderId from token: {SenderId}", senderId);
        
        var message = await _messageService.SendMessage(senderId, request, cancellationToken: default);
        return Ok(message); 
    }
    
}