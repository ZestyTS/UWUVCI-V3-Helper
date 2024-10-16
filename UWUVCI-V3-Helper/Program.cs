using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace UWUVCI_V3_Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UWUVCI MAC/LINUX HELPER";
            Console.WriteLine("*****************************************");
            Console.WriteLine("* UWUVCI V3 MAC/LINUX HELPER            *");
            Console.WriteLine("* Made By ZestyTS                       *");
            Console.WriteLine("*****************************************");
            Console.WriteLine();

            // Set up configuration to read from appsettings.json
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
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
            string currentDirectory = Directory.GetCurrentDirectory();

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

            // Execute the workflow based on tools.json
            if (workflowManager.ExecuteWorkflow(toolsJsonPath))
            {
                Console.WriteLine("*****************************************");
                Console.WriteLine("* All steps completed!                  *");
                Console.WriteLine("* Please return to UWUVCI and           *");
                Console.WriteLine("* click the 'OK' button                 *");
                Console.WriteLine("* to continue the process.              *");
                Console.WriteLine("*****************************************");
            }
            else
                Console.WriteLine("Error: Workflow execution failed.");
        }
    }
}
