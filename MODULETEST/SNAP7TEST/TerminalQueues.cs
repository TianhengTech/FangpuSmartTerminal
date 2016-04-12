using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNAP7TEST
{
    class TerminalQueues
    {
        public static ConcurrentQueue<TempObject> plcreadqueue = new ConcurrentQueue<TempObject>();  //plc通信队列        
    }
    public class TempObject
    {
        public float temp;
        public DateTime data_time;
    }
}
