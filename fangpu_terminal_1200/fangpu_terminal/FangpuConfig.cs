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
        public static Dictionary<string,string> dbconfig=new Dictionary<string,string>();
        public static Dictionary<string, string> warnmsg = new Dictionary<string, string>();
        public static Dictionary<string, string> warnmsg_PLC1 = new Dictionary<string, string>();
        public static Dictionary<string, string> warnmsg_PLC2 = new Dictionary<string, string>();
        public static string ReadIniData(string Section, string Key)
        {
            string NoText = ""; string iniFilePath = "./fangpu_config.ini";
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder();
                GetPrivateProfileString(Section, Key, NoText, temp, 255, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        public static byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[255];

            GetPrivateProfileString(section, key, "", temp, 255, "./fangpu_config.ini");
            return temp;
        }
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
            catch(Exception e)
            { Trace.Write("配置文件读取失败!");
            }
           
        }
        public static void ReadDBIniFile(string path)
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
                        dbconfig[key] = value;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.Write("配置文件读取失败!");
            }

        }


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
        public static Dictionary<string,string> ReadInfoIniFile(string path,string section)
        {
            warnmsg.Clear();
            string[] allKeys = ReadIniAllKeys(section, path);
            foreach (string Key_Value in allKeys)
            {
                string[] _Key_Value = Key_Value.Split('=');
                string key = _Key_Value[0];
                string value = _Key_Value[1];
                warnmsg[key] = value;
            }
            return warnmsg;
        }
    }


}