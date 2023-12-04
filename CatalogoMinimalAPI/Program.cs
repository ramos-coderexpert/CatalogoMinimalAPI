using CatalogoMinimalAPI.ApiEndpoints;
using CatalogoMinimalAPI.AppServicesExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiSwagger();
builder.AddPersistence();
builder.AddAuthenticationJwt();
builder.Services.AddCors();


var app = builder.Build();

app.MapAutenticacaoEndpoints();
app.MapCategoriasEndpoints();
app.MapProdutosEndpoints();


app.UseExceptionHandling(app.Environment)
    .UseSwaggerMiddleware()
    .UseAppCors();


app.UseAuthentication();
app.UseAuthorization();

app.Run();