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

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
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
        public async Task<bool> VerifyPassword(string password)
        {
            return await _profileService.IsPasswordValid(password);
        }

        [HttpPut]
        [Route("bitpanda/apikey/{apiKey}")]
        public async Task<bool> SetBitpandaApiKey(string apiKey)
        {
            return await _profileService.SetBitpandaApiKey(apiKey);
        }
    }
}
