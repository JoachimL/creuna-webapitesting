using System.Globalization;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using WebApiExampleApplication.Models;
using WebApiExampleApplication.Services;
using Creuna.WebApiTesting;

namespace WebApiExampleApplication.Controllers
{
    [TestFixture]
    class GamesControllerTests
    {
        private ApiControllerTestHelper _apiControllerHelper; 
        private Mock<IGameService> _gameServiceMock;
        private GamesController _controllerToTest;

        [SetUp]
        public void SetUp()
        {
            _gameServiceMock = new Mock<IGameService>();
            CreateAndPopulateController();
        }

        private void CreateAndPopulateController()
        {
            _controllerToTest = new GamesController(_gameServiceMock.Object);
            _apiControllerHelper = new ApiControllerTestHelper(_controllerToTest);
        }

        public class The_CreateNewGame_Method : GamesControllerTests
        {
            const string UserName = "User";
            [Test]
            public void Returns_Unauthorized_Status_If_User_Is_Not_Authorized()
            {
                // Arrange
                _gameServiceMock
                    .Setup(s => s.CreateNewGame(It.IsAny<string>(), It.IsAny<Game>()))
                    .Throws(new UnauthorizedAccessException("Only authorized users can create games."));

                // Act
                var response = CreateNewGameThroughControllerAndReturnResponse();

                // Assert
                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }

            [Test]
            public void Returns_InternalServerError_If_Nonspecific_Error_Occurs()
            {
                // Arrange
                _gameServiceMock
                    .Setup(s => s.CreateNewGame(It.IsAny<string>(), It.IsAny<Game>()))
                    .Throws(new Exception());

                // Act
                var response = CreateNewGameThroughControllerAndReturnResponse();

                // Assert
                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            }

            [Test]
            public void Returns_Created_When_Creation_Through_Service_Succeeds()
            {
                // Arrange
                _apiControllerHelper.SetLoggedOnUserNameTo(UserName);
                SetUpNewGameInService();

                // Act
                var response = CreateNewGameThroughControllerAndReturnResponse();

                // Assert
                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            }

            [Test]
            public void Returns_Uri_To_New_Game_When_Creation_Through_Service_Succeeds()
            {
                // Arrange
                _apiControllerHelper.SetLoggedOnUserNameTo(UserName);
                var newGameId = SetUpNewGameInService();
                var expectedResultUri = "http://localhost/Games/" + newGameId.ToString(CultureInfo.InvariantCulture);

                // Act
                var response = CreateNewGameThroughControllerAndReturnResponse();

                // Assert
                Assert.AreEqual(expectedResultUri, response.Headers.Location.AbsoluteUri);
            }

            private int SetUpNewGameInService()
            {
                int newGameId = new Random().Next(1, int.MaxValue);
                _gameServiceMock.Setup(s => s.CreateNewGame(UserName, It.IsAny<Game>())).Returns(newGameId);
                return newGameId;
            }

            private HttpResponseMessage CreateNewGameThroughControllerAndReturnResponse()
            {
                return _controllerToTest.CreateNewGame(new Game()
                    {
                        Rating = "M",
                        ReleaseDate = new DateTime(2007, 9, 25),
                        Title = "Halo 3"
                    });
            }

        }
    }
}
