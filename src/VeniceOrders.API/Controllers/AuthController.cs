using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeniceOrders.API.DTOs.Auth;
using VeniceOrders.API.Services;

namespace VeniceOrders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Gera um token JWT para autenticação
    /// Implementação simplificada conforme requisito do teste
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username e password são obrigatórios" });
        }

        var userId = Guid.NewGuid().ToString();
        var token = _tokenService.GenerateToken(userId, request.Username);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }
}
