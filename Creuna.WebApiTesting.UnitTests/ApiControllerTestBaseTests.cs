using NUnit.Framework;
using System;
using System.Threading;
using System.Web.Http;
using System.Net.Http;

namespace Creuna.WebApiTesting
{
    [TestFixture]
    public class ApiControllerTestBaseTests
    {
        private ApiControllerTestHelper _apiControllerHelper;
        private ValuesController _controllerImplementation;
        [SetUp]
        public virtual void SetUp()
        {
            _controllerImplementation = new ValuesController();
            _apiControllerHelper = new ApiControllerTestHelper(_controllerImplementation);
        }

        public class The_BaseUrl_Property : ApiControllerTestBaseTests
        {
            [Test]
            public void Is_Localhost_By_Default()
            {
                Assert.AreEqual("http://localhost/", _apiControllerHelper.BaseUrl);
            }

            [Test]
            public void Can_Be_Set()
            {
                const string newUri = "http://some.other.domain.com";
                _apiControllerHelper.BaseUrl = newUri;
                Assert.AreEqual(newUri, _apiControllerHelper.BaseUrl);
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
                _apiControllerHelper.BaseUrl = invalidUri;
            }
        }

        public class When_a_test_helper_is_created : ApiControllerTestBaseTests
        {

            [Test]
            public void IsAuthenticated_is_set_to_false_For_Current_User()
            {
                Assert.IsFalse(Thread.CurrentPrincipal.Identity.IsAuthenticated);
            }

            [Test]
            public void The_current_user_has_no_identity()
            {
                Assert.AreEqual("", Thread.CurrentPrincipal.Identity.Name);
            }

            [Test]
            public void The_Request_property_is_set()
            {
                Assert.IsNotNull(_controllerImplementation.Request);
            }

            [Test]
            public void The_Url_property_is_set()
            {
                Assert.IsNotNull(_controllerImplementation.Url);
            }

        }

        public class The_ApiController_Being_Tested : ApiControllerTestBaseTests
        {
            public override void SetUp()
            {
                base.SetUp();
                _controllerImplementation = new ValuesController();
                _apiControllerHelper = new ApiControllerTestHelper(_controllerImplementation);
            }

            [Test]
            public void Can_create_links_relative_to_base_url()
            {
                _apiControllerHelper.BaseUrl = "http://reviews.com";

                var link = _controllerImplementation.Url.Link("DefaultApi", new { controller = "games", id = 69 });

                Assert.AreEqual("http://reviews.com/games/69", link);
            }

            [Test]
            public void Can_create_links_to_mvc_actions()
            {
                // Arrange
                _apiControllerHelper.BaseUrl = "http://reviews.com";

                // Act
                string link = _controllerImplementation.Url.Link("Default", new { controller = "games", action = "Edit", id = 69 });

                // Assert
                Assert.AreEqual("http://reviews.com/games/Edit/69", link);
            }

            [Test]
            public void Can_Create_Links_Without_Id()
            {
                // Arrange
                _apiControllerHelper.BaseUrl = "http://base.com";

                // Act
                string link = _controllerImplementation.Url.Link("Default", new { controller = "games", action = "new" });

                // Assert
                Assert.AreEqual("http://base.com/games/new", link);
            }

            [Test]
            public void Can_Create_Links_Without_Action_And_Id()
            {
                // Arrange
                _apiControllerHelper.BaseUrl = "http://base.com";

                // Act
                string link = _controllerImplementation.Url.Link("Default", new { controller = "games" });

                // Assert
                Assert.AreEqual("http://base.com/games", link);
            }

            [Test]
            public void Can_set_logged_on_users_user_name()
            {
                // Act 
                _apiControllerHelper.SetLoggedOnUserNameTo("someone");

                // Assert
                Assert.AreEqual("someone", _controllerImplementation.User.Identity.Name);
            }

            [Test]
            public void Can_set_logged_on_user_to_user_with_one_role()
            {
                _apiControllerHelper.SetLoggedOnUserNameWithRole("admin", "administrators");

                Assert.IsTrue(_controllerImplementation.User.IsInRole("administrators"));
            }

            [Test]
            public void Can_set_logged_on_user_to_user_with_multiple_roles()
            {
                _apiControllerHelper.SetLoggedOnUserNameWithRoles("admin", "administrators", "moderators");

                Assert.That(_controllerImplementation.User.IsInRole("administrators"));
                Assert.That(_controllerImplementation.User.IsInRole("moderators"));
            }

            [Test]
            public void Has_Request_with_route_data()
            {
                var result = _controllerImplementation.Request.GetRouteData();

                Assert.IsNotNull(result);
            }
        }

        private class ValuesController : ApiController { }

        public class The_Dispose_Method
        {
            [Test]
            public void Should_Be_Idempotent()
            {
                var tester = new ApiControllerTestHelper(new ValuesController());
                tester.Dispose();
                tester.Dispose();
            }
        }
    }
}
