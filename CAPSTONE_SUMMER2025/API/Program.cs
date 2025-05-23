﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure.Models;
using Infrastructure.Repository;
using API.Mapping;
using Infrastructure.Security;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using API.Repositories.Interfaces;
using AutoMapper;
using API.Mapping;
using API.Repositories;
using API.Service;


var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt");

//Config JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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
        IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(builder.Configuration["Jwt:Key"])),
        LifetimeValidator = JwtTokenLifetimeManager.ValidateTokenLifetime
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var userClaims = context.Principal.Claims.ToList();
            return Task.CompletedTask;
        }
    };
}).AddCookie()
.AddGoogle(options =>
{
    IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
    options.ClientId = googleAuthNSection["ClientId"];
    options.ClientSecret = googleAuthNSection["ClientSecret"];
});
// yeu cau nhap token
builder.Services.AddSwaggerGen(options =>
{
    // Định nghĩa SecurityScheme cho Bearer Token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer' [space] và token hợp lệ. Ví dụ: Bearer eyJhbGciOiJIUzI1NiIsInR5c..."
    });

    // Áp dụng yêu cầu bảo mật toàn cầu
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});
//Config CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
// Authorization
builder.Services.AddAuthorization();
// DBContext
builder.Services.AddDbContext<CAPSTONE_SUMMER2025Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBContext")));
builder.Services.AddScoped<CAPSTONE_SUMMER2025Context>();

// Dependency Injection
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<GoogleService>();
// AutoMapper
builder.Services.AddAutoMapper(typeof(Program),
                               typeof(MappingAccount));


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
