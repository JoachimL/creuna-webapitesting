using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Routing;

namespace Creuna.WebApiTesting
{
    internal class HttpVirtualPathDataStub : IHttpVirtualPathData
    {
        public HttpVirtualPathDataStub(HttpRouteStub route)
        {
            Route = route;
        }

        IHttpRoute IHttpVirtualPathData.Route { get { return this.Route; } }

        public HttpRouteStub Route { get; set; }

        public string VirtualPath
        {
            get
            {
                var url = Route.BuildUrlFromCachedDictionaryValues();
                Route.ClearCachedDictionaryValues();
                return url;
            }
            set { throw new NotImplementedException(); }
        }

    }
}
