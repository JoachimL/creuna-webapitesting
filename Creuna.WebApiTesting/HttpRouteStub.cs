using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing;

namespace Creuna.WebApiTesting
{
    internal class HttpRouteStub : IHttpRoute
    {
        private readonly ApiControllerTestHelper _apiControllerHelper;
        private readonly HttpRequestMessage _request;
        private readonly HttpVirtualPathDataStub _virtualPathData;
        private string _currentController;
        private string _currentAction;
        private object _currentId;
        public HttpRouteStub(ApiControllerTestHelper apiControllerHelper, HttpRequestMessage request)
        {
            _apiControllerHelper = apiControllerHelper;
            _request = request;
            _virtualPathData = new HttpVirtualPathDataStub(this);
        }
        public IDictionary<string, object> Constraints
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, object> DataTokens
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary<string, object> Defaults
        {
            get { throw new NotImplementedException(); }
        }

        public IHttpRouteData GetRouteData(string virtualPathRoot, System.Net.Http.HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }

        public IHttpVirtualPathData GetVirtualPath(System.Net.Http.HttpRequestMessage request, IDictionary<string, object> values)
        {
            if (request != _request)
                return null;
            if (IsMvcOrApiRoute(values))
                return _virtualPathData;
            return null;
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

        internal void ClearCachedDictionaryValues()
        {
            _currentId = null;
            _currentAction = null;
            _currentController = null;
        }

        public System.Net.Http.HttpMessageHandler Handler
        {
            get { throw new NotImplementedException(); }
        }

        public string RouteTemplate
        {
            get { throw new NotImplementedException(); }
        }
        
        internal string BuildUrlFromCachedDictionaryValues()
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
    }
}
