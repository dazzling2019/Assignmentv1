#nullable disable
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Assignmentv1.Models;

namespace Assignmentv1.Data
{
    public class Assignmentv1Context : IdentityDbContext<ApplicationUser>
    {
        public Assignmentv1Context(DbContextOptions<Assignmentv1Context> options)
        : base(options)
        {
        }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Fete> Fete { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Student> Student { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
          

            {
                IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

                optionsBuilder
               .UseSqlServer(Configuration.GetConnectionString("Assignmentv1"))
               .EnableSensitiveDataLogging()
               .LogTo(x => System.Diagnostics.Debug.WriteLine(x));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasIndex(x => x.Name)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasData(
                    new Student { Id = 1, Name = "Daryl Leach", Email = "dl5bbs@bolton.ac.uk" }
                );

            modelBuilder.Entity<Room>()
                .HasData(
                    new Room { Id = 1, Name = "Main Hall", RoomTitleId = "F2-08", Capacity = 150 }

                );

            modelBuilder.Entity<Fete>()
                 .HasData(
                    new Fete { Id = 1, Name = "Boxing", Synopsis = "Local Boxing event", FeteTime = new DateTime(2015, 12, 25) }
                );
            modelBuilder.Entity<Booking>()
                .HasData(
                    new Booking { Id = 1, StudentId = 1, FeteId = 1, FeteTime = new DateTime(2015, 12, 25) });

            
        }
        private object WithMany(Func<object, object> value)
        {
            throw new NotImplementedException();
        }
    }
    }








