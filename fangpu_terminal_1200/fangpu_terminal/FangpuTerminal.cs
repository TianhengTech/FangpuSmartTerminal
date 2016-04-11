using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using System.Threading;
using System.Data.SQLite;
using System.Data.Objects;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Timers;
using System.Runtime.InteropServices;
using Snap7;
//using DevExpress.XtraSplashScreen;


namespace fangpu_terminal
{     

    public partial class FangpuTerminal : Form
    {
        #region 成员定义
        
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        TerminalTcpClientAsync tcpobject;
 
        S7_PPI PPI;
        private byte[] Buffer_PLC1 = new byte[1024];//通信缓存
        private byte[] Buffer_PLC2 = new byte[1024];//通信缓存
        SynchronizationContext thread_updateui_syncContext = null;
        
        public delegate void UpdateText(PlcDAQCommunicationObject daq_input);
        Thread tcpuplink_dataprocess_thread;  //tcp数据上行线程队列
        Thread tcpdownlink_dataprocess_thread;  //tcp数据下行处理
        Thread plccommunication_thread;         //plc数据通信处理
        Thread plcdatahandler_thread;         //plc数据处理
        Thread datacenter_storage_thread;     //数据库存储对象
        Thread local_storage_thread;           //本地数据缓存
        Thread plcread_thread;                  //PLC读线程
        Thread plcread_thread2;
        System.Threading.Timer timer_read_interval_60s;       //60s读一次
        System.Threading.Timer timer_check_interval_60m;
        System.Threading.Timer timer_tcp_heart_connection;    //tcp心跳连接
        string[] _1DBWord;
        string[] _2DBWord;
        string mode = "manual";
        string warntext = "报警信息";
        DataSet CurveData;


        //读控制变量
        bool shuayou_consume_fudong = false;
        bool kaomo_consume_fudong = false;
        bool kaoliao_consume_fudong = false;
        bool lengque_consume_fudong = false;
        bool jinliao_consume_fudong = false;
        bool read_interval_60s_flag = false;
        bool isFirst = true;
        bool stopSendCmdtoPLC = false;
        bool enableWarn = false;
        bool typeexist = true;
        bool enableSync = true;
        bool onetime = true;
        bool buzuo = true;
        bool jinliao = true;
        int cyclenum = -1;
        int syncount = 0;
        double zuomotime = 0;
        private int vw2010 = 0, vw2014 = 0, vw2016 = 0, vw2012 = 0;
        public numberboard numberBoard;

        object shuayou_base;
        object shuayou_upper;
        object shuayou_lower;
        object yurelu_temp_base;
        object yurelu_temp_upper;
        object yurelu_temp_lower;
        object kaomo_consume_base;
        object kaomo_consume_upper;
        object kaomo_consume_lower;
        object kaoliaolu_temp_base;
        object kaoliaolu_temp_upper;
        object kaoliaolu_temp_lower;
        object kaoliao_consume_base;
        object kaoliao_consume_upper;
        object kaoliao_consume_lower;
        object qigangjinliao_consume_base;
        object qigangjinliao_consume_upper;
        object qigangjinliao_consume_lower;
        object lengque_consume_base;
        object lengque_consume_upper;
        object lengque_consume_lower;
        object jinliao_consume_base;
        object jinliao_consume_upper;
        object jinliao_consume_lower;
        object product_id ;
        object material;
        object shangsheng_speed;
        object xiajiang_speed;
        object hutao_total_length;
        object yemian_height;

        #endregion
        private S7Client S7S;
        private S7Client S7S_2;
        

        public FangpuTerminal()
        {
            
            InitializeComponent();
            Init();
        }
        //==================================================================
        //模块名：  Init
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    初始化对象，启动线程
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void Init()
        {
            //SplashScreenManager.ShowForm(typeof(TianhengLogin));
          InitGlobalParameter();
           
           // UpdateLoadGUIConfig("正在尝试连接...", 30);
          //S7S = new S7_Socket(Properties.TerminalParameters.Default.plc1_tcp_ip);
          //S7S_2 = new S7_Socket(Properties.TerminalParameters.Default.plc2_tcp_ip);
          tabPage_pg2.Parent = null;
          //S7S.Connect(0,1,1);
          //S7S_2.Connect(0,1,1);
          S7S = new S7Client();
          S7S_2 = new S7Client();
        int result1 = S7S.ConnectTo(Properties.TerminalParameters.Default.plc1_tcp_ip, 0, 0);
        int result2 = S7S_2.ConnectTo(Properties.TerminalParameters.Default.plc2_tcp_ip, 0, 0);
          
          

           //UpdateLoadGUIConfig("启动心跳连接...", 50);
          thread_updateui_syncContext = SynchronizationContext.Current;
          tcpobject = new TerminalTcpClientAsync();
          


          tcpuplink_dataprocess_thread = new Thread(TcpCommunicationThread);
          tcpuplink_dataprocess_thread.IsBackground = true;
          tcpuplink_dataprocess_thread.Priority = ThreadPriority.Lowest;
 //         tcpuplink_dataprocess_thread.Start();

          tcpdownlink_dataprocess_thread=new Thread(TcpDownlickDataProcessThread);
          //tcpdownlink_dataprocess_thread.IsBackground=true;
          //tcpdownlink_dataprocess_thread.Start();

         // UpdateLoadGUIConfig("载入中", 60);
          plcread_thread = new Thread(PlcReadCycle);
          plcread_thread.IsBackground = true;
         // plcread_thread.Priority = ThreadPriority.BelowNormal;
          plcread_thread.Start();


          plccommunication_thread = new Thread(PlcCommunicationThread);
          plccommunication_thread.IsBackground = true;
          plccommunication_thread.Priority = ThreadPriority.BelowNormal;
          //plccommunication_thread.Priority = ThreadPriority.Highest;
          plccommunication_thread.Start();

          plcdatahandler_thread = new Thread(PlcDataProcessThread);
          plcdatahandler_thread.IsBackground = true;
          plcdatahandler_thread.Priority=ThreadPriority.BelowNormal;
          plcdatahandler_thread.Start();

            //UpdateLoadGUIConfig("载入中", 80);
          datacenter_storage_thread = new Thread(DataCenterStorageThread);
          datacenter_storage_thread.IsBackground = true;
          datacenter_storage_thread.Priority = ThreadPriority.BelowNormal;
          datacenter_storage_thread.Start();

           //UpdateLoadGUIConfig("载入中", 90);
          local_storage_thread = new Thread(PlcDataLocalStorage);
          local_storage_thread.IsBackground = true;
          local_storage_thread.Priority = ThreadPriority.BelowNormal;
 //         local_storage_thread.Start();


           timer_read_interval_60s = new System.Threading.Timer(new TimerCallback(Timer_60s_handler),null,0,60000);
 //          timer_check_interval_60m = new System.Threading.Timer(new TimerCallback(DataAutoSync), null, 0, 600000);
         //  timer_tcp_heart_connection = new System.Threading.Timer(new TimerCallback(TcpToServerHeartConnect), null, 0, Properties.TerminalParameters.Default.heart_connect_interval * 1000);
           
           //UpdateLoadGUIConfig("载入中", 100);
           //SplashScreenManager.CloseForm();
        }

        //==================================================================
        //模块名： Timer_60s_handler
        //日期：    2015.12.11
        //功能：    每60秒读一次C区开机状态描述
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================

        void Timer_60s_handler(object sender)
        {
            read_interval_60s_flag = true;
            GC.Collect();
        }
        //==================================================================
        //模块名： InitGlobalParameter
        //日期：    2015.12.11
        //功能：    初始化变量
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================

        void InitGlobalParameter()
        {
            WarnInfoRead();//读取错误记录
            CurveData = new DataSet();
            DataColumn[] datacolumns=new DataColumn[]{new DataColumn("时间"),new DataColumn("温度")};
            DataColumn[] datacolumns2 = new DataColumn[] { new DataColumn("时间"), new DataColumn("温度") };
            CurveData.Tables.AddRange(new DataTable[] { new DataTable(), new DataTable() });
            CurveData.Tables[0].Columns.AddRange(datacolumns);
            CurveData.Tables[1].Columns.AddRange(datacolumns2);
            fangpu_config.ReadAddrIniFile("./fangpu_config.ini");//读取地址信息
            fangpu_config.ReadDBIniFile("./DBconfig.ini");//读取DB区分布信息，WORD地址
            _1DBWord=fangpu_config.dbconfig["1DBW5"].Split(',');
            _2DBWord = fangpu_config.dbconfig["2DBW5"].Split(',');
            //读取报警信息
            fangpu_config.warnmsg_PLC1 = new Dictionary<string, string>(fangpu_config.ReadInfoIniFile("./fangpu_warn.ini", "PLC1"));
            fangpu_config.warnmsg_PLC2 = new Dictionary<string, string>(fangpu_config.ReadInfoIniFile("./fangpu_warn.ini", "PLC2"));
            foreach(var item in fangpu_config.warnmsg_PLC1)
            {
                TerminalCommon.warn_info_PLC1[item.Key] = item.Value;
            }
            foreach (var item in fangpu_config.warnmsg_PLC2)
            {
                TerminalCommon.warn_info_PLC2[item.Key] = item.Value;
            }
           
           
        }
        //==================================================================
        //模块名： UpdateLoadGUIConfig
        //日期：    2015.12.11
        //功能：    启动界面
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================
        //public void UpdateLoadGUIConfig(string labelinfo, int progress)
        //{
        //    SplashScreenManager.Default.SendCommand(TianhengLogin.SplashScreenCommand.Setinfo, new TianhengLoginInfo() { LabelText = labelinfo, Pos = progress });
        //}

        //==================================================================
        //模块名： UpdateTextMethod
        //日期：    2015.12.11
        //功能：    委托函数，用于到主线程刷新界面
        //输入参数：要在GUI上刷新的数据
        //返回值：  无
        //修改记录：
        //==================================================================

