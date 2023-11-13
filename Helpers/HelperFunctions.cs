using System.Security.Cryptography;
using System.Text;

namespace backend.Helpers
{
    public static class HelperFunctions
    {
        public static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        }
        public static string CreateRandomPassword(int length = 8)
        {

            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+";

            if (length <= 0)
            {
                throw new ArgumentException("Password length must be greater than 0.");
            }

            StringBuilder password = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(allowedChars.Length);
                password.Append(allowedChars[index]);
            }

            return password.ToString();


        }
        public static class GuidGenerator
        {
            public static readonly Guid PostFixSecretKey = Guid.NewGuid();
        }
    }
}
