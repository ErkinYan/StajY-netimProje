namespace StajYonetim.Identity.DataAccess;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StajYonetim.Identity.Core.Models;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<School>(entity =>
        {
            entity.HasOne(x => x.AcademicYear)
                .WithMany()
                .HasForeignKey(x => x.AcademicYearId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.District).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(400).IsRequired();
            entity.Property(x => x.PrincipalName).HasMaxLength(200);
            entity.Property(x => x.PrincipalTc).HasMaxLength(11);
            entity.Property(x => x.PrincipalEmail).HasMaxLength(256);
            entity.Property(x => x.PrincipalPhone).HasMaxLength(20);
        });

        builder.Entity<AcademicYear>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
        });
    }
}
