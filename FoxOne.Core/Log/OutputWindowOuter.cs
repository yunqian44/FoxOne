using log4net.Appender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoxOne.Core
{
    public class OutputWindowOuter : AppenderSkeleton
    {
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            System.Diagnostics.Debug.WriteLine(loggingEvent.MessageObject);
        }
    }

    public class MailOuter : AppenderSkeleton
    {
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            MailHelper.SendMail(SysConfig.AppSettings["MaintainerEmails"], "来自系统【{0}】的异常信息".FormatTo(SysConfig.SystemTitle), loggingEvent.MessageObject.ToString());
        }
    }
}
