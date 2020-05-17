using FoxOne.Core;
using System.Linq; 
namespace FoxOne.Business.Security
{
    public sealed class Sec
    {
        private static ISecurityProvider _provider;
        public static IUser User
        {
            get { return Provider.GetCurrentUser(); }
        }

        public static ISecurityProvider Provider
        {
            get 
            {
                return _provider ?? (_provider = new SecurityProvider());
            }
        }

        public static bool IsSuperAdmin
        {
            get
            {
                return User.Roles.Count(o => o.RoleType.Name.Equals(SysConfig.SuperAdminRoleName)) > 0;
            }
        }
    }
}