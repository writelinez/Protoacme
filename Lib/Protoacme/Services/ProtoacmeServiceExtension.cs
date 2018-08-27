using Microsoft.Extensions.DependencyInjection;
using Protoacme.Core;
using Protoacme.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Services
{
    public static class ProtoacmeServiceExtension
    {
        public static IServiceCollection AddProtoacme(this IServiceCollection services, Action<ProtoacmeOptions> options = null)
        {
            ProtoacmeOptions opt = new ProtoacmeOptions();
            if (options != null)
                options(opt);

            if (opt.UseStaging)
            {
                IAcmeRestApi api = new AcmeRestApi(ProtoacmeContants.LETSENCRYPT_STAGING_ENDPOINT);
                services.AddTransient<ProtoacmeClient>(srv => new ProtoacmeClient(api));
            }
            else
            {
                services.AddTransient<ProtoacmeClient>(srv => new ProtoacmeClient());
            }

            return services;
        }
    }

    public class ProtoacmeOptions
    {
        public bool UseStaging { get; set; } = false;
    }
}
