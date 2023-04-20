using Servers.DbManagers;

namespace Servers.Validators;

public class SessionTokenValidator
{
    private readonly SrvDbManager _srvDbManager;
    
    public string GetValidToken()
    {
        string token = Guid.NewGuid().ToString();

        while (!_srvDbManager.IsTokenHashValid(token))
        {
            token = Guid.NewGuid().ToString();
        }

        return token;
    }

    public SessionTokenValidator(SrvDbManager srvDbManager)
    {
        _srvDbManager = srvDbManager;
    }
}