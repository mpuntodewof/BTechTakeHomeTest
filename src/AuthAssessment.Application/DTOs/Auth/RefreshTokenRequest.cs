using System.ComponentModel.DataAnnotations;

namespace AuthAssessment.Application.DTOs.Auth;

public record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = default!;
}
