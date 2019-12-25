using HelloCoreApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCoreApp.ViewModels
{
    public class EditRoleViewModel
    {
        
        public string Id { get; set; }
        public string RoleName { get; set; }

        //public List<ApplicationUser> Users { get; set; }

        public List<string> Users { get; set; }

        public EditRoleViewModel()
        {
            Users = new List<string>();
        }

    }
}
