using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Collections.Generic;

namespace Waffler.Web.Controllers
{
    [Route("api/[controller]")]
    public class AppSettingsController : Controller
    {

        private readonly IConfiguration _configuration;

        public AppSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<string, string> GetPublicAppSettings()
        {
            var appSettings = new Dictionary<string, string>();

            appSettings.Add("API:Version", _configuration.GetValue<string>("API:Version"));
            appSettings.Add("API:BaseURL", _configuration.GetValue<string>("API:BaseURL"));

            return appSettings;
        }
    }
}
