using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModelService;
using System;

namespace DataService
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(
                new { Id = "1", Name = "Administrator", NormalizedName = "ADMINISTRATOR", RoleName = "Administrator", Handle = "administrator", IsActive = true },
                new { Id = "2", Name = "User", NormalizedName = "USER", RoleName = "User", Handle = "User", IsActive = true }
            );
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<TokenModel> Tokens { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<BookmarkCard> BookMarkCards { get; set; }
    }
}
