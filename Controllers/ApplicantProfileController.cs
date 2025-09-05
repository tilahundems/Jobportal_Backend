using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // <-- This is required for Include/ThenInclude


namespace JobPortalAPI;
         [ApiController]
         [Route("api/[controller]")]
public class ApplicantProfileController:ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ApplicantProfileController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }



[HttpPost("createUpdate")]
public async Task<IActionResult> CreateOrUpdateProfile([FromBody] ApplicantProfileDto dto)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Unauthorized("You are not authorized.");
           
    var existingProfile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.UserId == "demouser");

    if (existingProfile != null)
    {
        existingProfile.FullName = dto.FullName;
        existingProfile.Phone = dto.Phone;
        existingProfile.Skills = dto.Skills;
        existingProfile.Education = dto.Education;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Profile updated (resume not changed)", profile = existingProfile });
    }

    var profile = new ApplicantProfile
    {
        UserId = userId,
        FullName = dto.FullName,
        Phone = dto.Phone,
        Skills = dto.Skills,
        Education = dto.Education
    };

    _context.ApplicantsProfile.Add(profile);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Profile created || Updated (resume not uploaded yet)", profile });
}
 


//    [HttpPost("{id}/upload-resume")]
// public async Task<IActionResult> UploadResume(int id, IFormFile file)
// {
//        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//     if (currentUserId == null)
//         return Unauthorized("You have to login first");

   
//     var applicantprofile = await _context.ApplicantsProfile.FindAsync(id);
//     if (applicantprofile == null) return NotFound("pls submit profile first !!!");
       

//     if (file == null || file.Length == 0)
//         return BadRequest("No file uploaded");

//     // Ensure directory exists
//     var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
//     if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

//     // Save file with unique name
//     var fileName = $"{Guid.NewGuid()}_{file.FileName}";
//     var filePath = Path.Combine(folderPath, fileName);
//     using (var stream = new FileStream(filePath, FileMode.Create))
//     {
//         await file.CopyToAsync(stream);
//     }

//     // Save file path in database (relative path)
//     //application.ResumeUrl = $"/resumes/{fileName}";
//     await _context.SaveChangesAsync();
//   return Ok();
//     // return Ok(new { message = "Resume uploaded", resumeUrl = application.ResumeUrl });

// }


[HttpPost("{profileId}/uploadResume")]
public async Task<IActionResult> UploadResume(int profileId, IFormFile file)
{
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (currentUserId == null) return Unauthorized("You have to login first");

    var applicantProfile = await _context.ApplicantsProfile
        .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == currentUserId);
//  var applicantProfile = await _context.ApplicantsProfile
//         .FirstOrDefaultAsync(p => p.Id == profileId);

    if (applicantProfile == null)
        return NotFound("Profile not found , pls Create profile  first || you don’t have permission.");

    if (file == null || file.Length == 0)
        return BadRequest("No file uploaded");

    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
    var filePath = Path.Combine(folderPath, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    // Save resume path
    applicantProfile.ResumeUrl= $"/resumes/{fileName}";
    await _context.SaveChangesAsync();

    return Ok(new { message = "Resume uploaded successfully", resumeUrl = applicantProfile.ResumeUrl });
}


   [HttpGet("me")] 
public async Task<IActionResult> GetMyProfile()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Unauthorized("U have to Login First");

    var profile = await _context.ApplicantsProfile
        .Include(p => p.Applications)
        .ThenInclude(a => a.Job)
        .FirstOrDefaultAsync(p => p.UserId == userId);

    if (profile == null) return NotFound("Profile not found pls create Ur Profile first");

    var dto = new ApplicantProfileDto
    {
        FullName = profile.FullName,
        Phone = profile.Phone,
        ResumeUrl = profile.ResumeUrl,
        Skills = profile.Skills,
        Education = profile.Education,
        
        Applications = profile.Applications.Select(a => new ApplicationDto
        {
            JobId = a.JobId,
             Id=a.Id,
            JobTitle = a.Job?.Title ?? "",
            AppliedDate = a.AppliedDate,
            Status = a.Status,
            CoverLetter=a.CoverLetter,
             Job= new JobDto{
                            Id=a.JobId,
                            Title=a.Job?.Title ?? "",
                            Company=a.Job?.Company ?? "",
                            Description=a.Job?.Description,
                            Location=a.Job?.Location,


                           }
            

        }).ToList()
    };

    return Ok(dto);
    
}




}
