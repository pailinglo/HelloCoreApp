using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HelloCoreApp.Models;
using HelloCoreApp.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace HelloCoreApp.Controllers
{
   
    //if we want to return an HTML View or JSON data from the Index() action method, 
    //our HomeController class has to derive from the Controller class provided by the framework
    [Route("[controller]/[action]")]
    [Authorize]
    public class HomeController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        //public string Index()
        //{
        //    return "Hello from MVC";
        //}

        //public JsonResult Index()
        //{
        //    return Json(new { id = 1, name = "pragim" });
        //}

        //value of read-only property can be initiated only during declaration or constructor.
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment hostingEnvironment;


        // Inject IEmployeeRepository using Constructor Injection
        // constructor injection, as we are using the constructor to inject the dependency
        // need to register MockEmployeeRepository class with the dependency injection container in ASP.NET core in ConfigureServices() method in Startup class
        // or we will get error msg "Unable to resolve service for type 'HelloCoreApp.Models.IEmployeeRepository"
        public HomeController(IEmployeeRepository employeeRepository,
             IWebHostEnvironment hostingEnvironment)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        // Retrieve employee name and return
        [Route("~/")]
        [Route("")]
        [Route("~/home/")]
        //[Route("[action]")]
        [AllowAnonymous]
        public ViewResult Index()
        {

            return View(_employeeRepository.GetAllEmployee());
        
        }

        //public JsonResult Details()
        //{
        //    Employee e = _employeeRepository.GetEmployee(1);
        //    //Create a JsonResult object that serialize the specified object to JSON.
        //    return Json(e);
        //}

        //[Route("[action]/{id?}")]
        [Route("{id?}")]
        [AllowAnonymous]
        public ViewResult Details(int id)
        {
            //throw new Exception("test");
            Employee employee = _employeeRepository.GetEmployee(id);
            if(employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);
            }

            //ViewData: loosely typed, use string as index.
            //ViewData["employee"] = e;   
            //ViewData["pageTitle"] = "Details as Title";

            //ViewBag: Dynamically binding properties.
            //ViewBag.employee = e;
            //ViewBag.pageTitle = "Details as Page Title";

            //simply return model for strongly typed model.

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
                PageTitle = "Details as Page Title"
            };
            

            return View(homeDetailsViewModel);
            //return View("Test");    //use a different cshtml in the same folder other than the default one. e.g. Home/Details
            
            //return View("MyViews/Test.cshtml");    //use a different cshtml in different folder other than the default one
            
            //return View("../../MyViews/Test");   //use a different cshtml in a different folder, but using relative path, then you can omit "cshtml"
            
        }
        
        //[Route("[action]")]
        [HttpGet]
        public ViewResult Create()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {

            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                // If the Photo property on the incoming model object is not null, then the user
                // has selected an image to upload.
                //if (model.Photos != null && model.Photos.Count > 0)
                if (model.Photo != null)
                {
                    //for multiple photos
                    //foreach(IFormFile photo in model.Photos)
                    //{
                    //    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    //    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(photo.FileName);
                    //    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    //    photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    //}

                    
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Photo.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                //extract the data from ViewModel
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    // Store the file name in PhotoPath property of the employee object
                    // which gets saved to the Employees database table
                    PhotoPath = uniqueFileName
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }

            return View();

        }


        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);

            if(employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id);
            }

            EmployeeEditViewModel model = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Department = employee.Department,
                Email = employee.Email,
                ExistingPhotoPath = employee.PhotoPath
            };

            return View(model);

        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {

            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Department = model.Department;
                employee.Email = model.Email;


                if (model.Photo != null)
                {
                    //Delete the existing file
                    if(model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }

                    employee.PhotoPath = processFileName(model);                   

                }

                _employeeRepository.Update(employee);

                return RedirectToAction("index");
            }

            return View(model);

        }

        private string processFileName(EmployeeEditViewModel model)
        {
            string uniqueFileName;
            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
            uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Photo.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using(var fileStream = new FileStream(filePath, FileMode.Create))
            {
                model.Photo.CopyTo(fileStream);
            }

            return uniqueFileName;
        }
    }
}