using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Waffler.Domain.Bitpanda.Private.Balance;
using System.Collections.Generic;
using Waffler.Common;

#pragma warning disable IDE0017 // Simplify object initialization
namespace Waffler.Test.Service
{
    public class ProfileServiceTest
    {
        private readonly ILogger<ProfileService> _logger = Substitute.For<ILogger<ProfileService>>();
        private readonly IBitpandaService _bitpandaService = Substitute.For<IBitpandaService>();
        private readonly IMapper _mapper; 

        public ProfileServiceTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task HasProfile_False()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var hasProfile = await profileService.HasProfileAsync();

            //Assert
            Assert.False(hasProfile);
        }

        [Fact]
        public async Task HasProfile_True()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var hasProfile = await profileService.HasProfileAsync();

            //Assert
            Assert.True(hasProfile);
        }

        [Fact]
        public async Task CreateProfile_AlreadyExists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profile = ProfileHelper.GetProfile();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var success = await profileService.CreateProfileAsync(ProfileHelper.GetProfileDTO());

            //Assert
            Assert.False(success);
            Assert.Single(context.WafflerProfiles);
        }

        [Fact]
        public async Task CreateProfile_Created()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var success = await profileService.CreateProfileAsync(ProfileHelper.GetProfileDTO());

            //Assert
            Assert.True(success);
            Assert.NotNull(context.WafflerProfiles.FirstOrDefault());
        }

        [Fact]
        public async Task GetProfile_DontExists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var profile = await profileService.GetProfileAsync();

            //Assert
            Assert.Null(profile);
        }

        [Fact]
        public async Task GetProfile_Exists()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var profile = await profileService.GetProfileAsync();

            //Assert
            Assert.NotNull(profile);
        }

        [Theory]
        [InlineData("TEST123", "")]
        [InlineData("TEST123", null)]
        [InlineData("", "TEST123")]
        [InlineData(null, "TEST123")]
        [InlineData("TEST123", "test123")]
        [InlineData("TEST123", "TEST124")]
        [InlineData("TEST123", "TEST1234")]
        [InlineData("TEST123", "_TEST123")]
        public async Task IsPasswordValid_False(string password, string testPassword)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);
            var profile = ProfileHelper.GetProfile();
            profile.Password = profileService.GetHashedPassword(password);
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            
            //Act
            var valid = await profileService.IsPasswordValidAsync(testPassword);

            //Assert
            Assert.False(valid);
        }

        [Theory]
        [InlineData("TEST123", "TEST123")]
        public async Task IsPasswordValid_True(string password, string testPassword)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);
            var profile = ProfileHelper.GetProfile();
            profile.Password = profileService.GetHashedPassword(password);
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();

            //Act
            var valid = await profileService.IsPasswordValidAsync(testPassword);

            //Assert
            Assert.True(valid);
        }

        [Fact]
        public async Task SetPassword_NoProfile()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var newPassword = "TEST123";
            var success = await profileService.SetPasswordAsync(newPassword);

            //Assert
            var valid = await profileService.IsPasswordValidAsync(newPassword);
            Assert.False(success);
            Assert.False(valid);
        }

        [Fact]
        public async Task SetPassword_Updated()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            context.WafflerProfiles.Add(ProfileHelper.GetProfile());
            context.SaveChanges();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var newPassword = "TEST123";
            var success = await profileService.SetPasswordAsync(newPassword);

            //Assert
            var valid = await profileService.IsPasswordValidAsync(newPassword);
            Assert.True(success);
            Assert.True(valid);
        }

        [Fact]
        public async Task UpdateProfile_NoProfile()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var success = await profileService.UpdateProfileAsync(ProfileHelper.GetProfileDTO());

            //Assert
            Assert.False(success);
            Assert.Empty(context.WafflerProfiles);
        }

        [Fact]
        public async Task UpdateProfile_Updated()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profile = ProfileHelper.GetProfile();
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var profileDTO = _mapper.Map<ProfileDTO>(profile);
            profileDTO.ApiKey = Guid.NewGuid().ToString();
            var success = await profileService.UpdateProfileAsync(profileDTO);

            //Assert
            Assert.True(success);
            Assert.Single(context.WafflerProfiles);
            Assert.Equal(profileDTO.ApiKey, context.WafflerProfiles.FirstOrDefault().ApiKey);
        }

        [Fact]
        public async Task GetBitpandaApiKeyAsync_NoProfile()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var apiKey = await profileService.GetBitpandaApiKeyAsync();

            //Assert
            Assert.Null(apiKey);
        }

        [Fact]
        public async Task GetBitpandaApiKey_HasProfile()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profile = ProfileHelper.GetProfile();
            context.WafflerProfiles.Add(profile);
            context.SaveChanges();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);

            //Act
            var apiKey = await profileService.GetBitpandaApiKeyAsync();

            //Assert
            Assert.Equal(profile.ApiKey, apiKey);
        }

        [Fact]
        public async Task GetBalance_Null()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);
            _bitpandaService.GetAccountAsync().Returns((AccountDTO)null);

            //Act
            var balance = await profileService.GetBalanceAsync();

            //Assert
            Assert.Empty(balance);
        }

        [Fact]
        public async Task GetBalance_NoApiKey()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var profileService = new ProfileService(_logger, context, _mapper, _bitpandaService);
            _bitpandaService.GetAccountAsync().Returns(new AccountDTO());

            //Act
            var balance = await profileService.GetBalanceAsync();

            //Assert
            Assert.NotNull(balance);
            _ = _bitpandaService.Received().GetAccountAsync();
        }
    }
}
