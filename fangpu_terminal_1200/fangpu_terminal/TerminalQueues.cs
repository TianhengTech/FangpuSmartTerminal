using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace fangpu_terminal
{
    class TerminalQueues
    {
        public static Queue tcpdownlinkqueue = new Queue();  //tcp数据上行队列
        public static Queue tcpuplinkqueue = new Queue();  //tcp数据上行队列

        public static ConcurrentQueue<PlcCommand> plccommandqueue = new ConcurrentQueue<PlcCommand>();  //plc通信队列
        public static ConcurrentQueue<PlcDAQCommunicationObject> plcdataprocessqueue = new ConcurrentQueue<PlcDAQCommunicationObject>();    //plc数据处理队列
        public static ConcurrentQueue<PlcDAQCommunicationObject> localdataqueue = new ConcurrentQueue<PlcDAQCommunicationObject>();
        public static ConcurrentQueue<PlcDAQCommunicationObject> datacenterprocessqueue = new ConcurrentQueue<PlcDAQCommunicationObject>();     //plc数据处理队列
        public static ConcurrentQueue<PLCWarningObject> warninfoqueue = new ConcurrentQueue<PLCWarningObject>();
    }
    public class PLCWarningObject
    {
        public string warndata;
        public DateTime warn_time;
    }
    //==================================================================
    //模块名： PlcDAQCommunicationObject
    //日期：    2015.12.11
    //功能：    建立PLC通信实例，包含一个数据字典和当前时间戳
    //输入参数：
    //返回值：  
    //修改记录：
    //==================================================================
    public class PlcDAQCommunicationObject
    {
        public Dictionary<string, byte[]> bytes_data;
        public Dictionary<string, int> int_data;
        public DateTime daq_time;

        public PlcDAQCommunicationObject()
        {
            bytes_data = new Dictionary<string, byte[]>();
            int_data = new Dictionary<string, int>();
            daq_time = DateTime.Now;
        }
    }
}
