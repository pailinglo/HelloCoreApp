using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloCoreApp.Models;


namespace HelloCoreApp.ViewModels
{
    //view model class is created when a object model doesn't contain all data a view needs
    public class HomeDetailsViewModel
    {
        public string PageTitle { get; set; }
        public Employee Employee { get; set; }
    }
}
