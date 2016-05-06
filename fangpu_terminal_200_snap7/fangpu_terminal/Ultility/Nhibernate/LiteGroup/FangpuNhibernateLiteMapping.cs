
using FluentNHibernate.Mapping;

namespace fangpu_terminal.Ultility.Nhibernate.LiteGroup
{
    public class historydataMapping : ClassMap<historydata_lite>
    {
        public historydataMapping()
        {
            Table("historydata");
            Id<int>("historydataid").GeneratedBy.Identity();
            Map(m => m.id).Unique();
            Map(m => m.cycletime).Nullable();
            Map(m => m.jinliao_consume_seconds).Nullable();
            Map(m => m.kaoliao_consume_seconds).Nullable();
            Map(m => m.kaoliao_temp).Nullable();
            Map(m => m.kaomo_consume_seconds).Nullable();
            Map(m => m.kaomo_temp).Nullable();
            Map(m => m.lengque_consume_seconds).Nullable();
            Map(m => m.shuayou_consume_seconds).Nullable();
            Map(m => m.recordtime);
            Map(m => m.systus).Nullable();
            Map(m => m.data);
        }
    }
    public class proceduretechnologybaseMapping : ClassMap<proceduretechnologybase_lite>
    {
        public proceduretechnologybaseMapping()
        {
            Table("proceduretechnologybase");
            Id<int>("idproceduretechnologybase").GeneratedBy.Identity();
            Map(m => m.idproceduretechnologybase).Unique();
            Map(m => m.hutao_length_base).Nullable();
            Map(m => m.hutao_length_lower).Nullable();
            Map(m => m.hutao_length_upper).Nullable();
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
            Map(m => m.material).Nullable();
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
    public class warninfoMapping : ClassMap<warninfo_lite>
    {
        public warninfoMapping()
        {
            Table("warninfo");
            Id<int>("warninfoid").GeneratedBy.Identity();
            Map(m => m.warninfoid).Unique();
            Map(m => m.warninfo);
            Map(m => m.warntime);
        }
    }
}
