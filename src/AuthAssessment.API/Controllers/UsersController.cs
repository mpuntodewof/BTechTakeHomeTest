using System.Security.Claims;
using AuthAssessment.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAssessment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(UserUseCase userUseCase) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await userUseCase.GetProfileAsync(userId, ct);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var result = await userUseCase.GetAllAsync(ct);
        return Ok(result);
    }
}
