﻿using MCS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using static MCS.Data.AppDbContext;

namespace MCS.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            try
            {
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public void InitializeDatabase()
        {
            try
            {
                var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    if (!databaseCreator.CanConnect()) databaseCreator.Create();
                    Log.Warning("Database does not exist! New Database has been Created!");

                    if (!databaseCreator.HasTables()) databaseCreator.CreateTables();
                    Log.Warning("Table does not exist! New Table has been Created!");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
