using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fangpu_terminal
{
    class PlcCommand
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
        private string _area;
        public string area
        {
            get 
            {
                return this._area;
            }
            set
            {
                if (value==TerminalCommon.S71200AreaDB||value==TerminalCommon.S71200AreaI||value==TerminalCommon.S71200AreaM
                    || value == TerminalCommon.S71200AreaQ ||value == TerminalCommon.S7200AreaS || value == TerminalCommon.S7200AreaSM || value == TerminalCommon.S7200AreaAI
                 || value == TerminalCommon.S7200AreaAQ || value == TerminalCommon.S7200AreaC || value == TerminalCommon.S7200AreaI
                 || value == TerminalCommon.S7200AreaQ || value == TerminalCommon.S7200AreaM || value == TerminalCommon.S7200AreaV
                 || value == TerminalCommon.S7200AreaT)
                {
                    this._area = value;
                }
                else
                {
                    this._area = "null";
                }
            }
        }
        private string _type;
        public string type
        {
            get
            {
                return this._type;
            }
            set
            {
                if (value == TerminalCommon.S71200DataByte || value == TerminalCommon.S71200DataBit || value == TerminalCommon.S71200DataWord || value == TerminalCommon.S71200DataDword||value == TerminalCommon.S7200DataByte || value == TerminalCommon.S7200DataBit || value == TerminalCommon.S7200DataWord)
                {
                    this._type = value;
                }
            }
        }
        public int data;
        public int addr;
        public int plc_no;
        public int section;
        private int _bitaddr;
        public int bitaddr
        {
            get
            {
                return _bitaddr;
            }
            set
            {
                if (value >= 0 && value <= 7)
                {
                    _bitaddr = value;
                }
            }
        }
        //==================================================================
        //模块名： PlcCommand
        //日期：    2015.12.11
        //功能：    建立通信指令实例
        //输入参数：PLC区域，类型，数据，地址
        //返回值：  无
        //修改记录：
        //==================================================================
        public PlcCommand(int input_plc_no,string input_area, string input_type, int input_data, int input_addr, int input_bitaddr = 0,int input_section=0)
        {
            plc_no = input_plc_no;
            area = input_area;
            section = input_section;
            type = input_type;
            data = input_data;
            addr = input_addr;
            bitaddr = input_bitaddr;
        }
    }
}
