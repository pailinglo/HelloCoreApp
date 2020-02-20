using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using HelloCoreApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using HelloCoreApp.Security;

namespace HelloCoreApp
{
    public class Startup
    {
        private IConfiguration _config;

        // Notice we are using Dependency Injection here
        public Startup(IConfiguration config)
        {
            _config = config;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //The difference between AddDbContext() and AddDbContextPool() methods is, AddDbContextPool() method provides DbContext pooling.
            //With DbContext pooling, an instance from the DbContext pool is provided if available, rather than creating a new instance.
            services.AddDbContextPool<AppDbContext>(
            options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));

            //IdentityUser class is provided by ASP.NET core and contains properties for UserName, PasswordHash, Email etc. 
            //This is the class that is used by default by the ASP.NET Core Identity framework to manage registered users of your application
            //we replace IdentitiyUser class with extended ApplicationUser class, which contains more properties.
            services.AddIdentity<ApplicationUser, IdentityRole>(
                        options =>
                        {
                            options.Password.RequireDigit = false;
                            options.Password.RequireUppercase = false;
                            options.Password.RequireNonAlphanumeric = false;
                            //require user's e-mail confirmed to be able to sign in.
                            options.SignIn.RequireConfirmedEmail = true;
                            options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                        }
                    ).AddEntityFrameworkStores<AppDbContext>()  //adds an implementation of entity framework for identity information store.
                    .AddDefaultTokenProviders()    //to generate token for user e-mail confirmation.
                    .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");


            // Set token life span to 5 hours
            // However all kinds of tokens e.g. password reset token and e-mail confirmation token will have the same life span.
            // to have different life span we need to have customized DataProtectionTokenProvider class (will be discussed later)
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromHours(1));

