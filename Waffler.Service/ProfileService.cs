﻿using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Service
{
    public interface IProfileService
    {
        Task<bool> HasProfile();

        Task<bool> CreateProfileAsync(ProfileDTO profile);

        Task<bool> IsPasswordValid(string password);

        Task<bool> SetBitpandaApiKey(string apiKey);

        Task<string> GetBitpandaApiKey();
        
    }

    public class ProfileService : IProfileService
    {
        private readonly WafflerDbContext _context;

        public ProfileService(WafflerDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasProfile()
        {
            return await _context.WafflerProfile.FirstOrDefaultAsync() != null;
        }

        public async Task<bool> CreateProfileAsync(ProfileDTO profile)
        {
            if(await HasProfile() == false)
            {
                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(profile.Password, salt);
                BCrypt.Net.BCrypt.Verify(profile.Password, passwordHash);

                await _context.WafflerProfile.AddAsync(new WafflerProfile()
                {
                    InsertByUser = 1,
                    InsertDate = DateTime.UtcNow,
                    Password = passwordHash
                });
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> IsPasswordValid(string password)
        {
            var passwordHash = (await _context.WafflerProfile.FirstOrDefaultAsync())?.Password;
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<bool> SetBitpandaApiKey(string apiKey)
        {
            var profile = await _context.WafflerProfile.FirstOrDefaultAsync();
            if(profile != null)
            {
                profile.ApiKey = apiKey;
                profile.UpdateByUser = 1;
                profile.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<string> GetBitpandaApiKey()
        {
            return (await _context.WafflerProfile.FirstOrDefaultAsync())?.ApiKey;
        }
    }
}
