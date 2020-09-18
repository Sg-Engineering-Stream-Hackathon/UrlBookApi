using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataService;
using DataService.Interfaces;
using DataService.Services;
using FunctionalService;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ModelService;
using Swashbuckle.Swagger;

namespace WebApiCatalog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddRazorViewEngine();
            services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("urlcatalogContext"), x => x.MigrationsAssembly("WebApiCatalog")));
            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });
            services.AddTransient<IAuthSvc, AuthSvc>();

            //Cookies
            services.AddHttpContextAccessor();
            services.AddTransient<CookieOptions>();
            services.AddTransient<ICookieSvc, CookieSvc>();

            //userservice
            services.AddTransient<IUserSvc, UserSvc>();

            //book mark service
            services.AddTransient<IBookmarkCardSvc, BookMarkCardSvc>();

            /*---------------------------------------------------------------------------------------------------*/
            /*                             Functional SERVICE                                                    */
            /*---------------------------------------------------------------------------------------------------*/
            services.AddTransient<IFunctionalSvc, FunctionalSvc>();
            services.Configure<AppUserOptions>(Configuration.GetSection("AppUserOptions"));

            var identityDefaultOptionsConfiguration = Configuration.GetSection("IdentityDefaultOptions");
            services.Configure<IdentityDefaultOptions>(identityDefaultOptionsConfiguration);
            var identityDefaultOptions = identityDefaultOptionsConfiguration.Get<IdentityDefaultOptions>();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = identityDefaultOptions.PasswordRequireDigit;
                options.Password.RequiredLength = identityDefaultOptions.PasswordRequiredLength;
                options.Password.RequireNonAlphanumeric = identityDefaultOptions.PasswordRequireNonAlphanumeric;
                options.Password.RequireUppercase = identityDefaultOptions.PasswordRequireUppercase;
                options.Password.RequireLowercase = identityDefaultOptions.PasswordRequireLowercase;
                options.Password.RequiredUniqueChars = identityDefaultOptions.PasswordRequiredUniqueChars;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes);
                options.Lockout.MaxFailedAccessAttempts = identityDefaultOptions.LockoutMaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = identityDefaultOptions.LockoutAllowedForNewUsers;

                // User settings
                options.User.RequireUniqueEmail = identityDefaultOptions.UserRequireUniqueEmail;

                // email confirmation require
                options.SignIn.RequireConfirmedEmail = identityDefaultOptions.SignInRequireConfirmedEmail;

            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            /*---------------------------------------------------------------------------------------------------*/
            /*                              ENABLE CORS                                                          */
            /*---------------------------------------------------------------------------------------------------*/
            services.AddCors(options => {
                options.AddPolicy("EnableCORS", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();
                });
            });
            /*---------------------------------------------------------------------------------------------------*/
            /*                              DATA PROTECTION SERVICE                                              */
            /*---------------------------------------------------------------------------------------------------*/
            var dataProtectionSection = Configuration.GetSection("DataProtectionKeys");
            services.Configure<DataProtectionKeys>(dataProtectionSection);
            services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();
            /*--------------------------------------------------------------------------------------------------------------------*/
            /*                      Anti Forgery Token Validation Service                                                         */
            /* We use the option patterm to configure the Antiforgery feature through the AntiForgeryOptions Class                */
            /* The HeaderName property is used to specify the name of the header through which antiforgery token will be accepted */
            /*--------------------------------------------------------------------------------------------------------------------*/
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
            });
            /*---------------------------------------------------------------------------------------------------*/
            /*                                 APPSETTINGS SERVICE                                               */
            /*---------------------------------------------------------------------------------------------------*/
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            /*---------------------------------------------------------------------------------------------------*/
            /*                                 JWT AUTHENTICATION SERVICE                                        */
            /*---------------------------------------------------------------------------------------------------*/
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(o => {
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = appSettings.ValidateIssuerSigningKey,
                    ValidateIssuer = appSettings.ValidateIssuer,
                    ValidateAudience = appSettings.ValidateAudience,
                    ValidIssuer = appSettings.Site,
                    ValidAudience = appSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero

                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAntiforgery antiforgery)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("EnableCORS");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            /* Configure the app to provide a token in a cookie called XSRF-TOKEN */
            /* Custom Middleware Component is required to Set the cookie which is named XSRF-TOKEN 
             * The Value for this cookie is obtained from IAntiForgery service
             * We must configure this cookie with HttpOnly option set to false so that browser will allow JS to read this cookie
             */
            app.Use(nextDelegate => context =>
            {
                string path = context.Request.Path.Value.ToLower();
                string[] directUrls = { "/admin", "/store", "/cart", "checkout", "/login" };
                if (path.StartsWith("/swagger") || path.StartsWith("/api") || string.Equals("/", path) || directUrls.Any(url => path.StartsWith(url)))
                {
                    AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(context);
                    context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions()
                    {
                        HttpOnly = false,
                        Secure = false,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict
                    });

                }

                return nextDelegate(context);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
