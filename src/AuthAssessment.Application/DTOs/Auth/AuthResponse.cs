namespace AuthAssessment.Application.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Email,
    string FullName,
    decimal Balance,
    string Role
);
