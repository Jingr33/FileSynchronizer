using Hangfire;
using Hangfire.MemoryStorage;
using Newtonsoft.Json;

namespace FileSynchronizer.Extensions;

public static class ConfigureHangfireExtensions
{
    public static WebApplicationBuilder ConfigureHangfire(this WebApplicationBuilder app)
    {
        app.Services.AddHangfire(configuration =>
        {
            configuration
                .UseSerializerSettings(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                })
                .UseMemoryStorage();
        });

        app.Services.AddHangfireServer();

        return app;
    }
}
