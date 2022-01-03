using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Waffler.Service.Infrastructure;
using Waffler.Data;
using Waffler.Data.Extensions;

namespace Waffler.Service.Background
{
    public class BackgroundDatabaseTuneService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundDatabaseTuneService> _logger;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;

        private readonly int FragmentationLimit = 10;
        private readonly object StartUpLock = new object();
        private readonly DateTime TriggerTime;

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundDatabaseTuneService(
            ILogger<BackgroundDatabaseTuneService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IDatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;

            var time = configuration.GetValue<string>("Database:Service:IndexFragmentationAnalasys:TriggerTime").Split(':');
            TriggerTime = DateTime.UtcNow.Date.AddHours(int.Parse(time[0])).AddMinutes(int.Parse(time[1]));

            _logger.LogDebug("BackgroundDatabaseTuneService instantiated");
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if(!cancellationToken.IsCancellationRequested)
            {
                _timer = new Timer(async _ => await HandleDatabaseFragmentationAsync(cancellationToken), null, GetNextTriggerTime(DateTime.UtcNow), Timeout.Infinite);
            }

            return Task.CompletedTask;
        }

        public int GetNextTriggerTime(DateTime fromTime)
        {
            var nextTrigger = fromTime.Date.Add(TriggerTime.TimeOfDay);
            if(fromTime >= nextTrigger)
            {
                nextTrigger = nextTrigger.AddDays(1);
            }

            return (int)(nextTrigger - fromTime).TotalMilliseconds;
        }

        public async Task HandleDatabaseFragmentationAsync(CancellationToken cancellationToken)
        {
            lock(StartUpLock)
            {
                if (InProgress)
                {
                    return;
                }

                InProgress = true;
            }

            _logger.LogInformation($"Waiting for database to be ready");
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);
            try
            {
                _logger.LogInformation($"Running index fragmentation analasys");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _logger.LogInformation($"- Setting up scoped services");
                    var _context = scope.ServiceProvider.GetRequiredService<WafflerDbContext>();

                    var indexes = await _context.sp_getIndexFragmentation();
                    var fragmentedIndexes = indexes.Where(_ => _.Fragmentation > FragmentationLimit).ToList();
                    if(fragmentedIndexes.Count > 0)
                    {
                        _databaseSetupSignal.SetDatabaseBusy();
                        foreach (var index in indexes.Where(_ => _.Fragmentation > FragmentationLimit))
                        {
                            _logger.LogInformation($"- Rebuilding index {index.IndexName} on table {index.TableName}");
                            await _context.RebuildIndex(index.TableName, index.IndexName);
                            _logger.LogInformation($"- Reorganizing index {index.IndexName} on table {index.TableName}");
                            await _context.ReorganizeIndex(index.TableName, index.IndexName);
                        }
                        _databaseSetupSignal.SetDatabaseReady();
                    } 
                    else
                    {
                        _logger.LogInformation($"- No index fragmentation");
                    }
                }
                _logger.LogInformation($"fragmentation analasys finished");

                if (_timer != null)
                {
                    _timer.Change(GetNextTriggerTime(DateTime.UtcNow), Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
            InProgress = false;
        }
    }
}
