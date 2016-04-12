using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace prototype_1
{
    class TerminalCommon
    {
        /*
        S7-200
        04-S 05-SM 06-AI 07-AQ 1E-C 81-I 82-Q 83-M 184-V 1F-T
        读写函数前三个参数为：存储区类型（如上），相应存储区开始地址，数据长度或数据
        CT为16位数据，其它的可以是8位到32位数据

        S7-1200
        81-I 82-Q 83-M 84-D
        读写函数前三个参数为：存储区类型（如上），相应存储区开始地址，数据长度或数据
        D区调用需要加为DB的那个块，如程序中DB3则为0x84+0x300
        S7S.Get_Cpu_State() 只适用于S7-1200
        */

        public static string S7200AreaS = "S";
        public static string S7200AreaSM = "SM";
        public static string S7200AreaAI = "AI";
        public static string S7200AreaAQ = "AQ";
        public static string S7200AreaC = "C";
        public static string S7200AreaI = "I";
        public static string S7200AreaQ = "Q";
        public static string S7200AreaM = "M";
        public static string S7200AreaV = "V";
        public static string S7200AreaT = "T";

        public static string S7200DataByte = "byte";
        public static string S7200DataBit = "bit";
        public static string S7200DataWord = "word";
        public static string S7200DataDword = "dword";

        public static Dictionary<string, string> realtime_gui = new Dictionary<string, string>();
        public static Dictionary<string, string> warn_info = new Dictionary<string, string>();

        //获取内网IP
        public static string GetInternalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

    }
}
