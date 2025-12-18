using CryptoPlatform.Application.DTOs;
using CryptoPlatform.Application.Interfaces.Repositories;
using CryptoPlatform.Application.Interfaces.Services;
using CryptoPlatform.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CryptoPlatform.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthController(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Registo de novo utilizador
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validações
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters");

        // Verificar se email já existe
        if (await _userRepository.EmailExistsAsync(request.Email))
            return Conflict("Email already registered");

        // Hash da password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Criar utilizador
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Gerar token
        var token = _jwtService.GenerateToken(user.Id, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email
        });
    }

    /// <summary>
    /// Login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Buscar user
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            return Unauthorized("Invalid credentials");

        // Verificar password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        // Gerar token
        var token = _jwtService.GenerateToken(user.Id, user.Email);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email
        });
    }
}