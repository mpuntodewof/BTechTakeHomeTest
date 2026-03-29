using System.ComponentModel.DataAnnotations;

namespace AuthAssessment.Application.DTOs.Auth;

public record RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = default!;

    [Required, MinLength(6)]
    public string Password { get; init; } = default!;

    [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; init; } = default!;

    [Required, StringLength(100)]
    public string FullName { get; init; } = default!;
}
