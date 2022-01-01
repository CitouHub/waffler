using System;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using AutoMapper;

using Waffler.Common.Util;
using Waffler.Data;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Service.Background;
using Waffler.Service.Infrastructure;

namespace Waffler.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options => {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            var connectionString = GetDatabaseConnectionString();
            services.AddDbContext<WafflerDbContext>(options => options.UseSqlServer(connectionString));
            services.AddDbContext<BaseDbContext>(options => options.UseSqlServer(connectionString));
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddHttpClient("Bitpanda", _ =>
            {
                _.BaseAddress = new Uri(_configuration.GetValue<string>("Bitpanda:BaseUri"));
                _.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddScoped<IBitpandaService, BitpandaService>();
            services.AddScoped<ITradeOrderService, TradeOrderService>();
            services.AddScoped<ITradeRuleService, TradeRuleService>();
            services.AddScoped<ITradeRuleConditionService, TradeRuleConditionService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ICandleStickService, CandleStickService>();
            services.AddScoped<ITradeService, TradeService>();
            services.AddScoped<IStatisticsService, StatisticsService>();

            services.AddHostedService<BackgroundDatabaseMigrationService>();
            services.AddHostedService<BackgroundChartSyncService>();
            services.AddHostedService<BackgroundTradeService>();
            services.AddHostedService<BackgroundTestTradeService>();
            services.AddHostedService<BackgroundTradeOrderSyncService>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });

            services.AddSingleton(mapperConfig.CreateMapper());
            services.AddSingleton<ICache, Cache>();
            services.AddSingleton<ITradeRuleTestQueue, TradeRuleTestQueue>();
            services.AddSingleton<IDatabaseSetupSignal, DatabaseSetupSignal>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Waffler.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waffler.API v1"));
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private string GetDatabaseConnectionString()
        {
            var server = _configuration.GetValue<string>("Database:Server");
            var database = _configuration.GetValue<string>("Database:Catalog");
            var credentials = _configuration.GetValue<string>("Database:Credentials");
            return $"Server={server};Initial Catalog={database};{credentials}";
        }
    }
}
