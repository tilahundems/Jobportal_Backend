using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI;
[ApiController]
    [Route("api/[controller]")]
public class JobsController : ControllerBase
{
        private readonly AppDbContext _context;
         private readonly static  DateTime today= DateTime.UtcNow;
        public JobsController(AppDbContext context)
        {
        _context = context;
         }




    [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            
            var jobs = await _context.Jobs.
             Where(j=> j.Deadline >=today)
            .ToListAsync();
             return Ok(jobs);

        }
        
           [AllowAnonymous]
            [HttpGet("{id}")]
                public async Task<IActionResult> GetJob(int id)
                {
                    var jobs =  await _context.Jobs.
                     FindAsync(id );
                        if (jobs == null || jobs.Deadline<today || jobs.Deadline < today)
                          return BadRequest("Job Not Found ");
                      return Ok(jobs);
                }


                // [Authorize(Roles = "Admin")]
                [HttpPost]
            public async Task<IActionResult> CreateJob([FromBody]Job job)
            {
                        job.PostedDate=DateTime.Now;
                          _context.Add(job);
                         await _context.SaveChangesAsync();
                         return CreatedAtAction(nameof(GetJob), new {id=job.Id}, job);

            }
            // [Authorize(Roles = "Admin,HR")]
            [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id,Job updatedJob)
        {
                 if(id != updatedJob.Id)
                 {
                    return BadRequest("Id Mismatch");
                 }  
                 var job= await _context.Jobs.FindAsync(id);
                    if (job == null)
                    return NotFound();
                   job.Title = updatedJob.Title;
                    job.Description = updatedJob.Description;
                    job.Location = updatedJob.Location;
                    job.Company = updatedJob.Company;
                    job.PostedDate = updatedJob.PostedDate;
                    _context.SaveChanges();
                    return Ok(job);
        }

                // [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id )
        {
            var job =await _context.Jobs.FindAsync(id);
            if(job == null)
            return NotFound();
            _context.Jobs.Remove(job);
            _context.SaveChanges();
            return Ok("JOb Deleted Successfully ");

        }


        
}

