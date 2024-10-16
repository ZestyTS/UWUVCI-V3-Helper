using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using UWUVCI_V3_Helper.Tools;
using UWUVCI_V3_Helper.Models;

namespace UWUVCI_V3_Helper
{
    public class WorkflowManager(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        // Reads and executes the steps defined in the tools.json file
        public bool ExecuteWorkflow(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    _logger.LogError($"JSON file {jsonFilePath} not found.");
                    return false;
                }

                string jsonContent = File.ReadAllText(jsonFilePath);
                var workflowSteps = JsonConvert.DeserializeObject<List<ToolStep>>(jsonContent);

                // Check if workflowSteps is null or empty
                if (workflowSteps == null || workflowSteps.Count == 0)
                {
                    _logger.LogError("WorkflowSteps are null or empty");
                    return false;
                }

                // Ensure none of the steps are null
                foreach (var step in workflowSteps)
                {
                    if (step == null)
                    {
                        _logger.LogError("Encountered a null step in workflowSteps.");
                        continue;
                    }

                    _logger.LogInformation($"Executing step: {step.ToolName}");

                    if (!ExecuteStep(step))
                    {
                        _logger.LogError($"Step {step.ToolName} failed.");
                        return false;
                    }
                }

                // After processing, delete the file
                DeleteToolJson(jsonFilePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing workflow: {ex.Message}");
                return false;
            }
        }
        private void DeleteToolJson(string jsonFilePath)
        {
            try
            {
                if (File.Exists(jsonFilePath))
                {
                    File.Delete(jsonFilePath);
                    _logger.LogInformation($"Successfully deleted {jsonFilePath} after processing.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting JSON file {jsonFilePath}: {ex.Message}");
            }
        }

        // Execute a single step from the tools.json
        private bool ExecuteStep(ToolStep step)
        {
            // Now run the switch statement
            switch (step.ToolName)
            {
                case "wit-mac":
                case "wit-linux":
                    return ExecuteNativeTool(step); // Execute wit as a native tool
                case "nfs2iso2nfs":
                    return RunNfs2Iso2NfsTool(step);
                default:
                    _logger.LogError($"Unknown tool: {step.ToolName}");
                    return false;
            }
        }

        // Make sure that the tool files have the execute permission
        private void EnsureExecutable(string filePath)
        {
            try
            {
                Process chmod = new Process();
                chmod.StartInfo.FileName = "chmod";
                chmod.StartInfo.Arguments = $"+x \"{filePath}\"";
                chmod.StartInfo.UseShellExecute = false;
                chmod.StartInfo.CreateNoWindow = true;
                chmod.Start();
                chmod.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting executable permission for {filePath}: {ex.Message}");
            }
        }

        // Execute any native tool
        private bool ExecuteNativeTool(ToolStep step)
        {
            try
            {
                _logger.LogInformation($"Running native tool: {step.ToolName} with arguments: {step.Arguments}");

                using Process toolProcess = new();
                toolProcess.StartInfo.FileName = step.ToolName;
                toolProcess.StartInfo.Arguments = step.Arguments;
                toolProcess.StartInfo.WorkingDirectory = step.CurrentDirectory;
                toolProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                toolProcess.Start();
                toolProcess.WaitForExit();

                if (toolProcess.ExitCode != 0)
                {
                    _logger.LogError($"Native tool {step.ToolName} failed with exit code {toolProcess.ExitCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running native tool {step.ToolName}: {ex.Message}");
                return false;
            }
        }

        // Run the nfs2iso2nfs tool
        private bool RunNfs2Iso2NfsTool(ToolStep step)
        {
            _logger.LogInformation($"Running nfs2iso2nfs tool with arguments: {step.Arguments}");
            var nfsTool = new NFS2Iso2NfsTool(_logger);

            // Parse arguments from step.Arguments and execute the tool
            return nfsTool.Execute(step.Arguments.Split(' '));
        }
    }
}
