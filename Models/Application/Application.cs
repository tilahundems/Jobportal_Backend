using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JobPortalAPI;

public class Application
{ 
    public int Id { get; set; }

    [Required]
    public int JobId { get; set; }
    public  virtual Job? Job { get; set; } 

    [Required]
    public int ApplicantProfileId { get; set; } 
     [JsonIgnore]
    public  virtual ApplicantProfile? ApplicantProfile { get; set; }

    [Required]
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    public string? CoverLetter { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending";

}
