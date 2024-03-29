using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ProCode.FileHosterRepo.Api
{
    public class Startup
    {
        #region Constants
        public const string _connectionStringName = "FileHosterRepoConnectionString";
        #endregion

        #region Fields
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment Environment;
        #endregion

        #region Constructors
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }
        #endregion


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Camel case for JSON.
            services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
                    opt.JsonSerializerOptions.DictionaryKeyPolicy = null;
                });

            // Enable Blazor calls.
            services.AddCors(option =>
            {
                /*option.AddPolicy(name: "PolicyName",
                    builder =>
                    {
                        // Allow Blazor app.
                        builder.WithOrigins("https://localhost:44301")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });*/
                option.AddDefaultPolicy(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    //.AllowCredentials()
                );
            });

            services.AddDbContext<Dal.DataAccess.FileHosterRepoContext>(options =>
                options.UseMySQL(Configuration.GetConnectionString(_connectionStringName))
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
                    IssuerSigningKey = (services.Where(x=>x.Lifetime==ServiceLifetime.Singleton && x.ServiceType.Name == nameof(IJwtAuthenticationManager)).FirstOrDefault().ImplementationInstance as JwtAuthenticationManager).GetSymmetricSecurityKey(),

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
            Common.Util.Trace("Environment in Api.Startup.Configure(): " + env.EnvironmentName);

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProCode.FileHosterRepo.Api v1"));
            }

            app.UseHttpsRedirection();  // Redirect from HTTP to HTTPS (307)

            app.UseRouting();

            // CORS has to be after UseRouting, and before UseAuthorization.
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
