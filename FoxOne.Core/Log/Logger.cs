/*********************************************************
 * 作　　者：刘海峰
 * 联系邮箱：mailTo:liuhf@FoxOne.net
 * 创建时间：2015/6/8 15:29:41
 * 描述说明：
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Configuration;
using log4net.Config;
using System.IO;

namespace FoxOne.Core
{
    public static class Logger
    {
        private const string CONFIG_FILE_NAME = "log4net.config";
        private const string CONFIG_SECTION_NAME = "log4net";
        private static ILog _ilog = LogManager.GetLogger(typeof(Logger));
        static Logger()
        {
            Configure();
        }

        private static void Configure()
        {
            if (null != ConfigurationManager.GetSection(CONFIG_SECTION_NAME))
            {
                XmlConfigurator.Configure();
            }
            else
            {
                FileInfo file;
                if (Utility.FindConfigFile(CONFIG_FILE_NAME, out file))
                {
                    XmlConfigurator.Configure(file);
                }
            }
        }

        public static ILog GetLogger(string name)
        {
            return LogManager.GetLogger(name);
        }

        public static void Info(string message)
        {
            _ilog.Info(message);
        }

        public static void Debug(string message)
        {
            _ilog.Debug(message);
        }

        public static void Debug(string format,params object[] args)
        {
            _ilog.Debug(format.FormatTo(args));
        }

        public static void Info(string format,params object[] args)
        {
            _ilog.Info(format.FormatTo(args));
        }

        public static void Error(string message, Exception ex)
        {
            _ilog.Error(message, ex);
        }

        public static ILog GetCurrentClassLogger()
        {
            return _ilog;
        }
    }
}
