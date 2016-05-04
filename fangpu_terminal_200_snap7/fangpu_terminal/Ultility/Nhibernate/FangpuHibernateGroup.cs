using NHibernate.Mapping;
using System;
using FluentNHibernate.Mapping;

namespace fangpu_terminal.Ultility.Nhibernate
{
    public class warn_info_hiber
    {
        public virtual int idwarn_info { get; set; }
        public virtual string device_name { get; set; }
        public virtual string warn_message { get; set; }
        public virtual DateTime storetime { get; set; }
    }

    public  class realtimedata_hiber
    {
        public virtual int realtimedataid { get; set; }
        public virtual string deviceid { get; set; }
        public virtual string value { get; set; }
        public virtual float shuayou_consume_seconds { get; set; }
        public virtual float kaomo_consume_seconds { get; set; }
        public virtual float kaoliao_consume_seconds { get; set; }
        public virtual float lengque_consume_seconds { get; set; }
        public virtual float jinliao_consume_seconds { get; set; }
        public virtual float kaomo_temp { get; set; }
        public virtual float kaoliao_temp { get; set; }
        public virtual float cycletime { get; set; }
        public virtual DateTime storetime { get; set; }
        public virtual string systus { get; set; }
        public virtual string device_on_time { get; set; }
        public virtual string produce_time { get; set; }
        public virtual string furnace_on_time { get; set; }
    }
    public class historydata_hiber
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

    public class terminalcmd
    {
        public virtual int idterminalcmd { set; get; }
        public virtual string device_name { set; get; }
        public virtual string command { set; get; }
        public virtual DateTime time { set; get; }
    }

    public class terminalcmdMapping : ClassMap<terminalcmd>
    {
        public terminalcmdMapping()
        {
            Table("terminalcmd");
            Id<int>("idterminalcmd").GeneratedBy.Identity();
            Map(m => m.command).Nullable();
            Map(m => m.device_name).Nullable();
            Map(m => m.time).Nullable();
        }
    }

    public class warn_infoMapping : ClassMap<warn_info_hiber>
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

    public class realtimedataMapping : ClassMap<realtimedata_hiber>
    {
        public realtimedataMapping()
        {
            Table("realtimedata");
            Id<int>("realtimedataid").GeneratedBy.Identity();
            Map(m => m.cycletime).Nullable();
            Map(m => m.device_on_time).Nullable();
            Map(m => m.deviceid);
            Map(m => m.furnace_on_time).Nullable();
            Map(m => m.jinliao_consume_seconds).Nullable();
            Map(m => m.kaoliao_consume_seconds).Nullable();
            Map(m => m.kaoliao_temp).Nullable();
            Map(m => m.kaomo_consume_seconds).Nullable();
            Map(m => m.kaomo_temp).Nullable();
            Map(m => m.lengque_consume_seconds).Nullable();
            Map(m => m.produce_time).Nullable();
            Map(m => m.shuayou_consume_seconds).Nullable();
            Map(m => m.storetime).Nullable();
            Map(m => m.systus).Nullable();
            Map(m => m.value).Nullable();
        }
    }
    public class historydataMapping : ClassMap<historydata_hiber>
    {
        public historydataMapping()
        {
            Table("historydata");
            Id<int>("historydataid").GeneratedBy.Identity();
            Map(m => m.cycletime).Nullable();
            Map(m => m.deviceid);
            Map(m => m.jinliao_consume_seconds).Nullable();
            Map(m => m.kaoliao_consume_seconds).Nullable();
            Map(m => m.kaoliao_temp).Nullable();
            Map(m => m.kaomo_consume_seconds).Nullable();
            Map(m => m.kaomo_temp).Nullable();
            Map(m => m.lengque_consume_seconds).Nullable();
            Map(m => m.shuayou_consume_seconds).Nullable();
            Map(m => m.storetime);
            Map(m => m.systus).Nullable();
            Map(m => m.value);
        }
    }
}