using BusinessLayer.Interface;
using BusinessLayer.Service;
using FundooNotes.Web.Configuration;
using FundooNotes.Web.Middleware;
using FundooNotes.Web.Extensions;
using FundooNotes.Web.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.Entity;
using NLog.Web;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using RepositoryLayer.Repository;
using System.Text;
using FundooNotes.Web.Messaging;

namespace FundooNotes.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            // Add services to the container.
            builder.Services.AddControllers();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<FundooDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();
            
            // add redis service
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
                options.InstanceName = "FundooNotes:";
            });

            // Add Swagger services
            //builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerConfiguration();

            // Add Exception Handler service
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // DI ---
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<INoteRepository, NoteRepository>();
            builder.Services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();
            builder.Services.AddSingleton<IEmailProducerService, EmailProducerService>();
            builder.Services.AddHostedService<EmailConsumerService>();
            //---
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();
            // ---

            var app = builder.Build();

            app.UseMiddleware<RequestTimingMiddleware>();

            app.UseExceptionHandler();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(); // Serves Swagger UI at /swagger
            }


            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
