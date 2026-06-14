using System.Security.Cryptography;
using System.Text;

namespace Prototype.Services
{
    public class PasswordHasherService
    {
        public string Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }
    }
}
