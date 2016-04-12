using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace prototype_1
{
    class TerminalQueues
    {
        public static ConcurrentQueue<PlcCommand> plccommandqueue = new ConcurrentQueue<PlcCommand>();  //plc通信队列
        public static ConcurrentQueue<PlcDAQCommunicationObject> plcdataprocessqueue = new ConcurrentQueue<PlcDAQCommunicationObject>();    //plc数据处理队列
        public static ConcurrentQueue<PlcDAQCommunicationObject> localdataqueue = new ConcurrentQueue<PlcDAQCommunicationObject>();//本地数据处理队列
        public static ConcurrentQueue<PlcDAQCommunicationObject> datacenterprocessqueue = new ConcurrentQueue<PlcDAQCommunicationObject>();     //云数据处理队列
        public static ConcurrentQueue<PLCWarningObject> warninfoqueue = new ConcurrentQueue<PLCWarningObject>();
        public static ConcurrentQueue<PLCWarningObject> warninfoqueue_local = new ConcurrentQueue<PLCWarningObject>();
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
        public Dictionary<string, int> plc_data;
        public DateTime daq_time;

        public PlcDAQCommunicationObject()
        {
            plc_data = new Dictionary<string, int>();
            daq_time = DateTime.Now;
        }
    }
}
