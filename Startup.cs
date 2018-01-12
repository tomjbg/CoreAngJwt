using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreAngJwt.Data;
using CoreAngJwt.Models;
using CoreAngJwt.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO;

namespace CoreAngJwt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private const string SecretKey = "Secret@SenhaToken"; // Senha para assinar o Token, no formato String
        private readonly SymmetricSecurityKey _signingkey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)); // Transformando senha em SymmetricSecuritykey


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("core_ang_jwt_DB")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc(opt => {
                opt.OutputFormatters.Remove(new XmlDataContractSerializerOutputFormatter());

                var policy = new AuthorizationPolicyBuilder()
                             .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                             .RequireAuthenticatedUser()
                             .Build();

                opt.Filters.Add(new AuthorizeFilter(policy));

            });



            services.AddAuthorization(opt => {
                    opt.AddPolicy("PodeLer", policy => policy.RequireClaim("Autonomia", "Ler"));
                    opt.AddPolicy("PodeModificar", policy => policy.RequireClaim("Autonomia", "Modificar"));
                    opt.AddPolicy("PodeTudo", policy => policy.RequireClaim("Autonomia", "Total"));
            });

            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtTokenOptions));

            services.Configure<JwtTokenOptions>(opt => {
                opt.Issuer = jwtAppSettingOptions[nameof(JwtTokenOptions.Issuer)];
                opt.Audience = jwtAppSettingOptions[nameof(JwtTokenOptions.Audience)];
                opt.signingCredentials = new SigningCredentials(_signingkey, SecurityAlgorithms.HmacSha256);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Use(async(context,next) => {
              await next();
              if (context.Response.StatusCode == 404 &&
                  !Path.HasExtension(context.Request.Path.Value) &&
                  !context.Request.Path.Value.StartsWith("/api/"))
              {
                  context.Request.Path = "/index.html";
                  await next();
              }


            });

            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtTokenOptions));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtTokenOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtTokenOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingkey,

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseMvcWithDefaultRoute();

            app.UseAuthentication();

            // app.UseMvc(routes =>
            // {
            //     routes.MapRoute(
            //         name: "default",
            //         template: "{controller=Home}/{action=Index}/{id?}");
            // });
        }
    }
}
