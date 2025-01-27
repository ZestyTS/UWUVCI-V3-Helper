using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using UWUVCI_V3_Helper.Tools;
using UWUVCI_V3_Helper.Models;
using UWUVCI_V3_Helper.Helpers;

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
            // Check if the tool has a specific suffix, if it does then it's safe
            if (step.ToolName.Contains("-mac") || step.ToolName.Contains("-linux"))
                return ExecuteNativeTool(step); 

            // Now run the switch statement for other specific cases
            switch (step.ToolName)
            {
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
                _logger.LogInformation($"Attempting to run tool: {step.ToolName} with arguments: {step.Arguments}");

                bool success = RunTool(step, false); // Try native execution first
                /*
                if (!success){
                    EnsureExecutable(step.ToolName);

                    if (PathHelper.IsAppleSilicon()){
                        _logger.LogWarning($"Native execution failed on Apple Silicon. Attempting x86_64 mode via Rosetta.");

                        // Modify ToolName to include x86_64 execution
                        step.ToolName = "arch -x86_64 " + step.ToolName;
                        success = RunTool(step, true);
                    } 
                    else
                        success = RunTool(step, false);
                }
                */
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running tool {step.ToolName}: {ex.Message}");
                return false;
            }
        }

        private bool RunTool(ToolStep step, bool isFallback)
        {
            string baseDirectory = AppContext.BaseDirectory;

            using Process toolProcess = new();

            if (!isFallback)
            {   
                toolProcess.StartInfo.FileName = step.ToolName;
                toolProcess.StartInfo.Arguments = step.Arguments;
            } 
            else 
            {
                toolProcess.StartInfo.FileName = "/usr/bin/arch";
                toolProcess.StartInfo.Arguments = $"-x86_64 {Path.Combine(baseDirectory, step.ToolName)} {step.Arguments}";
            }

            toolProcess.StartInfo.WorkingDirectory = step.CurrentDirectory;
            toolProcess.StartInfo.RedirectStandardOutput = true;
            toolProcess.StartInfo.RedirectStandardError = true;
            toolProcess.StartInfo.UseShellExecute = false;
            toolProcess.StartInfo.CreateNoWindow = true;

            try
            {
                toolProcess.Start();

                // Timeout logic
                bool exited = toolProcess.WaitForExit(5000); // 5 seconds timeout

                if (!exited)
                {
                    toolProcess.Kill(); // Kill the process if it exceeds the timeout
                    _logger.LogError($"Tool {step.ToolName} timed out and was terminated.");
                    return false;
                }

                // Capture output and error
                string output = toolProcess.StandardOutput.ReadToEnd();
                string error = toolProcess.StandardError.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(output))
                    _logger.LogInformation($"Output ({(isFallback ? "x86_64 mode" : "native")}): {output}");

                if (!string.IsNullOrWhiteSpace(error))
                    _logger.LogWarning($"Error ({(isFallback ? "x86_64 mode" : "native")}): {error}");

                if (toolProcess.ExitCode != 0)
                {
                    _logger.LogError($"Tool {step.ToolName} failed with exit code {toolProcess.ExitCode}.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running tool {step.ToolName}: {ex.Message}");
                return false;
            }
            finally
            {
                // Ensure process resources are cleaned up
                if (!toolProcess.HasExited)
                    toolProcess.Kill();

                toolProcess.Dispose();
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
