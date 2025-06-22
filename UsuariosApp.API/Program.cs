using Microsoft.EntityFrameworkCore;
using UsuariosApp.API.MiddleWare;
using UsuariosApp.InfraStructure.Context;
using UsuariosApp.Settings.InjecoesDependencias;


var builder = WebApplication.CreateBuilder(args);

// ?? Connection string e DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UsuarioDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region Configura��es Swagger

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
  
    c.OperationFilter<AddInformativeHeaderFilter>();
});
#endregion


#region Configura��es de CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
#endregion


// Configura��o de Inje��o de Depend�ncias
builder.Services.RegistrarServicos();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tratamento de Exce��es Middleware para capturar erros globais
app.UseMiddleware<TratamentoExcecoesMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
