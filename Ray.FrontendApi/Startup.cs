using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Ray.Dtos.Configuration;
using Ray.Managers.Implementation;
using Ray.Model.NewContext;
using Ray.Repositories.Implementation;
using Microsoft.AspNetCore.Mvc;
using Ray.Model.NewContext.Entities;
using Microsoft.AspNetCore.Identity;
using Ray.FrontendApi.CustomTokenProviders;
using System;
using Microsoft.AspNetCore.DataProtection;
using Ray.Utils.Mail;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ray.Managers;
using Microsoft.AspNetCore.Authorization;
using Ray.FrontendApi.Attributes;
using Newtonsoft.Json.Serialization;
//using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

namespace Ray.FrontendApi
{
    public class Startup
    {
        private const string protectorName = "rayMedia";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ModelContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                    .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

            services.AddMemoryCache();
            services.AddResponseCaching();
            services.AddIdentity<User, Role>(op =>
            {
                //op.SignIn.RequireConfirmedEmail = true;
                op.Tokens.PasswordResetTokenProvider = "ResetPassword";
                op.Tokens.EmailConfirmationTokenProvider = protectorName;
                //op.Stores.MaxLengthForKeys = 128,

            })
                .AddEntityFrameworkStores<ModelContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<InvitationTokenProvider<User>>("Invitation")
                .AddTokenProvider<ResetPasswordTokenProvider<User>>("ResetPassword");


            services.AddDataProtection()
                .SetApplicationName(protectorName);


            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(4);
                options.Tokens.EmailConfirmationTokenProvider = protectorName;
                options.User.RequireUniqueEmail = false; //Allows creating users with only facebookId or TwitterId
            });

            services.AddLazyCache();

            services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                
            })
             .AddNewtonsoftJson(options =>
             {
                 options.SerializerSettings.ContractResolver = new DefaultContractResolver();
             })
            .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddHttpClient();

            services.Configure<AppSettings>(Configuration.GetSection("appSettings"));
            var config = new AppSettings();
            Configuration.Bind("appSettings", config);
            services.AddSingleton(config);

            services.AddManagers();
            services.AddScoped<IAuthorizationHandler, HasPermissionsHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasPermissionPolicy", policy =>
                    policy.Requirements.Add(new HasPermissionRequirement(CustomAuthorizationType.HasAccessToControllerAndAction)));
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddCookie()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = config.Jwt.Issuer,
                    ValidIssuer = config.Jwt.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.Jwt.Key))
                };
            });
            services
                .AddCors(options =>
                {
                    var allowedOrigins = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                    options.AddPolicy("CorsPolicy",
                        builder =>
                        {
                            if (allowedOrigins.Length == 1 && allowedOrigins[0] == "*")
                            {
                                builder.AllowAnyOrigin();
                            }
                            else
                            {
                                builder.WithOrigins(allowedOrigins);
                            }

                            builder
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                });
            services.AddRepositories();
            services.AddScoped<IMailSender, MailSender>();
            services.AddScoped<IFrontEndUserManager, FrontEndUserManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c =>
                //{
                //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ray.BackendApi v1");
                //    c.RoutePrefix = string.Empty; // Esto hace que Swagger est� disponible en la ra�z (http://localhost:<puerto>/)
                //});
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseResponseCaching();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
