using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UWUVCI_V3_Helper.Helpers
{
    public static class PathHelper
    {
        private static bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private static bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsAppleSilicon() 
        {
            return IsMacOS && RuntimeInformation.OSArchitecture == Architecture.Arm64;
        }

        public static string ConvertWindowsPathToUnix(string windowsPath)
        {
            if (string.IsNullOrEmpty(windowsPath))
            {
                Logger.LogInfo("Provided path is null or empty.");
                return windowsPath;
            }

            // This regex matches each path component within the argument string
            var pathPattern = @"[a-zA-Z]:[\\\/][^""\s]*"; // Match any path starting with a drive letter

            // Use Regex to find and replace each Windows-style path in the argument string
            return Regex.Replace(windowsPath, pathPattern, match =>
            {
                var matchedPath = match.Value;

                // Convert the matched Windows path to a Unix-style path after match
                return ConvertSingleWindowsPathToUnix(matchedPath);
            });
        }

        // Convert a single Windows path to Unix-style
        private static string ConvertSingleWindowsPathToUnix(string windowsPath)
        {
            // Extract the drive letter (e.g., C:) if it exists
            var drivePattern = @"^[a-zA-Z]:";

            if (Regex.IsMatch(windowsPath, drivePattern))
            {
                // Extract the drive letter and relative path
                var driveLetter = windowsPath.Substring(0, 2).ToLower(); // e.g., 'C:'
                var relativePath = windowsPath.Substring(2).TrimStart('/'); // Remove drive and leading slashes

                // 1. Check for mounted external drives first
                string? mountedPath = CheckMountedDrivesForPath(relativePath);
                if (!string.IsNullOrEmpty(mountedPath))
                {
                    Logger.LogInfo($"Mapped Windows drive {driveLetter} to mounted path: {mountedPath}");
                    return mountedPath;
                }

                // 2. Check for Wine mapping
                string? unixDrivePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? null : GetWineDriveMapping(driveLetter);
                if (!string.IsNullOrEmpty(unixDrivePath))
                    return Path.Combine(unixDrivePath, relativePath);

                // 3. If no mapping found, return as Unix path by removing the drive letter and normalizing slashes
                var newPath = relativePath.Replace("\\", "/");
                newPath = newPath.Replace(@"\\", "/");
                return newPath;
            }

            // Normalize slashes for paths without a drive letter
            return windowsPath.Replace(@"\\", "/");
        }

        public static string? CheckMountedDrivesForPath(string relativePath)
        {
            try
            {
                // Define common mount points for Linux and macOS
                string[] linuxMountPoints = { "/media", "/mnt", "/run/media", "/run/mount", "/media/removable", "/var/run/media", "/srv", "/home", "/opt", "/mount" };
                string[] macMountPoints = { "/Volumes" };

                Logger.LogInfo($"Linux: {IsLinux} & Mac: {IsMacOS}");

                string[] mountPoints = IsLinux ? linuxMountPoints : macMountPoints;

                // Define system and hidden directories to skip
                string[] systemDirs = { "/proc", "/dev", "/sys", "/run", "/tmp", "/var/run", "/var/tmp" };

                bool ShouldSkipDirectory(string path) =>
                    systemDirs.Any(d => path.StartsWith(d)) || Path.GetFileName(path).StartsWith(".");

                // Process known mount points
                foreach (var mountBase in mountPoints)
                {
                    if (Directory.Exists(mountBase))
                    {
                        foreach (var drive in Directory.GetDirectories(mountBase))
                        {
                            if (ShouldSkipDirectory(drive))
                            {
                                Logger.LogInfo($"Skipping system or hidden directory: {drive}");
                                continue;
                            }

                            var potentialPath = SearchDirectory(drive, relativePath);
                            if (potentialPath != null)
                                return potentialPath;
                        }
                    }
                }

                // Perform a deep scan for USB-like directories if on Linux
                if (IsLinux)
                {
                    string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    Logger.LogInfo($"UserHome: {userHome}");

                    string homePath = Path.Combine(userHome, relativePath);
                    Logger.LogInfo($"HomePath: {homePath}");
                    if (Directory.Exists(homePath) || File.Exists(homePath))
                    {
                        Logger.LogInfo($"Found mounted drive in /home/<user>: {homePath}");
                        return homePath;
                    }

                    // Look for USB-like directories
                    string[] externalNames = { "usb", "external", "hdd", "media", "drive" };
                    foreach (var subDir in Directory.EnumerateDirectories(userHome, "*", SearchOption.TopDirectoryOnly))
                    {
                        if (ShouldSkipDirectory(subDir))
                        {
                            Logger.LogInfo($"Skipping system or hidden directory during deep scan: {subDir}");
                            continue;
                        }

                        var potentialPath = SearchDirectory(subDir, relativePath, externalNames);
                        if (potentialPath != null)
                            return potentialPath;
                    }
                }
                else if (IsMacOS)
                {
                    foreach (var mountBase in macMountPoints)
                    {
                        if (Directory.Exists(mountBase))
                        {
                            foreach (var drive in Directory.GetDirectories(mountBase))
                            {
                                if (ShouldSkipDirectory(drive))
                                {
                                    Logger.LogInfo($"Skipping system or hidden directory: {drive}");
                                    continue;
                                }

                                var potentialPath = SearchDirectory(drive, relativePath);
                                if (potentialPath != null)
                                    return potentialPath;
                            }
                        }
                    }
                }

                Logger.LogInfo("No matching path found on mounted drives.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogError("Error while checking mounted drives", ex);
                Console.WriteLine("WARNING: Permission denied while accessing certain directories. Please run the application with elevated permissions.");
            }
            catch (IOException ex)
            {
                Logger.LogInfo($"I/O error while checking path: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error while checking mounted drives", ex);
            }

            return null;
        }

        // Helper method to recursively search for the target path, skipping any hidden or system directories
        private static string? SearchDirectory(string directory, string relativePath, string[]? keywords = null)
        {
            if (Path.GetFileName(directory).StartsWith("."))
            {
                Logger.LogInfo($"Skipping hidden directory: {directory}");
                return null;
            }

            foreach (var subDir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (Path.GetFileName(subDir).StartsWith("."))
                {
                    Logger.LogInfo($"Skipping hidden directory: {subDir}");
                    continue;
                }

                if (keywords == null || keywords.Any(keyword => subDir.ToLower().Contains(keyword)))
                {
                    var potentialPath = Path.Combine(subDir, relativePath.TrimStart('/'));
                    if (Directory.Exists(potentialPath) || File.Exists(potentialPath))
                    {
                        Logger.LogInfo($"Found potential path: {potentialPath}");
                        return potentialPath;
                    }
                }

                // Recursively search within each subdirectory
                var result = SearchDirectory(subDir, relativePath, keywords);
                if (result != null)
                    return result;
            }

            return null;
        }


        private static string? GetWineDriveMapping(string driveLetter)
        {
            try
            {
                string wineDrivePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".wine", "dosdevices", driveLetter[0].ToString().ToLower() + ":");

                if (Directory.Exists(wineDrivePath))
                    return wineDrivePath;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error while retrieving Wine drive mapping for {driveLetter}", ex);
            }

            return null;
        }
    }
}
