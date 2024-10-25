using Xunit;
using UWUVCI_V3_Helper.Helpers;
using UWUVCI_V3_Helper.Models;
using Moq;

namespace UWUVCI_V3_Helper_Tests
{
    public class ToolStepTests
    {
        [Fact]
        public void ToolStep_ShouldConvertWindowsToUnixPathInArguments()
        {
            // Arrange
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"Z:\\path\\to\\destination.iso\"";
            var currentDirectory = "Z:\\path\\to\\directory";
            var function = "Wii";

            // Act
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function);

            // Assert: Validate that the arguments' paths are converted to Unix-style
            Assert.Equal("copy --source \"/path/to/source.wbfs\" --dest \"/path/to/destination.iso\"", toolStep.Arguments);
         }

        [Fact]
        public void ToolStep_ShouldConvertWindowsToUnixPathInCurrentDirectory()
        {
            // Arrange
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"Z:\\path\\to\\destination.iso\"";
            var currentDirectory = "Z:\\path\\to\\directory";
            var function = "Wii";

            // Act
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function);

            // Assert: Validate that the current directory path is converted to Unix-style
            Assert.Equal("/path/to/directory", toolStep.CurrentDirectory);
        }

        [Fact]
        public void ToolStep_ShouldNotChangeNonWindowsPaths()
        {
            // Arrange
            var toolName = "wit";
            var arguments = "copy --source \"/path/to/source.wbfs\" --dest \"/path/to/destination.iso\"";
            var currentDirectory = "/path/to/directory";
            var function = "Wii";

            // Act
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function);

            // Assert: Validate that Unix-style paths are unchanged
            Assert.Equal(arguments, toolStep.Arguments);
            Assert.Equal(currentDirectory, toolStep.CurrentDirectory);
        }

        [Fact]
        public void ToolStep_ShouldFallbackToDriveLetterStrippingIfNoExternalDrive()
        {
            // Arrange
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"C:\\path\\to\\destination.iso\"";
            var currentDirectory = "C:\\path\\to\\directory";
            var function = "Wii";

            // Act
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function);

            // Assert: Validate that drive letters are stripped if no external drive is found
            Assert.Equal("copy --source \"/path/to/source.wbfs\" --dest \"/path/to/destination.iso\"", toolStep.Arguments);
        }

        //THESE UNIT TESST DO NOT WORK ON WINDOWS
        /*
        [Fact]
        public void ToolStep_ShouldHandleMacOSMountedDrivePathConversion()
        {
            var mockPathHelper = CreateMockPathHelperForMountedDrive();

            // Arrange
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"I:\\path\\to\\external.iso\"";
            var currentDirectory = "I:\\path\\to\\directory"; // External drive
            var function = "Wii";

            // Act
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function, mockPathHelper.Object);

            // Expected path should be mapped to macOS /Volumes (for Unix)
            string expectedArguments = "copy --source \"/path/to/source.wbfs\" --dest \"/media/external_drive/external.iso\"";
            string expectedCurrentDirectory = "/media/external_drive/directory";

            // Assert: Validate that the MacOS-mounted drive paths are handled correctly
            Assert.Equal(expectedArguments, toolStep.Arguments);
            Assert.Equal(expectedCurrentDirectory, toolStep.CurrentDirectory);
        }

        [Fact]
        public void ToolStep_ShouldHandleMountedDrivePathConversion()
        {
            // Arrange
            var toolName = "wit";
            var arguments = "copy --source \"C:\\path\\to\\source.wbfs\" --dest \"I:\\path\\to\\external.iso\"";
            var currentDirectory = "I:\\path\\to\\directory"; // External drive
            var function = "Wii";

            // Act
            var toolStep = new ToolStep(toolName, arguments, currentDirectory, function, _pathHelper);

            // Assert: Validate that the arguments' paths are converted to Unix-style and external drive is handled
            Assert.Equal("copy --source \"/path/to/source.wbfs\" --dest \"/media/external_drive/path/to/external.iso\"", toolStep.Arguments);
            Assert.Equal("/media/external_drive/path/to/directory", toolStep.CurrentDirectory);
        }
        */
    }
}
