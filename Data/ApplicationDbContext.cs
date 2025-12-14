using System.Collections.Generic;

namespace Inventario_Inteligente.Data
{
    public class ApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<Product> Products { get; set; }

        internal async Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        internal void Update(object productToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
}
