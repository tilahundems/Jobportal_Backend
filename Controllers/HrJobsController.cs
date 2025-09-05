using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI;

        [ApiController]
        [Route("api/hr/jobs")]
        // [Authorize(Roles = "HR")] // HR-only
public class HrJobsController:ControllerBase
{
     private readonly AppDbContext _context;
         private readonly UserManager<IdentityUser> _userManager;

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public HrJobsController(AppDbContext context)
    {
        _context = context;
    }
// Helper: current user id

             [HttpPost]
        public async Task<IActionResult> CreateJob ([FromBody] CreateJobDto dto)
        {
            var userId = CurrentUserId;
        if (userId == null) return Unauthorized("Login required.");
           
            var job = new Job
            {
                Title = dto.Title,
                Company = dto.Company,
                Location = dto.Location,
                Description = dto.Description,
                Department = dto.Department,
                PostedDate = DateTime.UtcNow,
                PostedByUserId = userId,
                Deadline = dto.Deadline,
                IsActive = true

            };

             await _context.Jobs.AddAsync(job);
             await _context.SaveChangesAsync();
              return CreatedAtAction(null, new { job.Id }, job); // or map to DTO

               
        }

    [HttpPut("{id}")]
      public  async Task<IActionResult> UpdateJob ( int id,[FromBody] UpdateJobDto dto)
    {
            var userId = CurrentUserId;
            if (userId == null) return Unauthorized();

             var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound("Job not found.");
       
        // if (job.PostedByUserId != userId) return Forbid();

             job.Title = dto.Title;
            job.Company = dto.Company;
            job.Location = dto.Location;
            job.Description = dto.Description;
            job.Department= dto.Department;
            job.IsActive = dto.IsActive;
            job.PostedDate = DateTime.UtcNow;
            job.Deadline = dto.Deadline;

             _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
        return Ok(job);

    }

      [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound();

        // if (job.PostedByUserId != userId) return Forbid();

        // Soft delete
        // job.IsActive = false;
        // _context.Jobs.Update(job);
        // await _context.SaveChangesAsync();
             _context.Jobs.Remove(job);
         await _context.SaveChangesAsync();

        return Ok(new { message = "Job deactivated." });
    }

     // GET: api/hr/jobs/{jobId}/applications
    [HttpGet("{jobId}/applications")]
    public async Task<IActionResult> GetJobApplications(int jobId)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized();

        var job = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null) return NotFound("Job not found.");
        // if (job.PostedByUserId != userId) return Forbid();

        var apps = await _context.Applications
            .Where(a => a.JobId == jobId)
            .Include(a => a.ApplicantProfile)
            .OrderByDescending(a => a.AppliedDate)
            .Select(a => new ApplicationLiteDto
            {
                ApplicationId = a.Id,
                AppliedDate = a.AppliedDate,
                Status = a.Status,
                CoverLetter = a.CoverLetter,
                ApplicantProfileId = a.ApplicantProfileId,
                ApplicantFullName = a.ApplicantProfile!.FullName,
                ApplicantPhone = a.ApplicantProfile!.Phone,
                ApplicantResumeUrl = a.ApplicantProfile!.ResumeUrl,
                ApplicantSkills = a.ApplicantProfile!.Skills,
                ApplicantEducation = a.ApplicantProfile!.Education
            })
            .ToListAsync();

        return Ok(apps);
    }

    [HttpGet("{jobId:int}/applications/download")]
public async Task<IActionResult> DownloadJobApplicationsCsv(int jobId)
{
    var userId = CurrentUserId;
    if (userId == null) return Unauthorized();

    var job = await _context.Jobs
        .AsNoTracking()
        .FirstOrDefaultAsync(j => j.Id == jobId);

    if (job == null) return NotFound("Job not found.");
    // if (job.PostedByUserId != userId) return Forbid();

    var apps = await _context.Applications
        .Where(a => a.JobId == jobId)
        .Include(a => a.ApplicantProfile)
        .OrderByDescending(a => a.AppliedDate)
        .ToListAsync();

    var csv = new StringBuilder();
    csv.AppendLine("ApplicationId,AppliedDate,Status,CoverLetter,FullName,Phone,ResumeUrl,Skills,Education");

    foreach (var a in apps)
    {
        csv.AppendLine($"{a.Id},{a.AppliedDate},{a.Status}," +
                       $"\"{a.CoverLetter?.Replace("\"", "\"\"")}\"," +
                       $"\"{a.ApplicantProfile?.FullName}\"," +
                       $"{a.ApplicantProfile?.Phone}," +
                       $"{a.ApplicantProfile?.ResumeUrl}," +
                       $"\"{a.ApplicantProfile?.Skills}\"," +
                       $"\"{a.ApplicantProfile?.Education}\"");
    }

    return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"applications_job_{jobId}.csv");
}

// for Multiple Screening  Use if needed 
// // screening applicants
//  [HttpPut("{applicationId}/select")]
// public async Task<IActionResult> SelectApplicant(int applicationId)
// {
//     var userId = CurrentUserId; // HR
//     if (userId == null) return Unauthorized();

//     var application = await _context.Applications
//         .Include(a => a.Job)
//         .FirstOrDefaultAsync(a => a.Id == applicationId);

//     if (application == null) return NotFound("Application not found.");
//     if (application.Job.PostedByUserId != userId) return Forbid();

//     application.Status = "Passed Screening";
//     await _context.SaveChangesAsync();

//     return Ok(application);
// }
// [HttpPut("select-multiple")]
// public async Task<IActionResult> SelectApplicants([FromBody] List<int> applicationIds)
// {
//     var userId = CurrentUserId;
//     if (userId == null) return Unauthorized();

//     var apps = await _context.Applications
//         .Include(a => a.Job)
//         .Where(a => applicationIds.Contains(a.Id))
//         .ToListAsync();

//     foreach (var a in apps)
//     {
//         if (a.Job.PostedByUserId != userId) continue; // only allow HR to update their jobs
//         a.Status = "Passed Screening";
//     }

//     await _context.SaveChangesAsync();
//     return Ok(apps);
// } 
[HttpPut("{applicationId}/select")]
public async Task<IActionResult> SelectApplicant(int applicationId, [FromServices] EmailService emailService)
{
    var userId = CurrentUserId;
    if (userId == null) return Unauthorized();

    var application = await _context.Applications
        .Include(a => a.Job)
        .Include(a => a.ApplicantProfile)
        .FirstOrDefaultAsync(a => a.Id == applicationId);

    if (application == null) return NotFound("Application not found.");
    // if (application.Job.PostedByUserId != userId) return Forbid();

    application.Status = "Selected";
    await _context.SaveChangesAsync();
  
    // Send Email
    if (application.ApplicantProfile != null)
    {

            //  var identityUser = await _userManager.FindByIdAsync(application.ApplicantProfile.UserId);

        var subject = $"Congratulations! You passed screening for {application.Job.Title}";
        var body = $"Dear {application.ApplicantProfile.FullName},<br/><br/>" +
                   $"Congratulations! You have passed the screening stage for the job <strong>{application.Job.Title}</strong> at {application.Job.Company}.<br/>" +
                   "Please check your portal account for next steps.<br/><br/>" +
                   "Best regards,<br/>Abay Bank HR";
                           await emailService.SendEmailAsync("Tilahundems271@gmail.com", subject, body);

// if (identityUser != null && !string.IsNullOrEmpty(identityUser.Email))
// {

// } 
    }

    return Ok(" Applicant  selected ");
}


}
