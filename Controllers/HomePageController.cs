using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI;

         [ApiController]
        [Route("api/notificaiton")]
public class HomePageController :ControllerBase
{
         private readonly AppDbContext _context;

    public HomePageController(AppDbContext context)
    {
        _context = context;
    }
// Notify Who Pass Screening Process   on the home page when Opend 
 [HttpGet("{jobId}/notifications")]
public async Task<IActionResult> GetJobNotifications(int jobId)
{
    var notifications = await _context.Applications
        .Where(a => a.JobId == jobId && a.Status == "Passed Screening")
        .Select(a => new 
        {
            ApplicantFullName = a.ApplicantProfile.FullName ?? "" ,
            status = a.Status,
            JobTitle = a.Job.Title ?? ""
        })
        .ToListAsync();

    return Ok(notifications);
}
}
