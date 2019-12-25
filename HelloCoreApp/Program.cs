using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace HelloCoreApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        //"=>"one line function: memeber=>expression.
        //In case of InProcess hosting, CreateDefaultBuilder() method calls UseIIS() method and host the app inside of the IIS worker process (w3wp.exe or iisexpress.exe). 
        //InProcess hosting delivers significantly higher request throughput than OutOfProcess hosting
        //In the case of IIS, the process name that executes the app is w3wp and in the case of IIS Express it is iisexpress
        //With out of process hosting
        //There are 2 web servers - An internal web server and an external web server.We will discuss out of process hosting in detail in our next video.
        //The internal web server is Kestrel and the external web server can be IIS, Nginx or Apache.
        //In Kestrel, the process used to host the app is dotnet.exe.
        //When we run a .NET Core application using the .NET Core CLI (Command-Line Interface), the application uses Kestrel as the web server. 
        //run the application through CLI - use "dotnet run" under the project folder.
        //C:\Users\paili\source\repos\HelloCoreApp\HelloCoreApp>dotnet run
        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                //customize for adding NLog as one of the logging provider.
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                    // Enable NLog as one of the Logging Provider
                    logging.AddNLog();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
