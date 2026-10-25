namespace Financial.Application.Exceptions;

public class RegistrationDisabledException : Exception
{
    public RegistrationDisabledException()
        : base("User registration is currently disabled.")
    {
    }
}
