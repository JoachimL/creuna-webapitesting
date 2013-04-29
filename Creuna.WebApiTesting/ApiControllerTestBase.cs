using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Creuna.WebApiTesting
{
    public abstract class ApiControllerTestBase : IDisposable
    {
        private const string RequiredMsHttpConfigurationKey = "MS_HttpConfiguration";
        private Mock<IHttpRoute> _routeMock;
        private HttpRequestMessage _request;
        private HttpRouteCollection _routes;
        private HttpConfiguration _httpConfiguration;
        private HttpRouteData _httpRouteData;
        private string _baseUrl = "http://localhost/";
        private object _currentId;
        private string _currentController;
        private string _currentAction;
        private bool _disposed;

        /// <summary>
        /// Gets or sets the base url of the api being tested and the request uri of the request.
        /// </summary>
        public virtual string BaseUrl
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

        /// <summary>
        /// Initializes local helper variables and sets current user to anonymous.
        /// </summary>
        public virtual void SetUp()
        {
            SetCurrentUserToAnonymous();
            _request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
            SetUpConfigurationWithDefaultRoutes();
        }

        private void SetUpConfigurationWithDefaultRoutes()
        {
            _routes = new HttpRouteCollection("/");
            SetUpDefaultRoutes();
            _httpConfiguration = new HttpConfiguration(_routes);
            _request.Properties[RequiredMsHttpConfigurationKey] = _httpConfiguration;
            _httpRouteData = new HttpRouteData(_httpConfiguration.Routes.First());
        }

        private void SetUpDefaultRoutes()
        {
            _routeMock = new Mock<IHttpRoute>();
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
            _routes.Add(routeName, _routeMock.Object);
        }

        private void SetCurrentUserToAnonymous()
        {
            var identityMock = CreateMockedAnonymousIdentity();
            var principalMock = CreateMockPrincipalWithIdentity(identityMock);
            StoreCurrentPrincipal(principalMock.Object);
        }

        private static Mock<IPrincipal> CreateMockPrincipalWithIdentity(Mock<IIdentity> identityMock)
        {
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);
            return principalMock;
        }

        private static Mock<IIdentity> CreateMockedAnonymousIdentity()
        {
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(i => i.Name).Returns(string.Empty);
            identityMock.SetupGet(i => i.IsAuthenticated).Returns(false);
            identityMock.SetupGet(i => i.AuthenticationType).Returns(string.Empty);
            return identityMock;
        }

        /// <summary>
        /// Sets up the controller with mocked/stubbed Request, ControllerContext and Url.
        /// </summary>
        /// <param name="controller"></param>
        public void SetUpController(ApiController controller)
        {
            controller.Request = _request;
            controller.ControllerContext = new HttpControllerContext(_httpConfiguration, _httpRouteData, _request);
            controller.Url = new UrlHelper(_request);
            SetUpLinkFactory();
        }

        private void SetUpLinkFactory()
        {
            var virtualPathDataMock = new Mock<IHttpVirtualPathData>();
            virtualPathDataMock.SetupGet(v => v.VirtualPath).Returns(GetUrlForCurrentDictionaryValues);
            virtualPathDataMock.SetupGet(v => v.Route).Returns(_routeMock.Object);
            _routeMock.Setup(
                r =>
                r.GetVirtualPath(_request,
                                 It.Is<IDictionary<string, object>>(d => IsMvcOrApiRoute(d))))
                      .Returns(virtualPathDataMock.Object);
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
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(i => i.Name).Returns(userName);
            var principalMock = CreatePrincipalMockWithIdentityAndRoles(identityMock, roles);
            StoreCurrentPrincipal(principalMock.Object);
        }

        private static Mock<IPrincipal> CreatePrincipalMockWithIdentityAndRoles(Mock<IIdentity> identityMock, string[] roles)
        {
            var principalMock = new Mock<IPrincipal>();
            principalMock.SetupGet(p => p.Identity).Returns(identityMock.Object);
            SetUpPrincipalsRoles(roles, principalMock);
            return principalMock;
        }

        private static void SetUpPrincipalsRoles(IEnumerable<string> roles, Mock<IPrincipal> principalMock)
        {
            principalMock.Setup(p => p.IsInRole(It.Is<string>(s => !roles.Contains(s)))).Returns(false);
            principalMock.Setup(p => p.IsInRole(It.Is<string>(s => roles.Contains(s)))).Returns(true);
        }

        private void StoreCurrentPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
        }

        private string GetUrlForCurrentDictionaryValues()
        {
            var url = BuildUrlFromCachedDictionaryValues();
            ClearCachedDictionaryValues();
            return url;
        }

        private void ClearCachedDictionaryValues()
        {
            _currentId = null;
            _currentAction = null;
            _currentController = null;
        }

        private string BuildUrlFromCachedDictionaryValues()
        {
            var idPart = GetUrlPartOrEmpty(_currentId);
            var actionPart = GetUrlPartOrEmpty(_currentAction);
            return _currentController + actionPart + idPart;
        }

        private string GetUrlPartOrEmpty(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return string.Empty;

            return string.Format(@"/{0}", value);
        }

        private bool IsMvcOrApiRoute(IDictionary<string, object> routeDictionary)
        {
            var isApiRoute = false;
            if (routeDictionary.ContainsKey("controller"))
            {
                _currentController = routeDictionary["controller"] as string;
                isApiRoute = !string.IsNullOrWhiteSpace(_currentController);
            }
            if (routeDictionary.ContainsKey("action"))
                _currentAction = routeDictionary["action"] as string;
            if (routeDictionary.ContainsKey("id"))
                _currentId = routeDictionary["id"];
            return isApiRoute;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ApiControllerTestBase()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
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
