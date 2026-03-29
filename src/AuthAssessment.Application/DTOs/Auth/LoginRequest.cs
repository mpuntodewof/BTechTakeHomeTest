using System.ComponentModel.DataAnnotations;

namespace AuthAssessment.Application.DTOs.Auth;

public record LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = default!;

    [Required]
    public string Password { get; init; } = default!;
}
