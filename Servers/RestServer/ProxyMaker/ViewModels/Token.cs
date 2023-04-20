namespace Servers.ProxyMaker.ViewModels;

public class Token
{
    public DateTime CreationDate { get; set; }
    public string TokenHash { get; set; }
    public string Email { get; set; }
    
    public Token(Database.ServerDatabase.Models.Token token)
    {
        CreationDate = token.CreationDate;
        TokenHash = token.TokenHash;
        Email = token.User.Email;
    }
}