using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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

namespace fangpu_terminal
{
    class S7_Socket
    {
        // Area ID
        public int AreaI = 0x81;
        public int AreaQ = 0x82;
        public int AreaM = 0x83;
        public int AreaV = 0x184;
        public int AreaD = 0x84;
        public int AreaC = 0x1E;
        public int AreaT = 0x1F;
        public int AreaAI = 0x06;//只读

        public int LenB = 0x01;
        public int LenW = 0x02;
        public int LenD = 0x04;
        public bool S7SConnected = true;
        private bool Debug = false;
        private int ServerPort = 102;
        private static byte[] result = new byte[1024];
        public Socket ClientSocket;
        private String IPStr;
        public S7_Socket(string plcip)
        {
            this.IPStr = plcip;
        }
        //连接服务器
        private bool Creat_Socket()
        {
            //设定服务器IP地址
            IPAddress IP = IPAddress.Parse(this.IPStr);
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ClientSocket.ReceiveTimeout = 1000;
                ClientSocket.Connect(new IPEndPoint(IP, ServerPort)); //配置服务器IP与端口
                if (true) Trace.WriteLine("连接服务器成功");
                return true;
            }
            catch
            {
                if (true) Trace.WriteLine("连接服务器失败！");
                return false;
            }
        }
        public static byte[] Hex2Bytes(string hex)
        {
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return result;
        }
        public static string BytesToHex(byte[] bytes)
        {
            return BytesToHex(bytes, bytes.Length);
        }
        public static string BytesToHex(byte[] bytes, int Len)
        {
            StringBuilder result = new StringBuilder();
            if (bytes != null)
            {
                for (int i = 0; i < Len; i++)
                {
                    result.Append(bytes[i].ToString("X2"));
                }
            }
            return result.ToString();
        }
        //通过 clientSocket 发送数据
        private string Send_Cmd(string Cmd)
        {
            int receiveLength = 0;
            try
            {
                //Thread.Sleep(1000);    //等待1秒钟
                ClientSocket.Send(Hex2Bytes(Cmd), Hex2Bytes(Cmd).Length, 0);
                receiveLength = ClientSocket.Receive(result, result.Length, 0);
                S7SConnected = true;
                if (Debug) Trace.WriteLine("服务器发送消息：" + BytesToHex(Hex2Bytes(Cmd)));
                if (Debug) Trace.WriteLine("服务器接收消息：" + BytesToHex(result, receiveLength));
            }
            catch (SocketException e)
            {
                Trace.WriteLine("SocketException: {0}" + e);
                S7SConnected = false;

            }
            if (receiveLength != 0)
            {
                return BytesToHex(result, receiveLength);
            }
            return "0000";
        }
        public String HexToStr(int argv, int Len)
        {
            String Str = argv.ToString("X");
            if (Str.Length < Len)
            {
                for (int i = Str.Length; i < Len; i++)
                    Str = "0" + Str;
            }
            return Str;
        }
        public bool Connect()
        {
            return Connect(0, 0, 1);
        }
        public bool Connect(int Rack, int Slot)
        {
            return Connect(Rack, Slot, 1);
        }
        //==================================================================
        //模块名： Connect
        //日期：    2015.12.11
        //功能：    尝试连接PLC
        //输入参数：
        //返回值：  是否成功
        //修改记录：
        //==================================================================
        public bool Connect(int Rack, int Slot, int ConnectType)
        {
            String ConnectCmd1,ConnectCmd2;
            int i = 0;
            if (Creat_Socket())
            {
                if (Slot == 0)
                    ConnectCmd1 = "0300001611E00000000100C0010AC1024D57C2024D57";
                else
                    ConnectCmd1 = "0300001611E00000000100C0010AC1021000C202" + this.HexToStr((Rack << 4) + Slot + (ConnectType << 8), 4);
                ConnectCmd2 = "0300001902F08032010000000000080000F0000001000101E0";
                for (i = 0; i < 50; i++)
                {
                    if (Send_Cmd(ConnectCmd1) != "0000")
                    {
                        Send_Cmd(ConnectCmd2);
                        return true;
                    }
                        
                    Thread.Sleep(100);
                    Creat_Socket();
                }
                if (i > 10) Trace.WriteLine("ConnectCmd ERROR");
            }
            return false;
        }
        //只适用于S7-200
        //==================================================================
        //模块名： Set_Cpu_State
        //日期：    2015.12.11
        //功能：    设置PLC状态
        //输入参数：Run Stop
        //返回值：  成功失败
        //修改记录：
        //==================================================================
        public bool Set_Cpu_State(string State)
        {
            string Cmd = "", Str;
            if (Creat_Socket())
            {
                Send_Cmd("0300001611E00000000200C1024D57C2024D57C0010A");
                Send_Cmd("0300001902F08032010000CCC100080000F0000001000103C0");
                if (State == "Run")
                    Cmd = "0300002502F0803201000000010014000028000000000000FD000009505F50524F4752414D";
                else if (State == "Stop")
                    Cmd = "0300002102F0803201000000010010000029000000000009505F50524F4752414D";
                else
                    Trace.WriteLine("CmdError");
                Str = Send_Cmd(Cmd);
                ClientSocket.Close();
                if (Str.Substring(0, 2) == "03")
                    return true;
            }
            return false;
        }

