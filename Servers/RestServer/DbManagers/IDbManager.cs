namespace Servers.DbManagers;

public interface IDbManager
{
    public bool EnsureCreated();
    public bool InitialInserts();
}