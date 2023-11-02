﻿using MCS.Models;
using Microsoft.EntityFrameworkCore;

namespace MCS.Data
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; set; }

        Task SaveChangesAsync();
    }
}
