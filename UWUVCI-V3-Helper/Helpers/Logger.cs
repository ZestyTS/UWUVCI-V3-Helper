using log4net;
using log4net.Config;

namespace UWUVCI_V3_Helper.Helpers
{
    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));

        static Logger()
        {
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetExecutingAssembly());

            // Programmatically configure log4net using log4net.config file
            var configFile = new FileInfo("log4net.config");
            if (configFile.Exists)
            {
                XmlConfigurator.Configure(logRepository, configFile);
            }
            else
            {
                // Fallback to a default configuration in case log4net.config is missing
                BasicConfigurator.Configure(logRepository);
                log.Warn("log4net.config not found, using basic configuration.");
            }
        }

        public static void LogInfo(string message)
        {
            log.Info(message);
        }

        public static void LogError(string message, Exception ex)
        {
            log.Error(message, ex);
        }
    }
}
