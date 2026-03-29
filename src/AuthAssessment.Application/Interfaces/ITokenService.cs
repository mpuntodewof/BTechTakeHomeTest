using AuthAssessment.Domain.Entities;

namespace AuthAssessment.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}
