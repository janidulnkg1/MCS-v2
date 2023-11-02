using Microsoft.EntityFrameworkCore;

namespace MCS.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) 
        {

        }
    }
}
