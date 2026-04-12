using FileSynchronizer.Abstracts;
using FileSynchronizer.Abstracts.Backups.Synchronization;
using FileSynchronizer.Abstracts.Handlers;
using FileSynchronizer.Abstracts.Jobs;
using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.Configuration;
using FileSynchronizer.Handlers;
using FileSynchronizer.Registries;
using FileSynchronizer.Services;
using FileSynchronizer.Services.Backups.Synchronization;
using FileSynchronizer.Services.Jobs;
using FileSynchronizer.Services.Synchronization;

namespace FileSynchronizer.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostBuilder ConfigureServices(this IHostBuilder builder, ApplicationOptions options) =>
        builder.ConfigureServices(configureDelegate: (_, services) =>
        {
            services.AddSingleton(options);
            services.AddSingleton<IApplicationOrchestrator, ApplicationOrchestrator>();

            services.AddSingleton<IMetadataSynchronizationManager, MetadataSynchronizatonManager>();
            services.AddSingleton<IHashSynchronizationManager, HashSynchronizationManager>();
            services.AddSingleton<IFullBackupCreationManager, FullBackupCreationManager>();
            services.AddSingleton<IFilesUpdateManager, FilesUpdateManager>();

            services.AddSingleton<ISynchronizationJobManager, SynchronizationJobManager>();

            ConfigureRegistries(builder);
            ConfigureHandlers(builder);
        });

    public static IHostBuilder ConfigureHandlers(this IHostBuilder builder) =>
        builder.ConfigureServices(configureDelegate: (_, services) =>
        {
            services.AddSingleton<ISynchronizationJobHandler, SynchronizationJobHandler>();
        });

    public static IHostBuilder ConfigureRegistries(this IHostBuilder builder) =>
        builder.ConfigureServices(configureDelegate: (_, services) =>
        {
            services.AddSingleton<IFileDataCacheRegistry, FileDataCacheRegistry>();
        });
}
