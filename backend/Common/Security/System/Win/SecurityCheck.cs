using StyleVerse.Backend.Models;

namespace StyleVerse.Backend.Common.Security.System.Win
{
    public class SecurityCheck
    {
        public static bool OrderSecurityCheck(List<Order> orders)
        {
            return orders != null;
        }

        public static bool VerifySecurityCheck()
        {
            return true;
        }
    }
}
