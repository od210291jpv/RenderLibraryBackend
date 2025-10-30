using DataBaseService;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles().UseRouting();
app.MapOpenApi();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
