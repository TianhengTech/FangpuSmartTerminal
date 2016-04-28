using NHibernate.Mapping;
using System;
using FluentNHibernate.Mapping;

namespace fangpu_terminal.Ultility.Nhibernate
{
    public class warn_info
    {
        public virtual int idwarn_info { get; set; }
        public virtual string device_name { get; set; }
        public virtual string warn_message { get; set; }
        public virtual DateTime storetime { get; set; }
    }

    public class historydata
    {
        public virtual int historydataid { set; get; }
        public virtual string deviceid { set; get; }
        public virtual string value { set; get; }
        public virtual float shuayou_consume_seconds { set; get; }
        public virtual float kaomo_consume_seconds { set; get; }
        public virtual float kaoliao_consume_seconds { set; get; }
        public virtual float lengque_consume_seconds { set; get; }
        public virtual float jinliao_consume_seconds { set; get; }
        public virtual float kaomo_temp { set; get; }
        public virtual float kaoliao_temp { set; get; }
        public virtual float cycletime { set; get; }
        public virtual DateTime storetime { set; get; }
        public virtual string systus { set; get; }
    }


    public class user_info
    {
        public virtual int iduser { get; set; }
        public virtual string username { get; set; }
        public virtual string password { get; set; }
    }


    public class warn_infoMapping : ClassMap<warn_info>
    {
        public warn_infoMapping()
        {
            Table("warn_info");
            Id<int>("idwarn_info").GeneratedBy.Identity();
            Map(m => m.device_name).Nullable();
            Map(m => m.storetime).Nullable();
            Map(m => m.warn_message).Nullable();
        }
    }
}