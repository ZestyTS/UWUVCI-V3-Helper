using Xunit;
using UWUVCI_V3_Helper.Models;

namespace UWUVCI_V3_Helper_Tests
{
    public class ToolStepTests
    {
        [Fact]
        public void ToolStep_ShouldConvertWindowsToUnixPathInArguments()
        {
            // Simulate macOS or Linux environment
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"Z:\\path\\to\\destination.iso\"";
            var currentDirectory = "Z:\\path\\to\\directory";
            var function = "Wii";

            // Create the ToolStep object
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function);

            // Validate that the arguments' paths are converted to Unix-style
            Assert.Equal("copy --source \"/path/to/source.wbfs\" --dest \"/path/to/destination.iso\"", toolStep.Arguments);
        }

        [Fact]
        public void ToolStep_ShouldConvertWindowsToUnixPathInCurrentDirectory()
        {
            // Simulate macOS or Linux environment
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"Z:\\path\\to\\destination.iso\"";
            var currentDirectory = "Z:\\path\\to\\directory";
            var function = "Wii";

            // Create the ToolStep object
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function);

            // Validate that the current directory path is converted to Unix-style
            Assert.Equal("/path/to/directory", toolStep.CurrentDirectory);
        }
    }
}
