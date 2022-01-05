using System.Threading;

using Xunit;

using Waffler.API.Security;

namespace Waffler.Test.Util
{
    public class UserSessionTest
    {
        [Fact]
        public void IsValid_False()
        {
            //Setup
            UserSession.SessionValidSeconds = 0;
            UserSession.New();

            //Act
            var valid = UserSession.IsValid();

            //Assert
            Assert.False(valid);
        }

        [Fact]
        public void IsValid_True()
        {
            //Setup
            UserSession.SessionValidSeconds = 1;
            UserSession.New();

            //Act
            var valid = UserSession.IsValid();

            //Assert
            Assert.True(valid);
        }

        [Fact]
        public void Refresh_IsValid_False()
        {
            //Setup
            UserSession.SessionValidSeconds = 1;
            UserSession.New();
            Thread.Sleep(1000);
            UserSession.Refresh();

            //Act
            var valid = UserSession.IsValid();

            //Assert
            Assert.False(valid);
        }

        [Fact]
        public void Refresh_IsValid_True()
        {
            //Setup
            UserSession.SessionValidSeconds = 2;
            UserSession.New();
            Thread.Sleep(800);
            UserSession.Refresh();
            Thread.Sleep(800);

            //Act
            var valid = UserSession.IsValid();

            //Assert
            Assert.True(valid);
        }

        [Fact]
        public void New_IsValid_True()
        {
            //Setup
            UserSession.SessionValidSeconds = 0;
            UserSession.New();
            UserSession.SessionValidSeconds = 1;
            UserSession.New();

            //Act
            var valid = UserSession.IsValid();

            //Assert
            Assert.True(valid);
        }
    }
}
