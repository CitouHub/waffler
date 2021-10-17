using System;
using System.Net.Http.Headers;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Waffler.Service;
using Waffler.Data;
using Waffler.Domain;
using Waffler.Common.Util;

[assembly: FunctionsStartup(typeof(Waffler.Function.Startup))]
namespace Waffler.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = GetConfig(builder);

            builder.Services.AddDbContext<WafflerDbContext>(options => options.UseSqlServer(config["Database:ConnectionString"]));
            builder.Services.AddDbContext<BaseDbContext>(options => options.UseSqlServer(config["Database:ConnectionString"]));

            builder.Services.AddScoped<IBitpandaService, BitpandaService>();
            builder.Services.AddScoped<ICandleStickService, CandleStickService>();
            builder.Services.AddScoped<ITradeRuleService, TradeRuleService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<ITradeOrderService, TradeOrderService>();

            builder.Services.AddHttpClient("Bitpanda", _ =>
            {
                _.BaseAddress = new Uri(config["Bitpanda:BaseUri"]);
                _.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });

            builder.Services.AddSingleton(mapperConfig.CreateMapper());
            builder.Services.AddSingleton(new Cache());
        }

        private IConfigurationRoot GetConfig(IFunctionsHostBuilder builder)
        {
            var executioncontextoptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;
            var currentDirectory = executioncontextoptions.AppDirectory;

            var config = new ConfigurationBuilder()
               .SetBasePath(currentDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();

            return config;
        }
    }
}
