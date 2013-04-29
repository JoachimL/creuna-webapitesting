using System.Security.Principal;

namespace Creuna.WebApiTesting
{
    public interface IPrincipalProxy
    {
        IPrincipal GetCurrentPrincipal();
    }
}
