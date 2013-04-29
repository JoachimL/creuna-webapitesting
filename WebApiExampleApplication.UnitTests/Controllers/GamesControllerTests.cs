using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using WebApiExampleApplication.Models;
using WebApiExampleApplication.Services;

namespace WebApiExampleApplication.Controllers
{
    [TestFixture]
    class GamesControllerTests : Creuna.WebApiTesting.ApiControllerTestBase
    {
        private Mock<IGameService> _gameServiceMock;
        private GamesController _controllerToTest;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _gameServiceMock = new Mock<IGameService>();
            CreateAndPopulateController();
        }

        private void CreateAndPopulateController()
        {
            _controllerToTest = new GamesController(_gameServiceMock.Object);
            SetUpController(_controllerToTest);
        }

        public class The_CreateNewGame_Method : GamesControllerTests
        {
            const string UserName = "User";
            [Test]
            public void Returns_Unauthorized_Status_If_User_Is_Not_Authorized()
            {
                _gameServiceMock
                    .Setup(s => s.CreateNewGame(It.IsAny<string>(), It.IsAny<Game>()))
                    .Throws(new UnauthorizedAccessException("Only authorized users can create games."));

                var response = CreateNewGameThroughControllerAndReturnResponse();

                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            }

            [Test]
            public void Returns_InternalServerError_If_Nonspecific_Error_Occurs()
            {
                _gameServiceMock
                    .Setup(s => s.CreateNewGame(It.IsAny<string>(), It.IsAny<Game>()))
                    .Throws(new Exception());

                var response = CreateNewGameThroughControllerAndReturnResponse();

                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            }

            [Test]
            public void Returns_Created_And_New_Resource_Url_If_Creation_Through_Service_Succeeds()
            {
                SetLoggedOnUserNameTo(UserName);
                _gameServiceMock.Setup(s => s.CreateNewGame(UserName, It.IsAny<Game>())).Returns(23);

                var response = CreateNewGameThroughControllerAndReturnResponse();

                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
                Assert.AreEqual("http://localhost/Games/23", response.Headers.Location.AbsoluteUri);
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
