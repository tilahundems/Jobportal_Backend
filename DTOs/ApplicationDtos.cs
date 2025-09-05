namespace JobPortalAPI;
// DTO for Application
public class ApplicationDto
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? CoverLetter { get; set; }
    public JobDto? Job { get; set; } 
    



  


}
    public class JobDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Company { get; set; }
}

// DTO for ApplicantProfile
public class ApplicantProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;

    public List<ApplicationDto>? Applications { get; set; } 
}
