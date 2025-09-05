using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI;


    [ApiController]
    [Route("api/[controller]")]
public class ApplicationsController:ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser>  _userManager;

    public ApplicationsController(AppDbContext context ,UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
        // [Authorize] 
 [HttpPost("apply")]
public async Task<IActionResult> Apply([FromBody] ApplyDto dto)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
       if (userId == null)
         return Unauthorized("You Aust Login  First To Apply");
    var profile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.UserId == userId);

    if (profile == null)
        return BadRequest("Applicant profile not found. Please create a profile first.");
             if (profile.ResumeUrl == null || profile.ResumeUrl=="" )
        return NotFound("Pls upload Resume!!.");
         
    var job = await _context.Jobs.FindAsync(dto.JobId);
    if (job == null)
        return NotFound("Job not found.");

    var alreadyApplied = await _context.Applications
        .AnyAsync(a => a.JobId == dto.JobId && a.ApplicantProfileId == profile.Id);

    if (alreadyApplied)
        return BadRequest("You have already applied for this job.");

    var application = new Application
    {
        JobId = dto.JobId,
        ApplicantProfileId = profile.Id, 
        CoverLetter = dto.CoverLetter,
        AppliedDate = DateTime.UtcNow
        
    };

    _context.Applications.Add(application);
    await _context.SaveChangesAsync();
       // Map to DTO to return clean response
    var response = new ApplicationDto
    {
         Id =application.Id,
          JobId= job.Id,
        JobTitle = job.Title,
        AppliedDate = application.AppliedDate,
        Status = application.Status,
        CoverLetter = application.CoverLetter,
         Job = new JobDto
            {
                Id= job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                Company = job.Company
            }

    };

    return Ok(response);

}
         // Get all applications
                //  [Authorize] 
        [HttpGet("MyApplications")]
public async Task<IActionResult> GetApplications()
{
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (currentUserId == null)
     return Unauthorized("You have to login first");

    var profile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.UserId == currentUserId);

    if (profile == null )
        return NotFound("Profile not found.");
       

    var apps = await _context.Applications
        .Where(a => a.ApplicantProfileId == profile.Id)
        .Include(a => a.Job)
        .OrderByDescending(a => a.AppliedDate)
        .Select(a => new ApplicationDto
        {
            Id = a.Id,
            JobId = a.JobId,
            JobTitle = a.Job != null ? a.Job.Title : string.Empty,
            Status = a.Status,
            AppliedDate = a.AppliedDate,
            CoverLetter = a.CoverLetter,
            Job = new JobDto
            {
                Id = a.Job.Id,
                Title = a.Job.Title ,
                Description = a.Job.Description,
                Location = a.Job.Location,
                Company = a.Job.Company
            }
        })
        .ToListAsync();

    return Ok(apps);
}

        // Get application for a specific job by user
                // [Authorize] 
    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetApplicationByJob( int jobId)
    {
          var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (currentUserId == null)
        return Unauthorized("You have to login first");

    var profile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.UserId == currentUserId);

    if (profile == null)
        return NotFound("Profile not found.");

        var application = await _context.Applications
            .Include(a => a.Job) // load job details
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.ApplicantProfileId == profile.Id);

        if (application == null)
            return NotFound("No application found for this job and user.");

        return Ok(application);
    }

        
          

  // DELETE: api/Applications/{applicationId}/Withdraw
        // [Authorize] 
[HttpDelete("{applicationId}/Withdraw")]
public async Task<IActionResult> WithdrawApplication(int applicationId)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
        return Unauthorized("You must be logged in to withdraw an application.");

    // 2️⃣ Get the applicant's profile
    var profile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.UserId == userId);

    if (profile == null)
        return BadRequest("Applicant profile not found. Please create a profile first.");

    // 3️⃣ Get the application and check ownership
    var application = await _context.Applications
        .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicantProfileId == profile.Id);

    if (application == null)
        return NotFound("Application not found or you do not have permission to withdraw this application.");

    // //  Soft delete: mark as withdrawn
    // application.Status = "Withdrawn";
    // _context.Applications.Update(application);
        _context.Applications.Remove(application);
            await _context.SaveChangesAsync();
    return Ok(new { message = "Application withdrawn successfully." });
}

// Implemented for Future 
// PUT: api/Applications/{applicationId}/Update
        // [Authorize] 
[HttpPut("{applicationId}/Update")]
public async Task<IActionResult> UpdateApplication(int applicationId, [FromBody] ApplicationDto dto)
{
    // 1️⃣ Get the logged-in user
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null)
        return Unauthorized("You must be logged in to update an application.");

    // 2️⃣ Get the applicant's profile
    var profile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.UserId == userId);

    if (profile == null)
        return BadRequest("Applicant profile not found. Please create a profile first.");

    // 3️⃣ Get the application and check ownership
    var application = await _context.Applications
        .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicantProfileId == profile.Id);

    if (application == null)
        return NotFound("Application not found or you do not have permission to edit this application.");

    // 4️⃣ Update allowed fields (e.g., CoverLetter)
    application.CoverLetter = dto.CoverLetter ?? application.CoverLetter;
    // You could allow updating Status only by admin/HR, not applicant
    // application.Status = dto.Status; <-- optional

    _context.Applications.Update(application);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Application updated successfully.", application });
}


  

     

}




