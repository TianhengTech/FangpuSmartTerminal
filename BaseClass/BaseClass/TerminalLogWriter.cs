using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace prototype_1
{
    class TerminalLogWriter
    {

        /// <summary>
        /// 输出日志到Log4Net
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>

        public static void WriteErroLog(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(msg);
        }

        public static void WriteWarnLog(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Warn(msg);
        }

        public static void WriteInfoLog(Type t, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(msg);
        }
    }
}
