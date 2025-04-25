using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SingularSystems_SelfKiosk_Software.Data; // ? Correct namespace for DataContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Correct frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger (for testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS policy BEFORE authorization and after HTTPS redirection
app.UseCors("AllowReact");

app.UseAuthorization();

// Map controller routes
app.MapControllers();

app.Run();
