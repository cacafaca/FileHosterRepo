using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api
{
    public class Startup
    {
        #region Constants
        public const string _connectionStringName = "FileHosterRepoConnectionString";
        #endregion

        #region Fields
        public IConfiguration Configuration { get; }
        #endregion

        #region Constructors
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDbContext<Dal.DataAccess.FileHosterContext>(options =>
                options.UseMySQL(Configuration.GetConnectionString(_connectionStringName))
                //options.UseMySQL("server=localhost;user id=filehoster_app;password=development;persistsecurityinfo=True;database=filehoster;")
            );

            var authManager = new JwtAuthenticationManager(Configuration["Jwt:Key"]);
            services.AddSingleton<IJwtAuthenticationManager>(authManager);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = (services.Where(x=>x.Lifetime==ServiceLifetime.Singleton && x.ServiceType.Name == nameof(IJwtAuthenticationManager)).FirstOrDefault().ImplementationInstance as JwtAuthenticationManager).GetSymmetricSecurityKey(), //authManager.GetSymmetricSecurityKey(),                    

                    ValidateIssuer = false,
                    ValidateAudience = false
                    //ValidateLifetime = true,
                    //ValidIssuer = Configuration["Jwt:Issuer"],
                    //ValidAudience = Configuration["Jwt:Issuer"],
                    //ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProCode.FileHosterRepo.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProCode.FileHosterRepo.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
