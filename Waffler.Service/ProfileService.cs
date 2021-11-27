using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBitpandaService _bitpandaService;

        public ProfileService(WafflerDbContext context, IMapper mapper, IBitpandaService bitpandaService)
        {
            _context = context;
            _mapper = mapper;
            _bitpandaService = bitpandaService;
        }

        private string GetHashedPassword(string password)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public async Task<bool> HasProfileAsync()
        {
            return await _context.WafflerProfile.FirstOrDefaultAsync() != null;
        }

        public async Task<bool> CreateProfileAsync(ProfileDTO profile)
        {
            if (await HasProfileAsync() == false)
            {
                profile.Password = GetHashedPassword(profile.Password);
                var wafflerProfile = _mapper.Map<WafflerProfile>(profile);
                wafflerProfile.InsertByUser = 1;
                wafflerProfile.InsertDate = DateTime.UtcNow;

                await _context.WafflerProfile.AddAsync(wafflerProfile);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<ProfileDTO> GetProfileAsync()
        {
            var profile = await _context.WafflerProfile.FirstOrDefaultAsync();
            return _mapper.Map<ProfileDTO>(profile);
        }

        public async Task<bool> IsPasswordValidAsync(string password)
        {
            var passwordHash = (await _context.WafflerProfile.FirstOrDefaultAsync())?.Password;
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<bool> SetPasswordAsync(string newPassword)
        {
            var profile = await _context.WafflerProfile.FirstOrDefaultAsync();
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
            var profile = await _context.WafflerProfile.FirstOrDefaultAsync();
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
            return (await _context.WafflerProfile.FirstOrDefaultAsync())?.ApiKey;
        }

        public async Task<List<BalanceDTO>> GetBalanceAsync()
        {
            var pb_account = await _bitpandaService.GetAccountAsync();
            return _mapper.Map<List<BalanceDTO>>(pb_account?.balances);
        }
    }
}
