using System.Text;
using AngularApp1.Server.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using Microsoft.Extensions.Configuration;

internal class Program
{


	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;


        // added cors 
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MyPolicy", builder => builder.WithOrigins("http://localhost:4200", "https://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
        var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my custom Secret key for authnetication"));

        //------------ Add services to the container. ---------------------------------------------------------                                                
        builder.Services.AddControllers();
		
		builder.Services.AddEndpointsApiExplorer();
		

		// ------------added swagger via swahbuckle------------------------------------------------------------
		builder.Services.AddSwaggerGen();
		

		//----------------------Added cors for making request via diff server---------------------------------------------------------------------------//
		builder.Services.AddCors(options =>{
			options.AddPolicy("My policy", builder =>
			{

				builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader();

			});
		});
         

		//-----------------------------added db context for connecting with database--------------------------------------------------------------------//
		builder.Services.AddDbContext<AppDbcontext>(options =>
		{
			options.UseSqlServer(builder.Configuration.GetConnectionString("default"));
		});

		//-------- configure the services of authentication and authorization-----------------------------------
		builder.Services.AddAuthentication(x =>
		{
			x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(x =>
		{
			x.RequireHttpsMetadata = false;
			x.SaveToken = true;
			x.TokenValidationParameters = new TokenValidationParameters
			{
                IssuerSigningKey = signinKey,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidateActor = false,
                ClockSkew = TimeSpan.Zero
            };
		});

		//---------------------http request pipeline(middleware)----------------------------------------------------------------//


		var app = builder.Build();

		app.UseDefaultFiles();
		app.UseStaticFiles();

		// Configure the HTTP request pipeline.(middleware pipeline) 
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();

		app.UseCors("My policy");
		app.UseAuthentication();
		app.UseAuthorization();

		app.MapControllers();
		app.MapFallbackToFile("/index.html");

		app.Run();
	}
}
