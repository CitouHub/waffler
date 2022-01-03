using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Waffler.Service.Infrastructure;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;

        public StatusController(IConfiguration configuration, IDatabaseSetupSignal databaseSetupSignal)
        {
            _configuration = configuration;
            _databaseSetupSignal = databaseSetupSignal;
        }

        [HttpGet]
        [Route("database/await/online")]
        public async Task IsDatabaseOnlineAsync()
        {
            var server = _configuration.GetValue<string>("Database:Server");
            var database = _configuration.GetValue<string>("Database:Catalog");
            var credentials = _configuration.GetValue<string>("Database:Credentials");

            var connectionString = $"Server={server};Initial Catalog={database};{credentials}";

            await _databaseSetupSignal.AwaitDatabaseOnlineAsync(new SqlConnection(connectionString));
        }
    }
}
