using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JobPortalAPI;

     [ApiController]
      [Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

 public AuthController(
    UserManager<IdentityUser> userManager,RoleManager<IdentityRole> roleManager,
    IConfiguration config
    )
        {
             _userManager = userManager;
             _roleManager = roleManager;
             _config = config;
        }


//         [HttpPost("jwt")]
// public async Task<IActionResult> JWTAsync([FromBody] LoginDTO dto)
// {
//     // Validate user credentials
//     var user = await _userManager.FindByNameAsync(dto.Username);
//     if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
//         return Unauthorized("Invalid credentials");

//     // Create claims
//     var claims = new[]
//     {
//         new Claim(ClaimTypes.NameIdentifier, user.Id),
//         new Claim(ClaimTypes.Name, dto.Username),
//         new Claim(ClaimTypes.Role, "HR")  // adjust role dynamically if needed
//     };

//     // Signing key
//     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QWERTYUIOPASDFGHJKLZXCVBNM1234qw"));
//     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//     // Token descriptor ensures correct 'aud' and 'exp'
//     var tokenHandler = new JwtSecurityTokenHandler();
//     var tokenDescriptor = new SecurityTokenDescriptor
//     {
//         Subject = new ClaimsIdentity(claims),
//         Expires = DateTime.UtcNow.AddHours(1),
//         Issuer = "jobportal",
//         Audience = "applicants",
//         SigningCredentials = creds
//     };

//     var token = tokenHandler.CreateToken(tokenDescriptor);
//     var tokenString = tokenHandler.WriteToken(token);

//     return Ok(new { token = tokenString });
// }


// //         [HttpPost("jwt")]
// // public async Task<IActionResult> JWTAsync([FromBody] LoginDTO dto)
// // {
// //     // Example: Validate user (replace with real DB check)
// //     // if (dto.Username == "hradmin" && dto.Password == "password123")
// //     // {
// //     var user = await _userManager.FindByNameAsync(dto.Username);
// //     if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        
// //         return Unauthorized("Invalid credentials");
// //         var claims = new[]
// //         {
// //             new Claim(ClaimTypes.NameIdentifier, "1"),
// //             new Claim(ClaimTypes.Name, dto.Username),
// //             new Claim(ClaimTypes.Role, "HR")
// //         };

// //         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QWERTYUIOPASDFGHJKLZXCVBNM1234qw"));
// //         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

// //         var token = new JwtSecurityToken(
// //             issuer: "jobportal",
// //             audience: "applicants",
// //             claims: claims,
// //             expires: DateTime.Now.AddHours(1),
// //             signingCredentials: creds);

// //         return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
// //    // }

// //     // return Unauthorized("Invalid credentials");
// // }


//         [Authorize(Roles = "HR")] 
//         [HttpGet("secure")]
//         public IActionResult SecureEndpoint()
//         {
//             return Ok("This is protected data for HR only!");
//         }

//  [HttpPost("jwt")]
//     public async Task<IActionResult> JWTAsync([FromBody] LoginDTO dto)
//     {
//         var user = await _userManager.FindByNameAsync(dto.Username);
//         if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
//             return Unauthorized("Invalid credentials");

//         var claims = new[]
//         {
//             new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
//             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//             new Claim(ClaimTypes.Role, "HR") // Set user role
//         };

//         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("QWERTYUIOPASDFGHJKLZXCVBNM1234qw"));
//         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//         var token = new JwtSecurityToken(
//             issuer: "jobportal",
//             audience: null,
//             claims: claims,
//             expires: DateTime.UtcNow.AddHours(1),
//             signingCredentials: creds
//         );

//         return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
//     }

   


[HttpGet("test-auth")]
// [Authorize]
public IActionResult TestAuth()
{
    return Ok(new { 
        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        UserName = User.Identity.Name,
        Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
    });
} 




[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDTO dto)
{
    var user = await _userManager.FindByNameAsync(dto.Username);
    if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        return Unauthorized("Invalid credentials");

    var roles = await _userManager.GetRolesAsync(user);
    
    // Create JWT token
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "C1CF4B7DC4C4175B6618DE4F55CA4AAA"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Ok(new { 
        token = tokenString,
        role = roles.FirstOrDefault() ?? "Applicant",
        username = user.UserName
    });
}

[HttpPost("logout")]
public IActionResult Logout()
{
    // JWT is stateless, so logout is handled client-side by removing the token
    return Ok(new { message = "Logout successful - remove token client-side" });
}










        // [Authorize(Roles = "Admin")]
       [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto) 
    {
          var existing = await _userManager.FindByNameAsync(dto.Username);
          if(existing != null)
          return BadRequest("Username Already Exists");

          var user = new IdentityUser {UserName = dto.Username};
          var result = await  _userManager.CreateAsync(user,dto.Password);
          if(!result.Succeeded)
          return BadRequest(result.Errors);

        var role = string.IsNullOrWhiteSpace(dto.Role) ? "Applicant" : dto.Role!;
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));


         await _userManager.AddToRoleAsync(user,role);
        return Ok(new { message = "Registered", role });
        
    }



        
