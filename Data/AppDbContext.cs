using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



namespace JobPortalAPI;

    public class AppDbContext : IdentityDbContext<IdentityUser>
{
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
{
    
    
}
public DbSet<Job> Jobs { get; set;}
public DbSet<Application> Applications { get; set; }
public DbSet<ApplicantProfile> ApplicantsProfile { get; set; }

}
