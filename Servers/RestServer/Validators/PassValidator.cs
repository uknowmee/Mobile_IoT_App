namespace Servers.Validators;

public class PassValidator
{
    private const int PasswordSalt = 12;

    public static int GetPasswordSalt()
    {
        return PasswordSalt;
    }
    
    public bool IsPassValid(string password)
    {
        if (!Char.IsUpper(password[0]))
        {
            return false;
        }

        return true;
    }

    public PassValidator()
    {
        
    }
}