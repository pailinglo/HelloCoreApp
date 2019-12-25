using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HelloCoreApp.Models;

namespace HelloCoreApp.ViewModels
{
    public class UserClaim
    {
        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }

    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }

        public string UserId { get; set; }
        public List<UserClaim> Claims { get; set; }
    }
}
