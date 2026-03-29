namespace AuthAssessment.Application.DTOs.Users;

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    decimal Balance,
    string Role,
    DateTime CreatedAt
);

public record UserListResponse(
    List<UserResponse> Items,
    int TotalCount
);
