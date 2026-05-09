namespace StajYonetim.Identity.DataAccess;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StajYonetim.Identity.Core.Models;
using StajYonetim.Identity.Core.Security;
using System.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<StajYonetim.Identity.Core.Models.AppUser>>();
        var dbContext = services.GetRequiredService<AppDbContext>();

        foreach (var roleName in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await EnsureUserTcKimlikNoColumnAsync(dbContext);

        var defaultEmail = "ilce@local.test";
        var defaultUser = await userManager.FindByEmailAsync(defaultEmail);
        if (defaultUser == null)
        {
            var user = new StajYonetim.Identity.Core.Models.AppUser
            {
                UserName = "ilce",
                Email = defaultEmail,
                EmailConfirmed = true,
                FullName = "İlçe Admin"
            };
            var result = await userManager.CreateAsync(user, "P@ssw0rd123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppRoles.Ilce);
            }
        }
        else if (!await userManager.IsInRoleAsync(defaultUser, AppRoles.Ilce))
        {
            await userManager.AddToRoleAsync(defaultUser, AppRoles.Ilce);
        }

        if (!await dbContext.AcademicYears.AnyAsync())
        {
            dbContext.AcademicYears.AddRange(
                new AcademicYear
                {
                    Name = "2025 – 2026",
                    StartDate = new DateOnly(2025, 9, 1),
                    EndDate = new DateOnly(2026, 6, 30),
                    IsActive = true,
                    ActivatedAt = DateTime.UtcNow
                },
                new AcademicYear
                {
                    Name = "2024 – 2025",
                    StartDate = new DateOnly(2024, 9, 1),
                    EndDate = new DateOnly(2025, 6, 30),
                    IsActive = false
                },
                new AcademicYear
                {
                    Name = "2023 – 2024",
                    StartDate = new DateOnly(2023, 9, 1),
                    EndDate = new DateOnly(2024, 6, 30),
                    IsActive = false
                });

            await dbContext.SaveChangesAsync();
        }

        await EnsureSchoolAcademicYearColumnAsync(dbContext);

        if (!await dbContext.Schools.AnyAsync())
        {
            var activeYearId = await dbContext.AcademicYears.Where(x => x.IsActive).Select(x => x.Id).FirstAsync();
            dbContext.Schools.AddRange(
                new School { AcademicYearId = activeYearId, Name = "Atatürk MYO", District = "Osmangazi, Bursa", Address = "Osmangazi, Bursa", PrincipalName = "Mehmet Demir", StudentCount = 214, IsActive = true },
                new School { AcademicYearId = activeYearId, Name = "Cumhuriyet MYO", District = "Nilüfer, Bursa", Address = "Nilüfer, Bursa", PrincipalName = "Fatma Kaya", StudentCount = 187, IsActive = true },
                new School { AcademicYearId = activeYearId, Name = "İnönü MYO", District = "Yıldırım, Bursa", Address = "Yıldırım, Bursa", StudentCount = 96, IsActive = true },
                new School { AcademicYearId = activeYearId, Name = "Barbaros MYO", District = "Osmangazi, Bursa", Address = "Osmangazi, Bursa", PrincipalName = "Ali Çelik", StudentCount = 230, IsActive = true },
                new School { AcademicYearId = activeYearId, Name = "Fatih MYO", District = "Gemlik, Bursa", Address = "Gemlik, Bursa", StudentCount = 74, IsActive = true },
                new School { AcademicYearId = activeYearId, Name = "Kemal Atatürk MYO", District = "Mudanya, Bursa", Address = "Mudanya, Bursa", PrincipalName = "Hasan Şahin", StudentCount = 155, IsActive = true });

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task EnsureUserTcKimlikNoColumnAsync(AppDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();
        var openedHere = false;
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            openedHere = true;
        }

        try
        {
            var columnExistsCmd = connection.CreateCommand();
            columnExistsCmd.CommandText = @"
SELECT COUNT(1)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'TcKimlikNo';";

            var columnExists = Convert.ToInt32(await columnExistsCmd.ExecuteScalarAsync()) > 0;
            if (columnExists)
            {
                return;
            }

            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD TcKimlikNo nvarchar(11) NULL;");
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task EnsureSchoolAcademicYearColumnAsync(AppDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();
        var openedHere = false;
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
            openedHere = true;
        }

        try
        {
            var columnExistsCmd = connection.CreateCommand();
            columnExistsCmd.CommandText = @"
SELECT COUNT(1)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Schools' AND COLUMN_NAME = 'AcademicYearId';";

            var columnExists = Convert.ToInt32(await columnExistsCmd.ExecuteScalarAsync()) > 0;
            if (columnExists)
            {
                return;
            }

            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Schools ADD AcademicYearId int NULL;");

            var activeYearId = await dbContext.AcademicYears
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (activeYearId != 0)
            {
                await dbContext.Database.ExecuteSqlRawAsync("UPDATE Schools SET AcademicYearId = {0} WHERE AcademicYearId IS NULL;", activeYearId);
            }

            await dbContext.Database.ExecuteSqlRawAsync("UPDATE Schools SET AcademicYearId = (SELECT TOP 1 Id FROM AcademicYears WHERE IsActive = 1) WHERE AcademicYearId IS NULL;");
            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE Schools ALTER COLUMN AcademicYearId int NOT NULL;");
            await dbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schools_AcademicYears_AcademicYearId')
BEGIN
    ALTER TABLE Schools
    ADD CONSTRAINT FK_Schools_AcademicYears_AcademicYearId
    FOREIGN KEY (AcademicYearId) REFERENCES AcademicYears(Id)
    ON DELETE CASCADE;
END");

            await dbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Schools_AcademicYearId' AND object_id = OBJECT_ID('Schools'))
BEGIN
    CREATE INDEX IX_Schools_AcademicYearId ON Schools(AcademicYearId);
END");
        }
        finally
        {
            if (openedHere)
            {
                await connection.CloseAsync();
            }
        }
    }
}
