using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Waffler.Domain;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IBitpandaService _bitpandaService;

        public ProfileController(IProfileService profileService, IBitpandaService bitpandaService)
        {
            _profileService = profileService;
            _bitpandaService = bitpandaService;
        }

        [HttpGet]
        [Route("exists")]
        public async Task<bool> HasProfile()
        {
            return await _profileService.HasProfile();
        }

        [HttpPost]
        public async Task<bool> CreateProfile([FromBody]ProfileDTO newProfile)
        {
            return await _profileService.CreateProfileAsync(newProfile);
        }

        [HttpPost]
        [Route("password/verify")]
        public async Task<bool> VerifyPassword([FromBody]ProfileDTO profile)
        {
            return await _profileService.IsPasswordValid(profile.Password);
        }

        [HttpPut]
        [Route("bitpanda/apikey/{apiKey}")]
        public async Task<bool> SetBitpandaApiKey(string apiKey)
        {
            return await _profileService.SetBitpandaApiKey(apiKey);
        }

        [HttpGet]
        [Route("bitpanda/balance")]
        public async Task<List<BalanceDTO>> GetBalance()
        {
            return await _profileService.GetBalanceAsync();
        }
    }
}
