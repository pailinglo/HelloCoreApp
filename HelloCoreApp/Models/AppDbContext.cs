using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelloCoreApp.Models
{
    //class used to manage database connection and save data in database
    //public class AppDbContext : DbContext
    
    //to be able to use ASP.NET CORE Identity, need to inherit from
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        //The DbContext class includes a DbSet<TEntity> property for each entity in the model.
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            //by default, the foreign keys in AspNetUserRoles table has Cascade DELETE behaviour.
            //change to Restrict, it will generate ON DELETE NO ACTION -- 
            //forbid referenced foreign key deleted in parent table. 
            //Raise error to urge user delete records in child table before deleting in the parent table.
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            //move the seeding code into extension.
            modelBuilder.Seed();


            //modelBuilder.Entity<Employee>().HasData(new Employee {
            //    Id = 1,
            //    Name = "John",
            //    Department = Dept.HR,
            //    Email = "john@test.com"
            //},
            //new Employee {
            //    Id = 2,
            //    Name = "Mary",
            //    Department = Dept.IT,
            //    Email = "mary@test.com"
            //});
        }
    }
}
