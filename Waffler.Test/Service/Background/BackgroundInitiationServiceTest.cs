using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using AutoMapper;
using NSubstitute;

using Waffler.Domain;
using Waffler.Service.Background;
using Waffler.Service.Infrastructure;
using Waffler.Service;

namespace Waffler.Test.Service.Background
{
    public class BackgroundInitiationServiceTest
    {
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly IDatabaseSetupSignal _databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();
        private readonly IConfigCache _configCache = Substitute.For<IConfigCache>();
        private readonly IProfileService _profileService = Substitute.For<IProfileService>();
        private readonly BackgroundInitiationService _backgroundInitiationService;

        public BackgroundInitiationServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundInitiationService>>();

            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            var _mapper = mapperConfig.CreateMapper();

            _serviceScope.ServiceProvider.GetService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetRequiredService<IProfileService>().Returns(_profileService);

            _backgroundInitiationService = new BackgroundInitiationService(logger, _serviceProvider, _databaseSetupSignal, _configCache);
        }

        [Fact]
        public async Task FetchCandleStickData_NoProfile()
        {
            //Setup
            var apiKey = "TestKey";
            _profileService.GetBitpandaApiKeyAsync().Returns(apiKey);

           //Act
           await _backgroundInitiationService.SetupConfigCache();

            //Asert
            _ = _profileService.Received().GetBitpandaApiKeyAsync();
            _configCache.Received().SetApiKey(Arg.Is<string>(_ => _ == apiKey));
        }
    }
}
