using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.core
{
    public static class Crs_LogHelper
    {
        static ILog logger;

        static Crs_LogHelper()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(@$"{AppDomain.CurrentDomain.BaseDirectory}log4net.config"));
            logger = LogManager.GetLogger(typeof(Crs_LogHelper));
        }

        public static void Fatal(string message, Exception exception = null)
        {
            logger.Fatal(message, exception);
        }

        public static void Error(string message, Exception exception = null)
        {
            logger.Error(message, exception);
        }

        public static void Warn(string message, Exception exception = null)
        {
            logger.Warn(message, exception);
        }

        public static void Debug(string message, Exception exception = null)
        {
            logger.Debug(message, exception);
        }

        public static void Info(string message, Exception exception = null)
        {
            logger.Info(message, exception);
        }
    }
}
