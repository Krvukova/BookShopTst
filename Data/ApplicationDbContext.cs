using BookShopTest.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookShopTest.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
            
        }
        
        public DbSet<Book> Books {  get; set; }   
    }
}