        public void UpdateTextMethod(PlcDAQCommunicationObject daq_input)
        {
            if (enableSync == true)
            {
                if (syncount > 5)
                {
                    SwitchSync(daq_input);
                    syncount = 0;
                }
                else
                {
                    syncount++;
                }
            }
            if (tabControl_terminal.SelectedTab==tabPage_pg1)
            {

                displayDouble_pg1_kailiaolujinludianji.Value = ToSingle(daq_input.bytes_data["2DBD5_50"]);
                displayDouble_pg1_kaoliaoluchuludianji.Value = ToSingle(daq_input.bytes_data["2DBD5_54"]);
                displayDouble_pg1_yureluchuludianji.Value = ToSingle(daq_input.bytes_data["1DBD5_102"]);
                displayDouble_pg1_yurelujinludianji.Value = ToSingle(daq_input.bytes_data["1DBD5_98"]);
                if((daq_input.int_data["1I0"]&0x20) == 0x20)
                {
                    button_pg1_shengliaoxiaochezuo.Text = "到位";
                }
                else
                {
                    button_pg1_shengliaoxiaochezuo.Text = "左";
                }
                if ((daq_input.int_data["1I0"] & 0x40) == 0x40)
                {
                    button_pg1_shengliaoxiaocheyou.Text = "到位";
                }
                else
                {
                    button_pg1_shengliaoxiaocheyou.Text = "右";
                }
                if ((daq_input.int_data["2I3"] & 0x01) == 0x01)
                {
                    button_pg1_shuayoujizuo.Text = "到位";
                }
                else
                {
                    button_pg1_shuayoujizuo.Text = "左";
                }
                if ((daq_input.int_data["2I3"] & 0x02) == 0x02)
                {
                    button_pg1_shuayoujiyou.Text = "到位";
                }
                else
                {
                    button_pg1_shuayoujiyou.Text = "右";
                }
                if ((daq_input.int_data["2I0"] & 0x80) == 0x20)
                {
                    button_pg1_tuomuxiaochezuo.Text = "到位";
                }
                else
                {
                    button_pg1_tuomuxiaochezuo.Text = "左";
                }
                if ((daq_input.int_data["2I0"] & 0x40) == 0x40)
                {
                    button_pg1_tuomuxiaocheyou.Text = "到位";
                }
                else
                {
                    button_pg1_tuomuxiaocheyou.Text = "右";
                }
            
            }

            if (tabControl_terminal.SelectedTab == tabPage_pg2)
            {
                displayDouble_pg2_yureluspd.Value = ToSingle(daq_input.bytes_data["1DBD5_98"]);
                displayDouble_pg2_yureluspd2.Value = ToSingle(daq_input.bytes_data["1DBD5_102"]);
                displayDouble_pg2_kaoliaoluspd.Value = ToSingle(daq_input.bytes_data["2DBD5_50"]);
                displayDouble_pg2_kaoliaoluspd2.Value = ToSingle(daq_input.bytes_data["2DBD5_54"]);

                
            }

            if (tabControl_terminal.SelectedTab == tabPage_pg3)
            {
                displayDouble_pg3_lengqueshijiandisplay.Value = daq_input.int_data["2DBD5_38"]/1000.0f;
                displayDouble_pg3_lengqueshijianset.Value = daq_input.int_data["2DBD5_28"] / 1000.0f;
                displayDouble_pg3_kaomushijiandisplay.Value = daq_input.int_data["1DBD5_94"] / 1000.0f;
                displayDouble_pg3_kaomushijianset.Value = daq_input.int_data["1DBD5_62"] / 1000.0f;
                displayDouble_pg3_kaoliaoshijiandisplay.Value = daq_input.int_data["2DBD5_46"] / 1000.0f;
                displayDouble_pg3_kaoliaoshijianset.Value = daq_input.int_data["2DBD5_32"] / 1000.0f;
                displayDouble_pg3_bamocishu.Value = daq_input.int_data["2DBW5_36"] / 1.0f;
                displayDouble_pg3_kaomoluwen.Value = ToSingle(daq_input.bytes_data["1DBD5_20"]);
                displayDouble_pg3_jinliaoqigangxiajiangshijian.Value = daq_input.int_data["1DBD5_24"] / 1000.0f;
                displayDouble_pg3_shuayoucishu.Value = daq_input.int_data["2DBW5_138"] / 1.0f;
                displayDouble_pg3_kaoliaoluwen.Value = ToSingle(daq_input.bytes_data["1DBD5_54"]);
                displayDouble_pg3_jinliaoshijiandisplay.Value = daq_input.int_data["1DBD5_38"] / 1000.0f;

            }
            if (tabControl_terminal.SelectedTab == tabPage_pg4)
            {
                #region 炉子监控界面参数显示
                if ((daq_input.int_data["1M7"]&0x02)==0x02)
                {
                    displayString_pg4_yurelu.BackColor = Color.Red;
                }
                else
                {
                    displayString_pg4_yurelu.BackColor = Color.Black;
                }
                if((daq_input.int_data["1M9"]&0x01)==0x01)
                {
                    displayString_pg4_kaoliaolu.BackColor = Color.Red;
                }
                else
                {
                    displayString_pg4_kaoliaolu.BackColor = Color.Black;
                }
                if((daq_input.int_data["1Q2"]&0x10)==0x10)
                {
                    displayString_pg4_yurelu1weilutou.BackColor = Color.Red;
                    displayString_pg4_yureludianhuo1.Value = "开";
                    displayString_pg4_yureludianhuo1.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_yurelu1weilutou.BackColor = Color.Black;
                    displayString_pg4_yureludianhuo1.Value = "关";
                    displayString_pg4_yureludianhuo1.ForeColor = Color.Red;
                }
                if((daq_input.int_data["1Q2"]&0x40)==0x40)
                {
                    displayString_pg4_yurelu2weilutou.BackColor = Color.Red;
                    displayString_pg4_yureludianhuo2.Value = "开";
                    displayString_pg4_yureludianhuo2.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_yurelu2weilutou.BackColor = Color.Black;
                    displayString_pg4_yureludianhuo2.Value = "关";
                    displayString_pg4_yureludianhuo2.ForeColor = Color.Red;
                }
                if((daq_input.int_data["1Q3"]&0x01)==0x01)
                {
                    displayString_pg4_yurelu3weilutou.BackColor = Color.Red;
                    displayString_pg4_yureludianhuo3.Value = "开";
                    displayString_pg4_yureludianhuo3.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_yurelu3weilutou.BackColor = Color.Black;
                    displayString_pg4_yureludianhuo3.Value = "关";
                    displayString_pg4_yureludianhuo3.ForeColor = Color.Red;
                }
                if((daq_input.int_data["1Q3"]&0x40)==0x40)
                {
                    displayString_pg4_kaoliaolu1weilutou.BackColor = Color.Red;
                    displayString_pg4_kaoliaoludianhuo1.Value = "开";
                    displayString_pg4_kaoliaoludianhuo1.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_kaoliaolu1weilutou.BackColor = Color.Black;
                    displayString_pg4_kaoliaoludianhuo1.Value = "关";
                    displayString_pg4_kaoliaoludianhuo1.ForeColor = Color.Red;
                }
                if((daq_input.int_data["1Q3"]&0x10)==0x10)
                {
                    displayString_pg4_kaoliaolu2weilutou.BackColor = Color.Red;
                    displayString_pg4_kaoliaoludianhuo2.Value = "开";
                    displayString_pg4_kaoliaoludianhuo2.ForeColor = Color.Lime;

                }
                else
                {
                    displayString_pg4_kaoliaolu2weilutou.BackColor = Color.Black;
                    displayString_pg4_kaoliaoludianhuo2.Value = "关";
                    displayString_pg4_kaoliaoludianhuo2.ForeColor = Color.Red;
                }

                
                if((daq_input.int_data["1M7"]&0x02)==0x02)
                {
                    displayString_pg4_yureluyunxingzhuangtai.ForeColor = Color.Lime;
                    displayString_pg4_yureluyunxingzhuangtai.Value = "运行";
                }
                else
                {
                    displayString_pg4_yureluyunxingzhuangtai.ForeColor = Color.Red;
                    displayString_pg4_yureluyunxingzhuangtai.Value = "停止";
                }
                if((daq_input.int_data["1M310"]&0x01)==0x01)
                {
                    displayString_pg4_yurelubaojingzhuangtai.ForeColor = Color.Red;
                    displayString_pg4_yurelubaojingzhuangtai.Value = "报警";
                }
                else
                {
                    displayString_pg4_yurelubaojingzhuangtai.ForeColor = Color.Lime;
                    displayString_pg4_yurelubaojingzhuangtai.Value = "无报警";
                }

                textBox_pg4_lengqueshuitempset.Text = ToSingle(daq_input.bytes_data["1DBD5_58"]).ToString() + "℃";
                textBox_pg4_lengqueshuitemp.Text = ToSingle(daq_input.bytes_data["1DBD5_86"]).ToString() + "℃";
                textBox_pg4_kaoliaolutempset.Text = ToSingle(daq_input.bytes_data["1DBD5_54"]).ToString() + "℃";
                textBox_pg4_yurelutempset.Text = ToSingle(daq_input.bytes_data["1DBD5_20"]).ToString() + "℃";
                if((daq_input.int_data["1M9"]&0x01)==0x01)
                {
                    displayString_pg4_kaoliaoluyunxingzhuangtai.Value = "运行";
                    displayString_pg4_kaoliaoluyunxingzhuangtai.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_kaoliaoluyunxingzhuangtai.Value = "停止";
                    displayString_pg4_kaoliaoluyunxingzhuangtai.ForeColor = Color.Red;
                }
                if((daq_input.int_data["1M310"]&0x02)==0x02)
                {
                    displayString_pg4_kaoliaolubaojingzhuangtai.Value = "报警";
                    displayString_pg4_kaoliaolubaojingzhuangtai.ForeColor = Color.Red;
                }
                else
                {
                    displayString_pg4_kaoliaolubaojingzhuangtai.Value = "无报警";
                    displayString_pg4_kaoliaolubaojingzhuangtai.ForeColor = Color.Lime;
                }

                if((daq_input.int_data["1Q2"]&0x20)==0x20)
                {
                    displayString_pg4_yureluwenkong1.Value = "开";
                    displayString_pg4_yureluwenkong1.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_yureluwenkong1.Value = "关";
                    displayString_pg4_yureluwenkong1.ForeColor = Color.Red;
                }
                if((daq_input.int_data["1Q2"]&0x80)==0x80)
                {
                    displayString_pg4_yureluwenkong2.Value = "开";
                    displayString_pg4_yureluwenkong2.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_yureluwenkong2.Value = "关";
                    displayString_pg4_yureluwenkong2.ForeColor = Color.Red;
                }
                if ((daq_input.int_data["1Q3"] & 0x02) == 0x02)
                {
                    displayString_pg4_yureluwenkong3.Value = "开";
                    displayString_pg4_yureluwenkong3.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_yureluwenkong3.Value = "关";
                    displayString_pg4_yureluwenkong3.ForeColor = Color.Red;
                }
                if ((daq_input.int_data["1Q3"] & 0x08) == 0x08)
                {
                    displayString_pg4_kaoliaoluwenkong1.Value = "开";
                    displayString_pg4_kaoliaoluwenkong1.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_kaoliaoluwenkong1.Value = "关";
                    displayString_pg4_kaoliaoluwenkong1.ForeColor = Color.Red;
                }
                if ((daq_input.int_data["1Q3"] & 0x20) == 0x20)
                {
                    displayString_pg4_kaoliaoluwenkong2.Value = "开";
                    displayString_pg4_kaoliaoluwenkong2.ForeColor = Color.Lime;
                }
                else
                {
                    displayString_pg4_kaoliaoluwenkong2.Value = "关";
                    displayString_pg4_kaoliaoluwenkong2.ForeColor = Color.Red;
                }
                if ((daq_input.int_data["1M300"] & 0x01) == 0x01)
                {
                    displayString_pg4_yurelubaojing1.Value = "有";
                    displayString_pg4_yurelubaojing1.ForeColor = Color.Red;
                }
                else
                {
                    displayString_pg4_yurelubaojing1.Value = "无";
                    displayString_pg4_yurelubaojing1.ForeColor = Color.Lime;
                }
                if ((daq_input.int_data["1M300"] & 0x02) == 0x02)
                {
                    displayString_pg4_yurelubaojing2.Value = "有";
                    displayString_pg4_yurelubaojing2.ForeColor = Color.Red;
                }
                else
                {
                    displayString_pg4_yurelubaojing2.Value = "无";
                    displayString_pg4_yurelubaojing2.ForeColor = Color.Lime;
                }
                if ((daq_input.int_data["1M300"] & 0x04) == 0x04)
                {
                    displayString_pg4_yurelubaojing3.Value = "有";
                    displayString_pg4_yurelubaojing3.ForeColor = Color.Red;
                }
                else
                {
                    displayString_pg4_yurelubaojing3.Value = "无";
                    displayString_pg4_yurelubaojing3.ForeColor = Color.Lime;
                }
                if ((daq_input.int_data["1M300"] & 0x08) == 0x08)
                {
                    displayString_pg4_kaoliaolubaojing1.Value = "有";
                    displayString_pg4_kaoliaolubaojing1.ForeColor = Color.Red;
                }
                else
                {
                    displayString_pg4_kaoliaolubaojing1.Value = "无";
                    displayString_pg4_kaoliaolubaojing1.ForeColor = Color.Lime;
                }
                if ((daq_input.int_data["1M300"] & 0x10) == 0x10)
                {
                    displayString_pg4_kaoliaolubaojing2.Value = "有";
                    displayString_pg4_kaoliaolubaojing2.ForeColor = Color.Red;
                }
                else
                {
                    displayString_pg4_kaoliaolubaojing2.Value = "无";
                    displayString_pg4_kaoliaolubaojing2.ForeColor = Color.Lime;
                }
                textBox_pg4_yureluwendu1.Text = ToSingle(daq_input.bytes_data["1DBD5_66"]).ToString()+"℃";
                textBox_pg4_yureluwendu2.Text = ToSingle(daq_input.bytes_data["1DBD5_70"]).ToString() + "℃";
                textBox_pg4_yureluwendu3.Text = ToSingle(daq_input.bytes_data["1DBD5_74"]).ToString() + "℃";
                textBox_pg4_kaoliaoluwendu1.Text = ToSingle(daq_input.bytes_data["1DBD5_78"]).ToString() + "℃";
                textBox_pg4_kaoliaoluwendu2.Text = ToSingle(daq_input.bytes_data["1DBD5_82"]).ToString() + "℃";
                #endregion
            }
            if (tabControl_terminal.SelectedTab == tabPage_pg5)
            {
                textBox_pg5_spddisplay.Text = ToSingle(daq_input.bytes_data["2DBD5_62"]).ToString()+"mm/s";
                textBox_pg5_bamocishu.Text = daq_input.int_data["2DBW5_36"].ToString()+"次";
                textBox_pg5_shangshengjuli.Text = ToSingle(daq_input.bytes_data["2DBD5_20"]).ToString()+"mm";
                textBox_pg5_xiajiangsudu.Text = ToSingle(daq_input.bytes_data["2DBD5_134"]).ToString()+"mm/s";
                textBox_pg5_xiajiangsongjiajuli.Text = ToSingle(daq_input.bytes_data["2DBD5_24"]).ToString()+"mm";

            }
            if (tabControl_terminal.SelectedTab == tabPage_pg6)
            {
                textBox_pg6_spddisplay.Text = ToSingle(daq_input.bytes_data["1DBD5_110"]).ToString()+"mm/s";
                //textBox_pg6_hutaochangdu.Text = daq_input.int_data["2DBD5_62"].ToString() + "mm";
                textBox_pg6_jinliaoshijian1.Text = (daq_input.int_data["1DBD5_38"]/1000.0f).ToString() + "秒";
                textBox_pg6_jinliaoshijian2.Text = (daq_input.int_data["1DBD5_90"] / 1000.0f).ToString() + "秒";

                textBox_pg6_shangshengsudu1.Text = (daq_input.int_data["1DBW5_34"]).ToString() + "转/分";
                textBox_pg6_shangshengsudu2.Text = (daq_input.int_data["1DBW5_36"]).ToString() + "转/分";
                textBox_pg6_shanshengjuli.Text = ToSingle(daq_input.bytes_data["1DBD5_30"]).ToString() + "mm";

                textBox_pg6_xiajiangsudu1.Text = (daq_input.int_data["1DBW5_46"]).ToString() + "转/分";
                textBox_pg6_xiajiangsudu2.Text = (daq_input.int_data["1DBW5_52"]).ToString() + "转/分";
                textBox_pg6_xiajiangjuli1.Text = ToSingle(daq_input.bytes_data["1DBD5_42"]).ToString() + "mm";
                textBox_pg6_xiajiangjuli2.Text = ToSingle(daq_input.bytes_data["1DBD5_48"]).ToString() + "mm";

            }
            //if (tabControl_terminal.SelectedTab == tabPage_pg7)
            
                if (enableWarn == true && (daq_input.int_data["1M12"] & 0x01) == 0x01)
                {
                    List<string> results = WarnInfoProcess(daq_input.int_data);
                    if (results.Count != 0)
                    {
                        foreach (string Warn in results)
                        {
                            PLCWarningObject plcwarn = new PLCWarningObject();
                            if (dataGridView_warn.Rows.Count > 500)
                            {
                                dataGridView_warn.Rows.RemoveAt(499);
                            }
                            int index = dataGridView_warn.Rows.Add();
                            dataGridView_warn.Rows[index].Cells[0].Value = Warn;
                            dataGridView_warn.Rows[index].Cells[1].Value = daq_input.daq_time;
                            warntext = Warn;
                            plcwarn.warndata = Warn;
                            plcwarn.warn_time = daq_input.daq_time;
                            WarnInfoLocalStorage(Warn, daq_input.daq_time);
                            TerminalQueues.warninfoqueue.Enqueue(plcwarn);
                            dataGridView_warn.Sort(dataGridView_warn.Columns[1], ListSortDirection.Descending);
                        }                        
                        displayWarninfo.Value = warntext;
                    }
                }
               
                if(tabControl_terminal.SelectedTab == tabPage_pg8)
                {
                    textBox_pg8_kaijishijianday.Text = daq_input.int_data["1DBW5_140"].ToString()+"天";
                    textBox_pg8_kaijishijianhour.Text = daq_input.int_data["1DBW5_138"].ToString()+"小时";
                    textBox_pg8_kaijishijianminute.Text = daq_input.int_data["1DBW5_136"].ToString()+"分钟";;
                    textBox_pg8_zuoguanshijianday.Text = daq_input.int_data["1DBW5_146"].ToString()+"天";
                    textBox_pg8_zuoguanshijianhour.Text = daq_input.int_data["1DBW5_144"].ToString()+"小时";;
                    textBox_pg8_zuoguanshijianminute.Text = daq_input.int_data["1DBW5_142"].ToString()+"分钟";;
                    textBox_pg8_kailushijianday.Text = daq_input.int_data["1DBW5_134"].ToString()+"天";
                    textBox_pg8_kailushijianhour.Text = daq_input.int_data["1DBW5_132"].ToString()+"小时";;
                    textBox_pg8_kailushijianminute.Text = daq_input.int_data["1DBW5_130"].ToString()+"分钟";;

                    textBox_pg8_danmomotoushu.Text = daq_input.int_data["2DBW5_70"].ToString() + "个";
                    textBox_pg8_jihuachanliang.Text = daq_input.int_data["2DBW5_72"].ToString() + "个";
                    textBox_pg8_shijichanliang.Text = daq_input.int_data["2DBW5_74"].ToString() + "个";
                                                                                                         
                    textBox_pg8_tongji11.Text = (daq_input.int_data["1DBD5_142"] / 1000.0f).ToString("F1")+"S";
                    textBox_pg8_tongji21.Text = (daq_input.int_data["1DBD5_146"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji31.Text = (daq_input.int_data["1DBD5_150"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji41.Text = (daq_input.int_data["1DBD5_154"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji51.Text = (daq_input.int_data["1DBD5_158"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji61.Text = (daq_input.int_data["1DBD5_162"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji12.Text = (daq_input.int_data["1DBD5_154"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji22.Text = (daq_input.int_data["1DBD5_158"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji32.Text = (daq_input.int_data["1DBD5_162"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji42.Text = (daq_input.int_data["1DBD5_166"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji52.Text = (daq_input.int_data["1DBD5_170"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji62.Text = (daq_input.int_data["1DBD5_174"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji13.Text = (daq_input.int_data["2DBD5_106"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji23.Text = (daq_input.int_data["2DBD5_110"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji33.Text = (daq_input.int_data["2DBD5_114"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji43.Text = (daq_input.int_data["2DBD5_118"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji53.Text = (daq_input.int_data["2DBD5_122"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji63.Text = (daq_input.int_data["2DBD5_126"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji14.Text = (daq_input.int_data["2DBD5_82"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji24.Text = (daq_input.int_data["2DBD5_86"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji34.Text = (daq_input.int_data["2DBD5_90"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji44.Text = (daq_input.int_data["2DBD5_94"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji54.Text = (daq_input.int_data["2DBD5_98"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_tongji64.Text = (daq_input.int_data["2DBD5_102"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_lengque.Text = (daq_input.int_data["2DBD5_38"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_kaoliao.Text = (daq_input.int_data["2DBD5_32"] / 1000.0f).ToString("F1") + "S";
                    textBox_pg8_kaomo.Text = (daq_input.int_data["2DBD5_62"] / 1000.0f).ToString("F1") + "S";

                }
                if (tabControl_terminal.SelectedTab == tabPage_pg10)
                {
                    chart_temp.Series["炉头1"].Points.DataBind(CurveData.Tables[0].AsEnumerable(), "时间", "温度", "");
                    chart_temp.Series["炉头2"].Points.DataBind(CurveData.Tables[1].AsEnumerable(), "时间", "温度", "");
                }

            
            
            
                       

                    

            
            
            if ((daq_input.int_data["1M12"] & 0x01) == 0x01)
            {
                enableWarn = false;
                led_warn.BlinkerEnabled = true;
                led_warn.Value.AsBoolean = false;
            }
            else
            { 
                enableWarn = true;
                displayWarninfo.Value = "报警信息";
                led_warn.BlinkerEnabled = false;
                led_warn.Value.AsBoolean = false;
            }
            if ((daq_input.int_data["1M5"] & 0x01) == 0x01 && (led_manul.BlinkerEnabled=true))
            {
                led_manul.BlinkerEnabled = false;
                led_manul.Value.AsBoolean = true;
                led_manul.Indicator.Text = "自动";
            }
            else if (led_manul.BlinkerEnabled == false)
            {
                led_manul.Value.AsBoolean = false;
                led_manul.BlinkerEnabled = true;
                led_manul.Indicator.Text = "手动";
            }
            if ((daq_input.int_data["1M5"] & 0x02) == 0x02 && (led_pause.BlinkerEnabled = true))
            {
                led_pause.BlinkerEnabled = false;
                led_pause.Value.AsBoolean = true;
                led_pause.Indicator.Text = "启动";
            }
            else if (led_pause.BlinkerEnabled == false )
            {
                led_pause.Value.AsBoolean = false;
                led_pause.BlinkerEnabled = true;
                led_pause.Indicator.Text = "暂停";
            }
      
        }
        //==================================================================
        //模块名： TcpToServerHeartConnect
        //日期：    2015.12.21
        //功能：    定时发送心跳包
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================

        public void TcpToServerHeartConnect(object sender)
        {
            Dictionary<string, string> heartinfo = new Dictionary<string, string>();
            heartinfo["deviceid"] = Properties.TerminalParameters.Default.terminal_id;
            heartinfo["ip"] = Properties.TerminalParameters.Default.terminal_server_ip;
            heartinfo["terminalname"] = Properties.TerminalParameters.Default.terminal_name;
            heartinfo["onlinetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            heartinfo["ip"] = TerminalCommon.GetInternalIP();

            try
            {
                TerminalQueues.tcpuplinkqueue.Enqueue(Newtonsoft.Json.JsonConvert.SerializeObject(heartinfo, Formatting.Indented));
            }
            catch (Exception e)
            {
                log.Error("Tcp Uplink Data Process Error!");
                
            }
        }
        //==================================================================
        //模块名： WarnInfoRead
        //日期：    2015.12.21
        //功能：    读取过去的错误记录
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================
        public void WarnInfoRead()
        {
            SQLiteConnection m_dbConnection = new SQLiteConnection(TerminalLocalDataStorage.connectionString);
            string strSql = String.Format("select warninfo,warntime from warninfo limit 500");
            DataSet ds = new DataSet();
            DataTable dTable = new DataTable();
            ds = TerminalLocalDataStorage.Query(strSql);
            dTable = ds.Tables[0];
            for (int i = dTable.Rows.Count; i > 0; i--)
            {
                int index = dataGridView_warn.Rows.Add();
                dataGridView_warn.Rows[index].Cells[0].Value = dTable.Rows[i-1]["warninfo"];
                dataGridView_warn.Rows[index].Cells[1].Value = dTable.Rows[i-1]["warntime"];
                if(index>=499)
                {
                    break;
                }
            }
            dataGridView_warn.Sort(dataGridView_warn.Columns[1], ListSortDirection.Descending);
        }
        //==================================================================
        //模块名： SelectUpdate
        //日期：    2015.12.11
        //功能：    更新工艺参数下拉列表
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================
        public void SelectUpdate()
        {
            this.typeselect.Items.Clear();
            SQLiteConnection m_dbConnection = new SQLiteConnection(TerminalLocalDataStorage.connectionString);
            string strSql = String.Format("select product_id from proceduretechnologybase");
            DataSet ds = new DataSet();
            DataTable dTable = new DataTable();
            ds = TerminalLocalDataStorage.Query(strSql);
            dTable = ds.Tables[0];
            for(int i=0;i<dTable.Rows.Count;i++)
            {
                this.typeselect.Items.Add(dTable.Rows[i]["product_id"]);
            }
        }
        //==================================================================
        //模块名： DataAutoSync
        //日期：    2015.12.11
        //功能：    每60分钟查询一次本地数据库，检索云数据库60分钟内是否有缺失数据，并尝试回传
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================
        public void DataAutoSync(object sender)
        {
            try
            {
                MySql.Data.MySqlClient.MySqlConnection dbConn = new MySql.Data.MySqlClient.MySqlConnection("Persist Security Info=False;server=192.168.0.53;database=fangpu_datacenter;uid=root;password=tianheng123");
                MySql.Data.MySqlClient.MySqlCommand cmd = dbConn.CreateCommand();
                cmd.CommandText = "select d.storetime,n.storetime from historydata d join historydata n on(n.historydataid=d.historydataid+1) where timediff(n.storetime, d.storetime) >3 AND d.storetime BETWEEN DATE_SUB(now(), INTERVAL 1 HOUR ) AND NOW();";
                dbConn.Open();

                MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();
                DataTable dTable = new DataTable();

                if (reader.Read())
                {
                    dTable.Load(reader);
                }

                dbConn.Close();
                int i = dTable.Rows.Count;
                int j;

                try
                {
                    var mysql = new FangpuDatacenterModelEntities();
                    
                    SQLiteConnection m_dbConnection = new SQLiteConnection(TerminalLocalDataStorage.connectionString);
                    string strSql = "select * from historydata where recordtime>@time1 and recordtime<@time2";
                    for (j = 0; j < i; j++)
                    {
                        SQLiteParameter[] parameters = new SQLiteParameter[]
                         {
                        new SQLiteParameter("@time1",dTable.Rows[j][0]),
                        new SQLiteParameter("@time2",dTable.Rows[j][1]),
                         };
                        DataSet ds = new DataSet();
                        ds = TerminalLocalDataStorage.Query(strSql, parameters);
                        DataTable synctable = new DataTable();
                        synctable = ds.Tables[0];
                        for (int n = 0; n < synctable.Rows.Count; n++)
                        {
                            var mytable = new historydata();
                            mytable.deviceid = Properties.TerminalParameters.Default.terminal_name;
                            if (Convert.IsDBNull(synctable.Rows[n][1]) == false)
                            mytable.value = Convert.ToString(synctable.Rows[n][1]);
                            if (Convert.IsDBNull(synctable.Rows[n][2]) == false)
                            mytable.storetime = Convert.ToDateTime(synctable.Rows[n][2]);
                            if (Convert.IsDBNull(synctable.Rows[n][3]) == false)
                            mytable.shuayou_consume_seconds = (float)Convert.ToDouble(synctable.Rows[n][3]);
                            if (Convert.IsDBNull(synctable.Rows[n][4]) == false)
                            mytable.kaomo_consume_seconds = (float)Convert.ToDouble(synctable.Rows[n][4]);
                            if (Convert.IsDBNull(synctable.Rows[n][5]) == false)
                            mytable.kaoliao_consume_seconds = (float)Convert.ToDouble(synctable.Rows[n][5]);
                            if(Convert.IsDBNull(synctable.Rows[n][6])==false)
                            mytable.lengque_consume_seconds=(float)Convert.ToDouble(synctable.Rows[n][6]);
                            if (Convert.IsDBNull(synctable.Rows[n][7])==false)
                            mytable.jinliao_consume_seconds = (float)Convert.ToDouble(synctable.Rows[n][7]);
                            if (Convert.IsDBNull(synctable.Rows[n][8])==false)
                            mytable.kaomo_temp = (float)Convert.ToDouble(synctable.Rows[n][8]);
                            if (Convert.IsDBNull(synctable.Rows[n][9])==false)
                            mytable.kaoliao_temp = (float)Convert.ToDouble(synctable.Rows[n][9]);
                            if (Convert.IsDBNull(synctable.Rows[n][10]) == false)
                            mytable.cycletime = (float)Convert.ToDouble(synctable.Rows[n][10]);
                            if (Convert.IsDBNull(synctable.Rows[n][11]) == false)
                            mytable.systus = Convert.ToString(synctable.Rows[n][11]);
                            mysql.historydata.Add(mytable);
                        }
                    }
                    mysql.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
        //==================================================================
        //模块名： FangpuTerminal_FormClosed
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    关闭相关进程
        //输入参数：窗口关闭事件处理函数，多线程终止执行，释放tcp连接，以及PLC连接
        //返回值：  无
        //修改记录：
        //==================================================================
        private void FangpuTerminal_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if(restartbutton==false)
                {
                    if (tcpuplink_dataprocess_thread.IsAlive == true)
                {
                    tcpuplink_dataprocess_thread.Abort();
                }


                if (tcpdownlink_dataprocess_thread.IsAlive == true)
                {
                    tcpdownlink_dataprocess_thread.Abort();
                }

                if (plccommunication_thread.IsAlive == true)
                {
                    plccommunication_thread.Abort();
                }


                if (plcdatahandler_thread.IsAlive == true)
                {
                    plcdatahandler_thread.Abort();
                }

                if (datacenter_storage_thread.IsAlive == true)
                {
                    datacenter_storage_thread.Abort();
                }

                if (local_storage_thread.IsAlive == true)
                {
                    local_storage_thread.Abort();
                }

                //if (PPI.SPort.IsOpen == true)
                //{
                //    PPI.SPort.Close();
                //}
                
                tcpobject.socket_stop_connect();
                Application.ExitThread();
                }
            }
            catch { }               
        }

        //==================================================================
        //模块名： TcpCommunicationThread
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    终端与上级数据服务平台TCP连接保持线程
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void TcpCommunicationThread()
        {
            while (true)
            {
                try
                {
                    if (TerminalQueues.tcpuplinkqueue.Count > 0)
                    {
                        tcpobject.socket_send(TerminalQueues.tcpuplinkqueue.Dequeue().ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.Error("tcp发送线程出错！");
                }
                Thread.Sleep(100);
                
            }
        }

        //==================================================================
        //模块名： TcpDownlickDataProcessThread
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    tcp下行数据处理线程，响应上级tcp控制命令
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void TcpDownlickDataProcessThread()
        {
            while (true)
            {
                try
                {
                    if (TerminalQueues.tcpdownlinkqueue.Count > 0)
                    {
                    }
                }
                catch (Exception ex)
                {
                    log.Error("tcp发送线程出错！");
                }
                Thread.Sleep(100);
            }
        }

        //==================================================================
        //模块名： PlcReadCycle
        //日期：    2015.12.01
        //功能：    plc读数据，将结果放进plcdataprocessqueue队列
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        bool plc1_read_done = false;
        bool plc2_read_done = false;
        PlcDAQCommunicationObject daq_data = new PlcDAQCommunicationObject();
        public void PlcReadCycle()
        {

            string[] range;
            int start=0 ;
            int end =0;
            int size=0;
            int cycletime;
            while (true)
            {
                if (S7S.Connected() == false || S7S_2.Connected() == false)
                {
                    if (S7S.Connected() == false)
                        S7S.ConnectTo(Properties.TerminalParameters.Default.plc1_tcp_ip, 0, 0);
                    if (S7S_2.Connected() == false)
                        S7S.ConnectTo(Properties.TerminalParameters.Default.plc2_tcp_ip, 0, 0);
                    continue;
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    plc1_read_done = false;
                    foreach (var item in fangpu_config.addr)
                    {

                        if (item.Key.Substring(0, 1).Equals("1"))
                        {
                            if (item.Key.Contains('R'))
                            {
                                range = item.Value.ToString().Split('-');
                                start = Convert.ToInt16(range[0]);
                                end = Convert.ToInt16(range[1]);
                                size = end - start;
                            }
                            else
                            {
                                start = Convert.ToInt32(item.Value);
                                size = 0;
                            }
                            if (item.Key.Substring(1, 1).Equals("M"))
                            {
                                if (S7S.MBRead(start, size + 1, Buffer_PLC1) == 0)
                                    HexDump(Buffer_PLC1, start, size + 1, "1M", 1);
                            }
                            else if (item.Key.Substring(1, 1).Equals("I"))
                            {
                                if (S7S.EBRead(start, size + 1, Buffer_PLC1) == 0)
                                    HexDump(Buffer_PLC1, start, size + 1, "1I", 1);
                            }
                            else if (item.Key.Substring(1, 1).Equals("Q"))
                            {
                                if (S7S.ABRead(start, size + 1, Buffer_PLC1) == 0)
                                    HexDump(Buffer_PLC1, start, size + 1, "1Q", 1);
                            }
                            else if (item.Key.Substring(1, 2).Equals("DB"))
                            {
                                if (S7S.DBRead(Convert.ToInt16(item.Key.Substring(3, 1)), start, size + 2, Buffer_PLC1) == 0)
                                    DBDump(Buffer_PLC1, start, size + 2, "1DB", "5", _1DBWord);
                            }
                        }
                        else if (item.Key.Substring(0, 1).Equals("2"))
                        {
                            if (item.Key.Contains('R'))
                            {
                                range = item.Value.ToString().Split('-');
                                start = Convert.ToInt16(range[0]);
                                end = Convert.ToInt16(range[1]);
                                size = end - start;
                            }
                            else
                            {
                                start = Convert.ToInt32(item.Value);
                                size = 0;
                            }
                            if (item.Key.Substring(1, 1).Equals("M"))
                            {
                                if (S7S_2.MBRead(start, size + 1, Buffer_PLC2) == 0)
                                    HexDump(Buffer_PLC2, start, size + 1, "2M", 1);
                            }
                            else if (item.Key.Substring(1, 1).Equals("I"))
                            {
                                if (S7S_2.EBRead(start, size + 1, Buffer_PLC2) == 0)
                                    HexDump(Buffer_PLC2, start, size + 1, "2I", 1);
                            }
                            else if (item.Key.Substring(1, 1).Equals("Q"))
                            {
                                if (S7S_2.ABRead(start, size + 1, Buffer_PLC2) == 0)
                                    HexDump(Buffer_PLC2, start, size + 1, "2Q", 1);
                            }
                            else if (item.Key.Substring(1, 2).Equals("DB"))
                            {
                                if (S7S_2.DBRead(Convert.ToInt32(item.Key.Substring(3, 1)), start, size + 2, Buffer_PLC2) == 0)
                                    DBDump(Buffer_PLC2, start, size + 2, "2DB", "5", _2DBWord);
                            }
                        }
                    }
                    daq_data.daq_time = DateTime.Now;
                    TerminalQueues.plcdataprocessqueue.Enqueue(daq_data);
                    sw.Stop();
                    TimeSpan tsw = sw.Elapsed;
                    Trace.WriteLine(tsw.TotalMilliseconds + "ms");
                    int timeused=Convert.ToInt32(tsw.TotalMilliseconds);
                    if (tsw.TotalMilliseconds < 960)
                    {
                        Thread.Sleep(960-timeused);
                    }
                    else
                    {
                        continue;
                    }
                }


                catch (Exception e)
                {
                    
                }
                
            }
            
        }
        

        //==================================================================
        //模块名： PlcCommunicationThread
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    终端与PLC通信线程，基于以太网或者PPI进行数据采集
        //输入参数：无
        //返回值：  无
        //修改记录：读写相应寄存器，16位取负数补码时将双字高16位置0，否则会通信错误
        //==================================================================
        public void PlcCommunicationThread()
        {
            PlcCommand temp_plccmd = new PlcCommand(1, TerminalCommon.S71200AreaI, TerminalCommon.S7200DataByte, 0, 0, 0);
            
            
            while (true)
            {

                try
                {
                    if (S7S.Connected() == false || S7S_2.Connected() == false)
                    {
                        if (S7S.Connected() == false)
                            S7S.ConnectTo(Properties.TerminalParameters.Default.plc1_tcp_ip, 0, 0);
                        if (S7S_2.Connected() == false)
                            S7S.ConnectTo(Properties.TerminalParameters.Default.plc2_tcp_ip, 0, 0);
                        continue;
                    }
                        if (TerminalQueues.plccommandqueue.Count > 0)
                        {
                            byte[] buffer = new byte[1];
                            enableSync = false;
                            
                            for (; TerminalQueues.plccommandqueue.Count > 0; )
                            {
                                TerminalQueues.plccommandqueue.TryDequeue(out temp_plccmd);
                                if(temp_plccmd.plc_no==1)
                                {
                                    if (temp_plccmd.area == "M")
                                    {
                                        buffer = System.BitConverter.GetBytes(temp_plccmd.data);
                                        S7S.WriteArea(S7Client.S7AreaMK, 0, temp_plccmd.addr * 8 + temp_plccmd.bitaddr, 1, S7Client.S7WLBit, buffer);
                                        //S7S.MBWrite(temp_plccmd.addr, 1, buffer);
                                    }
                                    else if (temp_plccmd.area == "DB")
                                    {
                                        if (temp_plccmd.type == TerminalCommon.S71200DataWord)
                                        {
                                            if (temp_plccmd.data < 0)
                                            {
                                                temp_plccmd.data &= 0x0000ffff;
                                            }
                                        }
                                        //S7S.Write_DB(S7S.AreaD,temp_plccmd.section,S7S.LenB,temp_plccmd.addr, temp_plccmd.data);
                                    }
                                                               
                                }
                                else if (temp_plccmd.plc_no==12)
                                {
                                    if (temp_plccmd.area == "M")
                                    {
                                        buffer = System.BitConverter.GetBytes(temp_plccmd.data);
                                        S7S.WriteArea(S7Client.S7AreaMK, 0, temp_plccmd.addr * 8 + temp_plccmd.bitaddr, 1, S7Client.S7WLBit, buffer);
                                        S7S_2.WriteArea(S7Client.S7AreaMK, 0, temp_plccmd.addr * 8 + temp_plccmd.bitaddr, 1, S7Client.S7WLBit, buffer);
                                    }
                                }
                            else if (temp_plccmd.plc_no == 2)
                                {
                                    if (temp_plccmd.area == "M")
                                    {
                                        buffer = System.BitConverter.GetBytes(temp_plccmd.data);
                                        S7S_2.WriteArea(S7Client.S7AreaMK, 0, temp_plccmd.addr * 8 + temp_plccmd.bitaddr, 1, S7Client.S7WLBit, buffer);
                                        continue;
                                    }
                                    else if (temp_plccmd.area == "DB")
                                    {
                                        if (temp_plccmd.type == TerminalCommon.S71200DataWord)
                                        {
                                            if (temp_plccmd.data < 0)
                                            {
                                                temp_plccmd.data &= 0x0000ffff;
                                            }
                                        }
                                        //S7S_2.Write_DB(S7S.AreaD, temp_plccmd.section, S7S.LenB, temp_plccmd.addr, temp_plccmd.data);
                                    }
                                }

                      
                            }
                            enableSync = true;
                        }
                }
                catch (Exception e)
                {
                    enableSync = true;
                }
                
            }
            
        }

        //==================================================================
        //模块名： PlcCommunicationThread
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    终端与PLC通信线程，基于以太网或者PPI进行数据采集
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void PlcCommunicationThread1()
        {
            PPI = new S7_PPI(Properties.TerminalParameters.Default.plc_com_port, Properties.TerminalParameters.Default.plc_com_baudrate);
            int read_count = 0;
            PPI.Connect(0x02);
            while (true)
            {
                try
                {
                    if (PPI.SPort.IsOpen == true)
                    {
                        //    PPI.Set_Cpu_State("Run");
                        //Trace.WriteLine(PPI.Send_Cmd("6821006802006C32010000D1D1000E00060501120A100200020001840000180004001004B0F216"));
                        //Trace.WriteLine(PPI.Write_Bit(PPI.AreaM, 1, 5, 1));
                        //  Trace.WriteLine(PPI.Write(PPI.AreaV, 3, PPI.LenW, 1234));
                        read_count++;
                        if (read_count > 10)
                        {
                            Dictionary<string, int> int_data = new Dictionary<string, int>();
                            int_data["M0"] = PPI.Read(PPI.AreaM, 0, PPI.LenD);
                            int_data["M1"] = PPI.Read(PPI.AreaM, 4, PPI.LenD);

                            int_data["VB4000"] = PPI.Read(PPI.AreaV, 4000, PPI.LenD);
                            int_data["VB4004"] = PPI.Read(PPI.AreaV, 4004, PPI.LenD);
                            int_data["VB4008"] = PPI.Read(PPI.AreaV, 4008, PPI.LenD);
                            int_data["T37"] = PPI.Read(PPI.AreaT, 37, PPI.LenW);
                            int_data["T38"] = PPI.Read(PPI.AreaT, 38, PPI.LenW);
                            int_data["T39"] = PPI.Read(PPI.AreaT, 39, PPI.LenW);
                            int_data["T40"] = PPI.Read(PPI.AreaT, 40, PPI.LenW);
                            int_data["T41"] = PPI.Read(PPI.AreaT, 41, PPI.LenW);
                            int_data["C1"] = PPI.Read(PPI.AreaC, 1, PPI.LenW);
                            int_data["C2"] = PPI.Read(PPI.AreaC, 2, PPI.LenW);
                            int_data["C4"] = PPI.Read(PPI.AreaC, 4, PPI.LenW);
                            read_count = 0;
                        }

                        if (TerminalQueues.plccommandqueue.Count > 0)
                        {
                            PlcCommand temp_plccmd = new PlcCommand(1,TerminalCommon.S7200AreaI, TerminalCommon.S7200DataByte, 0, 0, 0);
                            for (; TerminalQueues.plccommandqueue.Count > 0; )
                            {
                                TerminalQueues.plccommandqueue.TryDequeue(out temp_plccmd);
                                switch (temp_plccmd.area)
                                {
                                    case "M": PPI.Write_Bit(PPI.AreaM, temp_plccmd.addr, temp_plccmd.bitaddr, temp_plccmd.data);
                                        break;
                                    case "V": PPI.Write(PPI.AreaV, temp_plccmd.addr, 2, temp_plccmd.data);
                                        break;

                                }
                                //Trace.WriteLine(temp_plccmd.area.ToString() + temp_plccmd.addr.ToString() + temp_plccmd.bitaddr.ToString() + temp_plccmd.data.ToString());
                            }
                        }
                        //PPI.SPort.Close();
                    }
                    else
                    {
                        PPI.SPort.Close();
                        PPI.Connect(0x02);
                    }
      
                }
                catch(Exception ex)
                {
                    log.Error("tcp发送线程出错！");
                }
            }
        }

        //==================================================================
        //模块名： PlcDataProcessThread
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：   plc数据处理线程，处理PLC采集到的数据，进行本地/远程数据存储，
        //本地界面数据更新操作
        //输入参数：无
        //返回值：  无
        //修改记录：用异步方式刷新GUI界面
        //==================================================================
        public void PlcDataProcessThread()
        {
            
            while (true)
            {
                try
                {
                   
                    while (TerminalQueues.plcdataprocessqueue.Count > 0)
                    {
                        PlcDAQCommunicationObject plc_temp_data = new PlcDAQCommunicationObject();//data,time
                        TerminalQueues.plcdataprocessqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data == null)
                        {
                            continue;
                        }
                        if(CurveData.Tables[0].Rows.Count>100)
                        {
                            CurveData.Tables[0].Rows.RemoveAt(0);
                            CurveData.Tables[1].Rows.RemoveAt(0);
                        }
                        CurveData.Tables[0].Rows.Add(plc_temp_data.daq_time.ToLongTimeString(), ToSingle(plc_temp_data.bytes_data["1DBD5_66"]));
                        CurveData.Tables[1].Rows.Add(plc_temp_data.daq_time.ToLongTimeString(), ToSingle(plc_temp_data.bytes_data["1DBD5_70"]));
                        CycleUpdateGuiDisplay(plc_temp_data);                   
                        TerminalQueues.datacenterprocessqueue.Enqueue(plc_temp_data);

                    }
                }
                catch (Exception ex)
                {
                    log.Error("plc数据处理线程出错！");
                }
                Thread.Sleep(200);
            }
        }

        //==================================================================
        //模块名： DataCenterStorageThread
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    数据远程存储处理线程，用于存储报警和生产流程信息
        //输入参数：无
        //返回值：  无
        //修改记录：
        //==================================================================
        public void DataCenterStorageThread()
        {
            while (true)
            {
                try
                {
                    var mysql = new FangpuDatacenterModelEntities();
                    PlcDAQCommunicationObject plc_temp_data = new PlcDAQCommunicationObject();
                    while (TerminalQueues.datacenterprocessqueue.Count > 0)
                    {
                        TerminalQueues.datacenterprocessqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data == null)
                        {
                            continue;
                        }
                        var historydata = new historydata_1200();
                        var warn_info = new warn_info();
                        historydata.device_name = Properties.TerminalParameters.Default.terminal_name;
                        historydata.storetime = plc_temp_data.daq_time;
                        historydata.pre_furnance1_temp = ToSingle(plc_temp_data.bytes_data["1DBD5_66"]);
                        historydata.pre_furnance2_temp = ToSingle(plc_temp_data.bytes_data["1DBD5_70"]);
                        historydata.pre_furnance3_temp = ToSingle(plc_temp_data.bytes_data["1DBD5_74"]);
                        historydata.burn_furnance1_temp = ToSingle(plc_temp_data.bytes_data["1DBD5_78"]);
                        historydata.burn_furnance2_temp = ToSingle(plc_temp_data.bytes_data["1DBD5_82"]);
                        mysql.historydata_1200.Add(historydata);
                        if (TerminalQueues.warninfoqueue.Count > 0)
                        {
                            PLCWarningObject plc_warn_data = new PLCWarningObject();
                            TerminalQueues.warninfoqueue.TryDequeue(out plc_warn_data);
                            if (plc_warn_data != null)
                            {
                                try
                                {
                                    warn_info.device_name = Properties.TerminalParameters.Default.terminal_name;
                                    warn_info.warn_message = plc_warn_data.warndata;
                                    warn_info.storetime = plc_warn_data.warn_time;
                                    mysql.warn_info.Add(warn_info);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    mysql.SaveChanges();
                }
                catch (Exception ex)
                {
                    log.Error("数据中心存储线程出错！");
                }
            }
        }

        //==================================================================
        //模块名： SendCommandToPlc
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    发送命令到PLC,由GUI界面操作触发
        //输入参数：string area：PLC区域；
        //          string type：数据读写类型，byte,word,bit；
        //          int data：发送数据；
        //          int addr：字节地址；
        //          int bitaddr=0:字节内地址，默认为0
        //返回值：  
        //修改记录：
        //==================================================================
        public void SendCommandToPlc(int plc_no,string area, string type, int data, int addr, int bitaddr=0,int section=0)
        {
            PlcCommand cmd = new PlcCommand(plc_no,area,type,data,addr,bitaddr,section);
            TerminalQueues.plccommandqueue.Enqueue(cmd);

        }

        //==================================================================
        //模块名： CycleUpdateGuiDisplay
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    GUI界面跨线程更新
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void CycleUpdateGuiDisplay(PlcDAQCommunicationObject daq_input)
            {
                UpdateText S1 = new UpdateText(UpdateTextMethod);
                this.BeginInvoke(S1,daq_input);
            

        }

        //==================================================================
        //模块名： WarnInfoProcess
        //作者：    Yang Chuan
        //日期：    2015.12.02
        //功能：    报警信息处理
        //输入参数：PLC读出
        //返回值：  相应的报警信息
        //修改记录：
        //==================================================================
        public List<string> WarnInfoProcess(Dictionary<string, int> info)
        {
            List<string> results = new List<string>();
            int base_zero = 0;
            int i = 0;

            for (int j = 0; j <= 6; j++)
            {
                if (info["1M30" + j.ToString()] > 0)
                {
                    for (i = 0; i <= 7; i++)
                    {
                        if ((info["1M30" + j.ToString()] & (base_zero | (1 << i))) == 1 << i)
                        {
                            results.Add(TerminalCommon.warn_info_PLC1["M30" + j.ToString() + "_" + i.ToString()]);
                        }
                    }
                }
              for (j=2; j <= 6; j++)
                if (info["2M30" + j.ToString()] > 0)
                {
                    for (i = 0; i <= 7; i++)
                    {
                        if ((info["2M30" + j.ToString()] & (base_zero | (1 << i))) == 1 << i)
                        {
                            results.Add(TerminalCommon.warn_info_PLC2["M30" + j.ToString() + "_" + i.ToString()]);
                        }
                    }
                }
            }
            return results;
        }

        //==================================================================
        //模块名： PlcDataLocalStorage
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    PLC数据本地存储
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void PlcDataLocalStorage()
        {
            int ret;
            
            while (true)
            {
                try
                {                          
                    if (TerminalQueues.localdataqueue.Count > 0)
                   {
                        PlcDAQCommunicationObject plc_temp_data = new PlcDAQCommunicationObject();
                        TerminalQueues.localdataqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data == null)
                        {
                            continue;
                        }

                        FangpuTerminalJsonModel jsonobj = new FangpuTerminalJsonModel();
                        FangpuTerminalJsonModel_systus jsonobj_2 = new FangpuTerminalJsonModel_systus(); 
                        jsonobj.V4000 = plc_temp_data.int_data["VB4000"];
                        jsonobj.V4001 = plc_temp_data.int_data["VB4001"];
                        jsonobj.V4002 = plc_temp_data.int_data["VB4002"];
                        jsonobj.V4003 = plc_temp_data.int_data["VB4003"];
                        jsonobj.V4004 = plc_temp_data.int_data["VB4004"];
                        jsonobj.V4005 = plc_temp_data.int_data["VB4005"];
                        jsonobj.V4006 = plc_temp_data.int_data["VB4006"];
                        jsonobj.V4007 = plc_temp_data.int_data["VB4007"];
                        jsonobj.V4008 = plc_temp_data.int_data["VB4008"];
                        jsonobj.M53 = ((plc_temp_data.int_data["M5"] & 0x08) == 0x08);

                        jsonobj_2.M37 = ((plc_temp_data.int_data["M3"] & 0x80) == 0x80);
                        jsonobj_2.M42 = ((plc_temp_data.int_data["M4"] & 0x04) == 0x04);
                        jsonobj_2.M52 = ((plc_temp_data.int_data["M5"] & 0x04) == 0x04);
                        jsonobj_2.M44 = ((plc_temp_data.int_data["M4"] & 0x10) == 0x10);
                        jsonobj_2.M67 = ((plc_temp_data.int_data["M6"] & 0x80) == 0x80);
                        jsonobj_2.M00 = ((plc_temp_data.int_data["M0"] & 0x01) == 0x01);
                        jsonobj_2.M01 = ((plc_temp_data.int_data["M0"] & 0x02) == 0x02);

                        StringBuilder strSql = new StringBuilder();
                        strSql.Append("insert into historydata(");
                        strSql.Append("data,systus,recordtime,shuayou_consume_seconds,kaomo_consume_seconds,kaoliao_consume_seconds,jinliao_consume_seconds,lengque_consume_seconds,cycletime)");
                        strSql.Append(" values(");
                        strSql.Append("@data,@systus,@recordtime,@shuayou_consume_seconds,@kaomo_consume_seconds,@kaoliao_consume_seconds,@jinliao_consume_seconds,@lengque_consume_seconds,@cycletime)");
                   
                        SQLiteParameter[] parameters = {  
                        TerminalLocalDataStorage.MakeSQLiteParameter("@data", DbType.String,100,JsonConvert.SerializeObject(jsonobj)), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@systus", DbType.String,100,JsonConvert.SerializeObject(jsonobj_2)), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@recordtime", DbType.DateTime,30,plc_temp_data.daq_time),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@shuayou_consume_seconds", DbType.Double,100,plc_temp_data.int_data["T38"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@kaomo_consume_seconds", DbType.Double,100,plc_temp_data.int_data["T39"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@kaoliao_consume_seconds", DbType.Double,100,plc_temp_data.int_data["T40"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@jinliao_consume_seconds", DbType.Double,100,plc_temp_data.int_data["T41"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@lengque_consume_seconds", DbType.Double,100,plc_temp_data.int_data["T37"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@cycletime", DbType.Double,100,(float)zuomotime),
                        };                                                                                      
                                                                                                                
                        if (TerminalLocalDataStorage.ExecuteSql(strSql.ToString(), parameters) >= 1)                
                        {
                            ret = 1;
                        }
                    }
            
                }
                catch (Exception ex)
                {
                    Trace.Write(ex);
                    log.Error("数据中心存储线程出错！"+DateTime.Now);
                }
                   
                //Thread.Sleep(10000);
                
            }
        }
        //==================================================================
        //模块名： WarnInfoLocalStorage
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    PLC警告信息存储
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public void WarnInfoLocalStorage(string warninfo,DateTime warntime)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into warninfo (");
            strSql.Append("warninfo,warntime)");
            strSql.Append(" values(");
            strSql.Append("@warninfo,@warntime)");
            SQLiteParameter[] parameters = {  
                        TerminalLocalDataStorage.MakeSQLiteParameter("@warninfo", DbType.String,200,warninfo), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@warntime", DbType.DateTime,30,warntime),
                        };
            TerminalLocalDataStorage.ExecuteSql(strSql.ToString(), parameters); 
        }

      
        #region 同步开关状态
        //==================================================================
        //模块名： SwitchSync
        //日期：    2015.12.11
        //功能：    GUI同步PLC开关
        //输入参数：PLC读数据通信实例
        //返回值：  无
        //修改记录：
        //==================================================================
        public void SwitchSync(PlcDAQCommunicationObject temp)
        {
            stopSendCmdtoPLC = true;
            if ((temp.int_data["1M5"]&0x01) == 0x01)//自动
            {
                switchRotary_runmode.Value = 1;              
            }
            else
            {
                switchRotary_runmode.Value = 0;
            }
            if ((temp.int_data["1M5"] & 0x02) == 0x02)//启动
            {
                switchRotary_runstatus.Value = 1;
            }
            else
            {
                switchRotary_runstatus.Value = 0;
            }

            if(tabControl_terminal.SelectedTab==tabPage_pg1)
            {
                if ((temp.int_data["1M9"] & 0x20) == 0x20)
                {
                    switchSlider_pg1_kaoliaoluhoumen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_kaoliaoluhoumen.Value = 0;
                }
                if ((temp.int_data["1M9"] & 0x40) == 0x40)
                {
                    switchSlider_pg1_kaoliaoluqianmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_kaoliaoluqianmen.Value = 0;
                }
                if ((temp.int_data["1M9"] & 0x80) == 0x80)
                {
                    switchSlider_pg1_shuixiangmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_shuixiangmen.Value = 0;
                }
                if ((temp.int_data["1M9"] & 0x08) == 0x08)
                {
                    switchRotary_pg1_shuixiang.Value = 1;
                }
                else
                {
                    switchRotary_pg1_shuixiang.Value = 0;
                }
                if ((temp.int_data["1M9"] & 0x10) == 0x10)
                {
                    switchRotary_pg1_tuomuqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg1_tuomuqigang.Value = 0;
                }
                if ((temp.int_data["1M6"] & 0x10) == 0x10)
                {
                    switchRotary_pg1_shengliaojiceshi.Value = 1;
                }
                else
                {
                    switchRotary_pg1_shengliaojiceshi.Value = 0;
                }

                if ((temp.int_data["1M12"] & 0x40) == 0x40)
                {
                    switchRotary_pg1_shuayoujishengjiang.Value = 1;
                }
                else
                {
                    switchRotary_pg1_shuayoujishengjiang.Value = 0;
                }
                if ((temp.int_data["1M6"] & 0x04) == 0x04)
                {
                    switchSlider_pg1_yureluqianmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_yureluqianmen.Value = 0;
                }
                if ((temp.int_data["1M6"] & 0x08) == 0x08)
                {
                    switchSlider_pg1_yureluhoumen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_yureluhoumen.Value = 0;
                }

            }
            
            if (tabControl_terminal.SelectedTab==tabPage_pg2)
            {

                if ((temp.int_data["1M6"] & 0x80) == 0x80)//进炉气缸
                {
                    switchRotary_pg2_yurelujinluqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_yurelujinluqigang.Value = 0;
                }
                if ((temp.int_data["1M7"] & 0x01) == 0x01)
                {
                    switchRotary_pg2_yureluchuluqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_yureluchuluqigang.Value = 0;
                }
                if ((temp.int_data["1M10"] & 0x20) == 0x20)
                {
                    switchRotary_pg2_kaoliaolujinluqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_kaoliaolujinluqigang.Value = 0;
                }
                if ((temp.int_data["1M10"] & 0x40) == 0x40)
                {
                    switchRotary_pg2_kaoliaoluchuluqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_kaoliaoluchuluqigang.Value = 0;
                }
                if ((temp.int_data["1M12"] & 0x40) == 0x40)
                {
                    switchRotary_pg2_dingmaoshuaqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_dingmaoshuaqigang.Value = 0;
                }
                if ((temp.int_data["1M11"] & 0x20) == 0x20)
                {
                    switchRotary_pg2_choufengji.Value = 1;
                }
                else
                {
                    switchRotary_pg2_choufengji.Value = 0;
                }
                if ((temp.int_data["1M11"] & 0x40) == 0x40)
                {
                    switchRotary_pg2_choufengjispd.Value = 1;
                }
                else
                {
                    switchRotary_pg2_choufengjispd.Value = 0;
                }
            }
            
            if (tabControl_terminal.SelectedTab == tabPage_pg3)
            {

                if ((temp.int_data["1M8"] & 0x20) == 0x20)
                {
                    switchRotary_pg3_shuixiang.Value = 1;
                }
                else
                {
                    switchRotary_pg3_shuixiang.Value = 0;
                }
                if ((temp.int_data["1M8"] & 0x40) == 0x40)//二号钳销
                {
                    switchRotary_pg3_bamoji.Value = 1;
                }
                else
                {
                    switchRotary_pg3_bamoji.Value = 0;
                }
                if ((temp.int_data["1M8"] & 0x80) == 0x80)//刷油机
                {
                    switchRotary_pg3_shuayouji.Value = 1;
                }
                else
                {
                    switchRotary_pg3_shuayouji.Value = 0;
                }
  
                if ((temp.int_data["1M8"] & 0x08) == 0x08)
                {
                    switchRotary_pg3_jinliaoqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg3_jinliaoqigang.Value = 0;
                }
                if ((temp.int_data["1M6"] & 0x01) == 0x01)
                {
                    switchRotary_pg3_shengliaojiyunxing.Value = 1;
                }
                else
                {
                    switchRotary_pg3_shengliaojiyunxing.Value = 0;
                }
            }
             if (tabControl_terminal.SelectedTab == tabPage_pg4)
                {
                    if ((temp.int_data["1M7"] & 0x02) == 0x02)
                    {
                        switchRotary_pg4_yurelukaiguan.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_yurelukaiguan.Value = 0;
                    }
                    if ((temp.int_data["1M9"] & 0x01) == 0x01)
                    {
                        switchRotary_pg4_kaoliaolukaiguan.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_kaoliaolukaiguan.Value = 0;
                    }
                    if ((temp.int_data["1M7"] & 0x04) == 0x04)
                    {
                        switchRotary_pg4_yurelutoukaiguan1.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_yurelutoukaiguan1.Value = 0;
                    }
                    if ((temp.int_data["1M7"] & 0x08) == 0x08)
                    {
                        switchRotary_pg4_yurelutoukaiguan2.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_yurelutoukaiguan2.Value = 0;
                    }
                    if ((temp.int_data["1M7"] & 0x10) == 0x10)
                    {
                        switchRotary_pg4_yurelutoukaiguan3.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_yurelutoukaiguan3.Value = 0;
                    }
                    if ((temp.int_data["1M9"] & 0x02) == 0x02)
                    {
                        switchRotary_pg4_kaoliaolutoukaiguan1.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_kaoliaolutoukaiguan1.Value = 0;
                    }
                    if ((temp.int_data["1M9"] & 0x04) == 0x04)
                    {
                        switchRotary_pg4_kaoliaolutoukaiguan2.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg4_kaoliaolutoukaiguan2.Value = 0;
                    }
                }
                if (tabControl_terminal.SelectedTab == tabPage_pg5)
                {
                    if ((temp.int_data["1M12"] & 0x08) == 0x08)
                    {
                        switchRotary_pg5_qianjinqigang.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg5_qianjinqigang.Value = 0;
                    }
                    if ((temp.int_data["1M12"] & 0x10) == 0x10)
                    {
                        switchRotary_pg5_houtuiqigang.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg5_houtuiqigang.Value = 0;
                    }
                    if ((temp.int_data["1M10"] & 0x02) == 0x02)
                    {
                        switchRotary_pg5_tuomofangshi.Value = 0;//反
                    }
                    else
                    {
                        switchRotary_pg5_tuomofangshi.Value = 1;
                    }
                }
                if (tabControl_terminal.SelectedTab == tabPage_pg6)
                {
                    if ((temp.int_data["1M6"] & 0x01) == 0x01)
                    {
                        switchRotary_pg6_shengliaojiyunxing.Value = 1;
                    }
                    else
                    {
                        switchRotary_pg6_shengliaojiyunxing.Value = 0;
                    }
                }           
            stopSendCmdtoPLC = false;

        }
        static float ToSingle(byte[] b)
        {
           
            //string j = Convert.ToString(x, 2).PadLeft(32, '0');
            
            //byte[] b = new byte[4];
            //for (int i = 0; i < 4;i++ )
            //{
            //    b[i] = Convert.ToByte(j.Substring(24 - i * 8, 8), 2);
            //}
                return BitConverter.ToSingle(b,0);
        }
        #endregion
        #region 辅助功能定义

        //==================================================================
        // 模块名:DBDump
        // 日期：2016/03/11
        // 功能描述:将DB区读到的byte数据转为有效格式存入字典
        // 输入参数:读缓存,开始位,字节长度,读取存储区（DB），DB区号，
        //          配置文件中DB区中Word类型的地址位置（其余默认为连续DW类型）
        // 返回值: void
        // 修改记录：
        //==================================================================
        private void DBDump(byte[] bytes,int start,int size,string area,string DBIndex,string[] DBword )
        {
            if (bytes == null)
                return;
            string key;
            string address;
          for (int i = 0; i < size;)
            {
              address = (i+start).ToString();
              if(DBword.Contains((i+start).ToString()))
              {             
                  byte[] b = new byte[4];
                  key=area+"W"+DBIndex+"_"+address;
                  daq_data.int_data[key]=Convert.ToInt32((bytes[i]<<8)+bytes[i+1]);
                  b[1] = bytes[i]; b[0] = bytes[i + 1];
                  daq_data.bytes_data[key]=b;
                  i=i+2;
              }
              else
              {
                  int value = 0;
                  byte[] b = new byte[4];
                  key=area+"D"+DBIndex+"_"+address;
                  if(area.Equals("2DB"))
                  {

                  }
                  for (int j = 0; j < 4;j++ )
                  {
                      value = value + (bytes[i + j] << (8 * (4 - j - 1)));
                      b[4 - j-1] = bytes[i + j];
                  }
                  daq_data.int_data[key] = value;
                  daq_data.bytes_data[key] = b;
                  i = i + 4;
              }
            }
        }
        
        //==================================================================
        // 模块名:HexDump
        // 日期：2016/03/11
        // 功能描述:将非DB区缓冲区中数据存入字典
        // 输入参数:缓冲区地址，起始地址，字节总长度，所读区域，单位字节长度(即byte:1;word:2;dword:4等)
        // 返回值: void
        // 修改记录：
        //==================================================================
        private void HexDump(byte[] bytes,int start,int size,string area,int Numofbyte )
        {
            if (bytes == null)
                return;            
            for (int i = 0; i < size; i = i + Numofbyte)
            {
                int value = 0;
                byte[] b= new byte[Numofbyte];
                string key=area+(start+i).ToString();
                for (int j = 0; j <Numofbyte; j++)
                {
                    value = value+(bytes[i + j] << (8 * (Numofbyte-j-1)));
                    b[Numofbyte-j-1] = bytes[i + j];
                }
                daq_data.int_data[key] = value;
                daq_data.bytes_data[key] = b;
            }
            
        }
        //==================================================================
        //模块名： cloudpara_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    读取数据库下载到本地
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void cloudpara_Click(object sender, EventArgs e)
        {
            try
            {
                var mysql = new FangpuDatacenterModelEntities();
                var para = from d in mysql.proceduretechnologybase
                           where d.device_name.Equals(Properties.TerminalParameters.Default.terminal_name)
                           select d;

                SQLiteConnection m_dbConnection = new SQLiteConnection(TerminalLocalDataStorage.connectionString);
                string strSql2 = "delete from proceduretechnologybase";
                TerminalLocalDataStorage.ExecuteSql(strSql2);
                foreach (var d in para)
                {
                    int ret = 1;
                    StringBuilder strSql = new StringBuilder();
                    strSql.Append("insert into proceduretechnologybase(");
                    strSql.Append("product_id,material,shuayou_base,shuayou_upper,shuayou_lower,yurelu_temp_base,yurelu_temp_upper,yurelu_temp_lower,kaomo_consume_base,kaomo_consume_upper,kaomo_consume_lower,");
                    strSql.Append("kaoliaolu_temp_base,kaoliaolu_temp_upper,kaoliaolu_temp_lower,kaoliao_consume_base,kaoliao_consume_upper,kaoliao_consume_lower,");
                    strSql.Append("qigangjinliao_consume_base,qigangjinliao_consume_upper,qigangjinliao_consume_lower,lengque_consume_base,lengque_consume_upper,lengque_consume_lower,");
                    strSql.Append("jinliao_consume_base,jinliao_consume_upper,jinliao_consume_lower,shangsheng_speed_base,shangsheng_speed_upper,shangsheng_speed_lower,");
                    strSql.Append("xiajiang_speed_base,xiajiang_speed_upper,xiajiang_speed_lower,hutao_length_base,hutao_length_upper,hutao_length_lower,yemian_distance_base,yemian_distance_upper,yemian_distance_lower)");
                    strSql.Append(" values(");
                    strSql.Append("@product_id,@material,@shuayou_base,@shuayou_upper,@shuayou_lower,@yurelu_temp_base,@yurelu_temp_upper,@yurelu_temp_lower,@kaomo_consume_base,@kaomo_consume_upper,@kaomo_consume_lower,");
                    strSql.Append("@kaoliaolu_temp_base,@kaoliaolu_temp_upper,@kaoliaolu_temp_lower,@kaoliao_consume_base,@kaoliao_consume_upper,@kaoliao_consume_lower,");
                    strSql.Append("@qigangjinliao_consume_base,@qigangjinliao_consume_upper,@qigangjinliao_consume_lower,@lengque_consume_base,@lengque_consume_upper,@lengque_consume_lower,");
                    strSql.Append("@jinliao_consume_base,@jinliao_consume_upper,@jinliao_consume_lower,@shangsheng_speed_base,@shangsheng_speed_upper,@shangsheng_speed_lower,");
                    strSql.Append("@xiajiang_speed_base,@xiajiang_speed_upper,@xiajiang_speed_lower,@hutao_length_base,@hutao_length_upper,@hutao_length_lower,@yemian_distance_base,@yemian_distance_upper,@yemian_distance_lower)");


                    SQLiteParameter[] parameters = new SQLiteParameter[] { 
                    
                    new SQLiteParameter("@product_id",d.product_id),
                    new SQLiteParameter("@material",d.material),
                    new SQLiteParameter("@shuayou_base",d.shuayou_base),
                    new SQLiteParameter("@shuayou_upper",d.shuayou_upper),
                    new SQLiteParameter("@shuayou_lower",d.shuayou_lower),
                    new SQLiteParameter("@yurelu_temp_base",d.yurelu_temp_base),
                    new SQLiteParameter("@yurelu_temp_upper",d.yurelu_temp_upper),
                    new SQLiteParameter("@yurelu_temp_lower",d.yurelu_temp_lower),
                    new SQLiteParameter("@kaomo_consume_base",d.kaomo_consume_base),
                    new SQLiteParameter("@kaomo_consume_upper",d.kaomo_consume_upper),
                    new SQLiteParameter("@kaomo_consume_lower",d.kaomo_consume_lower),
                    new SQLiteParameter("@kaoliaolu_temp_base",d.kaoliaolu_temp_base),
                    new SQLiteParameter("@kaoliaolu_temp_upper",d.kaoliaolu_temp_upper),
                    new SQLiteParameter("@kaoliaolu_temp_lower",d.kaoliaolu_temp_lower),
                    new SQLiteParameter("@kaoliao_consume_base",d.kaoliao_consume_base),
                    new SQLiteParameter("@kaoliao_consume_upper",d.kaoliao_consume_upper),
                    new SQLiteParameter("@kaoliao_consume_lower",d.kaoliao_consume_lower),
                    new SQLiteParameter("@qigangjinliao_consume_base",d.qigangjinliao_consume_base),
                    new SQLiteParameter("@qigangjinliao_consume_upper",d.qigangjinliao_consume_upper),
                    new SQLiteParameter("@qigangjinliao_consume_lower",d.qigangjinliao_consume_lower),
                    new SQLiteParameter("@lengque_consume_base",d.lengque_consume_base),
                    new SQLiteParameter("@lengque_consume_upper",d.lengque_consume_upper),
                    new SQLiteParameter("@lengque_consume_lower",d.lengque_consume_lower),
                    new SQLiteParameter("@jinliao_consume_base",d.jinliao_consume_base),
                    new SQLiteParameter("@jinliao_consume_upper",d.jinliao_consume_upper),
                    new SQLiteParameter("@jinliao_consume_lower",d.jinliao_consume_lower),
                    new SQLiteParameter("@shangsheng_speed_base",d.shangsheng_speed_base),
                    new SQLiteParameter("@shangsheng_speed_upper",d.shangsheng_speed_upper),
                    new SQLiteParameter("@shangsheng_speed_lower",d.shangsheng_speed_lower),
                    new SQLiteParameter("@xiajiang_speed_base",d.xiajiang_speed_base),
                    new SQLiteParameter("@xiajiang_speed_upper",d.xiajiang_speed_upper),
                    new SQLiteParameter("@xiajiang_speed_lower",d.xiajiang_speed_lower),
                    new SQLiteParameter("@hutao_length_base",d.hutao_length_base),
                    new SQLiteParameter("@hutao_length_upper",d.hutao_length_upper),
                    new SQLiteParameter("@hutao_length_lower",d.hutao_length_lower),
                    new SQLiteParameter("@yemian_distance_base",d.yemian_distance_base),
                    new SQLiteParameter("@yemian_distance_upper",d.yemian_distance_upper),
                    new SQLiteParameter("@yemian_distance_lower",d.yemian_distance_lower),
            };

                    if (TerminalLocalDataStorage.ExecuteSql(strSql.ToString(), parameters) >= 1)
                    {
                        ret = 1;
                    }
                }
                MessageBox.Show("从中央数据库下载成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库连接出错!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                log.Error("下载功能出错", ex);
                return;
            }
            try
            {
                SelectUpdate();
            }
            catch
            {
                MessageBox.Show("本地列表刷新失败", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //==================================================================
        //模块名： typeselect_SelectedIndexChanged
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    从本地数据库选择工艺参数
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void typeselect_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            try
            {

                if (typeexist == true)
                {
                    product_id = Convert.ToString(typeselect.SelectedItem);
                    SQLiteConnection m_dbConnection = new SQLiteConnection(TerminalLocalDataStorage.connectionString);
                    string strSql = String.Format("select * from proceduretechnologybase where product_id='{0}' ", product_id);
                    DataSet ds = new DataSet();
                    DataTable dTable = new DataTable();
                    ds = TerminalLocalDataStorage.Query(strSql);
                    dTable = ds.Tables[0];
                    if (dTable.Rows.Count != 0)
                    {
                        material = Convert.ToString(dTable.Rows[0]["material"]);
                        shuayou_base = dTable.Rows[0]["shuayou_base"];
                        shuayou_upper = dTable.Rows[0]["shuayou_upper"];
                        shuayou_lower = dTable.Rows[0]["shuayou_lower"];
                        yurelu_temp_base = (dTable.Rows[0]["yurelu_temp_base"]);
                        yurelu_temp_upper = (dTable.Rows[0]["yurelu_temp_upper"]);
                        yurelu_temp_lower = (dTable.Rows[0]["yurelu_temp_lower"]);
                        kaomo_consume_base = (dTable.Rows[0]["kaomo_consume_base"]);
                        kaomo_consume_upper = (dTable.Rows[0]["kaomo_consume_upper"]);
                        kaomo_consume_lower = (dTable.Rows[0]["kaomo_consume_lower"]);
                        kaoliaolu_temp_base = (dTable.Rows[0]["kaoliaolu_temp_base"]);
                        kaoliaolu_temp_upper = (dTable.Rows[0]["kaoliaolu_temp_upper"]);
                        kaoliaolu_temp_lower = (dTable.Rows[0]["kaoliaolu_temp_lower"]);
                        kaoliao_consume_base = (dTable.Rows[0]["kaoliao_consume_base"]);
                        kaoliao_consume_upper = (dTable.Rows[0]["kaoliao_consume_upper"]);
                        kaoliao_consume_lower = (dTable.Rows[0]["kaoliao_consume_lower"]);
                        qigangjinliao_consume_base = (dTable.Rows[0]["qigangjinliao_consume_base"]);
                        qigangjinliao_consume_upper = (dTable.Rows[0]["qigangjinliao_consume_upper"]);
                        qigangjinliao_consume_lower = (dTable.Rows[0]["qigangjinliao_consume_lower"]);
                        lengque_consume_base = (dTable.Rows[0]["lengque_consume_base"]);
                        lengque_consume_upper = (dTable.Rows[0]["lengque_consume_upper"]);
                        lengque_consume_lower = (dTable.Rows[0]["lengque_consume_lower"]);
                        jinliao_consume_base = (dTable.Rows[0]["jinliao_consume_base"]);
                        jinliao_consume_upper = (dTable.Rows[0]["jinliao_consume_upper"]);
                        jinliao_consume_lower = (dTable.Rows[0]["jinliao_consume_lower"]);
                        shangsheng_speed = (dTable.Rows[0]["shangsheng_speed_base"]);
                        xiajiang_speed = (dTable.Rows[0]["xiajiang_speed_base"]);
                        hutao_total_length = (dTable.Rows[0]["hutao_length_base"]);
                        yemian_height = (dTable.Rows[0]["yemian_distance_base"]);

   
                        productlabel.Text = "预备:" + product_id;
                    }
                    else
                    {
                        MessageBox.Show("数据不存在", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        typeexist = false;
                        typeselect.Text = null;
                        product_id = null;

                        typeexist = true;
                    }
                }
            }
            catch (Exception ex)
            { 
                MessageBox.Show("出现错误!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                log.Error("读取本地工艺参数出错", ex);
            }
        }

        //==================================================================
        //模块名： keyboard_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    打开键盘
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void keyboard_Click(object sender, EventArgs e)
        {
            ShowInputPanel();
        }
        //==================================================================
        //模块名： restart_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    重启程序
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        bool restartbutton = false;
        private void restart_Click(object sender, EventArgs e)
        {
            
            if(MessageBox.Show("确定要重启吗？","提示", MessageBoxButtons.OKCancel,MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    if (tcpuplink_dataprocess_thread.IsAlive == true)
                    {
                        tcpuplink_dataprocess_thread.Abort();
                    }

                    if (tcpdownlink_dataprocess_thread.IsAlive == true)
                    {
                        tcpdownlink_dataprocess_thread.Abort();
                    }

                    if (plccommunication_thread.IsAlive == true)
                    {
                        plccommunication_thread.Abort();
                    }


                    if (plcdatahandler_thread.IsAlive == true)
                    {
                        plcdatahandler_thread.Abort();
                    }

                    if (datacenter_storage_thread.IsAlive == true)
                    {
                        datacenter_storage_thread.Abort();
                    }

                    if (local_storage_thread.IsAlive == true)
                    {
                        local_storage_thread.Abort();
                    }

                    //if (PPI.SPort.IsOpen == true)
                    //{
                    //    PPI.SPort.Close();
                    //}
                }
                catch { }
                restartbutton = true;
                tcpobject.socket_stop_connect();
                Application.ExitThread();
                Application.Restart();
                //System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -t 00");
                //System.Diagnostics.Process.Start(startinfo);
                //System.Environment.Exit(System.Environment.ExitCode);
        
            }
        }
        //==================================================================
        //模块名： paraupload_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    上传参数
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void paraupload_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("上传设置参数？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    var mysql = new FangpuDatacenterModelEntities();
                    var d = new proceduretechnologybase_work();
                    d.device_name = Properties.TerminalParameters.Default.terminal_name; 
                    d.product_id = Convert.ToString(product_id);
                    d.material = Convert.ToString(material);
                    if (Convert.IsDBNull(kaomo_consume_base) == false)
                    {
                        d.kaomo_consume_base = this.vw2014 / 10.0f + (float)Convert.ToDouble(kaomo_consume_base);
                    }
                    if (Convert.IsDBNull(lengque_consume_base) == false)
                    {
                        d.lengque_consume_base = this.vw2012 / 10.0f + (float)Convert.ToDouble(lengque_consume_base);
                    }
                    if (Convert.IsDBNull(kaoliao_consume_base) == false)
                    {
                        d.kaoliao_consume_base = this.vw2016 / 10.0f + (float)Convert.ToDouble(kaoliao_consume_base);
                    }
                    if (Convert.IsDBNull(jinliao_consume_base) == false)
                    {
                        d.jinliao_consume_base = (float)Convert.ToDouble(jinliao_consume_base);
                    }
                    if (Convert.IsDBNull(shuayou_base) == false)
                    {
                        d.shuayou_base = this.vw2010/10.0f + (float)Convert.ToDouble(shuayou_base);
                    }
                    d.storetime = DateTime.Now;
                    mysql.proceduretechnologybase_work.Add(d);
                    mysql.SaveChanges();
                    MessageBox.Show("上传成功","提示",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("上传失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        
            
        }
       
        //==================================================================
        //模块名： isgoodnumber
        //日期：    2015.12.11
        //功能：    判断键盘输入是否为有效数字（整数或小数
        //输入参数：
        //返回值：  是否有效布尔值
        //修改记录：
        //==================================================================
        public bool isgoodnumber(TextBox box, KeyPressEventArgs e)
        {
            try
            {
                int kc = (int)e.KeyChar;
                if ((kc < 48 || kc > 57) && kc != 8 && kc != 46)
                {
                    return true;
                }
                else if (kc == 46)
                {
                    if (box.Text.Length <= 0)
                    {
                        return true;
                    }
                    else
                    {
                        float f;
                        float oldf;
                        bool b1 = false, b2 = false;
                        b1 = float.TryParse(box.Text, out oldf);
                        b2 = float.TryParse(box.Text + e.KeyChar.ToString(), out f);
                        if (b2 == false)
                        {
                            if (b1 == true)
                                return true;
                            else
                                return false;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }

        }
        //==================================================================
        //模块名： jinliaoshezhi_KeyPress
        //日期：    2016.1.15
        //功能：    按回车后输入，输入值不能为空
        //输入参数：
        //返回值：
        //修改记录：
        //==================================================================
       
        #region 最小化窗口


        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [DllImport("User32.dll ", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);//关键方法  
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessage(IntPtr HWnd, uint Msg, int WParam, int LParam);


        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_RESTORE = 0xF120;
        public const int SC_CLOSE = 0xF060;
        public void HideInputPanel()
        {

            IntPtr TouchhWnd = new IntPtr(0);
            TouchhWnd = FindWindow("IPTip_Main_Window", null);
            if (TouchhWnd == IntPtr.Zero)
                return;
             PostMessage(TouchhWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
        }
        private void MiniMizeAppication(string processName)
        {
            Process[] processs = Process.GetProcessesByName(processName);
            if (processs != null)
            {
                foreach (Process p in processs)
                {
                    IntPtr handle = FindWindow(null, p.MainWindowTitle);
                    //IntPtr handle = FindWindow("YodaoMainWndClass",null);  
                    PostMessage(handle, WM_SYSCOMMAND, SC_MINIMIZE, 0);
                }
            }
        }
        private void MaxMizeAppication(string processName)
        {
            Process[] processs = Process.GetProcessesByName(processName);
            if (processs != null)
            {
                foreach (Process p in processs)
                {
                    IntPtr handle = FindWindow(null, p.MainWindowTitle);
                    //IntPtr handle = FindWindow("YodaoMainWndClass",null);  
                    PostMessage(handle, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
                }
            }
        }
        private void RestoreAppication(string processName)
        {
            Process[] processs = Process.GetProcessesByName(processName);
            if (processs != null)
            {
                foreach (Process p in processs)
                {
                    IntPtr handle = FindWindow(null, p.MainWindowTitle);
                    //IntPtr handle = FindWindow("YodaoMainWndClass",null);  
                    PostMessage(handle, WM_SYSCOMMAND, SC_RESTORE, 0);
                }
            }
        }
        public static int ShowInputPanel()
        {
            try
            {
                dynamic file = "C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe";
                if (!System.IO.File.Exists(file))
                    return -1;
                Process.Start(file);
                return 1;
            }
            catch (Exception e)
            {
                return 255;

            }
        }
        
        private void KeyboardStart()
        {
            Process[] thisproc = Process.GetProcessesByName("TabTip");
            if (thisproc.Length == 0)
            {
                try
                {
                    System.Diagnostics.Process.Start("tabtip.exe");
       
                }
                catch
                {
                    MessageBox.Show("打开键盘失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

        }
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(System.IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        #endregion
 
       

       

        private void restartsystem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要重启吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    if (tcpuplink_dataprocess_thread.IsAlive == true)
                    {
                        tcpuplink_dataprocess_thread.Abort();
                    }

                    if (tcpdownlink_dataprocess_thread.IsAlive == true)
                    {
                        tcpdownlink_dataprocess_thread.Abort();
                    }

                    if (plccommunication_thread.IsAlive == true)
                    {
                        plccommunication_thread.Abort();
                    }


                    if (plcdatahandler_thread.IsAlive == true)
                    {
                        plcdatahandler_thread.Abort();
                    }

                    if (datacenter_storage_thread.IsAlive == true)
                    {
                        datacenter_storage_thread.Abort();
                    }

                    if (local_storage_thread.IsAlive == true)
                    {
                        local_storage_thread.Abort();
                    }
                }
                catch { }
                restartbutton = true;
                tcpobject.socket_stop_connect();
                //Application.Restart();
                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -t 00");
                System.Diagnostics.Process.Start(startinfo);
                //System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        private void type_accept_Click(object sender, EventArgs e)
        {
            try
            {
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(shuayou_base) * 10), 2000);
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(kaomo_consume_base) * 10), 2004);
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(kaoliao_consume_base) * 10), 2006);
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(lengque_consume_base) * 10), 2002);
                productlabel.Text = Convert.ToString(typeselect.SelectedItem);
                this.vw2010 = 0;
                this.vw2012 = 0;
                this.vw2014 = 0;
                this.vw2016 = 0;
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2010, 2010);
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2012, 2012);
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2014, 2014);
                SendCommandToPlc(1,TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2016, 2016);
                shuayou_consume_fudong = true;
                kaoliao_consume_fudong = true;
                kaomo_consume_fudong = true;
                lengque_consume_fudong = true;
                MessageBox.Show("已提交", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("提交失败", "警告", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

        }
        private void button_pg1_nextpage_Click(object sender, EventArgs e)
        {
            tabPage_pg1.Parent = null;
            tabControl_terminal.TabPages.Insert(0, tabPage_pg2);
            tabControl_terminal.SelectedIndex = 0;
        }

        private void button_pg2_back_Click(object sender, EventArgs e)
        {
            tabPage_pg2.Parent = null;
            tabControl_terminal.TabPages.Insert(0, tabPage_pg1);
            tabControl_terminal.SelectedIndex = 0;
        }
        #region 按键定义
        private void button_pg1_shengliaoxiaochezuo_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12,TerminalCommon.S71200AreaM,TerminalCommon.S71200DataBit, 1, 5, 3);
        }

        private void button_pg1_shengliaoxiaochezuo_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 3);
        }

        private void button_pg1_shengliaoxiaocheyou_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 4);
        }

        private void button_pg1_shengliaoxiaocheyou_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 4);
        }

        private void button_pg1_mujujinkaoliaolu_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 4);
        }

        private void button_pg1_mujujinkaoliaolu_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 4);
        }

        private void button_pg1_mujudaoshuixiangwei_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 3);
        }

        private void button_pg1_mujudaoshuixiangwei_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 3);
        }

        private void switchSlider_pg1_kaoliaoluhoumen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if(switchSlider_pg1_kaoliaoluhoumen.Value.AsInteger==0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 5);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 5);
            }
            
        }

        private void switchSlider_pg1_kaoliaoluqianmen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchSlider_pg1_kaoliaoluqianmen.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 6);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 6);
            }
        }

        private void switchSlider_pg1_shuixiangmen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchSlider_pg1_shuixiangmen.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 7);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 7);
            }
        }

        private void switchRotary_pg1_shuixiang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg1_shuixiang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 3);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 3);
            }
        }

        private void SwitchRotary_pg1_tuomuqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg1_tuomuqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 0);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 0);
            }
        }

        private void switchRotary_pg1_shengliaojiceshi_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg1_shengliaojiceshi.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 4);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 4);
            }
        }

        private void switchSlider_pg1_yureluhoumen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchSlider_pg1_yureluhoumen.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 3);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 3);
            }
        }

        private void switchSlider_pg1_yureluqianmen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchSlider_pg1_yureluqianmen.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 2);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 2);
            }
        }

        private void switchRotary_pg1_shuayoujishengjiang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg1_shuayoujishengjiang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 6);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 6);
            }
        }

        private void button_pg1_mujudaotuomoji_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 2);
        }

        private void button_pg1_mujudaotuomoji_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 2);
        }

       

        private void switchRotary_pg2_yurelujinluqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_yurelujinluqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 7);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 7);
            }
        }

        private void switchRotary_pg2_yureluchuluqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_yureluchuluqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 0);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 0);
            }
        }

        private void switchRotary_pg2_kaoliaolujinluqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_kaoliaolujinluqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 5);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 5);
            }
        }

        private void switchRotary_pg2_kaoliaoluchuluqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_kaoliaolujinluqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 6);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 6);
            }
        }

        private void switchRotary_pg2_dingmaoshuaqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_dingmaoshuaqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 6);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 6);
            }
        }

        private void switchRotary_pg2_choufengji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_choufengji.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 5);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 5);
            }
        }

        private void switchRotary_pg2_choufengjispd_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg2_choufengjispd.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 6);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 6);
            }
        }

        private void switchRotary_runmode_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_runmode.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 0);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 0);
            }
        }

        private void switchRotary_runstatus_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_runstatus.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 1);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 1);
            }
        }

        private void switchRotary_pg3_shuixiang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg3_shuixiang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 5);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 5);
            }
        }

        private void switchRotary_pg3_bamoji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg3_bamoji.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 6);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 6);
            }
        }

        private void switchRotary_pg3_shuayouji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg3_shuayouji.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 7);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 7);
            }
        }

        private void switchRotary_pg3_jinliaoqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg3_jinliaoqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 4);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 4);
            }
        }

        private void switchRotary_pg3_shengliaojiyunxing_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg3_shengliaojiyunxing.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 0);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 0);
            }
        }

        private void switchRotary_pg4_yurelukaiguan_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_yurelukaiguan.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 1);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 1);
            }
        }

        private void switchRotary_pg4_kaoliaolukaiguan_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_kaoliaolukaiguan.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 0);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 0);
            }
        }

        private void switchRotary_pg4_yurelutoukaiguan1_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_yurelutoukaiguan1.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 2);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 2);
            }
        }

        private void switchRotary_pg4_yurelutoukaiguan2_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_yurelutoukaiguan2.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 3);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 3);
            }
        }

        private void switchRotary_pg4_yurelutoukaiguan3_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_yurelutoukaiguan3.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 4);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 4);
            }
        }

        private void switchRotary_pg4_kaoliaolutoukaiguan1_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_kaoliaolutoukaiguan1.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 1);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 1);
            }
        }

        private void switchRotary_pg4_kaoliaolutoukaiguan2_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg4_kaoliaolutoukaiguan2.Value.AsInteger == 0)
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 9, 2);
            }
            else
            {
                SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 9, 2);
            }
        }

        private void switchRotary_pg5_qianjinqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg5_qianjinqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 3);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 3);
            }
        }

        private void switchRotary_pg5_houtuiqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg5_houtuiqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 4);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 4);
            }
        }

        private void switchRotary_pg5_tuomofangshi_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg5_tuomofangshi.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 1);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 1);
            }
        }

        private void switchRotary_pg6_shengliaojiyunxing_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
            {
                return;
            }
            if (switchRotary_pg6_shengliaojiyunxing.Value.AsInteger == 0)
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 0);
            }
            else
            {
                SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 0);
            }
        }

        private void button_pg1_bamujiceshi_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 0);
        }

        private void button_pg1_bamujiceshi_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 0);
        }

        private void button_pg1_shengliaojiceshi_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 7);
        }

        private void button_pg1_shengliaojiceshi_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 7);
        }

        private void button_pg1_mujudaoshengliaoji_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 2);
        }

        private void button_pg1_mujudaoshengliaoji_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 2);
        }

        private void button_pg1_mujujinyurelu_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 7);
        }

        private void button_pg1_mujujinyurelu_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 7);
        }

        private void button_pg1_shuayoujizuo_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 5);
        }

        private void button_pg1_shuayoujizuo_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 5);
        }

        private void button_pg1_shuayoujiyou_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 6, 6);
        }

        private void button_pg1_shuayoujiyou_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 6, 6);
        }

        private void button_pg1_tuomuxiaochezuo_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 4);
        }

        private void button_pg1_tuomuxiaochezuo_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 4);
        }

        private void button_pg1_tuomuxiaocheyou_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 3);
        }

        private void button_pg1_tuomuxiaocheyou_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 3);
        }

        private void button_pg2_yureluup_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 5);
        }

        private void button_pg2_yureluup_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 5);
        }

        private void button_pg2_yureludown_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 7, 6);
        }

        private void button_pg2_yureludown_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 7, 6);
        }

        private void button_pg2_yureluup2_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 1);
        }

        private void button_pg2_yureluup2_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 1);
        }

        private void button_pg2_yureludown2_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 0);
        }

        private void button_pg2_yureludown2_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 0);
        }

        private void button_pg2_kaoliaoluup_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 0);
        }

        private void button_pg2_kaoliaoluup_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 0);
        }

        private void button_pg2_kaoliaoludown_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 7);
        }

        private void button_pg2_kaoliaoludown_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 7);
        }

        private void button_pg2_kaoliaoluup2_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 1);
        }

        private void button_pg2_kaoliaoluup2_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 1);
        }

        private void button_pg2_kaoliaoludown2_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 11, 2);
        }

        private void button_pg2_kaoliaoludown2_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 11, 2);
        }
        public float kaomu_vary = 0;
        public float kaoliao_vary = 0;
        private void button_pg3_kaomushijianadd_Click(object sender, EventArgs e)
        {

        }

        private void button_pg3_kaomushijianminus_Click(object sender, EventArgs e)
        {
            
        }

        private void button_pg3_kaoliaoshijianadd_Click(object sender, EventArgs e)
        {

        }

        private void button_pg3_kaoliaoshijianminus_Click(object sender, EventArgs e)
        {

        }

        private void button_pg5_shoudongshangsheng_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 1);
        }

        private void button_pg5_shoudongshangsheng_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 1);
        }

        private void button_pg5_shoudongxiajiang_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 2);
        }

        private void button_pg5_shoudongxiajiang_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 2);
        }

        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 10, 0);
        }

        private void button3_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 10, 0);
        }

        private void button_pg6_shoudongshangsheng_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 5);
        }

        private void button_pg6_shoudongshangsheng_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 5);
        }

        private void button_pg6_shoudongxiajiang_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 6);
        }

        private void button_pg6_shoudongxiajiang_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 6);
        }

        private void button_pg6_ceshishengliaoji_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 5, 7);
        }

        private void button_pg6_ceshishengliaoji_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(1, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 5, 7);

        }

        private void button_resetwarn_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 8, 3);
        }

        private void button_resetwarn_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 8, 3);
        }


        private void button_pg8_chanliangclear_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 5);
        }

        private void button_pg8_chanliangclear_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 5);
        }

        private void button_pg8_tuoguanclear_MouseDown(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 1, 12, 7);
        }

        private void button_pg8_tuoguanclear_MouseUp(object sender, MouseEventArgs e)
        {
            SendCommandToPlc(12, TerminalCommon.S71200AreaM, TerminalCommon.S71200DataBit, 0, 12, 7);
        }
        #endregion


















        // //密码保护
       //public static int iOperCount = 0;
       // //检测是否有有效输入
       // //internal class MyMessager : IMessageFilter
       // //{
       // //    FangpuTerminal mainTerminal;
       // //    public MyMessager(FangpuTerminal terminal)
       // //    {
       // //        mainTerminal = terminal;
       // //    }
       // //    public bool PreFilterMessage(ref Message m)
       // //    {
       // //        //如果检测到有鼠标或则键盘的消息，则使计数为0.....
       // //        if (/*m.Msg == 0x0200 || */m.Msg == 0x0201 || m.Msg == 0x0204 || m.Msg == 0x0207)
       // //        {
       // //            iOperCount = 0;
       // //        }
       // //        if (!mainenable && m.Msg == 0x0201 || m.Msg == 0x0204 || m.Msg == 0x0207)
       // //        {
       // //            Passwordinput form5 = new Passwordinput(mainTerminal);
       // //            form5.Show();
       // //            mainenable = true;
       // //        }
       // //        return false;
       // //    }
       // //}
       // //触发锁屏
       // public static bool mainenable = true;
       // private void timer1_Tick(object sender, EventArgs e)
       // {
       //     iOperCount++;
       //     if (iOperCount > 2)
       //     {
       //         timer1.Enabled = false;
       //         mainenable = false;
       //         lockScreen();
       //     }
       // }
       // private void lockScreen()
       // {
       //     tabControl_terminal.Enabled = false;
       //     button_resetwarn.Enabled = false;
       //     button_system_init.Enabled = false;
       //     switchRotary_runmode.Enabled = false;
       //     switchRotary_runstatus.Enabled = false;
       //     restart.Enabled = false;
       //     keyboard.Enabled = false;
       // }
       // public void unlockScreen()
       // {
       //     tabControl_terminal.Enabled = true;
       //     button_resetwarn.Enabled = true;
       //     button_system_init.Enabled = true;
       //     switchRotary_runmode.Enabled = true;
       //     switchRotary_runstatus.Enabled = true;
       //     restart.Enabled = true;
       //     keyboard.Enabled = true;
       //     timer1.Enabled = true;
       // }


        //[StructLayout(LayoutKind.Sequential)] 
        //struct LASTINPUTINFO 
        //{ 
        //[MarshalAs(UnmanagedType.U4)] 
        //public int cbSize; 
        //[MarshalAs(UnmanagedType.U4)] 
        //public uint dwTime; 
        //} 

        //[DllImport( "user32.dll ")] 
        //static extern bool GetLastInputInfo(ref LASTINPUTINFO plii); 

        //static long GetLastInputTime() 
        //{ 
        //LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO(); 
        //vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo); 
        //if (!GetLastInputInfo(ref vLastInputInfo)) return 0; 
        //return Environment.TickCount - (long)vLastInputInfo.dwTime; 
        //} 

        //private void timer1_Tick(object sender, EventArgs e) 
        //{ 
        //Text = string.Format( "用户已经{0}秒没有路过了 ", GetLastInputTime() / 1000); 
        //if (GetLastInputTime() > 1000 * 1) //用户一分钟不操作 
        // {
        //     this.Enabled = false;
        //     timer1.Enabled = false;
        // }
        //}
    }
        #endregion
    //public class MyMessager : IMessageFilter
    //{
    //    FangpuTerminal mainTerminal;
    //    public MyMessager(FangpuTerminal terminal)
    //    {
    //        mainTerminal = terminal;
    //    }
    //    public bool PreFilterMessage(ref Message m)
    //    {
    //        //如果检测到有鼠标或则键盘的消息，则使计数为0.....
    //        if (/*m.Msg == 0x0200 || */m.Msg == 0x0201 || m.Msg == 0x0204 || m.Msg == 0x0207)
    //        {
    //            FangpuTerminal.iOperCount = 0;
    //            Passwordinput.iOperCount = 0;
    //        }
    //        if (!FangpuTerminal.mainenable && m.Msg == 0x0201 || m.Msg == 0x0204 || m.Msg == 0x0207)
    //        {
    //            Passwordinput pswd = new Passwordinput(mainTerminal);
    //            pswd.Show();
    //            pswd.timer1.Enabled = true;
    //            FangpuTerminal.mainenable = true;
    //        }
    //        return false;
    //    }
    //}


}
