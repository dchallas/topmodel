﻿using Microsoft.Extensions.DependencyInjection;
using TopModel.Core.Loaders;
using TopModel.Utils;

namespace TopModel.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddModelStore(this IServiceCollection services, FileChecker fileChecker, ModelConfig? config = null, string? rootDir = null)
    {
        services
            .AddMemoryCache()
            .AddSingleton(fileChecker)
            .AddSingleton<ClassLoader>()
            .AddSingleton<DecoratorLoader>()
            .AddSingleton<EndpointLoader>()
            .AddSingleton<PropertyLoader>()
            .AddSingleton<ModelFileLoader>()
            .AddSingleton<ModelConfig>()
            .AddSingleton<TranslationStore>()
            .AddSingleton<ModelStore>();

        if (config != null && rootDir != null)
        {
            config.ModelRoot ??= string.Empty;
            ModelUtils.CombinePath(rootDir, config, c => c.ModelRoot);
            if (config.Langs != null)
            {
                ModelUtils.CombinePath(rootDir, config.Langs, c => c.RootPath);
            }

            services.AddSingleton(config);
        }

        return services;
    }
}