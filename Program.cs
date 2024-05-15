using backend.Repository.Interfaces;
using backend.Repository;
using backend.ServiceFiles.API.Services;
using backend.ServiceFiles.Interfaces;
using backend.ServiceFiles;
using backend.Extensions;
using backend.Configurations;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<FacebookApiOptions>(builder.Configuration.GetSection("FacebookApi"));
builder.Services.AddHttpClient<FacebookApiService>();
builder.Services.AddControllers();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


builder.Services.AddJWTServices(builder.Configuration);
// Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// services

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountService, AccountsService>();
builder.Services.AddScoped<IFB, FacebookApiService>();
builder.Services.AddScoped<IGoogleApiService, GoogleApiService>();

builder.Services.AddScoped<IEmailService, EmailService>();
//cors

builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
