using Butce_Garantile.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Butce_Garantile.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gelir> Gelirler { get; set; }
        public DbSet<Gider> Giderler { get; set; }
        public DbSet<UserTarget> UserTargets { get; set; } // Kullanıcı hedefleri için DbSet ekleme

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Gider>().ToTable("Giderler");
            modelBuilder.Entity<Gelir>().ToTable("Gelirler");
            modelBuilder.Entity<UserTarget>().ToTable("UserTargets"); // UserTarget için tablo ismi ayarlama
        }
    }
}
