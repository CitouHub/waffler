using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Service
{
    public interface IProfileService
    {
        Task<bool> HasProfileAsync();

        Task<bool> CreateProfileAsync(ProfileDTO profile);

        Task<ProfileDTO> GetProfileAsync();

        Task<bool> IsPasswordValidAsync(string password);

        Task<bool> SetPasswordAsync(string newPassword);

        Task<bool> UpdateProfileAsync(ProfileDTO profile);

        Task<string> GetBitpandaApiKeyAsync();

        Task<List<BalanceDTO>> GetBalanceAsync();
    }

    public class ProfileService : IProfileService
    {
        private readonly ILogger<ProfileService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBitpandaService _bitpandaService;

        public ProfileService(ILogger<ProfileService> logger, WafflerDbContext context, IMapper mapper, IBitpandaService bitpandaService)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _bitpandaService = bitpandaService;
            _logger.LogDebug("ProfileService instantiated");
        }

        public string GetHashedPassword(string password)
        {
            if(string.IsNullOrEmpty(password))
            {
                return "INVALID PASSWORD";
            }

            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public async Task<bool> HasProfileAsync()
        {
            return await _context.WafflerProfiles.FirstOrDefaultAsync() != null;
        }

        public async Task<bool> CreateProfileAsync(ProfileDTO profile)
        {
            if (await HasProfileAsync() == false)
            {
                profile.Password = GetHashedPassword(profile.Password);
                var wafflerProfile = _mapper.Map<WafflerProfile>(profile);
                wafflerProfile.InsertByUser = 1;
                wafflerProfile.InsertDate = DateTime.UtcNow;

                await _context.WafflerProfiles.AddAsync(wafflerProfile);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<ProfileDTO> GetProfileAsync()
        {
            var profile = await _context.WafflerProfiles.FirstOrDefaultAsync();
            return _mapper.Map<ProfileDTO>(profile);
        }

        public async Task<bool> IsPasswordValidAsync(string password)
        {
            try
            {
                var passwordHash = (await _context.WafflerProfiles.FirstOrDefaultAsync())?.Password;
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch { }

            return false;
        }

        public async Task<bool> SetPasswordAsync(string newPassword)
        {
            var profile = await _context.WafflerProfiles.FirstOrDefaultAsync();
            if (profile != null)
            {
                profile.Password = GetHashedPassword(newPassword);
                profile.UpdateByUser = 1;
                profile.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateProfileAsync(ProfileDTO profileDto)
        {
            var profile = await _context.WafflerProfiles.FirstOrDefaultAsync();
            if (profile != null)
            {
                profileDto.Password = profile.Password; //Keep password
                _mapper.Map(profileDto, profile);
                profile.UpdateByUser = 1;
                profile.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<string> GetBitpandaApiKeyAsync()
        {
            return (await _context.WafflerProfiles.FirstOrDefaultAsync())?.ApiKey;
        }

        public async Task<List<BalanceDTO>> GetBalanceAsync()
        {
            var pb_account = await _bitpandaService.GetAccountAsync();
            return _mapper.Map<List<BalanceDTO>>(pb_account?.Balances);
        }
    }
}
