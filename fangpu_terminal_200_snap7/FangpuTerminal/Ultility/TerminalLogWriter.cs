using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fangpu_terminal
{
    class TerminalLogWriter
    {

        /// <summary>
        /// 输出日志到Log4Net
        /// </summary>
        /// <param name="t">类</param>
        /// <param name="msg">消息</param>

        public static void WriteErroLog(Type t, string msg,Exception ex=null)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Error(msg,ex);
        }

        public static void WriteWarnLog(Type t, string msg, Exception ex=null)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Warn(msg, ex);
        }

        public static void WriteInfoLog(Type t, string msg,Exception ex=null)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(t);
            log.Info(msg, ex);
        }
    }
}
