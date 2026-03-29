using System.Security.Claims;
using AuthAssessment.Application.DTOs.Transactions;
using AuthAssessment.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthAssessment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController(TransactionUseCase transactionUseCase) : ControllerBase
{
    [HttpPost("transfer")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await transactionUseCase.TransferFundAsync(userId, request, ct);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetMyTransactions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await transactionUseCase.GetHistoryForUserAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await transactionUseCase.GetAllHistoryAsync(page, pageSize, ct);
        return Ok(result);
    }
}
