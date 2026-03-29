using AuthAssessment.Application.DTOs.Users;
using AuthAssessment.Application.Interfaces;

namespace AuthAssessment.Application.UseCases;

public sealed class UserUseCase(IUserRepository userRepository)
{
    public async Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        return new UserResponse(
            user.Id, user.Email, user.FullName, user.Balance, user.Role.ToString(), user.CreatedAt
        );
    }

    public async Task<UserListResponse> GetAllAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);

        return new UserListResponse(
            users.Select(u => new UserResponse(
                u.Id, u.Email, u.FullName, u.Balance, u.Role.ToString(), u.CreatedAt
            )).ToList(),
            users.Count
        );
    }
}
