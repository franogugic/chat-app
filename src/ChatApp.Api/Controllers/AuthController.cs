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

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserRequestDTO? request, CancellationToken cancellationToken = default)
    {
        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        var created = await _authService.CreateUserAsync(request, cancellationToken).ConfigureAwait(false);

        return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return BadRequest(new { message = "Id must be a non-empty GUID." });

        var userIdFromToken = User.FindFirst("id")?.Value;
        if (userIdFromToken != id.ToString())
            return Forbid();
        
        var user = await _authService.GetUserByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserRequestDTO request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var authResponse = await _authService.LoginAsync(request, cancellationToken).ConfigureAwait(false);
        
        return Ok(authResponse);
    }
}