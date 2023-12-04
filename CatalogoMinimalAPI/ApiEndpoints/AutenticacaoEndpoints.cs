using CatalogoMinimalAPI.Models;
using CatalogoMinimalAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace CatalogoMinimalAPI.ApiEndpoints
{
    public static class AutenticacaoEndpoints
    {
        public static void MapAutenticacaoEndpoints(this WebApplication app)
        {
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
        }
    }
}