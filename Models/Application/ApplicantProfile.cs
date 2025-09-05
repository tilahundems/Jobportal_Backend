using Microsoft.AspNetCore.Identity;

namespace JobPortalAPI;

public class ApplicantProfile
{
     public int Id { get; set; }

    // Link to Identity User
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();


}
