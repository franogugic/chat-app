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
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDTO request, CancellationToken cancellationToken)
    {
        var user = await _authService.LoginAsync(request, cancellationToken);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
        
        Response.Cookies.Append("access-token", user.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, 
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(5)
        });
        
        Response.Cookies.Append("refresh-token", user.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7) 
        });

        return Ok(new
        {
            Id = user.UserId,
            Name = user.name,
            Mail = user.mail
        });
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var userIdFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                              ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdFromToken))
        {
            _logger.LogWarning("GetCurrentUser: Nije pronađen 'sub' ili 'id' claim u tokenu.");
            return Unauthorized(new { message = "Invalid token" });
        }

        if (!Guid.TryParse(userIdFromToken, out var userId))
        {
            _logger.LogWarning("GetCurrentUser: ID korisnika u tokenu nije ispravan GUID: {UserId}", userIdFromToken);
            return BadRequest(new { message = "Invalid user ID format" });
        }

        var user = await _authService.GetUserByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("GetCurrentUser: Korisnik s ID-em {UserId} više ne postoji u bazi.", userId);
            return NotFound(new { message = "User not found" });
        }

        return Ok(new
        {
            Id = user.Id,
            Name = user.Name, 
            Mail = user.Mail,   
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt
        });
    }
    
    [Authorize]
    [HttpGet("search")]
    public async Task<ActionResult<List<AllUsersBySearchResponseDTO>>> GetAllUsersBySearch([FromQuery] string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            return Ok(new List<AllUsersBySearchResponseDTO>()); 
        }
        
        var userIdFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                              ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdFromToken, out var userId) || string.IsNullOrEmpty(userIdFromToken))
        {
            _logger.LogWarning("GetCurrentUser: ID korisnika u tokenu nije ispravan GUID: {UserId}", userIdFromToken);
            return BadRequest(new { message = "Invalid user ID format" });
        }    
        var users = await _authService.GetAllUsersBySearchAsync(userId, searchTerm, cancellationToken);

        return users;
    }
    
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access-token", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax
        });
        
        Response.Cookies.Delete("refresh-token", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax
        });
        return Ok();
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        _logger.LogCritical("PINGAN JE REFRESH TOKEN ENDPOINT");
        var accessToken = Request.Cookies["access-token"];
        var refreshToken = Request.Cookies["refresh-token"];
        
        var request = new RefreshRequestDTO
        {
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty
        };
        _logger.LogInformation("Refreshing token with request: " + request.RefreshToken);
        _logger.LogInformation("Access token with request: " + request.AccessToken);
        
        var result = await _authService.RefreshTokenAsync(request, ct);

        if (result is null)
        {
            return Unauthorized("Session expired. Please login again.");
        }
        
        Response.Cookies.Append("access-token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(5)
        });

        Response.Cookies.Append("refresh-token", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
        });

        return Ok(result);
    }
}