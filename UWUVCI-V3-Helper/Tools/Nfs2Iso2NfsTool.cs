using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nfs2iso2nfs;
using System;

namespace UWUVCI_V3_Helper.Tools
{
    public class NFS2Iso2NfsTool
    {
        private readonly ILogger _logger;

        public NFS2Iso2NfsTool(ILogger logger)
        {
            _logger = logger;
        }

        // Map arguments and execute the tool
        public bool Execute(string[] args)
        {
            try
            {
                var config = MapArgumentsToConfig(args);

                if (config == null)
                {
                    _logger.LogError("Error mapping arguments to configuration.");
                    return false;
                }

                // Create an instance of the Pack class with the config and logger
                var packInstance = new Pack(config, (ILogger<Pack>?)_logger);

                // Run the conversion asynchronously
                packInstance.ConvertAsync().GetAwaiter().GetResult();

                _logger.LogInformation("nfs2iso2nfs conversion complete.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running nfs2iso2nfs: {ex.Message}");
                return false;
            }
        }

        // Map command-line arguments to the PackConfiguration object
        private PackConfiguration MapArgumentsToConfig(string[] args)
        {
            var config = new PackConfiguration();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-dec":
                        config.IsEncrypted = false;
                        break;
                    case "-enc":
                        config.IsEncrypted = true;
                        break;
                    case "-keep":
                        config.KeepFiles = true;
                        break;
                    case "-legit":
                        config.KeepLegit = true;
                        break;
                    case "-key":
                        if (i + 1 < args.Length)
                            config.KeyFilePath = args[i + 1];
                        i++;
                        break;
                    case "-wiikey":
                        if (i + 1 < args.Length)
                            config.WiiKeyFilePath = args[i + 1];
                        i++;
                        break;
                    case "-iso":
                        if (i + 1 < args.Length)
                            config.IsoFilePath = args[i + 1];
                        i++;
                        break;
                    case "-nfs":
                        if (i + 1 < args.Length)
                            config.NfsDirectory = args[i + 1];
                        i++;
                        break;
                    case "-fwimg":
                        if (i + 1 < args.Length)
                            config.FwImageFilePath = args[i + 1];
                        i++;
                        break;
                    case "-lrpatch":
                        config.MapShoulderToTrigger = true;
                        break;
                    case "-wiimote":
                        config.VerticalWiimote = true;
                        break;
                    case "-horizontal":
                        config.HorizontalWiimote = true;
                        break;
                    case "-homebrew":
                        config.HomebrewPatches = true;
                        break;
                    case "-passthrough":
                        config.PassthroughMode = true;
                        break;
                    case "-instantcc":
                        config.InstantCC = true;
                        break;
                    case "-nocc":
                        config.NoClassicController = true;
                        break;
                    case "-output":
                        if (i + 1 < args.Length)
                            config.OutputDirectory = args[i + 1];
                        break;
                    default:
                        _logger.LogWarning($"Unknown argument: {args[i]}");
                        break;
                }
            }

            return config;
        }
    }
}
