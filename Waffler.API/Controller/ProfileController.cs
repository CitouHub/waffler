using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Waffler.Domain;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IProfileService _profileService;

        public ProfileController(IConfiguration configuration, IProfileService profileService)
        {
            _configuration = configuration;
            _profileService = profileService;
        }

        [HttpGet]
        [Route("exists")]
        public async Task<bool> HasProfileAsync()
        {
            return await _profileService.HasProfileAsync();
        }

        [HttpPost]
        public async Task<bool> CreateProfileAsync([FromBody]ProfileDTO newProfile)
        {
            newProfile.CandleStickSyncOffsetDays = _configuration.GetValue<int>("Profile:DefaultCandleStickSyncOffsetDays");
            return await _profileService.CreateProfileAsync(newProfile);
        }

        [HttpPost]
        [Route("password/verify")]
        public async Task<bool> VerifyPasswordAsync([FromBody]ProfileDTO profile)
        {
            return await _profileService.IsPasswordValidAsync(profile.Password);
        }

        [HttpPut]
        [Route("")]
        public async Task<bool> UpdateProfileAsync([FromBody]ProfileDTO profile)
        {
            return await _profileService.UpdateProfileAsync(profile);
        }

        [HttpGet]
        [Route("balance")]
        public async Task<List<BalanceDTO>> GetBalanceAsync()
        {
            return await _profileService.GetBalanceAsync();
        }
    }
}
