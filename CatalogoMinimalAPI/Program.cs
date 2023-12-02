using CatalogoMinimalAPI.Context;
using CatalogoMinimalAPI.Models;
using CatalogoMinimalAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiCatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"JWT Authorization header using the Bearer scheme.
                    Enter 'Bearer'[space].Example: \'Bearer 12345abcdef\'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         new string[] {}
                    }
                });
});

builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddAuthentication
                 (JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = builder.Configuration["Jwt:Issuer"],
                         ValidAudience = builder.Configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey
                         (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                     };
                 });

builder.Services.AddAuthorization();

var app = builder.Build();

//Login Endpoint

#region Login
app.MapPost("/login", [AllowAnonymous] (UserModel usermodel, ITokenService tokenservice) =>
{
    if (usermodel is null)
        return Results.BadRequest("Login Inválido");

    if (usermodel.UserName == "macoratti" && usermodel.Password == "numsey#123")
    {
        var tokenString = tokenservice.GerarToken(app.Configuration["Jwt:Key"],
                                                  app.Configuration["Jwt:Issuer"],
                                                  app.Configuration["Jwt:Audience"],
                                                  usermodel);

        return Results.Ok(new { token = tokenString });
    }
    else return Results.BadRequest("Login Inválido");
})
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status200OK)
    .WithName("Login")
    .WithTags("Autenticacao");

#endregion

//Categorias
#region Categorias Endpoints

app.MapGet("/", () => "Catálogo de Produtos - 2022").ExcludeFromDescription();

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

#endregion

//Produtos
#region Produtos Endpoints

app.MapGet("/produtos", async (AppDBContext db) => await db.Produtos.ToListAsync()).WithTags("Produtos").RequireAuthorization();

app.MapGet("/produtos/{id:int}", async (int id, AppDBContext db) =>
{
    return await db.Produtos.FindAsync(id)
                                is Produto produto
                                ? Results.Ok(produto)
                                : Results.NotFound();
});

app.MapPost("/produtos", async (Produto produto, AppDBContext db) =>
{
    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.ProdutoId}", produto);
})
    .Accepts<Produto>("application/json")
    .Produces<Produto>(StatusCodes.Status201Created);

app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDBContext db) =>
{
    if (produto.ProdutoId != id)
        return Results.BadRequest();

    var produtoDB = await db.Produtos.FindAsync(id);

    if (produtoDB is null)
        return Results.NotFound();

    produtoDB.Nome = produto.Nome;
    produtoDB.Descricao = produto.Descricao;
    produtoDB.Preco = produto.Preco;
    produtoDB.Imagem = produto.Imagem;
    produtoDB.DataCompra = produto.DataCompra;
    produtoDB.Estoque = produto.Estoque;

    await db.SaveChangesAsync();

    return Results.Ok(produtoDB);
});

app.MapDelete("produtos/{id:int}", async (int id, AppDBContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto is null)
        return Results.NotFound();

    db.Remove(produto);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();