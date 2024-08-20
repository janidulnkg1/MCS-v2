using MCS_WEB_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;

namespace MCS_WEB_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            try
            {
                var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    //create database if not available
                    if (!databaseCreator.CanConnect()) databaseCreator.Create();


                    //Create Tables if no tables exist
                    if (!databaseCreator.HasTables()) databaseCreator.CreateTables();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public DbSet<User> users { get; set; }

        public DbSet<Appointment> appointments { get; set;}

        public DbSet<Doctor> doctors { get; set; }




    }

}
