using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options => {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            services.AddDbContext<WafflerDbContext>(options =>
                options.UseSqlServer(Configuration["Database:ConnectionString"]));
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddHttpClient("Bitpanda", _ =>
            {
                _.BaseAddress = new Uri(Configuration["Bitpanda:BaseUri"]);
                _.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddScoped<IBitpandaService, BitpandaService>();
            services.AddScoped<ITradeOrderService, TradeOrderService>();
            services.AddScoped<ITradeRuleService, TradeRuleService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ICandleStickService, CandleStickService>();
            services.AddScoped<ITradeService, TradeService>();

            services.AddHostedService<BackgroundChartSyncService>();
            services.AddHostedService<BackgroundTradeService>();
            services.AddHostedService<BackgroundTestTradeService>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });

            services.AddSingleton(mapperConfig.CreateMapper());
            services.AddSingleton(new Cache());
            services.AddSingleton(new TestTradeRuleQueue());

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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
