﻿using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;

using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Javascript;

public class GeneratorRegistration : IGeneratorRegistration<JavascriptConfig>
{
    public void Register(IServiceCollection services, JavascriptConfig config, int number)
    {
        TrimSlashes(config, c => c.ApiClientFilePath);
        TrimSlashes(config, c => c.ApiClientRootPath);
        TrimSlashes(config, c => c.DomainPath);
        TrimSlashes(config, c => c.FetchPath);
        TrimSlashes(config, c => c.ModelRootPath);
        TrimSlashes(config, c => c.ResourceRootPath);

        if (config.ModelRootPath != null)
        {
            services.AddGenerator<TypescriptDefinitionGenerator, JavascriptConfig>(config, number);
            services.AddGenerator<TypescriptReferenceGenerator, JavascriptConfig>(config, number);

            if (config.ApiClientRootPath != null)
            {
                if (config.TargetFramework == TargetFramework.ANGULAR)
                {
                    services.AddGenerator<AngularApiClientGenerator, JavascriptConfig>(config, number);
                }
                else
                {
                    services.AddGenerator<JavascriptApiClientGenerator, JavascriptConfig>(config, number);
                }
            }
        }

        if (config.ResourceRootPath != null)
        {
            services.AddGenerator<JavascriptResourceGenerator, JavascriptConfig>(config, number);
        }
    }
}