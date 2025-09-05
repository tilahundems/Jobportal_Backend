using System.ComponentModel.DataAnnotations;

namespace JobPortalAPI;

public class RegisterDTO
{
 [Required]   public string Username { get; set; }
 [Required]   public string Password { get; set; }
  public string? Role { get; set; }

}

public class LoginDTO
{
   [Required] public string Username { get; set;}
    [Required] public string Password { get; set; }
}