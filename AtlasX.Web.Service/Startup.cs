using AtlasX.Engine.AgsResourceProxy;
using AtlasX.Engine.AgsResourceProxy.Services;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.RemoteDirectory.Services;
using AtlasX.Web.Service.Core;
using AtlasX.Web.Service.Mail.Services;
using AtlasX.Web.Service.Notification.Services;
using AtlasX.Web.Service.OAuth.Repositories;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using AtlasX.Web.Service.OAuth.Services;
using AtlasX.Web.Service.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using Telerik.Reporting.Cache.File;
using Telerik.Reporting.Services;

namespace AtlasX.Web.Service;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    private readonly string _allowSpecificOrigins = "_AtlasXWebServiceAllowSpecificOrigins";

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(_allowSpecificOrigins, builder =>
            {
                Configuration.GetSection("WebServiceSettings:CorsPolicy").GetChildren().ToList().ForEach(corsPolicy =>
                {
                    builder.WithOrigins(corsPolicy.Value)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowedToAllowWildcardSubdomains();
                });
            });
        });

        IdentityModelEventSource.ShowPII = true;

        services.AddControllersWithViews().AddNewtonsoftJson();

        // If using Kestrel:
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            options.AddServerHeader = false;
        });

        // If using IIS:
        services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            // Relate with "maxAllowedContentLength" in web.config file.
            // The default value is 30000000, which is approximately 28.6MB.
            // See. https://docs.microsoft.com/en-us/iis/configuration/system.webServer/security/requestFiltering/requestLimits/#configuration
            options.MaxRequestBodySize = null;
        });

        // Register the Swagger services
        services.AddSwaggerDocument(configure => { configure.Title = "Web Service API"; });

        // Register services - Application settings
        services.Configure<AppSettings>(Configuration.GetSection("WebServiceSettings"));

        // Add database context.
        services.AddDbDataAccess(Configuration, "WebServiceSettings:Database");
        // Add file server service.
        services.AddDirectoryAccess(Configuration, "WebServiceSettings:FileServer");

        // Register services
        services.AddTransient<IAppAuthenService, AppAuthenService>();
        services.AddSingleton<INotificationService, NotificationServiceFcmLegacyHttp>();
        services.AddSingleton<IUserTokenRepository, UserTokenRepository>();
        services.AddSingleton<IAuthorizationCodeRepository, AuthorizationCodeInMemoryRepository>();

        /*
         Register repository services with the following:
         - UserInfoFakeRepository: You can mockup user in your code and use any password for all user when logging in.
           This method is suitable for you, if you don't have data source authentication system (e.g., LDAP, AD, Database).
         - UserInfoRepository: Authentication with database system.
         - UserInfoLdapRepository: Authentication with LDAP.
         - UserInfoMultiSourceRepository: If you would like more than one data source, you can use this to solve requirement.
           You want authentication with LDAP first, if fails, authentication with database next, you can use this.
           See more detail in https://atlasx.cdg.co.th/docs/frameworks/axws/latest/services/app-authen
        */
        services.AddSingleton<IUserInfoRepository, UserInfoRepository>();

        // OAuth2: Resource Server
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.UseSecurityTokenValidators = true;
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.Audience = Configuration.GetValue<string>("WebServiceSettings:OAuth:Issuer");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            Configuration.GetValue<string>("WebServiceSettings:OAuth:SecretKey"))),
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero
                };
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                options => { options.LoginPath = "/AppLogin/"; });

        // Setting up mail service.
        services.AddSingleton<IAppMailService, AppMailService>();

        // Configure dependencies for ReportsController.
        string tempDataReport = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Telerik Reporting Temp");
        Log.Information($"Telerik Reporting Temp Data: {tempDataReport}");
        services.TryAddSingleton<IReportServiceConfiguration>(sp =>
            new ReportServiceConfiguration
            {
                ReportingEngineConfiguration = sp.GetService<IConfiguration>(),
                HostAppId = "AtlasXWebService",
                Storage = new FileStorage(tempDataReport),
                ReportSourceResolver = new UriReportSourceResolver(
                    Path.Join(
                        sp.GetService<IWebHostEnvironment>().ContentRootPath,
                        "Report",
                        "Templates"
                    )
                )
            });

        // Setting up proxy config.
        services.AddAgsProxyServer();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // The base path of application (If running application behind reverse proxy, define in environment variable).
        string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_BASEPATH");
        if (!string.IsNullOrEmpty(basePath))
        {
            app.Use(async (context, next) =>
            {
                context.Request.PathBase = basePath;
                await next.Invoke();
            });
        }

        // app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(_allowSpecificOrigins);

        app.UseAuthentication();
        app.UseAuthorization();

        // Register the Swagger generator and the Swagger UI middlewares
        app.UseOpenApi();
        app.UseSwaggerUi3();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        // Setting up proxy page.
        app.UseWhen(context =>
        {
            return context.Request.Path.Value.ToLower().StartsWith("/api/AppProxy", StringComparison.OrdinalIgnoreCase);
            //&& context.User.Identity.IsAuthenticated; // Add this back in to keep unauthenticated users from utilzing the proxy.
        }, builder => builder.UseAgsProxyServer(
            app.ApplicationServices.GetService<IProxyConfigService>(),
            app.ApplicationServices.GetService<IProxyService>(),
            app.ApplicationServices.GetService<IMemoryCache>()
        ));
    }
}