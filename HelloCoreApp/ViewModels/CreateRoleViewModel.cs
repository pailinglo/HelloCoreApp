using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCoreApp.ViewModels
{
    public class CreateRoleViewModel
    {
        //prop tab -> create property
        [Required]
        [Display(Name ="Role")]
        public string RoleName { get; set; }
    }
}
