using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Storefront.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(SymmetricSecurityKey signingKey) : ControllerBase
{
    private readonly SymmetricSecurityKey _signingKey = signingKey;

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Problem(
                detail: "Username and password are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var (isValid, role) = ValidateUser(request.UserName.Trim(), request.Password);
        if (!isValid || role is null)
        {
            return Problem(
                detail: "Invalid username or password.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var token = BuildToken(request.UserName.Trim(), role);

        return Ok(new LoginResponse
        {
            Token = token,
            UserName = request.UserName.Trim(),
            Role = role
        });
    }

    private static (bool IsValid, string? Role) ValidateUser(string userName, string password)
    {
        if (userName.Equals("admin", StringComparison.OrdinalIgnoreCase) && password == "admin123")
        {
            return (true, "Admin");
        }

        if (userName.Equals("user", StringComparison.OrdinalIgnoreCase) && password == "user123")
        {
            return (true, "User");
        }

        return (false, null);
    }

    private string BuildToken(string userName, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed class LoginRequest
{
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
