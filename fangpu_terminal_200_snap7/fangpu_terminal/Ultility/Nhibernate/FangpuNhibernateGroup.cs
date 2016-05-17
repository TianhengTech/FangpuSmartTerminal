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
        public virtual string warn_level { get; set; }
        public virtual DateTime storetime { get; set; }
    }

    public  class realtimedata
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

    public class terminalcmd
    {
        public virtual int idterminalcmd { set; get; }
        public virtual string device_name { set; get; }
        public virtual string command { set; get; }
        public virtual string status { set; get; }
        public virtual DateTime time { set; get; }
    }
    public class bandchecklist
    {
        public virtual int idchecklist { get; set; }
        public virtual string deviceid { get; set; }
        public virtual string sn { get; set; }
        public virtual Nullable<DateTime> date { get; set; }
        public virtual string position { get; set; }
        public virtual string name { get; set; }
        public virtual string producesn { get; set; }
        public virtual string cardnumber { get; set; }
        public virtual string weight { get; set; }
        public virtual string singleweight { get; set; }
        public virtual string materialnumber { get; set; }
        public virtual string waste { get; set; }
        public virtual string producttype { get; set; }
        public virtual string remark { get; set; }
        public virtual Nullable<double> subtotal { get; set; }
        public virtual Nullable<double> total { get; set; }
    }
    public  class fieldchecklist
    {
        public virtual int idchecklist { get; set; }
        public virtual string deviceid { get; set; }
        public virtual string producer { get; set; }
        public virtual string type { get; set; }
        public virtual string datetime { get; set; }
        public virtual string material { get; set; }
        public virtual Nullable<double> singleweight { get; set; }
        public virtual Nullable<double> standardlength { get; set; }
        public virtual Nullable<double> actuallength { get; set; }
        public virtual Nullable<double> standardthickness { get; set; }
        public virtual Nullable<double> actualthickness { get; set; }
        public virtual Nullable<bool> burn { get; set; }
        public virtual Nullable<bool> bubble { get; set; }
        public virtual Nullable<bool> irregular { get; set; }
        public virtual Nullable<bool> impurity { get; set; }
        public virtual Nullable<bool> deformation { get; set; }
        public virtual Nullable<bool> injure { get; set; }
        public virtual Nullable<bool> raw { get; set; }
        public virtual Nullable<bool> band { get; set; }
        public virtual Nullable<bool> spot { get; set; }
        public virtual Nullable<bool> internalfail { get; set; }
        public virtual Nullable<bool> @double { get; set; }
        public virtual Nullable<bool> judge { get; set; }
        public virtual string handle { get; set; }
        public virtual string monitorcheck { get; set; }
        public virtual string name { get; set; }
        public virtual Nullable<System.DateTime> datetime2 { get; set; }
        public virtual string confirm { get; set; }
        public virtual string sn { get; set; }
    }
    public class dailychecklist
    {
        public virtual int idchecklist { get; set; }
        public virtual string deviceid { get; set; }
        public virtual string procedure { get; set; }
        public virtual string name { get; set; }
        public virtual string shift { get; set; }
        public virtual Nullable<System.DateTime> checkdate { get; set; }
        public virtual Nullable<bool> airpressure_check { get; set; }
        public virtual Nullable<bool> gaspressure_check { get; set; }
        public virtual Nullable<bool> airleak_check { get; set; }
        public virtual Nullable<bool> gasleak_check { get; set; }
        public virtual Nullable<bool> belt_check { get; set; }
        public virtual Nullable<bool> furnacecylinder_check { get; set; }
        public virtual Nullable<bool> surfacesensor_check { get; set; }
        public virtual Nullable<bool> demouldcylinder_check { get; set; }
        public virtual Nullable<bool> airtap_check { get; set; }
        public virtual Nullable<bool> ventilator_check { get; set; }
        public virtual Nullable<bool> screen_check { get; set; }
        public virtual Nullable<bool> groundclean_check { get; set; }
        public virtual Nullable<bool> tableclean_check { get; set; }
        public virtual Nullable<float> normal { get; set; }
        public virtual Nullable<float> debug { get; set; }
        public virtual Nullable<float> tempup { get; set; }
        public virtual Nullable<float> cleanmould { get; set; }
        public virtual Nullable<float> changemoud { get; set; }
        public virtual Nullable<float> changematerial { get; set; }
        public virtual Nullable<float> device_error { get; set; }
        public virtual Nullable<float> wait { get; set; }
        public virtual Nullable<float> @else { get; set; }
        public virtual string tablename { get; set; }
    }
    public class historydata_jsoncopy
    {
        public virtual int historydataid { get; set; }
        public virtual string deviceid { get; set; }
        public virtual string data_json { get; set; }
        public virtual DateTime storetime { get; set; }
        public virtual string systus { get; set; }
    }

    public class haltinfo
    {
        public virtual int idhaltinfo { get; set; }
        public virtual string device_name { get; set; }
        public virtual string halt_reason { get; set; }
        public virtual Nullable<System.DateTime> time_start { get; set; }
        public virtual Nullable<System.DateTime> time_end { get; set; }
        public virtual Nullable<System.DateTime> storetime { get; set; }
    }
    public  class proceduretechnologybase
    {
        public virtual int idproceduretechnologybase { get; set; }
        public virtual string device_name { get; set; }
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
        public virtual string uniqueidentifier { get; set; }
        public virtual string ID { get; set; }
        public virtual Nullable<System.DateTime> timestamp { get; set; }
    }

    public class proceduretechnologybase_work
    {
        public virtual int idproceduretechnologybase { get; set; }
        public virtual string device_name { get; set; }
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
        public virtual DateTime storetime { get; set; }
        public virtual string reason { get; set; }
    }

  
}