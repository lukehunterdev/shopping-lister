using HealthChecks.UI.Client;
using LhDev.ShoppingLister.Middleware;
using LhDev.ShoppingLister.SettingsModels;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.Reflection;
using LhDev.ShoppingLister.Services;
using LhDev.ShoppingLister.Exceptions;
using LhDev.ShoppingLister.HealthChecks;

namespace LhDev.ShoppingLister;

public class Program
{
    public const string CookieSession = "SessionId";
    public const string CookieLoginError = "LoginError";
    public const string ListEditError = "ListEditError";

    public static JwtSettings JwtSettings { get; private set; } = null!;

    public static int Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            ReadSettings(builder);
            DbManager.InitDatabase();
            AddServices(builder.Services, builder.Configuration);
            AddWeb(builder.Services, builder.Configuration);
            SwaggerGenerator(builder.Services);
            AddHealthChecks(builder);

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.EnableTryItOutByDefault();
                    options.DisplayRequestDuration();
                    options.InjectStylesheet("/css/swagger-dark.css");
                });
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            MapHealthChecks(app);
            ConfigApp(app);

            app.Run();
        }
        catch (ShoppingListerException ex)
        {
            Console.WriteLine(ex);

            return ex.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was an unexpected exception reported.\n{ex}");

            return 100;
        }

        return 0;
    }

    private static void ReadSettings(WebApplicationBuilder builder)
    {
        
        
        JwtSettings = (JwtSettings)(builder.Configuration.GetSection("Jwt").Get(typeof(JwtSettings)) ?? 
                                    throw ShoppingListerException.NoJwtSettings());

    }

    private static void AddServices(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddCors();
        services.AddControllers();

        // Get sections from app settings file.
        //services.AddOptions();
        //services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        //services.Configure<DmsSettings>(builder.Configuration.GetSection("Dms"));

        // Doing this to ensure one instance of the list websocket service.
        var listWsService = new ListWsService();
        services.AddScoped<IListWsService, ListWsService>(_ => listWsService);

        // Set up dependency injection for services required
        services.AddScoped<IDbUserService, DbUserService>();
        services.AddScoped<IDbListService, DbListService>();
        services.AddScoped<IDbItemService, DbItemService>();
        services.AddScoped<IDbWorkingListService, DbWorkingListService>();
        services.AddScoped<IJwtService, JwtService>(_ => new JwtService(JwtSettings));
    }

    private static void AddWeb(IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddRazorPages();
    }

    private static void SwaggerGenerator(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping Lister", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.",
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }

    public static void AddHealthChecks(WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck<TestHealthCheck>("TestHealthCheck", tags: ["test"]);
    }

    public static void MapHealthChecks(WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/test", new HealthCheckOptions
        {
            Predicate = reg => reg.Tags.Contains("test"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        });
    }

    public static void ConfigApp(WebApplication app)
    {
        //app.ConfigureExceptionHandler();

        app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        // Use custom JWT middleware to ensure validation on controllers marked with JwtAuthoriseAttribute.
        app.UseMiddleware<JwtMiddleware>();

        // Use custom middle to catch exceptions and produce a standard response.
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseStaticFiles();
        app.MapRazorPages();
        app.MapControllers();
        app.UseWebSockets();
    }
}