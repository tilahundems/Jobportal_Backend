using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JobPortalAPI;

[ApiController]
[Route("api/[controller]")]
public class JwtAuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ConfigurationManager _config;

    public JwtAuthController(IJwtService jwtService, UserManager<IdentityUser> userManager,ConfigurationManager config)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = _jwtService.GenerateToken(claims);

        return Ok(new { Token = token, ExpiresIn = 3600 });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var principal = _jwtService.ValidateToken(request.Token);
            var newToken = _jwtService.GenerateToken(principal.Claims);
            
            return Ok(new { Token = newToken });
        }
        catch
        {
            return Unauthorized("Invalid token");
        }
    }

    [HttpGet("validate")]
    [Authorize] // Requires valid JWT
    public IActionResult Validate()
    {
        return Ok(new { 
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        });
    }


    [HttpGet("manual-test")]
public IActionResult ManualTest([FromQuery] string token)
{
    try
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );
        
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = key
        }, out SecurityToken validatedToken);
        
        return Ok(new { 
            Success = true, 
            User = principal.Identity.Name,
            Roles = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
        });
    }
    catch (Exception ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
}

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RefreshRequest
{
    public string Token { get; set; }
}