        //只适用于S7-1200
        //==================================================================
        //模块名： Get_Cpu_State
        //日期：    2015.12.11
        //功能：    获取PLC状态
        //输入参数：
        //返回值：  RUN OR STOP
        //修改记录：
        //==================================================================
        public string Get_Cpu_State()
        {
            string Str = Send_Cmd("0300002102F080320700000100000800080001120411440100FF09000404240000");
            if (Str.Substring(Str.Length - 33, 1) == "8")
                return "Run";
            else if (Str.Substring(Str.Length - 33, 1) == "3")
                return "Stop";
            else
                return "Get_Cpu_State Error";
        }
        //==================================================================
        //模块名： Read_Bit
        //日期：    2015.12.11
        //功能：    读一位
        //输入参数：PLC区域，地址，位
        //返回值：  位值
        //修改记录：
        //==================================================================
        public int Read_Bit(int type, int addr, int bitaddr)
        {
            if (bitaddr <= 7)
                return (Read(type, addr) >> bitaddr) & 0x01;
            Trace.WriteLine("bitaddr ERROR");
            return 0;
        }
        public int Read(int type, int addr)
        {
            return Read(type, addr, 1);
        }
        public int Read_DB(int type,int section,int addr,int size)
        {
            return Read(type + (section<<8), addr, size);
        }
        //==================================================================
        //模块名： Read
        //日期：    2015.12.11
        //功能：    读PLC变量值
        //输入参数：PLC区域，地址，数据长度(位）
        //返回值：  值
        //修改记录：
        //==================================================================
        public int Read(int type, int addr, int size)
        {
            int Len;
            string readCmd, rStr;
            if (type == 0x1E || type == 0x1F)
            {
                Len = type;
                size = 1;
            }
            else if (type == 0x06)
            {
                Len = 4;
                addr = addr * 8;
            }

            else
            {
                Len = 2;
                addr = addr * 8;
            }
            readCmd = "0300001F02F080320100000100000E00000401120A10" + HexToStr(Len, 2) + HexToStr(size, 4) + HexToStr(type, 6) + HexToStr(addr, 6);
            // Trace.WriteLine(readCmd);
            rStr = Send_Cmd(readCmd);
            //Trace.WriteLine(rStr);
            if (rStr != "")
            {
                if (rStr.Length - size * 4 > 0)
                {
                    if (type == 0x1E || type == 0x1F || type == 0x06)
                        rStr = rStr.Substring(rStr.Length - size * 4, size * 4);
                    else
                        rStr = rStr.Substring(rStr.Length - size * 2, size * 2);
                    //Trace.WriteLine(rStr);
                    return Convert.ToInt32(rStr, 16);
                }
            }


            return 0;
        }
        public int SizeOfInt(int argv)
        {
            int len = 1;
            while (argv / 0x100 != 0)
            {
                argv = argv / 0x100;
                len = len + 1;
            }
            return len;
        }
        //==================================================================
        //模块名： Write_Bit
        //日期：    2015.12.11
        //功能：    写一位
        //输入参数：PLC区域，地址，位，数据
        //返回值：  是否成功
        //修改记录：
        //==================================================================
        public bool Write_Bit(int type, int addr, int bitaddr, int data)
        {
            if (data >= 1) data = 1;
            else data = 0;
            return Write(type, addr, data, 1, bitaddr);
        }
        public bool Write(int type, int addr, int data)
        {
            return Write(type, addr, data, 1, 8);
        }
        public bool Write_DB(int type,int db,int size,int addr,int data)
        {
            return Write(type + db, addr, data, size, 8);
        }
        public bool Write(int type, int addr, int size, int data)
        {
            return Write(type, addr, data, size, 8);
        }
        //==================================================================
        //模块名： Write
        //日期：    2015.12.11
        //功能：    写
        //输入参数：PLC区域，地址，数据，数据长度(位），比特位（写位以外为8）
        //返回值：  是否成功
        //修改记录：
        //==================================================================
        public bool Write(int type, int addr, int data, int size, int bitaddr)
        {
            int Len, bitsize;
            //int size=SizeOfInt(data);
            string WriteCmd, rStr;
            if (type == 0x1E)
            {
                Len = type;
                size = 1;
                WriteCmd = "03" + HexToStr(0x23 + size * 4, 6) + "02F080320100009425000E" + HexToStr(size * 4 + 0x04, 4) + "0501120A10" + HexToStr(Len, 2) +
                    HexToStr(size, 4) + HexToStr(type, 6) + HexToStr(addr, 6) + "0004" + HexToStr(size * 0x18, 4) + "00" + HexToStr(data, size * 4) + "00";
            }
            else if (type == 0x1F)
            {
                Len = type;
                size = 1;
                WriteCmd = "03" + HexToStr(0x23 + size * 6, 6) + "02F0803201000079AC000E" + HexToStr(size * 6 + 0x04, 4) + "0501120A10" +
                    HexToStr(Len, 2) + HexToStr(size, 4) + HexToStr(type, 6) + HexToStr(addr, 6) + "0004" + HexToStr(size * 0x28, 4) + "000000" + HexToStr(data, size * 4) + "00";
            }
            else
            {
                if (bitaddr >= 8)
                {
                    addr = addr * 8;
                    bitsize = size * 8;
                    Len = 2;
                }
                else
                {
                    addr = addr * 8 + bitaddr;
                    bitsize = size;
                    Len = 1;
                }
                WriteCmd = "03" + HexToStr(0x23 + size, 6) + "02F080320100000100000E" + HexToStr(size + 0x04, 4) + "0501120A10" +
                    HexToStr(Len, 2) + HexToStr(size, 4) + HexToStr(type, 6) + HexToStr(addr, 6) +
                    HexToStr(Len + 2, 4) + HexToStr(bitsize, 4) + HexToStr(data, size * 2);
            }
            //Trace.WriteLine(WriteCmd);
            rStr = Send_Cmd(WriteCmd);
            //Trace.WriteLine(rStr);
            if (rStr.Substring(rStr.Length - 2, 2) == "FF")
            {
                //Trace.WriteLine("Write OK");
                return true;
            }
            Trace.WriteLine("错误代码：" + rStr.Substring(rStr.Length - 2, 2));
            return false;
        }
    }
}
