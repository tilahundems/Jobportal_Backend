namespace JobPortalAPI;


    public class CreateJobDto
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Deadline { get; set; } 
}

public class UpdateJobDto
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime Deadline { get; set; } 
   public string Department { get; set; } = string.Empty;


}

public class ApplicationLiteDto
{
    public int ApplicationId { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? CoverLetter { get; set; }

    // Applicant details
    public int ApplicantProfileId { get; set; }
    public string ApplicantFullName { get; set; } = string.Empty;
    public string ApplicantPhone { get; set; } = string.Empty;
    public string ApplicantResumeUrl { get; set; } = string.Empty;
    public string ApplicantSkills { get; set; } = string.Empty;
    public string ApplicantEducation { get; set; } = string.Empty;
}