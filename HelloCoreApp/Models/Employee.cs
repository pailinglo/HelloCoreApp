using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelloCoreApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [NotMapped]
        public string EncryptedId { get; set; }
        [Required,MaxLength(50,ErrorMessage ="max length is 50")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",ErrorMessage ="Invalid E-mail format")]
        [Display(Name="Office E-mail")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }   //make Department type nullable such that, it won't display "invalid value" error when opetion "Select a value" is selected.
        public string PhotoPath { get; set; }  
    }

    public interface IEmployeeRepository
    {
        Employee GetEmployee(int Id);
        IEnumerable<Employee> GetAllEmployee();
        Employee Add(Employee employee);
        Employee Update(Employee employeeChanges);
        Employee Delete(int Id);
    }

    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee() { Id = 1, Name = "Mary", Department = Dept.HR, Email = "mary@pragimtech.com" },
                new Employee() { Id = 2, Name = "John", Department = Dept.IT, Email = "john@pragimtech.com" },
                new Employee() { Id = 3, Name = "Sam", Department = Dept.IT, Email = "sam@pragimtech.com" },
            };
        }

        public Employee GetEmployee(int Id)
        {
            return this._employeeList.FirstOrDefault(e => e.Id == Id);
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return this._employeeList;
        }

        public Employee Add(Employee employee)
        {
            employee.Id = this._employeeList.Max(r => r.Id) + 1;
            this._employeeList.Add(employee);
            return employee;
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
            if (employee != null)
            {
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
            }
            return employee;
        }

        public Employee Delete(int Id)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == Id);
            if (employee != null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
        }
    }
}
