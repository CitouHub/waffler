using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waffler.Data;
using Waffler.Service.Background;
using Waffler.Service.Infrastructure;
using Waffler.Test.Helper;
using Xunit;

namespace Waffler.Test.Service.Background
{
    public class BackgroundDatabaseTuneServiceTest
    {
        private readonly ILogger<BackgroundDatabaseTuneService> _logger = Substitute.For<ILogger<BackgroundDatabaseTuneService>>();
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly IDatabaseSetupSignal _databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();

        public BackgroundDatabaseTuneServiceTest()
        {
            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);

            _serviceScope.ServiceProvider.GetService<WafflerDbContext>().Returns(DatabaseHelper.GetContext());
            _serviceScope.ServiceProvider.GetRequiredService<WafflerDbContext>().Returns(DatabaseHelper.GetContext());
        }

        [Theory]
        [InlineData("02:00", "2022-01-01 02:00:00", 24 * 60 * 60 * 1000)]
        [InlineData("02:00", "2022-01-01 02:00:01", 24 * 60 * 60 * 1000 - 1000)]
        [InlineData("02:00", "2022-01-01 01:59:59", 1000)]
        public void GetNextTriggerTime(string triggerTimeString, DateTime currentTime, long expectedNextTriggerMilliseconds)
        {
            //Setup
            var settings = new Dictionary<string, string> {
                {"Database:Service:IndexFragmentationAnalasys:TriggerTime", triggerTimeString}
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var backgroundDatabaseTuneService = new BackgroundDatabaseTuneService(_logger, configuration, _serviceProvider, _databaseSetupSignal);

            //Act
            var nextTriggerMilliseconds = backgroundDatabaseTuneService.GetNextTriggerTime(currentTime);

            //Assert
            Assert.Equal(expectedNextTriggerMilliseconds, nextTriggerMilliseconds);
        }
    }
}
