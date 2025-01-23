using System.Runtime.InteropServices;
using UWUVCI_V3_Helper.Helpers;

namespace UWUVCI_V3_Helper.Models
{
    public class ToolStep
    {
        public string ToolName { get; set; }
        public string Arguments { get; set; }
        public string CurrentDirectory { get; set; }
        public string Function { get; set; }
        
        public ToolStep(string toolName, string arguments, string currentDirectory, string function)
        {
            // Ensure the tool name is correct for macOS/Linux
            if (toolName == "wit" || toolName == "wstrt")
                toolName += IsMacOS() ? "-mac" : "-linux";

            ToolName = toolName;

            // First check for external drive paths in arguments and current directory
            string? externalDrivePathInArgs = PathHelper.CheckMountedDrivesForPath(arguments);
            string? externalDrivePathInDir = PathHelper.CheckMountedDrivesForPath(currentDirectory);

            // If external drive paths are found, prioritize them; otherwise, convert the paths normally
            Arguments = !string.IsNullOrEmpty(externalDrivePathInArgs)
                ? externalDrivePathInArgs
                : PathHelper.ConvertWindowsPathToUnix(arguments);

            CurrentDirectory = !string.IsNullOrEmpty(externalDrivePathInDir)
                ? externalDrivePathInDir
                : PathHelper.ConvertWindowsPathToUnix(currentDirectory);

            Function = function;
        }

        private static bool IsMacOS()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        private static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}
