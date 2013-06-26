using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Creuna.WebApiTesting
{
    internal sealed class IdentityStub : IIdentity
    {
        public string AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }
    }
    internal sealed class PrincipalStub : IPrincipal
    {
        public IIdentity Identity
        {
            get;set;
        }

        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }

        public IEnumerable<string> Roles { get; set; } 
    }
}