// [HttpPost("login")]
// public async Task<IActionResult> Login([FromBody] LoginDTO dto)
// {
//     // ✅ Find user
//     var user = await _userManager.FindByNameAsync(dto.Username);
//     if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
//         return Unauthorized("Invalid credentials");

//     // ✅ Create Identity principal (User + Roles claims)
//     var claims = new List<Claim>
//     {
//         new Claim(ClaimTypes.Name, user.UserName!),
//         new Claim(ClaimTypes.NameIdentifier, user.Id),   

//     };

//     var roles = await _userManager.GetRolesAsync(user);
//     foreach (var role in roles)
//     {
//         claims.Add(new Claim(ClaimTypes.Role, role));
        
//     }

//     var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

//     // ✅ Sign in with cookie 
//     await HttpContext.SignInAsync(
//         IdentityConstants.ApplicationScheme,
//         new ClaimsPrincipal(claimsIdentity),
//         new AuthenticationProperties
//         {
//             IsPersistent = true, // keeps login after browser close if cookie lifetime allows
//             ExpiresUtc = DateTime.UtcNow.AddHours(1)
//         });

//     // return Ok(new { message = "Login successful" });
//     return Ok(new { 
//         message = "Login successful", 
//         role = roles.FirstOrDefault() ?? "Applicant" 
//     });
// }

//  [HttpPost("logout")]
//             public async Task<IActionResult> Logout()
//             {
//                 await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
//                 return Ok(new { message = "Logged out successfully" });
//             }
           

}


//             [HttpPost("login")]
//         public async Task<IActionResult> Login([FromBody] LoginDTO dto)
//         {
//              var user = await _userManager.FindByNameAsync(dto.Username);
//              if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
//                 return Unauthorized("Invalid credentials");
//                 var roles = await _userManager.GetRolesAsync(user);
//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.Name, user.UserName!),
//                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                new Claim(JwtRegisteredClaimNames.Iss, _config["Jwt:Issuer"]!),
//                 new Claim(JwtRegisteredClaimNames.Aud, _config["Jwt:Audience"]!)
//             };
//             foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

//             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
//             var token = new JwtSecurityToken(
//                 issuer: _config["Jwt:Issuer"],
//                 audience: _config["Jwt:Audience"],
//                 expires: DateTime.UtcNow.AddHours(1), // ← Ensure this is set!
//                 claims: claims,
//                 signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
//             );

//        Console.WriteLine("Roles in token XXXXX: " + string.Join(",", roles));
//  Console.WriteLine($"Generated Token: {new JwtSecurityTokenHandler().WriteToken(token)}");
//     Console.WriteLine($"Claims: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");
    
//     return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token),
//     expires = token.ValidTo,
//                 roles });
//             // return Ok(new
//             // {
//             //     token = new JwtSecurityTokenHandler().WriteToken(token),
//             //     expires = token.ValidTo,
//             //     roles
//             // });


//         }


       // [HttpPost("login")]
//public async Task<IActionResult> Login([FromBody] LoginDTO dto)
// {
//     var user = await _userManager.FindByNameAsync(dto.Username);
//     if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
//         return Unauthorized("Invalid credentials");

