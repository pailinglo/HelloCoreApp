using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCoreApp.ViewModels
{
    // to not to duplicate code, we inherit editview from creatview
    public class EmployeeEditViewModel : EmployeeCreateViewModel
    {
        public string ExistingPhotoPath { get; set; }
        public int Id { get; set; }
    }
}
