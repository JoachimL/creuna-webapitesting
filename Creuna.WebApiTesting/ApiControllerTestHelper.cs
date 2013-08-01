using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Creuna.WebApiTesting
{
    /// <summary>
    /// Class meant to be used as ancestor to unit test classes testing ApiController implementations. Exposes functionality for 
    /// working with ApiControllers as if they were in a hosted environment (User, Request and Url property set).
    /// </summary>
    public sealed class ApiControllerTestHelper : IDisposable
    {
        private HttpRouteStub _routeMock;
        private HttpRequestMessage _request;
        private HttpRouteCollection _routes;
        private HttpConfiguration _httpConfiguration;
        private HttpRouteData _httpRouteData;
        private string _baseUrl = "http://localhost/";
        private bool _disposed;
        private readonly ApiController _controllerUnderTest;

        /// <summary>
        /// Creates a new instance of the ApiControllerTestsHelper class, supporting the supplied <see cref="ApiController"/>.
        /// </summary>
        /// <param name="controller"></param>
        public ApiControllerTestHelper(ApiController controller)
        {
            _controllerUnderTest = controller;
            DecorateController(controller);
        }

        /// <summary>
        /// The controller being tested, which this instance supports with hosting-like parameters and set up.
        /// </summary>
        public ApiController ControllerUnderTest { get { return _controllerUnderTest; } }

        /// <summary>
        /// Gets or sets the base url of the api being tested and the request uri of the request.
        /// </summary>
        public string BaseUrl
        {
            get { return _baseUrl; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("value cannot be empty.");
                if (value != BaseUrl)
                {
                    _baseUrl = value;
                    if (_request != null)
                        _request.RequestUri = new Uri(value);
                }
            }
        }

        private void SetUpConfigurationWithDefaultRoutes()
        {
            _routes = new HttpRouteCollection("/");
            SetUpDefaultRoutes();
            _httpConfiguration = new HttpConfiguration(_routes);
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = _httpConfiguration;
            _httpRouteData = new HttpRouteData(_httpConfiguration.Routes.First());
            _request.Properties[HttpPropertyKeys.HttpRouteDataKey] = _httpRouteData;
        }

        private void SetUpDefaultRoutes()
        {
            _routeMock = new HttpRouteStub(this, _request);
            AddDefaultApiRoute();
            AddDefaultMvcRoute();
        }

        private void AddDefaultApiRoute()
        {
            AddMockedRoute("DefaultApi");
        }

        private void AddDefaultMvcRoute()
        {
            AddMockedRoute("Default");
        }

        private void AddMockedRoute(string routeName)
        {
            _routes.Add(routeName, _routeMock);
        }

        private void SetCurrentUserToAnonymous()
        {
            var identity = CreateMockedAnonymousIdentity();
            var principalMock = CreateMockPrincipalWithIdentity(identity);
            StoreCurrentPrincipal(principalMock);
        }

        private static IPrincipal CreateMockPrincipalWithIdentity(IIdentity identityMock)
        {
            return new PrincipalStub()
            {
                Identity = identityMock
            };
        }

        private static IIdentity CreateMockedAnonymousIdentity()
        {
            return new IdentityStub()
            {
                Name = string.Empty,
                IsAuthenticated = false,
                AuthenticationType = string.Empty
            };
        }


        /// <summary>
        /// Sets up the controller with mocked/stubbed Request, ControllerContext and Url.
        /// </summary>
        /// <param name="controller"></param>
        [Obsolete("Set up is now done in constructor.")]
        public void SetUpController(ApiController controller)
        {
            
        }

        private void DecorateController(ApiController controller)
        {
            SetCurrentUserToAnonymous();
            CreateAndSetRequest(controller);
            SetUpConfigurationWithDefaultRoutes();

            controller.ControllerContext = new HttpControllerContext(_httpConfiguration, _httpRouteData, _request);
            controller.Url = new UrlHelper(_request);
        }

        private void CreateAndSetRequest(ApiController controller)
        {
            _request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
            controller.Request = _request;
        }

       

        /// <summary>
        /// Sets the logged on user to the supplied user name.
        /// </summary>
        public void SetLoggedOnUserNameTo(string userName)
        {
            SetLoggedOnUserNameWithRoles(userName, new string[0]);
        }

        /// <summary>
        /// Sets the logged on user to the supplied user name with the supplied role.
        /// </summary>
        public void SetLoggedOnUserNameWithRole(string userName, string role)
        {
            SetLoggedOnUserNameWithRoles(userName, new[] { role });
        }

        /// <summary>
        /// /// Sets the logged on use to the supplied user name with the supplied roles.
        /// </summary>
        public void SetLoggedOnUserNameWithRoles(string userName, params string[] roles)
        {

            var identity = new IdentityStub() { Name = userName };
            var principalMock = CreatePrincipalMockWithIdentityAndRoles(identity, roles);
            StoreCurrentPrincipal(principalMock);
        }

        private static IPrincipal CreatePrincipalMockWithIdentityAndRoles(IIdentity identityMock, string[] roles)
        {
            return new PrincipalStub() { Identity = identityMock, Roles = roles };
        }


        private void StoreCurrentPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
        }

        

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~ApiControllerTestHelper()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        /// <param name="disposing">Set to <c>true</c> if disposing manually.</param>
        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_request != null) _request.Dispose();
                if (_routes != null) _routes.Dispose();
                if (_httpConfiguration != null) _httpConfiguration.Dispose();
            }

            _disposed = true;
        }
    }
}
