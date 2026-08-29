using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ray.Dtos.Configuration;
using Ray.Managers.Implementation;
using Ray.Model.NewContext;
using Ray.Model.NewContext.Entities;
using Ray.Repositories.Implementation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Ray.BackendApi.CustomTokenProviders;
using Ray.Utils.Mail;
using LazyCache.Providers;
using Ray.BackendApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ray.Common.Authentication;
using Ray.Utils.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace Ray.BackendApi
{
    public class Startup
    {
        private const string protectorName = "rayMedia";
        private static System.Timers.Timer SyncLayoutInstancesTimer;
      
        public static AppSettings AppSettings;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ModelContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

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
            services.AddHttpContextAccessor();

            services.AddDataProtection()
                .SetApplicationName(protectorName);

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(4);
                options.Tokens.EmailConfirmationTokenProvider = protectorName;
            });
            services.AddLazyCache();
            services
                .AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                            .WithOrigins("*")
                            .AllowAnyHeader()
                            .AllowAnyMethod());
                });

            services.AddControllers()
                .AddNewtonsoftJson(opt => opt.SerializerSettings.ContractResolver = null)
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ray.BackendApi", Version = "v1" });
            });




            services.Configure<AppSettings>(Configuration.GetSection("appSettings"));
            services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
            });

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            var config = new AppSettings();
            Configuration.Bind("appSettings", config);

            AppSettings = config;

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
            services.AddRepositories();

            services.AddScoped<ICurrentUserService, UserService>();
            services.AddScoped<IMailSender, MailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ray.BackendApi v1"));
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            InitializeSyncLayoutInstancesTimer();
        }


        private static void InitializeSyncLayoutInstancesTimer()
        {
            SyncLayoutInstancesTimer = new System.Timers.Timer();
            SyncLayoutInstancesTimer.AutoReset = false;
            SyncLayoutInstancesTimer.Elapsed += Init;
            SyncLayoutInstancesTimer.Enabled = true;
            SyncLayoutInstancesTimer.Start();
        }

        private static async void Init(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(AppSettings.SyncLayout.Url))
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", AppSettings.SyncLayout.Token);
                        await client.PostAsync(AppSettings.SyncLayout.Url, null);
                    }
                }
            }
            catch (Exception ex)
            {
                CMSLogger.Error(ex.Message);
            }

            //check interval
            SyncLayoutInstancesTimer.Interval = new TimeSpan(0, AppSettings.SyncLayout.TimeInMinutes, 60).TotalMilliseconds;
        }
    }

}

