using System.Security.Claims;
using AuthAssessment.Application.DTOs.Auth;
using AuthAssessment.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAssessment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthUseCase authUseCase) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authUseCase.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authUseCase.LoginAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await authUseCase.RefreshTokenAsync(request, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await authUseCase.LogoutAsync(userId, ct);
        return NoContent();
    }
}
