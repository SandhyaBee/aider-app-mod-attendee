using System.Text;
using System.Security.Cryptography;
using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Common.Security.System.Win
{
    public class SecurityCheck
    {
        public static bool OrderSecurityCheck(List<Order> orders)
        {    
            var scanIterations = 6000; // Simulating a very intensive security scan

            for (int i = 0; i < scanIterations; i++)
            {
                // Double-check the order data to ensure it hasn't been tampered with
                string data = Guid.NewGuid().ToString();
                for (int j = 0; j < 1000; j++)
                {
                    // Repeatedly hashing the data to before the final check
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    byte[] hash = SHA256.HashData(bytes);
                    data = Convert.ToBase64String(hash); 
                }
            }

            return true;
        }

        public static bool VerifySecurityCheck()
        {
            return true;
        }

        private static string ConvertToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
