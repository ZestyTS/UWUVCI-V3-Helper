using System.Runtime.InteropServices;

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
            if (toolName == "wit")
            {
                if (IsMacOS())
                    toolName += "-mac";
                else if (IsLinux())
                    toolName += "-linux";
            }

            if (toolName == "wstrt")
            {
                if (IsMacOS())
                    toolName += "-mac";
                else if (IsLinux())
                    toolName += "-linux";
            }

            ToolName = toolName;

            // Parse Windows-style paths to Unix-style paths
            Arguments = ParseUnixPath(arguments);
            CurrentDirectory = ParseUnixPath(currentDirectory);

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

        // Function to replace backslashes with forward slashes and remove drive letters (like Z:)
        private static string ParseUnixPath(string arguments)
        {
            // This pattern matches paths that look like "C:\path\to\file" or "Z:\another\path"
            var driveLetterPattern = @"[a-zA-Z]:\\[^""\s]*";

            // Use Regex to find all Windows-style paths in the arguments
            return System.Text.RegularExpressions.Regex.Replace(arguments, driveLetterPattern, match =>
            {
                var path = match.Value;

                // Remove the drive letter (first two characters, e.g., "C:")
                path = path.Substring(2);

                // Replace backslashes with forward slashes
                path = path.Replace("\\", "/");

                // Ensure the path starts with a "/"
                return path.StartsWith('/') ? path : "/" + path;
            });
        }
    }
}
