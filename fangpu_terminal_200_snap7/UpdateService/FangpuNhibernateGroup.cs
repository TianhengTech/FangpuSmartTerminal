using System;

namespace UpdateService
{
    public class update_info
    {
        public virtual int idupdate_info { get; set; }
        public virtual string device_name { get; set; }
        public virtual string update_version { get; set; }       
        public virtual string update_message { get; set; }
        public virtual string ID { get; set; }
        public virtual DateTime updatetime { get; set; }
    }
    public class terminalcmd
    {
        public virtual int idterminalcmd { set; get; }
        public virtual string device_name { set; get; }
        public virtual string command { set; get; }
        public virtual string status { set; get; }
        public virtual DateTime time { set; get; }
    }

    

  
}