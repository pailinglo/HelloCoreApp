using HelloCoreApp.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCoreApp.ViewModels
{
    public class EmployeeCreateViewModel
    {
        [Required, MaxLength(50, ErrorMessage = "max length is 50")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid E-mail format")]
        [Display(Name = "Office E-mail")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }   //make Department type nullable such that, it won't display "invalid value" error when opetion "Select a value" is selected.
        public IFormFile Photo { get; set; }
        //public List<IFormFile> Photos { get; set; }
    }
}
