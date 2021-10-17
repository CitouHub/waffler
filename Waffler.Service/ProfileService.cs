using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Waffler.Data;

namespace Waffler.Service
{
    public interface IProfileService
    {
        Task CreateProfileAsync(string password);

        Task<bool> IsPasswordValid(string password);

        Task<bool> SetApiKey(string password, string apiKey);

        Task<string> GetApiKey();
    }

    public class ProfileService : IProfileService
    {
        private readonly WafflerDbContext _context;

        public ProfileService(WafflerDbContext context)
        {
            _context = context;
        }

        public async Task CreateProfileAsync(string password)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            BCrypt.Net.BCrypt.Verify(password, passwordHash);

            await _context.WafflerProfile.AddAsync(new WafflerProfile()
            {
                InsertByUser = 1,
                InsertDate = DateTime.UtcNow,
                Password = passwordHash
            });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsPasswordValid(string password)
        {
            var passwordHash = (await _context.WafflerProfile.FirstOrDefaultAsync())?.Password;
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public async Task<bool> SetApiKey(string password, string apiKey)
        {
            var profile = await _context.WafflerProfile.FirstOrDefaultAsync();
            if(profile != null && BCrypt.Net.BCrypt.Verify(password, profile.Password))
            {
                profile.ApiKey = apiKey;
                profile.UpdateByUser = 1;
                profile.UpdateDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<string> GetApiKey()
        {
            return (await _context.WafflerProfile.FirstOrDefaultAsync())?.ApiKey;
        }
    }
}
