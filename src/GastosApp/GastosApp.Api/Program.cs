using GastosApp.Api.Middleware;
using GastosApp.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -- Infraestructura (DbContext, Identity, UnitOfWork) --
builder.Services.AddInfrastructure(builder.Configuration);

// -- Controladores y OpenAPI --
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// -- Autenticacion JWT --
// La clave secreta se lee de la variable de entorno GASTOSAPP_JWT_KEY.
// Nunca la pongas en appsettings.json ni en el repositorio.
var jwtKey = Environment.GetEnvironmentVariable("GASTOSAPP_JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "No se encontro la clave JWT. Define la variable de entorno GASTOSAPP_JWT_KEY.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// -- CORS (ajustar origenes en produccion) --
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
