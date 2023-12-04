using CatalogoMinimalAPI.Context;
using CatalogoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoMinimalAPI.ApiEndpoints
{
    public static class CategoriasEndpoints
    {
        public static void MapCategoriasEndpoints(this WebApplication app)
        {
            app.MapGet("/categorias", async (AppDBContext db) => await db.Categorias.ToListAsync()).WithTags("Categorias").RequireAuthorization();

            app.MapGet("/categorias/{id:int}", async (int id, AppDBContext db) =>
            {
                return await db.Categorias.FindAsync(id)
                                            is Categoria categoria
                                            ? Results.Ok(categoria)
                                            : Results.NotFound();
            });

            app.MapPost("/categorias", async (Categoria categoria, AppDBContext db) =>
            {
                db.Categorias.Add(categoria);
                await db.SaveChangesAsync();

                return Results.Created($"/categorias/{categoria.CategoriaId}", categoria);
            })
                .Accepts<Categoria>("application/json")
                .Produces<Categoria>(StatusCodes.Status201Created);

            app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDBContext db) =>
            {
                if (categoria.CategoriaId != id)
                    return Results.BadRequest();

                var categoriaDB = await db.Categorias.FindAsync(id);

                if (categoriaDB is null)
                    return Results.NotFound();

                categoriaDB.Nome = categoria.Nome;
                categoriaDB.Descricao = categoria.Descricao;

                await db.SaveChangesAsync();

                return Results.Ok(categoriaDB);
            });

            app.MapDelete("categorias/{id:int}", async (int id, AppDBContext db) =>
            {
                var categoria = await db.Categorias.FindAsync(id);

                if (categoria is null)
                    return Results.NotFound();

                db.Remove(categoria);
                await db.SaveChangesAsync();

                return Results.NoContent();
            });
        }
    }
}