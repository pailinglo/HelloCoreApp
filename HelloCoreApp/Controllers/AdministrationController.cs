using HelloCoreApp.Models;
using HelloCoreApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HelloCoreApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        //ctro tab twice -> constructor
        //ctrl + period -> create private field
        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ApplicationUser> userManager,
                                        ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole(model.RoleName);
                var result = await this.roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(String.Empty, error.Description);
                }


            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ListRoles()
        {
            //IQueryable, which implements IEnumerable.
            var roles = roleManager.Roles;
            
            return View(roles);
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var model = userManager.Users.ToList<ApplicationUser>();

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User Id {id} Can't be found";
                return View("NotFound");
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var userClaims = await userManager.GetClaimsAsync(user);

            EditUserViewModel model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Roles = userRoles,
                Claims = userClaims.Where(x=>x.Value=="true").Select(x => x.Type).ToList()


            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User Id {model.Id} Can't be found";
                return View("NotFound");
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.City = model.City;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers", "Administration");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User Id {id} Can't be found";
                return View("NotFound");
            }
            try
            {
                var result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "Administration");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch(DbUpdateException ex)
            {
                
                ViewBag.ErrorTitle = "Unable to delete user";
                ViewBag.ErrorMessage = "Delete the user claims first before delete the user";

                logger.LogError("Update Datebase error", ex);

                return View("Error");
            }
            
            return View("ListUsers");
        }

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role Id {id} Can't be found";
                return View("NotFound");
            }

            try
            {
                var result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            catch (DbUpdateException ex)
            {
                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. If you want to delete this role, please remove the users from the role and then try to delete";

                logger.LogError("Update Database Error:", ex);
                

                return View("Error");
            }

            
            return View("ListRoles");
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"Role Id {id} Can't be found";
                return View("NotFound");
            }

            EditRoleViewModel model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
                
            };

            //if I used "userManager.Users" I got exception. the error happends since .netcore 3.0 
            foreach(var user in userManager.Users.ToList())
            {
                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }

                
            }

            return View(model);

        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            IdentityRole role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role Id {model.Id} Can't be found";
                return View("NotFound");
            }

            role.Name = model.RoleName;

            IdentityResult result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("ListRoles", "Administration");
            }

            foreach(var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);

        }


        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            IdentityRole role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role Id {roleId} Can't be found";
                return View("NotFound");
            }
            
            ViewBag.roleId = roleId;

            List<UserRoleViewModel> model = new List<UserRoleViewModel>();

            foreach(var user in userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserName = user.UserName,
                    UserId = user.Id
                };

                userRoleViewModel.IsSelected = await userManager.IsInRoleAsync(user, role.Name);

                model.Add(userRoleViewModel);
            }

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(string roleId, List<UserRoleViewModel> model)
        {
            IdentityRole role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role Id {roleId} Can't be found";
                return View("NotFound");
            }

            ViewBag.roleId = roleId;

            //foreach(var user in model)
            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);
 
                IdentityResult result = null;
                
                ApplicationUser applicationUser = await userManager.FindByIdAsync(model[i].UserId);
                if(model[i].IsSelected && !(await userManager.IsInRoleAsync(applicationUser, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(applicationUser, role.Name);
                }
                else if (!model[i].IsSelected && (await userManager.IsInRoleAsync(applicationUser, role.Name)))
                {
                    result = await userManager.RemoveFromRoleAsync(applicationUser, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded) {

                    if (i < (model.Count - 1))
                        continue;
                    else
                        //not necessarily will come here since the last one might not have any change.
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError(String.Empty, error.Description);
                    }
                    return View(model);
                }

            }

            return RedirectToAction("EditRole", new { Id = roleId });

        }



        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User Id {userId} Can't be found";
                return View("NotFound");
            }

            ViewBag.userId = userId;

            List<UserRolesViewModel> model = new List<UserRolesViewModel>();

            //need toList, or weird exception:InvalidOperationException: There is already an open DataReader associated with this Command which must be closed first.
            //when we call "Microsoft.AspNetCore.Identity.UserManager<TUser>.IsInRoleAsync(TUser user, string role)"
            foreach (var role in roleManager.Roles.ToList())
            {
                UserRolesViewModel userRolesViewModel = new UserRolesViewModel();
                userRolesViewModel.RoleId = role.Id;
                userRolesViewModel.RoleName = role.Name;

                userRolesViewModel.IsSelected = await userManager.IsInRoleAsync(user, role.Name);
                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User Id {userId} Can't be found";
                return View("NotFound");
            }

            ViewBag.userId = userId;

            IList<string> existingRoles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, existingRoles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = userId });

        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            // Loop through each claim we have in our application
            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                // If the user has the claim, set IsSelected property to true, so the checkbox
                // next to the claim is checked on the UI
                if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            // Get all the user existing claims and delete them
            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            // Add all the claims that are selected on the UI
            result = await userManager.AddClaimsAsync(user,
                model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, "true")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });

        }
    }
}
