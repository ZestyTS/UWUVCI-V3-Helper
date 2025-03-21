using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace UWUVCI_V3_Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UWUVCI MAC/LINUX HELPER";
            Console.WriteLine("*******************************************");
            Console.WriteLine("*******************************************");
            Console.WriteLine("** UWUVCI V3 MAC/LINUX HELPER            **");
            Console.WriteLine("** Made By ZestyTS                       **");
            Console.WriteLine("*******************************************");
            Console.WriteLine("*******************************************");
            Console.WriteLine();

            // Set up configuration to read from appsettings.json
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Check if toolsJsonPath exists in config and assign value
            string toolsJsonFileName = config["toolsJsonPath"] ?? string.Empty;

            if (string.IsNullOrEmpty(toolsJsonFileName))
            {
                Console.WriteLine("toolsJsonPath is not set or is empty in the appsettings.json file.");
                return;
            }

            // Get current directory (where the helper is running)
            string currentDirectory = AppContext.BaseDirectory;


            // Move up one level to find tools.json in the UWUVCI root folder
            string toolsJsonPath = Path.Combine(currentDirectory, "..", toolsJsonFileName);

            // Resolve to absolute path
            toolsJsonPath = Path.GetFullPath(toolsJsonPath);

            if (!File.Exists(toolsJsonPath))
            {
                Console.WriteLine($"tools.json file not found at {toolsJsonPath}");
                return;
            }

            // Set up logging
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            if (logger == null)
            {
                Console.WriteLine("Logger service is not available.");
                return;
            }

            // Set up workflow manager
            WorkflowManager workflowManager = new(logger);

            /*
            if (PathHelper.IsAppleSilicon())
            {
                Console.WriteLine("Apple Silicon detected.");
                if (!IsRosettaInstalled())
                {
                    Console.WriteLine("Rosetta 2 may be required to run some tools in x86_64 mode");
                    Console.WriteLine("Attempting to install Rosetta 2");
                    
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/usr/sbin/softwareupdate",
                        Arguments = "--install-rosetta --agree-to-license",
                        UseShellExecute = true
                    })?.WaitForExit();
                }
            }
            */

            // Execute the workflow based on tools.json
            if (workflowManager.ExecuteWorkflow(toolsJsonPath))
            {
                Console.WriteLine("*******************************************");
                Console.WriteLine("*******************************************");
                Console.WriteLine("** All steps completed!                  **");
                Console.WriteLine("** Please return to UWUVCI and           **");
                Console.WriteLine("** click the 'OK' button                 **");
                Console.WriteLine("** to continue the process.              **");
                Console.WriteLine("*******************************************");
                Console.WriteLine("*******************************************");
            }
            else
            {
                Console.WriteLine("Error: Workflow execution failed.");
                Console.WriteLine("For assistance, please checkout the FAQ in the ReadMe.txt file.");
            }
                
        }

        private static bool IsRosettaInstalled()
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/arch",
                        Arguments = "-x86_64 echo test",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                // Check if it exited successfully
                return process.ExitCode == 0;
            }
            catch
            {
                return false; // Assume not installed if any exception occurs
            }
        }
    }
}
