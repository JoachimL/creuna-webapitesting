using NUnit.Framework;
using System;
using System.Threading;
using System.Web.Http;

namespace Creuna.WebApiTesting
{
    [TestFixture]
    public class ApiControllerTestBaseTests
    {
        private ApiControllerTestBase _controllerTestBeingTested;

        [SetUp]
        public virtual void SetUp()
        {
            _controllerTestBeingTested = new ApiControllerTestBaseImplementation();
        }

        public class The_BaseUrl_Property : ApiControllerTestBaseTests
        {
            [Test]
            public void Is_Localhost_By_Default()
            {
                Assert.AreEqual("http://localhost/", _controllerTestBeingTested.BaseUrl);
            }

            [Test]
            public void Can_Be_Set()
            {
                const string newUri = "http://some.other.domain.com";
                _controllerTestBeingTested.BaseUrl = newUri;
                Assert.AreEqual(newUri, _controllerTestBeingTested.BaseUrl);
            }

            [Test]
            [TestCase("")]
            [TestCase("\t")]
            [TestCase("   ")]
            [TestCase(" ")]
            [TestCase(null)]
            [ExpectedException(typeof(ArgumentException))]
            public void Does_Not_Allow_Null_Or_Empty_Uri(string invalidUri)
            {
                _controllerTestBeingTested.BaseUrl = invalidUri;
            }
        }


        public class The_SetUpController_Method : ApiControllerTestBaseTests
        {

            [Test]
            public void Sets_IsAuthenticated_To_false_For_Current_User()
            {
                // Arrange
                var controllerImplementation = new ValuesController();
                Assert.IsNull(controllerImplementation.Request);

                // Act 
                _controllerTestBeingTested.SetUpController(controllerImplementation);

                // Assert
                Assert.IsFalse(Thread.CurrentPrincipal.Identity.IsAuthenticated);
            }

            [Test]
            public void Sets_The_Current_User_To_Have_No_Identity()
            {
                // Arrange
                var controllerImplementation = new ValuesController();
                Assert.IsNull(controllerImplementation.Request);

                // Act 
                _controllerTestBeingTested.SetUpController(controllerImplementation);

                // Assert
                Assert.AreEqual("", Thread.CurrentPrincipal.Identity.Name);
            }

            [Test]
            public void Sets_The_Request_Property()
            {
                // Arrange
                var controllerImplementation = new ValuesController();
                Assert.IsNull(controllerImplementation.Request);

                // Act 
                _controllerTestBeingTested.SetUpController(controllerImplementation);

                // Assert
                Assert.IsNotNull(controllerImplementation.Request);
            }

            [Test]
            public void Sets_The_Url_Property()
            {
                // Arrange
                var controllerImplementation = new ValuesController();
                // The Url property throws an ArgumentNullException on get if it's not set.
                try { Assert.IsNull(controllerImplementation.Url); }
                catch (ArgumentNullException) { }

                //Act
                _controllerTestBeingTested.SetUpController(controllerImplementation);

                // Assert
                Assert.IsNotNull(controllerImplementation.Url);
            }

        }

        public class The_ApiController_Being_Tested : ApiControllerTestBaseTests
        {
            private ValuesController _controllerImplementation;

            public override void SetUp()
            {
                base.SetUp();
                _controllerImplementation = new ValuesController();
                _controllerTestBeingTested.SetUp();
                _controllerTestBeingTested.SetUpController(_controllerImplementation);
            }

            [Test]
            public void Enables_Creation_Of_Base_Url_Relative_Api_Links()
            {
                //Arrange
                _controllerTestBeingTested.BaseUrl = "http://reviews.com";

                // Act
                string link = _controllerImplementation.Url.Link("DefaultApi", new { controller = "games", id = 69 });

                //Assert
                Assert.AreEqual("http://reviews.com/games/69", link);
            }

            [Test]
            public void Enables_Creation_Of_Base_Url_Relative_Mvc_Links()
            {
                // Arrange
                _controllerTestBeingTested.BaseUrl = "http://reviews.com";

                // Act
                string link = _controllerImplementation.Url.Link("Default", new { controller = "games", action = "Edit", id = 69 });

                // Assert
                Assert.AreEqual("http://reviews.com/games/Edit/69", link);
            }

            [Test]
            public void Can_Create_Links_Without_Id()
            {
                // Arrange
                _controllerTestBeingTested.BaseUrl = "http://base.com";

                // Act
                string link = _controllerImplementation.Url.Link("Default", new { controller = "games", action = "new" });

                // Assert
                Assert.AreEqual("http://base.com/games/new", link);
            }

            [Test]
            public void Can_Create_Links_Without_Action_And_Id()
            {
                // Arrange
                _controllerTestBeingTested.BaseUrl = "http://base.com";

                // Act
                string link = _controllerImplementation.Url.Link("Default", new { controller = "games" });

                // Assert
                Assert.AreEqual("http://base.com/games", link);
            }

            [Test]
            public void Enables_Setting_Current_User_Without_Roles()
            {
                // Act 
                _controllerTestBeingTested.SetLoggedOnUserNameTo("someone");

                // Assert
                Assert.AreEqual("someone", _controllerImplementation.User.Identity.Name);
            }

            [Test]
            public void Enables_Setting_Current_User_With_Single_Role()
            {
                _controllerTestBeingTested.SetLoggedOnUserNameWithRole("admin", "administrators");

                Assert.IsTrue(_controllerImplementation.User.IsInRole("administrators"));
            }

            [Test]
            public void Enables_Setting_Current_User_With_Multiple_Role()
            {
                _controllerTestBeingTested.SetLoggedOnUserNameWithRoles("admin", "administrators", "moderators");

                Assert.That(_controllerImplementation.User.IsInRole("administrators"));
                Assert.That(_controllerImplementation.User.IsInRole("moderators"));
            }


        }

        private class ValuesController : ApiController { }

        public class The_Dispose_Method
        {
            [Test]
            public void Should_Be_Idempotent()
            {
                var tester = new ApiControllerTestBaseImplementation();
                tester.Dispose();
                tester.Dispose();
            }
        }
    }
}