            // Changes token lifespan of just the Email Confirmation Token type
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o =>
                    o.TokenLifespan = TimeSpan.FromDays(3));

            //configure password options:
            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequireDigit = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //});

            // to use claim based authorization - create policy:
            services.AddAuthorization(options =>
            {
                options.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role"));

                //claim type is case-insensitive, however claim value is case-sensitive.
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role","true"));

                //use function to create policy: either in (Admin role and has Edit Role claim) or is SuperAdmin role.

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                //    context.User.IsInRole("Admin") &&
                //    context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //    context.User.IsInRole("Super Admin")
                //));

                //instead of using function, we use customized authorization requirement:
                
                options.AddPolicy("EditRolePolicy", policy =>
                    policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));
               
                               
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));
            
            });

            //to change the default route of Access denied page:
            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            //});


            //services.AddMvc();  //add MVC to .NET CORE application step 1
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;  //need to disable EndpointRouting since we used UseMvcWithDefaultRoute

                //to apply [Authorize] globally throughout the application:
                var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                options.Filters.Add(new AuthorizeFilter(policy));


            });

            services.AddAuthentication()
                .AddGoogle(options =>
            {
                options.ClientId = "924068766498-av93lkto87p8frb3d3er3fkb3ut45t5s.apps.googleusercontent.com";
                options.ClientSecret = "xRQLLFkRdO47803wyHnneuhk";

            }).AddFacebook(options =>
            {
                options.ClientId = "191304868604179";
                options.ClientSecret = "301964dd94a3bef84e41a3cee4cf97c9";

            });

            //.AddXmlSerializerFormatters();    //???? what's the usage of AddXmlSerializerFormatters

            //???Difference between AddMvcCore and AddMvc
            //InvalidOperationException: No service for type 'Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory' has been registered
            //services.AddMvcCore(options =>
            //{
            //    options.EnableEndpointRouting = false;
            //});


            // Unable to resolve service for type 'HelloCoreApp.Models.IEmployeeRepository
            // a Singleton service is created only one time per application and that single instance is used throughout the application life time.
            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            //custom authorization requirement:
            //check: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            // Register the second handler. either handler fulfill the requirement then the user can pass the check.
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if there is an exception and if the environment is Development, this middleware responds with the developer exception page. 
            //???? I don't know why an path not found, 404 error page doesn't show.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // else serve User-friendly Error Page with Application Support Contact Info
            else if (env.IsStaging() || env.IsProduction() || env.IsEnvironment("UAT"))
            {
                app.UseExceptionHandler("/Error");  //if exceptions happens and not caught, go to Error controller 
                //app.UseStatusCodePages();   //simply shows "Status Code: 404; Not Found   " not very useful
                
                //app.UseStatusCodePagesWithRedirects("/Error/{0}");    //redirect which changes the url and return 200 as status code
                app.UseStatusCodePagesWithReExecute("/Error/{0}");      //retain the url and also return 404 not found.
            }



            //display default page when user browse the root (call this function before useStaticFiles because UseDefaultFiles simply rewrites the url to the default page)
            //default page can be: index.htm(l), default.htm(l)
            //app.UseDefaultFiles();


            // In case you want to specify page other than default/index.htm(l) as the default document
            /*
            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("about.html");
            // Add Default Files Middleware
            app.UseDefaultFiles(defaultFilesOptions);
            */


            //serve static files
            app.UseStaticFiles();
            app.UseAuthentication();

            //"UseFileServer" combines three middleware: UseDefaultFiles, UseStaticFiles and UseDirectoryBrowser
            //app.UseFileServer();

            //app.UseMvcWithDefaultRoute();

            //app.UseMvc();

            //instead of using the default route, specify the route:
            //id? => id is an optional field. HomeController is default controller if not specified. Index function is default action if not specified.
            app.UseMvc(routes =>
            {
                //same as the default route.
                routes.MapRoute("default", "{controller=Home}/{Action=Index}/{id?}");
            });




            //app.Run(async (context) =>
            //{
            //    //throw new Exception("The page doesn't exists");
            //    await context.Response.WriteAsync("Hosting environment:" + env.EnvironmentName);

            //    if (env.IsDevelopment())
            //    {
            //        await context.Response.WriteAsync("HOST:Developing");
            //    }

            //    else if (env.IsStaging())
            //    {
            //        await context.Response.WriteAsync("HOST:Staging");
            //    }
            //    else if (env.IsProduction())
            //    {
            //        await context.Response.WriteAsync("HOST:Production");
            //    }
            //    else if (env.IsEnvironment("UAT"))
            //    {
            //        await context.Response.WriteAsync("HOST:UAT");
            //    }
            //});



        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure_tmp2(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if there is an exception and if the environment is Development, this middleware responds with the developer exception page. 
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseRouting();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        //await context.Response.WriteAsync("Hello World!");
            //        //To get the process name executing the app, use 
            //        await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            //        await context.Response.WriteAsync(_configuration["MyKey"]);


            //    });
            //});

            //simpler version:
            //RequestDelegate: public delegate Task RequestDelegate(HttpContext context)
            //It is through this HttpContext object, the middleware gains access to both the incoming http request and outgoing http response.
            //Instead of passing the request delegate inline as an anonymous method, we can define the request delegate in a separate reusable class.
            //With this Run() extension method we can only add a terminal middleware to the request pipeline. (A terminal middleware is a middleware that does not call the next middleware in the pipeline)

            //If you want your middleware to be able to call the next middleware in the pipeline, then register the middleware using Use() method 
            //Notice, Use() method has 2 parameters. The first parameter is the HttpContext context object and the second parameter is the Func type
            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("Hello from 1st Middleware");
                await next();

            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello from 2nd Middleware");
            });


        }


        public void Configure_tmp(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
                ILogger<Startup> logger)
        {
            //If you run the project using the .NET Core CLI, you can see the logged information on the Console window
            //If you run the project directly from Visual Studio, you can see the logged information in the output window. 
            //Select ASP.NET Core Web Server from the dropdownlist in the output window.
            app.Use(async (context, next) =>
            {
                logger.LogInformation("MW1: Incoming Request");
                await next();   //go to the next middleware until it finishes
                logger.LogInformation("MW1: Outgoing Response");
            });

            app.Use(async (context, next) =>
            {
                logger.LogInformation("MW2: Incoming Request");
                await next();   //go to the next middleware until it finishes
                logger.LogInformation("MW2: Outgoing Response");
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("MW3: Request handled and response produced");
                logger.LogInformation("MW3: Request handled and response produced");
            });
        }


    }
}
