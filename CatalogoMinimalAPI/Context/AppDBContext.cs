using CatalogoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoMinimalAPI.Context
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        
    }
}