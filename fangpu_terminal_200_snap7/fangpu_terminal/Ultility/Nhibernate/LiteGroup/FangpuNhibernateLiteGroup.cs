using System;

namespace fangpu_terminal.Ultility.Nhibernate.LiteGroup
{
    public class historydata_lite
    {
        public virtual int id { get; set; }
        public virtual string data { set; get; }
        public virtual float shuayou_consume_seconds { set; get; }
        public virtual float kaomo_consume_seconds { set; get; }
        public virtual float kaoliao_consume_seconds { set; get; }
        public virtual float lengque_consume_seconds { set; get; }
        public virtual float jinliao_consume_seconds { set; get; }
        public virtual float kaomo_temp { set; get; }
        public virtual float kaoliao_temp { set; get; }
        public virtual float cycletime { set; get; }
        public virtual DateTime recordtime { set; get; }
        public virtual string systus { set; get; }
    }

    public class proceduretechnologybase_lite
    {
        public virtual int idproceduretechnologybase { get; set; }
        public virtual string product_id { get; set; }
        public virtual string material { get; set; }
        public virtual Nullable<float> shuayou_base { get; set; }
        public virtual Nullable<float> shuayou_upper { get; set; }
        public virtual Nullable<float> shuayou_lower { get; set; }
        public virtual Nullable<float> yurelu_temp_base { get; set; }
        public virtual Nullable<float> yurelu_temp_upper { get; set; }
        public virtual Nullable<float> yurelu_temp_lower { get; set; }
        public virtual Nullable<float> kaomo_consume_base { get; set; }
        public virtual Nullable<float> kaomo_consume_upper { get; set; }
        public virtual Nullable<float> kaomo_consume_lower { get; set; }
        public virtual Nullable<float> kaoliaolu_temp_base { get; set; }
        public virtual Nullable<float> kaoliaolu_temp_upper { get; set; }
        public virtual Nullable<float> kaoliaolu_temp_lower { get; set; }
        public virtual Nullable<float> kaoliao_consume_base { get; set; }
        public virtual Nullable<float> kaoliao_consume_upper { get; set; }
        public virtual Nullable<float> kaoliao_consume_lower { get; set; }
        public virtual Nullable<float> qigangjinliao_consume_base { get; set; }
        public virtual Nullable<float> qigangjinliao_consume_upper { get; set; }
        public virtual Nullable<float> qigangjinliao_consume_lower { get; set; }
        public virtual Nullable<float> lengque_consume_base { get; set; }
        public virtual Nullable<float> lengque_consume_upper { get; set; }
        public virtual Nullable<float> lengque_consume_lower { get; set; }
        public virtual Nullable<float> shangsheng_speed_base { get; set; }
        public virtual Nullable<float> shangsheng_speed_upper { get; set; }
        public virtual Nullable<float> shangsheng_speed_lower { get; set; }
        public virtual Nullable<float> xiajiang_speed_base { get; set; }
        public virtual Nullable<float> xiajiang_speed_upper { get; set; }
        public virtual Nullable<float> xiajiang_speed_lower { get; set; }
        public virtual Nullable<double> jinliao_consume_base { get; set; }
        public virtual Nullable<float> jinliao_consume_lower { get; set; }
        public virtual Nullable<float> jinliao_consume_upper { get; set; }
        public virtual Nullable<float> hutao_length_base { get; set; }
        public virtual Nullable<float> hutao_length_upper { get; set; }
        public virtual Nullable<float> hutao_length_lower { get; set; }
        public virtual Nullable<float> yemian_distance_base { get; set; }
        public virtual Nullable<float> yemian_distance_upper { get; set; }
        public virtual Nullable<float> yemian_distance_lower { get; set; }
    }

    public class warninfo_lite
    {
        public virtual int warninfoid { set; get; }
        public virtual DateTime warntime { set; get; }
        public virtual string warninfo { set; get; }
    }
    
}
