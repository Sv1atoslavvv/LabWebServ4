using EmailSenderWeb.Lifetimes;
using EmailSenderWeb.Logging;
using EmailSenderWeb.Services;

namespace EmailSenderWeb;

public sealed class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddProvider(new FileLoggerProvider(
                Path.Combine(_environment.ContentRootPath, "Logs")));
        });

        services.AddControllersWithViews();
        services.Configure<EmailSenderOptions>(
            _configuration.GetSection(EmailSenderOptions.SectionName));

        // EmailSender is stateless, so Transient creates a fresh instance when requested.
        services.AddTransient<IEmailSender, EmailSender>();

        // Demonstration of DI lifetimes required by the assignment.
        services.AddTransient<TransientProbe>();
        services.AddScoped<ScopedProbe>();
        services.AddSingleton<SingletonProbe>();
    }

    public void Configure(WebApplication app, IWebHostEnvironment environment)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();

        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // Files are served only through FilesController, which validates the path
        // and chooses safe inline types. Do not expose uploaded files as static content.
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/Files", out var remainder))
            {
                var route = remainder.Value?.Trim('/');
                var isFilesPage = string.IsNullOrEmpty(route)
                    || string.Equals(route, "Details", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(route, "Upload", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(route, "Preview", StringComparison.OrdinalIgnoreCase);

                if (!isFilesPage)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }

            await next();
        });

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    }
}
