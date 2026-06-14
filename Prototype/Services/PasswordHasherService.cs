using Microsoft.AspNetCore.Identity;
using Prototype.Models;

namespace Prototype.Services
{
    public class PasswordHasherService
    {
        private readonly PasswordHasher<User> hasher = new();

        public string Hash(User user, string password) => hasher.HashPassword(user, password);

        public bool Verify(User user, string password)
        {
            try
            {
                var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
                return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
