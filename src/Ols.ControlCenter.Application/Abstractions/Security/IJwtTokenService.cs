using Ols.ControlCenter.Domain.Entities;

namespace Ols.ControlCenter.Application.Abstractions.Security;

public sealed record TokenResult(string Token, DateTimeOffset ExpiresAt);

/// <summary>JWT access token üretimi ve refresh token oluşturma.</summary>
public interface IJwtTokenService
{
    TokenResult CreateAccessToken(User user, IReadOnlyList<string> roles);
    string CreateRefreshToken();
}
