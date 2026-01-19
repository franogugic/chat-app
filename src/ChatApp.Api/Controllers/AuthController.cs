// csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Application.DTO_s;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserRequestDTO? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            _logger.LogWarning("RegisterUser called with null request body.");
            return BadRequest(new { message = "Request body is required." });
        }
        
        _logger.LogInformation("RegisterUser: creating user.");
        var created = await _authService.CreateUserAsync(request, cancellationToken);

        _logger.LogInformation("RegisterUser: user created with id {UserId}.", created.Id);
        return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("GetUserById called with empty GUID.");
            return BadRequest(new { message = "Id must be a non-empty GUID." });
        }

        var userIdFromToken = User.FindFirst("id")?.Value;
        if (userIdFromToken != id.ToString())
        {
            _logger.LogWarning("GetUserById: token id mismatch. TokenId={TokenId}, RequestedId={RequestedId}", userIdFromToken, id);
            return Forbid();
        }
        
        _logger.LogInformation("GetUserById: fetching user {UserId}.", id);
        var user = await _authService.GetUserByIdAsync(id, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("GetUserById: user not found {UserId}.", id);
            return NotFound();
        }

        _logger.LogInformation("GetUserById: returning user {UserId}.", id);
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserRequestDTO request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("LoginUser: authentication attempt.");
        
        var authResponse = await _authService.LoginAsync(request, cancellationToken);
        
        if (authResponse is null)
        {
            _logger.LogWarning("LoginUser: authentication failed.");
            return Unauthorized();
        }
        
        _logger.LogInformation("LoginUser: authentication succeeded.");
        return Ok(authResponse);
    }
}