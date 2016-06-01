using FluentNHibernate.Mapping;

namespace fangpu_terminal.Ultility.Nhibernate
{
    public class terminalcmdMapping : ClassMap<terminalcmd>
    {
        public terminalcmdMapping()
        {
            Table("terminalcmd");
            Id<int>("idterminalcmd").GeneratedBy.Identity();
            Map(m => m.idterminalcmd).Unique();
            Map(m => m.command).Nullable();
            Map(m => m.status).Nullable();
            Map(m => m.device_name).Nullable();
            Map(m => m.time).Nullable();
        }
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
            Map(m => m.warn_level).Nullable();
        }
    }

    public class realtimedataMapping : ClassMap<realtimedata>
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
    public class historydataMapping : ClassMap<historydata>
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
    public class bandchecklistMapping : ClassMap<bandchecklist>
    {
        public bandchecklistMapping()
        {
            Table("bandchecklist");
            Id<int>("idchecklist").GeneratedBy.Identity();

            Map(m => m.cardnumber);
            Map(m => m.date).Nullable();
            Map(m => m.deviceid);
            Map(m => m.materialnumber);
            Map(m => m.name);
            Map(m => m.position);
            Map(m => m.producesn);
            Map(m => m.producttype);
            Map(m => m.remark);
            Map(m => m.singleweight);
            Map(m => m.sn);
            Map(m => m.subtotal).Nullable();
            Map(m => m.total).Nullable();
            Map(m => m.waste);
            Map(m => m.weight);

        }
    }

    public class fieldchecklisMapping : ClassMap<fieldchecklist>
    {
        public fieldchecklisMapping()
        {
            Table("fieldchecklist");
            Id<int>("idchecklist").GeneratedBy.Identity();
            Map(m => m.actuallength).Nullable();
            Map(m => m.actualthickness).Nullable();
            Map(m => m.band).Nullable();
            Map(m => m.bubble).Nullable();
            Map(m => m.burn).Nullable();
            Map(m => m.confirm).Nullable();
            Map(m => m.datetime).Nullable();
            Map(m => m.datetime2).Nullable();
            Map(m => m.deformation).Nullable();
            Map(m => m.deviceid);
            Map(m => m.@double,"`double`").Nullable();
            Map(m => m.handle).Nullable();
            Map(m => m.impurity).Nullable();
            Map(m => m.injure).Nullable();
            Map(m => m.internalfail).Nullable();
            Map(m => m.irregular).Nullable();
            Map(m => m.judge).Nullable();
            Map(m => m.material).Nullable();
            Map(m => m.monitorcheck).Nullable();
            Map(m => m.name).Nullable();
            Map(m => m.producer).Nullable();
            Map(m => m.raw).Nullable();
            Map(m => m.singleweight).Nullable();
            Map(m => m.sn).Nullable();
            Map(m => m.spot).Nullable();
            Map(m => m.standardlength).Nullable();
            Map(m => m.standardthickness).Nullable();
            Map(m => m.type).Nullable();
        }
    }

    public class dailychecklistMapping : ClassMap<dailychecklist>
    {
        public dailychecklistMapping()
        {
            Table("dailychecklist");
            Id<int>("idchecklist").GeneratedBy.Identity();
            Map(m => m.airleak_check).Nullable();
            Map(m => m.airpressure_check).Nullable();
            Map(m => m.airtap_check).Nullable();
            Map(m => m.belt_check).Nullable();
            Map(m => m.changematerial).Nullable();
            Map(m => m.changemoud).Nullable();
            Map(m => m.checkdate).Nullable();
            Map(m => m.cleanmould).Nullable();
            Map(m => m.debug).Nullable();
            Map(m => m.demouldcylinder_check).Nullable();
            Map(m => m.device_error).Nullable();
            Map(m => m.deviceid);
            Map(m => m.@else,"`else`").Nullable();
            Map(m => m.furnacecylinder_check).Nullable();
            Map(m => m.gasleak_check).Nullable();
            Map(m => m.gaspressure_check).Nullable();
            Map(m => m.groundclean_check).Nullable();
            Map(m => m.name).Nullable();
            Map(m => m.normal).Nullable();
            Map(m => m.procedure,"`procedure`");
            Map(m => m.screen_check).Nullable();
            Map(m => m.shift);
            Map(m => m.surfacesensor_check).Nullable();
            Map(m => m.tableclean_check).Nullable();
            Map(m => m.tablename);
            Map(m => m.tempup).Nullable();
            Map(m => m.ventilator_check).Nullable();
            Map(m => m.wait).Nullable();
        }
    }

    public class historydata_jsoncopyMapping : ClassMap<historydata_jsoncopy>
    {
        public historydata_jsoncopyMapping()
        {
            Table("historydata_jsoncopy");
            Id<int>("historydataid").GeneratedBy.Identity();
            Map(m => m.data_json);
            Map(m => m.deviceid);
            Map(m => m.storetime);
            Map(m => m.systus);
        }        
    }
    public class haltinfoMapping : ClassMap<haltinfo>
    {
        public haltinfoMapping()
        {
            Table("haltinfo");
            Id<int>("idhaltinfo").GeneratedBy.Identity();
            Map(m => m.device_name);
            Map(m => m.halt_reason);
            Map(m => m.storetime).Nullable();
            Map(m => m.time_end).Nullable();
            Map(m => m.time_start).Nullable();
        }
    }

    public class proceduretechnologybaseMapping : ClassMap<proceduretechnologybase>
    {
        public proceduretechnologybaseMapping()
        {
            Table("proceduretechnologybase");
            Id<int>("idproceduretechnologybase").GeneratedBy.Identity();
            Map(m => m.device_name);
            Map(m => m.hutao_length_base).Nullable();
            Map(m => m.hutao_length_lower).Nullable();
            Map(m => m.hutao_length_upper).Nullable();
            Map(m => m.ID);
            Map(m => m.jinliao_consume_base).Nullable();
            Map(m => m.jinliao_consume_lower).Nullable();
            Map(m => m.jinliao_consume_upper).Nullable();
            Map(m => m.kaoliao_consume_base).Nullable();
            Map(m => m.kaoliao_consume_lower).Nullable();
            Map(m => m.kaoliao_consume_upper).Nullable();
            Map(m => m.kaoliaolu_temp_base).Nullable();
            Map(m => m.kaoliaolu_temp_lower).Nullable();
            Map(m => m.kaoliaolu_temp_upper).Nullable();
            Map(m => m.kaomo_consume_base).Nullable();
            Map(m => m.kaomo_consume_lower).Nullable();
            Map(m => m.kaomo_consume_upper).Nullable();
            Map(m => m.lengque_consume_base).Nullable();
            Map(m => m.lengque_consume_lower).Nullable();
            Map(m => m.lengque_consume_upper).Nullable();
            Map(m => m.material);
            Map(m => m.product_id);
            Map(m => m.qigangjinliao_consume_base).Nullable();
            Map(m => m.qigangjinliao_consume_lower).Nullable();
            Map(m => m.qigangjinliao_consume_upper).Nullable();
            Map(m => m.shangsheng_speed_base).Nullable();
            Map(m => m.shangsheng_speed_lower).Nullable();
            Map(m => m.shangsheng_speed_upper).Nullable();
            Map(m => m.shuayou_base).Nullable();
            Map(m => m.shuayou_lower).Nullable();
            Map(m => m.shuayou_upper).Nullable();
            Map(m => m.timestamp).Nullable();
            Map(m => m.uniqueidentifier);
            Map(m => m.xiajiang_speed_base).Nullable();
            Map(m => m.xiajiang_speed_lower).Nullable();
            Map(m => m.xiajiang_speed_upper).Nullable();
            Map(m => m.yemian_distance_base).Nullable();
            Map(m => m.yemian_distance_lower).Nullable();
            Map(m => m.yemian_distance_upper).Nullable();
            Map(m => m.yurelu_temp_base).Nullable();
            Map(m => m.yurelu_temp_lower).Nullable();
            Map(m => m.yurelu_temp_upper).Nullable();
        }
    }
    public class proceduretechnologybase_workMapping : ClassMap<proceduretechnologybase_work>
    {
        public proceduretechnologybase_workMapping()
        {
            Table("proceduretechnologybase_work");
            Id<int>("idproceduretechnologybasework").GeneratedBy.Identity();
            Map(m => m.device_name);
            Map(m => m.hutao_length_base).Nullable();
            Map(m => m.hutao_length_lower).Nullable();
            Map(m => m.hutao_length_upper).Nullable();
            Map(m => m.storetime);
            Map(m => m.jinliao_consume_base).Nullable();
            Map(m => m.jinliao_consume_lower).Nullable();
            Map(m => m.jinliao_consume_upper).Nullable();
            Map(m => m.kaoliao_consume_base).Nullable();
            Map(m => m.kaoliao_consume_lower).Nullable();
            Map(m => m.kaoliao_consume_upper).Nullable();
            Map(m => m.kaoliaolu_temp_base).Nullable();
            Map(m => m.kaoliaolu_temp_lower).Nullable();
            Map(m => m.kaoliaolu_temp_upper).Nullable();
            Map(m => m.kaomo_consume_base).Nullable();
            Map(m => m.kaomo_consume_lower).Nullable();
            Map(m => m.kaomo_consume_upper).Nullable();
            Map(m => m.lengque_consume_base).Nullable();
            Map(m => m.lengque_consume_lower).Nullable();
            Map(m => m.lengque_consume_upper).Nullable();
            Map(m => m.material);
            Map(m => m.product_id);
            Map(m => m.qigangjinliao_consume_base).Nullable();
            Map(m => m.qigangjinliao_consume_lower).Nullable();
            Map(m => m.qigangjinliao_consume_upper).Nullable();
            Map(m => m.shangsheng_speed_base).Nullable();
            Map(m => m.shangsheng_speed_lower).Nullable();
            Map(m => m.shangsheng_speed_upper).Nullable();
            Map(m => m.shuayou_base).Nullable();
            Map(m => m.shuayou_lower).Nullable();
            Map(m => m.shuayou_upper).Nullable();
            Map(m => m.reason);
            Map(m => m.xiajiang_speed_base).Nullable();
            Map(m => m.xiajiang_speed_lower).Nullable();
            Map(m => m.xiajiang_speed_upper).Nullable();
            Map(m => m.yemian_distance_base).Nullable();
            Map(m => m.yemian_distance_lower).Nullable();
            Map(m => m.yemian_distance_upper).Nullable();
            Map(m => m.yurelu_temp_base).Nullable();
            Map(m => m.yurelu_temp_lower).Nullable();
            Map(m => m.yurelu_temp_upper).Nullable();
        }
    }
}
