﻿using CatalogoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoMinimalAPI.Context
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            //Categoria
            mb.Entity<Categoria>().HasKey(c => c.CategoriaId);
            mb.Entity<Categoria>().Property(c => c.Nome)
                .HasMaxLength(100)
                .IsRequired();
            mb.Entity<Categoria>().Property(c => c.Descricao)
                .HasMaxLength(150)
                .IsRequired();

            //Produto
            mb.Entity<Produto>().HasKey(c => c.ProdutoId);
            mb.Entity<Produto>().Property(c => c.Nome)
                .HasMaxLength(100)
                .IsRequired();
            mb.Entity<Produto>().Property(c => c.Descricao)
                .HasMaxLength(150)
                .IsRequired();
            mb.Entity<Produto>().Property(c => c.Imagem)
                .HasMaxLength(100)
                .IsRequired();
            mb.Entity<Produto>().Property(c => c.Preco)
                .HasPrecision(14, 2);


            //Relacionamento

            mb.Entity<Produto>()
                .HasOne<Categoria>(c => c.Categoria)
                .WithMany(p => p.Produtos)
                .HasForeignKey(c => c.CategoriaId);
        }
    }
}