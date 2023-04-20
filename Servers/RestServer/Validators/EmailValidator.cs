namespace Servers.Validators;

public class EmailValidator
{
    public bool IsEmailValid(string email)
    {
        if (!email.Contains("@"))
        {
            return false;
        }

        return true;
    }

    public EmailValidator()
    {
        
    }
}