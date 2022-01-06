using AutoMapper;
using Xunit;

using Waffler.Domain;
using Waffler.Test.Helper;

namespace Waffler.Test.Mapping
{
    public class ProfileMappingTest
    {
        private readonly IMapper _mapper;

        public ProfileMappingTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public void TradeRule_Manual()
        {
            //Setup
            var profile = ProfileHelper.GetProfile();
            profile.ApiKey = "This is my secret API key";

            //Act
            var profileDTO = _mapper.Map<ProfileDTO>(profile);

            //Assert
            Assert.NotEqual(profile.ApiKey, profileDTO.ApiKey);
            Assert.NotNull(profileDTO.ApiKey);
        }
    }
}