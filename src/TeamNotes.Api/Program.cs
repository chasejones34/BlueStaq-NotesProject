using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TeamNotes.Api.Data;
using TeamNotes.Api.Domain;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter Valid JWT Token"
            });

        options.AddSecurityRequirement(document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document)
                ] = new List<string>()
            });
    });

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TeamNotes.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TeamNotes.Api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("postgresql");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception =
            context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        context.Response.ContentType =
            "application/problem+json";

        await Results.Problem(
            statusCode: context.Response.StatusCode,
            title: "An unexpected error occurred.",
            detail: app.Environment.IsDevelopment()
                ? exception?.Message
                : null)
            .ExecuteAsync(context);
    });
});

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () =>
    Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.MapHealthChecks("/health/database")
    .WithName("DatabaseHealthCheck");

app.MapControllers();

app.Run();

public partial class Program;