//     var roles = await _userManager.GetRolesAsync(user);
//     var claims = new List<Claim>
//     {
//         new Claim(ClaimTypes.Name, user.UserName!),
//         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//         // Add these REQUIRED claims:
//         new Claim(JwtRegisteredClaimNames.Iss, _config["Jwt:Issuer"]!),
            
//         new Claim(JwtRegisteredClaimNames.Aud, _config["Jwt:Audience"]!),
//         new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
//     };
//     foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

//     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
//     var token = new JwtSecurityToken(
//         issuer: _config["Jwt:Issuer"],
//         audience: _config["Jwt:Audience"],
//         expires: DateTime.UtcNow.AddHours(2), // ← Must match the exp claim!
//         claims: claims,
//         signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
//     );

//     return Ok(new { 
//         token = new JwtSecurityTokenHandler().WriteToken(token),
//         expires = token.ValidTo
//     });
// }

 // use this one finalizing 

// [HttpPost("login")]
// public async Task<IActionResult> Login([FromBody] LoginDTO dto)
// {
//      var user = await _userManager.FindByNameAsync(dto.Username);
//     if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
//         return Unauthorized("Invalid credentials");

//     var roles = await _userManager.GetRolesAsync(user);
//     var claims = new List<Claim>
//     {
//         new Claim(ClaimTypes.Name, user.UserName!),
//         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//         // Add these REQUIRED claims:
//         new Claim(JwtRegisteredClaimNames.Iss, _config["Jwt:Issuer"]!),
            
//         new Claim(JwtRegisteredClaimNames.Aud, _config["Jwt:Audience"]!),
//         new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
//     };
//     foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

//     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    

//     var token = new JwtSecurityToken(
//         issuer: _config["Jwt:Issuer"],
//         audience: _config["Jwt:Audience"],
//         expires: DateTime.UtcNow.AddHours(2),
//         claims: claims,
//         signingCredentials: new SigningCredentials(
//             new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
//             SecurityAlgorithms.HmacSha256)
//     );

//     // 👇 Add this validation
//     var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
//     if (tokenString.Count(c => c == '.') != 2)
//     {
//         throw new Exception("Generated token is malformed!");
//     }

//     return Ok(new { token = tokenString });
// }

//  [Authorize(Roles = "Admin")]


            //         [HttpGet("verify-token")]
            //         // [Authorize] // Requires any valid token
            // public IActionResult VerifyToken()
            // {
            //     return Ok(new
            //     {
            //         // User.Identity.Name,
            //         Roles = User.Claims
            //             .Where(c => c.Type == ClaimTypes.Role)
            //             .Select(c => c.Value),
            //         Issued = DateTimeOffset.FromUnixTimeSeconds(
            //             long.Parse(User.FindFirst("iat")!.Value))
            //     });
            // }

//             [HttpGet("debug-claims")]
// [Authorize]
// public IActionResult DebugClaims()
// {
//     return Ok(new
//     {
//         User.Identity?.Name,
//         Claims = User.Claims.Select(c => new { c.Type, c.Value }),
//         IsAuthenticated = User.Identity?.IsAuthenticated,
//         AuthenticationType = User.Identity?.AuthenticationType
//     });
// }





            // [HttpGet("decode-token")]
            // public IActionResult DecodeToken([FromQuery] string token)
            // {
            //     var handler = new JwtSecurityTokenHandler();
            //     var jwtToken = handler.ReadJwtToken(token);
                
            //     // Extract ALL claims (including standard ones)
            //     var claims = jwtToken.Claims.Select(c => new { 
            //         c.Type, 
            //         c.Value,
            //         ValueType = c.ValueType 
            //     }).ToList();

            //     // 👇 Handle audience as an array (since it can be multiple values)
            //     var audiences = jwtToken.Audiences.ToList(); // Get all audiences
                
            //     return Ok(new
            //     {
            //         Claims = claims,
            //         ValidTo = jwtToken.ValidTo,
            //         Issuer = jwtToken.Issuer,
            //         Audience = audiences.FirstOrDefault(), // Return first audience (or null)
            //         AllAudiences = audiences              // Return full list for debugging
            //     });
            // }

            // }
