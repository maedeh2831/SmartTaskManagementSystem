namespace SmartTask.Web.Services.PasswordHasher
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string passwordHash);
    }
}