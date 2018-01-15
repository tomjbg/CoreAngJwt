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
            //  BANCO DBCONTEXT
            // Adicionando o DBContext e Definindo o tipo de Banco de dados SQL Lite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("core_ang_jwt_DB")));
            
            //  IDENTITY
            // Adicionando o Identity e definindo que vai usar provedor de token padrão
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //  SERVIÇOS TRANSITÓRIOS
            // Add application services. Adicionando um serviço do tipo Transitório
            services.AddTransient<IEmailSender, EmailSender>();

            //  FILTERS OPTIONS
            // Adicionando o Serviço, a funcionalidade MVC e definindo que o retorno não será do tipo Xml e 
            //que nas requisições é exigido um Token JwtBearer e usuário autenticado.
            services.AddMvc(opt => {
                opt.OutputFormatters.Remove(new XmlDataContractSerializerOutputFormatter());

                var policy = new AuthorizationPolicyBuilder()
                             .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                             .RequireAuthenticatedUser()
                             .Build();

                opt.Filters.Add(new AuthorizeFilter(policy));

            });

            //  AUTHORIZATION
            // Definindo regras de Autorização

            services.AddAuthorization(opt => {
                    opt.AddPolicy("PodeLer", policy => policy.RequireClaim("Autonomia", "Ler"));
                    opt.AddPolicy("PodeModificar", policy => policy.RequireClaim("Autonomia", "Modificar"));
                    opt.AddPolicy("PodeTudo", policy => policy.RequireClaim("Autonomia", "Total"));
            });

            //  AUTHENTICATION

            //  TOKEN
            // JWT BEARER TOKEN
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtTokenOptions));

            services.Configure<JwtTokenOptions>(opt => {
                opt.Issuer = jwtAppSettingOptions[nameof(JwtTokenOptions.Issuer)];
                opt.Audience = jwtAppSettingOptions[nameof(JwtTokenOptions.Audience)];
                opt.signingCredentials = new SigningCredentials(_signingkey, SecurityAlgorithms.HmacSha256);
            });

            // Parametros do Token, definições
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, // Sim, valide o Emissor
                ValidIssuer = jwtAppSettingOptions[nameof(JwtTokenOptions.Issuer)], // Atribuindo quem é o Emissor deste Toker

                ValidateAudience = true, // Sim, valide o endereço o site Emissor deste Token
                ValidAudience = jwtAppSettingOptions[nameof(JwtTokenOptions.Audience)], // Atribuindo quem é o endereço o site Emissor deste Token

                ValidateIssuerSigningKey = true, // Sim, valide a Assinatura do Emissor
                IssuerSigningKey = _signingkey, // Atribuindo a Assinatura para ser validado

                RequireExpirationTime = true, // Sim, requer um Tempo de Expiração
                ValidateLifetime = true, // Sim, Valide Tempo de Vida
                
                ClockSkew = TimeSpan.Zero // Estabelecendo o Marco Zero do Relógio, no caso inicia no Zero
            };

            // Adicionando o Serviço Autenticação, definindo o tipo de autenticação como JwtBearer Token e 
            // aplicando os parametros de validação do token
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(opt => { 
                        opt.TokenValidationParameters = tokenValidationParameters;
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


            app.Use(async (context , next) => {
               await next();
               if(context.Response.StatusCode == 404 &&
                  !Path.HasExtension(context.Request.Path.Value) && 
                  !context.Request.Path.Value.StartsWith("/api/")) {
                      context.Request.Path = "/index.html";
                      await next();
               }
            });


            // CORS -> Cross-origin resource sharing = Compartilhamento de Recursos de Origem Cruzada
            // Determinamos por exemplo quais domínios (nomesite.dominio.com) podem fazer Request para nosso site
            app.UseCors(c => { 
                c.AllowAnyHeader(); // Permitir qualquer Header
                c.AllowAnyMethod(); // Permitir qualquer Método
                c.AllowAnyOrigin(); // Permitir qualquer Origem
            });

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
