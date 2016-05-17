using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace fangpu_terminal
{
    public class fangpu_config
    {
        #region DLL库
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);
        #endregion

        public static Dictionary<string, string> addr = new Dictionary<string, string>();
        public static Dictionary<string, string> warnmsg = new Dictionary<string, string>();
        /// <summary>
        /// Read all keys from a section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string[] ReadIniAllKeys(string section, string filePath)
        {
            UInt32 MAX_BUFFER = 32767;

            string[] items = new string[0];

            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            UInt32 bytesReturned = GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, filePath);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {
                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);

                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);

            return items;
        }
        /// <summary>
        /// Get all Section names
        /// </summary>
        /// <param name="iniFile"></param>
        /// <returns></returns>
        public static string[] INIGetAllSectionNames(string iniFile)
        {
            uint MAX_BUFFER = 32767;    //默认为32767  

            string[] sections = new string[0];      //返回值  

            //申请内存  
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
            uint bytesReturned = fangpu_config.GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, iniFile);
            if (bytesReturned != 0)
            {
                //读取指定内存的内容  
                string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned).ToString();

                //每个节点之间用\0分隔,末尾有一个\0  
                sections = local.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            //释放内存  
            Marshal.FreeCoTaskMem(pReturnedString);

            return sections;
        }
        /// <summary>
        /// Read PLC Address
        /// </summary>
        /// <param name="path"></param>
        public static void ReadAddrIniFile(string path)
        {
            
            try
            {
                string[] sections = INIGetAllSectionNames(path);
                foreach (string section in sections)
                {
                    string[] allKeys = ReadIniAllKeys(section, path);
                    foreach (string Key_Value in allKeys)
                    {
                        string[] _Key_Value = Key_Value.Split('=');
                        string key = _Key_Value[0];
                        string value = _Key_Value[1];
                        addr[key] = value;
                    }
                }
            }
            catch
            { Trace.Write("配置文件读取失败!");
            }
           
        }
        /// <summary>
        /// Read warnning message
        /// </summary>
        /// <param name="path"></param>
        public static void ReadInfoIniFile(string path)
        {
            try
            {
                string[] sections = INIGetAllSectionNames(path);
                foreach (string section in sections)
                {
                    string[] allKeys = ReadIniAllKeys(section, path);
                    foreach (string Key_Value in allKeys)
                    {
                        string[] _Key_Value = Key_Value.Split('=');
                        string key = _Key_Value[0];
                        string value = _Key_Value[1];
                        warnmsg[key] = value;
                    }
                }
            }
            catch
            {
                Trace.Write("配置文件读取失败!");
            }

        }
        /// <summary>
        /// Convert KeyValue Pairs into a Dictionary
        /// </summary>
        /// <param name="allKeyValue"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertToDictionary(string[] allKeyValue)
        {
            var results = new Dictionary<string, string>();
            foreach (var Key_Value in allKeyValue)
            {
                var keyValue = Key_Value.Split('=');
                var key = keyValue[0];
                var value = keyValue[1];
                results[key] = value;
            }
            return results;
        }
    }


}