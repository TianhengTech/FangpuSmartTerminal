using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using fangpu_terminal.Properties;
using fangpu_terminal.Ultility;
using fangpu_terminal.Ultility.Nhibernate;
using fangpu_terminal.Ultility.Nhibernate.LiteGroup;
using Iocomp.Classes;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using Snap7;
using historydata = fangpu_terminal.Ultility.Nhibernate.historydata;
using proceduretechnologybase = fangpu_terminal.Ultility.Nhibernate.proceduretechnologybase;
using Timer = System.Threading.Timer;

//using DevExpress.XtraSplashScreen;

namespace fangpu_terminal
{
    public partial class FangpuTerminal : Form
    {
        #region 成员定义

        public static ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private object lockobject = new object(); //数据库线程锁
        private TerminalTcpClientAsync tcpobject;
        private QuartzSchedule schedule;
        private S7Client S7SNAP;
        private S7_PPI PPI;
        private SynchronizationContext thread_updateui_syncContext = null;

        public delegate void UpdateText(PlcDAQCommunicationObject daq_input);

        public delegate void WebCommandUI(string cmd);

        public delegate void WebCommandProc();

        public delegate void HaltUI();

        public delegate void TextUI(string str);

        private Thread tcpuplink_dataprocess_thread; //tcp数据上行线程队列
        private Thread tcpdownlink_dataprocess_thread; //tcp数据下行处理
        private Thread plccommunication_thread; //plc数据通信处理
        private Thread plcdatahandler_thread; //plc数据处理
        private Thread datacenter_storage_thread; //数据库存储对象
        private Thread local_storage_thread; //本地数据缓存
        private Thread plcread_thread; //PLC读线程
        private Thread controlcmdread_thread; //读WEB指令线程
        private Timer timer_read_interval_60s; //60s读一次
        private Timer timer_check_interval_60m;
        private Timer timer_tcp_heart_connection; //tcp心跳连接
        private string mode = "manual";
        private string warntext = "报警信息";
        private ISessionFactory sessionfactory;


        //读控制变量
        private bool shuayou_consume_fudong;
        private bool kaomo_consume_fudong;
        private bool kaoliao_consume_fudong;
        private bool lengque_consume_fudong;
        private bool jinliao_consume_fudong = false;
        private bool isFirst = true;
        private bool stopSendCmdtoPLC;
        private bool enableWarn;
        private bool tuomuWarn;
        private bool typeexist = true;
        private bool enableSync = true;
        private bool warnflag = true;
        private bool tuomuflag = true;
        private bool onetime = true;
        private bool buzuo = true;
        private bool jinliao = true;
        private bool shiftflag_1;
        private bool shiftflag_2;
        private bool shiftflag_3;
        private bool shiftflag_4;
        private int syncount;
        private double zuomotime;
        private int vw68, vw70, vw72, vw74;
        private int youguanno;
        public numberboard numberBoard;


        private object shuayou_base;
        private object shuayou_upper;
        private object shuayou_lower;
        private object yurelu_temp_base;
        private object yurelu_temp_upper;
        private object yurelu_temp_lower;
        private object kaomo_consume_base;
        private object kaomo_consume_upper;
        private object kaomo_consume_lower;
        private object kaoliaolu_temp_base;
        private object kaoliaolu_temp_upper;
        private object kaoliaolu_temp_lower;
        private object kaoliao_consume_base;
        private object kaoliao_consume_upper;
        private object kaoliao_consume_lower;
        private object qigangjinliao_consume_base;
        private object qigangjinliao_consume_upper;
        private object qigangjinliao_consume_lower;
        private object lengque_consume_base;
        private object lengque_consume_upper;
        private object lengque_consume_lower;
        private object jinliao_consume_base;
        private object jinliao_consume_upper;
        private object jinliao_consume_lower;
        private object product_id;
        private object material;
        private object shangsheng_speed;
        private object xiajiang_speed;
        private object hutao_total_length;
        private object yemian_height;

        #endregion

        public FangpuTerminal()
        {
            InitializeComponent();

            //MyMessager msg = new MyMessager(this);
            //Application.AddMessageFilter(msg);
            //timer1.Enabled = true; 
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
            InitGlobalParameter();
            schedule = new QuartzSchedule();
            schedule.StartSchedule();
            log.Info("Schedule Start");
            S7SNAP = new S7Client();
            ushort localtsap = (ushort) TerminalParameters.Default.PLC_TSAP_Remote;
            ushort remotetsap = (ushort) TerminalParameters.Default.PLC_TSAP_Local;
            S7SNAP.SetConnectionParams(TerminalParameters.Default.plc1_tcp_ip, localtsap, remotetsap);
            int result = S7SNAP.Connect();
            if (result == 0)
            {
                log.Info("First touch established!");
            }
            else
            {
                log.Warn("First touch failed!");
            }

            ////UpdateLoadGUIConfig("启动心跳连接...", 50);
            //thread_updateui_syncContext = SynchronizationContext.Current;
            //tcpobject = new TerminalTcpClientAsync();


            //tcpuplink_dataprocess_thread = new Thread(TcpCommunicationThread);
            //tcpuplink_dataprocess_thread.IsBackground = true;
            //tcpuplink_dataprocess_thread.Priority = ThreadPriority.Lowest;
            ////          tcpuplink_dataprocess_thread.Start();

            //tcpdownlink_dataprocess_thread = new Thread(TcpDownlickDataProcessThread);
            ////tcpdownlink_dataprocess_thread.IsBackground=true;
            ////tcpdownlink_dataprocess_thread.Start();
            plcread_thread = new Thread(PlcReadCycle);
            plcread_thread.IsBackground = true;
            plcread_thread.Priority = ThreadPriority.BelowNormal;
            plcread_thread.Start();
            log.Info("PlcReadCycle Thread Start");

            plccommunication_thread = new Thread(PlcCommunicationThread);
            plccommunication_thread.IsBackground = true;
            plccommunication_thread.Priority = ThreadPriority.BelowNormal;
            //plccommunication_thread.Priority = ThreadPriority.Highest;
            plccommunication_thread.Start();
            log.Info("PlcCommunicationThread Thread Start");

            plcdatahandler_thread = new Thread(PlcDataProcessThread);
            plcdatahandler_thread.IsBackground = true;
            //plcdatahandler_thread.Priority = ThreadPriority.BelowNormal;
            plcdatahandler_thread.Start();
            log.Info("PlcDataProcessThread Thread Start");

            ////UpdateLoadGUIConfig("载入中", 80);
            datacenter_storage_thread = new Thread(DataCenterStorageThread);
            datacenter_storage_thread.IsBackground = true;
            datacenter_storage_thread.Priority = ThreadPriority.BelowNormal;
            datacenter_storage_thread.Start();
            log.Info("DataCenterStorageThread Thread Start");

            //UpdateLoadGUIConfig("载入中", 90);
            local_storage_thread = new Thread(PlcDataLocalStorage);
            local_storage_thread.IsBackground = true;
            local_storage_thread.Priority = ThreadPriority.BelowNormal;
            local_storage_thread.Start();
            log.Info("PlcDataLocalStorage Thread Start");

            controlcmdread_thread = new Thread(WebCommand);
            controlcmdread_thread.IsBackground = true;
            controlcmdread_thread.Priority = ThreadPriority.Lowest;
            controlcmdread_thread.Start();
            log.Info("WebCommand Thread Start");


            timer_read_interval_60s = new Timer(new TimerCallback(Timer_60s_handler), null, 0, 60000);
            //timer_check_interval_60m = new System.Threading.Timer(new TimerCallback(DataAutoSync), null, 0, 600000);
            //  // timer_tcp_heart_connection = new System.Threading.Timer(new TimerCallback(TcpToServerHeartConnect), null, 0, Properties.TerminalParameters.Default.heart_connect_interval * 1000);
            //  //UpdateLoadGUIConfig("载入中", 100);
            //  //SplashScreenManager.CloseForm();
        }

        //==================================================================
        //模块名： Timer_60s_handler
        //日期：    2015.12.11
        //功能：    每60秒读一次C区开机状态描述
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================

        private void Timer_60s_handler(object sender)
        {
            GC.Collect();
            var p = Process.GetCurrentProcess();
            string queuestatus = "plccommandqueue:" + TerminalQueues.datacenterprocessqueue.Count() + "\n" +
                                 "localdataqueue:" + TerminalQueues.localdataqueue.Count() + "\n" +
                                 "plccommandqueue:" + TerminalQueues.plccommandqueue.Count() + "\n" +
                                 "plcdataprocessqueue:" + TerminalQueues.plcdataprocessqueue.Count() + "\n" +
                                 "warninfoqueue:" + TerminalQueues.warninfoqueue.Count() + "\n" +
                                 "warninfoqueue_local:" + TerminalQueues.warninfoqueue_local.Count() + "\n";

            log.Info("当前内存:" + (p.PagedMemorySize64/1024.0d/1024.0d).ToString("0.0") + "MB\n" + queuestatus);
        }

        //==================================================================
        //模块名： InitGlobalParameter
        //日期：    2015.12.11
        //功能：    初始化变量
        //输入参数：
        //返回值：  无
        //修改记录：
        //==================================================================

        private void InitGlobalParameter()
        {
            try
            {
                SelectUpdate();
            }
            catch (Exception ex)
            {
                log.Error("本地参数显示刷新错误", ex);
            }
            WarnInfoRead(); //读取错误记录
            fangpu_config.ReadAddrIniFile("./fangpu_config.ini"); //读取地址信息
            TerminalCommon.warn_info =
                fangpu_config.ConvertToDictionary(fangpu_config.ReadIniAllKeys("warn", "./fangpu_warn.ini")); //读取报警信息
            TerminalCommon.warn_stop_info = fangpu_config.ReadIniAllKeys("stopwarn", "./fangpu_warn.ini"); //读取停机信息
           
        }

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
            if (enableSync)
            {
                if (syncount > 3)
                {
                    SwitchSync(daq_input);
                    syncount = 0;
                }
                else
                {
                    syncount++;
                }
            }
            if (tabControl_terminal.SelectedTab == tabPage_pg2)
            {
                if ((daq_input.aream_data["I5"] & 0x01) == 0x01)
                {
                    button_pg2_tuomoxiaocheqianjin.Text = "到位";
                }
                else
                {
                    button_pg2_tuomoxiaocheqianjin.Text = "前进";
                }

                if ((daq_input.aream_data["I5"] & 0x02) == 0x02)
                {
                    button_pg2_tuomoxiaochehoutui.Text = "到位";
                }
                else
                {
                    button_pg2_tuomoxiaochehoutui.Text = "后退";
                }

                if ((daq_input.aream_data["I5"] & 0x04) == 0x04)
                {
                    button_pg2_jinliaoxiaocheqianjin.Text = "到位";
                }
                else
                {
                    button_pg2_jinliaoxiaocheqianjin.Text = "前进";
                }

                if ((daq_input.aream_data["I5"] & 0x08) == 0x08)
                {
                    button_pg2_jinliaoxiaochehoutui.Text = "到位";
                }
                else
                {
                    button_pg2_jinliaoxiaochehoutui.Text = "后退";
                }

                if ((daq_input.aream_data["I4"] & 0x01) == 0x01)
                {
                    button_pg2_tuomojiqianjin.Text = "到位";
                }
                else
                {
                    button_pg2_tuomojiqianjin.Text = "前进";
                }
                if ((daq_input.aream_data["I4"] & 0x02) == 0x02)
                {
                    button_pg2_tuomojihoutui.Text = "到位";
                }
                else
                {
                    button_pg2_tuomojihoutui.Text = "后退";
                }

                if ((daq_input.aream_data["I4"] & 0x08) == 0x08)
                {
                    button_pg2_shuayoujiqianjin.Text = "到位";
                }
                else
                {
                    button_pg2_shuayoujiqianjin.Text = "前进";
                }
                if ((daq_input.aream_data["I4"] & 0x10) == 0x10)
                {
                    button_pg2_shuayoujihoutui.Text = "到位";
                }
                else
                {
                    button_pg2_shuayoujihoutui.Text = "后退";
                }
            }

            if (tabControl_terminal.SelectedTab == tabPage_pg3)
            {
                displayDouble_pg3_shuayoushijiandisplay.Value = daq_input.aream_data["VW50"]/10.0f;
                displayDouble_pg3_kaomushijiandisplay.Value = daq_input.aream_data["VW52"]/10.0f;
                displayDouble_pg3_kaoliaoshijiandisplay.Value = daq_input.aream_data["VW54"]/10.0f;
                displayDouble_pg3_lengqueshijiandisplay.Value = daq_input.aream_data["VW48"]/10.0f;
                displayDouble_pg3_jinliaoshijiandisplay.Value = daq_input.aream_data["VW56"]/10.0f;

                displayDouble_pg3_shuayoushijianset.Value = daq_input.aream_data["VW0"]/10.0f;
                displayDouble_pg3_kaomushijianset.Value = daq_input.aream_data["VW2"]/10.0f;
                displayDouble_pg3_kaoliaoshijianset.Value = daq_input.aream_data["VW4"]/10.0f;
                displayDouble_pg3_lengqueshijianset.Value = daq_input.aream_data["VW12"]/10.0f;
                displayDouble_pg3_jinliaoshijianset.Value = daq_input.aream_data["VW6"]/10.0f;
                displayDouble_pg3_buzuoguanshijianset.Value = daq_input.aream_data["VW10"]/10.0f;
                displayDouble_pg3_lengqueshijian2.Value = daq_input.aream_data["VW8"]/10.0f;

                if (!buzuoguan.Focused)
                {
                    buzuoguan.Text = (daq_input.aream_data["VW10"]/10.0f).ToString();
                }

                if (!jinliaoshezhi.Focused)
                {
                    jinliaoshezhi.Text = (daq_input.aream_data["VW6"]/10.0f).ToString();
                }
            }

            //if (tabControl_terminal.SelectedIndex == 3)


            if (enableWarn && (daq_input.aream_data["MB5"] & 0x08) == 0x08)
            {
                var results = GetWarnInfo(daq_input.aream_data);
                if (results.Count != 0)
                {
                    foreach (var Warn in results)
                    {
                        if (dataGridView_warn.Rows.Count >= 500)
                        {
                            dataGridView_warn.Rows.RemoveAt(499);
                        }
                        var index = dataGridView_warn.Rows.Add();
                        dataGridView_warn.Rows[index].Cells[0].Value = Warn.Key; //报警信息
                        dataGridView_warn.Rows[index].Cells[1].Value = daq_input.daq_time;
                        dataGridView_warn.Rows[index].Cells[2].Value = Warn.Value; //报警等级
                        warntext = Warn.Key;
                        dataGridView_warn.Sort(dataGridView_warn.Columns[1], ListSortDirection.Descending);
                    }
                    displayWarninfo.Value = warntext;
                }
            }

            if (tuomuWarn && (daq_input.aream_data["I5"] & 0x40) == 0x40)
            {
                if (dataGridView_demould.Rows.Count >= 500)
                {
                    dataGridView_demould.Rows.RemoveAt(499);
                }
                var index = dataGridView_demould.Rows.Add();
                dataGridView_demould.Rows[index].Cells[0].Value = "这一板模没有脱掉";
                dataGridView_demould.Rows[index].Cells[1].Value = daq_input.daq_time;
                dataGridView_demould.Sort(dataGridView_demould.Columns[1], ListSortDirection.Descending);
                tuomucount.Text=(++youguanno).ToString();
            }

            if ((daq_input.aream_data["I5"] & 0x40) == 0x40)
            {
                tuomuWarn = false;
            }
            else
            {
                tuomuWarn = true;
            }
            
            if ((daq_input.aream_data["MB5"] & 0x08) == 0x08)
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


            if (tabControl_terminal.SelectedTab == tabPage_pg5)
            {
                displayInteger_currentshift_kaijishijian_hour.Value = daq_input.aream_data["VW16"];
                displayInteger_currentshift_kaijishijian_minute.Value = daq_input.aream_data["VW14"];
                displayInteger_currentshift_zuoguanshijian_hour.Value = daq_input.aream_data["VW20"];
                displayInteger_currentshift_zuoguanshijian_minute.Value = daq_input.aream_data["VW18"];
                displayInteger_currentshift_kailushijian_hour.Value = daq_input.aream_data["VW24"];
                displayInteger_currentshift_kailushijian_minute.Value = daq_input.aream_data["VW22"];
                displayDouble_chanliangtongji_danmomotou_count.Value = daq_input.aream_data["VW26"];
                displayDouble_chanliangtongji_jihua_count.Value = daq_input.aream_data["VW28"];
                displayDouble_chanliangtongji_shiji_count.Value = daq_input.aream_data["VW30"];
            }
            bool myflag;
            if ((daq_input.aream_data["VB100"] & 0x01) == 0x01)
            {
                myflag = true;
                if (myflag != shiftflag_1) //这次和上次状态不一样
                {
                    displayDouble_zuomushijian_1.Value = displayDouble_zuomushijian_2.Value;
                }
                displayDouble_zuomushijian_2.Value = daq_input.aream_data["VW34"]/10.0f;
                zuomotime = daq_input.aream_data["VW34"]/10.0f;
            }
            else
            {
                myflag = false;
                ;
                if (myflag != shiftflag_1) //这次和上次状态不一样
                {
                    displayDouble_zuomushijian_1.Value = displayDouble_zuomushijian_2.Value;
                }
                displayDouble_zuomushijian_2.Value = daq_input.aream_data["VW32"]/10.0f;
                zuomotime = daq_input.aream_data["VW32"]/10.0f;
            }
            shiftflag_1 = myflag;

            if ((daq_input.aream_data["VB100"] & 0x02) == 0x02)
            {
                myflag = true;
                if (myflag != shiftflag_2) //这次和上次状态不一样
                {
                    displayDouble_tuomushijian_1.Value = displayDouble_tuomushijian_2.Value;
                }
                displayDouble_tuomushijian_2.Value = daq_input.aream_data["VW38"]/10.0d;
            }
            else
            {
                myflag = false;
                if (myflag != shiftflag_2) //这次和上次状态不一样
                {
                    displayDouble_tuomushijian_1.Value = displayDouble_tuomushijian_2.Value;
                }
                displayDouble_tuomushijian_2.Value = daq_input.aream_data["VW36"]/10.0d;
            }
            shiftflag_2 = myflag;

            if ((daq_input.aream_data["VB100"] & 0x04) == 0x04)
            {
                myflag = true;
                if (myflag != shiftflag_3) //这次和上次状态不一样
                {
                    displayDouble_shuayoushijian_1.Value = displayDouble_shuayoushijian_2.Value;
                }
                displayDouble_shuayoushijian_2.Value = daq_input.aream_data["VW42"]/10.0d;
            }
            else
            {
                myflag = false;
                if (myflag != shiftflag_3) //这次和上次状态不一样
                {
                    displayDouble_shuayoushijian_1.Value = displayDouble_shuayoushijian_2.Value;
                }
                displayDouble_shuayoushijian_2.Value = daq_input.aream_data["VW40"]/10.0d;
            }
            shiftflag_3 = myflag;

            if ((daq_input.aream_data["VB100"] & 0x08) == 0x08)
            {
                myflag = true;
                if (myflag != shiftflag_4) //如果这次和上次状态不一样
                {
                    displayDouble_jinliaoshijian_1.Value = displayDouble_jinliaoshijian_2.Value;
                }
                displayDouble_jinliaoshijian_2.Value = daq_input.aream_data["VW46"]/10.0d;
            }
            else
            {
                myflag = false;
                if (myflag != shiftflag_4) //如果这次和上次状态不一样
                {
                    displayDouble_jinliaoshijian_1.Value = displayDouble_jinliaoshijian_2.Value;
                }
                displayDouble_jinliaoshijian_2.Value = daq_input.aream_data["VW44"]/10.0d;
            }
            shiftflag_4 = myflag;


            if ((daq_input.aream_data["MB0"] & 0x01) == 0x01 && (led_manul.BlinkerEnabled = true))
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
            if ((daq_input.aream_data["MB0"] & 0x02) == 0x02 && (led_pause.BlinkerEnabled = true))
            {
                led_pause.BlinkerEnabled = false;
                led_pause.Value.AsBoolean = true;
                led_pause.Indicator.Text = "启动";
            }
            else if (led_pause.BlinkerEnabled == false)
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
            var heartinfo = new Dictionary<string, string>();
            heartinfo["deviceid"] = TerminalParameters.Default.terminal_id;
            heartinfo["ip"] = TerminalParameters.Default.terminal_server_ip;
            heartinfo["terminalname"] = TerminalParameters.Default.terminal_name;
            heartinfo["onlinetime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            heartinfo["ip"] = TerminalCommon.GetInternalIP();

            try
            {
                TerminalQueues.tcpuplinkqueue.Enqueue(JsonConvert.SerializeObject(heartinfo,
                    Formatting.Indented));
            }
            catch (Exception e)
            {
                log.Error("Tcp Uplink Data Process Error!", e);
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
            var strSql = "select warninfo,warntime,warnlevel from warninfo order by warninfoid desc limit 500";
            var ds = new DataSet();
            var dTable = new DataTable();
            ds = TerminalLocalDataStorage.Query(strSql);
            dTable = ds.Tables[0];
            for (var i = 0; i < dTable.Rows.Count; i++)
            {
                var index = dataGridView_warn.Rows.Add();
                dataGridView_warn.Rows[index].Cells[0].Value = dTable.Rows[i]["warninfo"];
                dataGridView_warn.Rows[index].Cells[1].Value = dTable.Rows[i]["warntime"];
                dataGridView_warn.Rows[index].Cells[2].Value = dTable.Rows[i]["warnlevel"];
                if (index >= 499)
                {
                    break;
                }
               
            }
            dataGridView_warn.Sort(dataGridView_warn.Columns[1], ListSortDirection.Descending);

            strSql = "select warninfo,warntime from warninfo where warnlevel='脱模报警' order by warninfoid desc limit 500";
            ds = new DataSet();
            dTable = new DataTable();
            ds = TerminalLocalDataStorage.Query(strSql);
            dTable = ds.Tables[0];
            for (var i = 0; i < dTable.Rows.Count; i++)
            {
                var index = dataGridView_demould.Rows.Add();
                dataGridView_demould.Rows[index].Cells[0].Value = dTable.Rows[i]["warninfo"];
                dataGridView_demould.Rows[index].Cells[1].Value = dTable.Rows[i]["warntime"];
                if (index >= 499)
                {
                    break;
                }        
            }

            
            dataGridView_demould.Sort(dataGridView_demould.Columns[1], ListSortDirection.Descending);
            
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
            typeselect.Items.Clear();
            var strSql = "select product_id from proceduretechnologybase";
            var ds = new DataSet();
            var dTable = new DataTable();
            ds = TerminalLocalDataStorage.Query(strSql);
            dTable = ds.Tables[0];
            for (var i = 0; i < dTable.Rows.Count; i++)
            {
                typeselect.Items.Add(dTable.Rows[i]["product_id"]);
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
        public static void DataAutoSync()
        {
            //var columns = "deviceid,value,shuayou_consume_seconds,kaomo_consume_seconds," +
            //              "kaoliao_consume_seconds,lengque_consume_seconds,jinliao_consume_seconds,kaomo_temp,kaoliao_temp,cycletime,storetime,systus";
            var tablename = "historydata_" + DateTime.Today.ToString("yyyyMMdd");
            var cfg = FluentNhibernateHelper.GetSessionConfig();
            FluentNhibernateHelper.MappingTablenames(cfg, typeof (historydata), tablename);
            var sefactory = cfg.BuildSessionFactory();
            using (var mysql = sefactory.OpenSession())
            {
                try
                {
                    var cmd = mysql.Connection.CreateCommand();
                    cmd.CommandText = "select d.storetime,n.storetime from " + tablename + " d join " + tablename +
                                      " n on(n.historydataid=d.historydataid+1) where timediff(n.storetime, d.storetime) >5 AND d.storetime BETWEEN DATE_SUB(now(), INTERVAL 1 HOUR ) AND NOW();";
                    IDataReader reader = cmd.ExecuteReader();
                    var dTable = new DataTable();
                    dTable.Load(reader);
                    var i = dTable.Rows.Count;
                    int j;
                    try
                    {
                        var strSql = "select * from historydata where recordtime>@time1 and recordtime<@time2";
                        for (j = 0; j < i; j++)
                        {
                            SQLiteParameter[] parameters =
                            {
                                new SQLiteParameter("@time1", dTable.Rows[j][0]),
                                new SQLiteParameter("@time2", dTable.Rows[j][1])
                            };
                            var ds = new DataSet();
                            ds = TerminalLocalDataStorage.Query(strSql, parameters);
                            var synctable = new DataTable();
                            synctable = ds.Tables[0];
                            for (var n = 0; n < synctable.Rows.Count; n++)
                            {
                                var mytable = new historydata();
                                mytable.deviceid = TerminalParameters.Default.terminal_name;
                                if (Convert.IsDBNull(synctable.Rows[n][1]) == false)
                                {
                                    mytable.value = Convert.ToString(synctable.Rows[n][1]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][2]) == false)
                                {
                                    mytable.shuayou_consume_seconds = (float) Convert.ToDouble(synctable.Rows[n][2]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][3]) == false)
                                {
                                    mytable.kaomo_consume_seconds = (float) Convert.ToDouble(synctable.Rows[n][3]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][4]) == false)
                                {
                                    mytable.kaoliao_consume_seconds = (float) Convert.ToDouble(synctable.Rows[n][4]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][5]) == false)
                                {
                                    mytable.lengque_consume_seconds = (float) Convert.ToDouble(synctable.Rows[n][5]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][6]) == false)
                                {
                                    mytable.jinliao_consume_seconds = (float) Convert.ToDouble(synctable.Rows[n][6]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][7]) == false)
                                {
                                    mytable.kaomo_temp = (float) Convert.ToDouble(synctable.Rows[n][7]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][8]) == false)
                                {
                                    mytable.kaoliao_temp = (float) Convert.ToDouble(synctable.Rows[n][8]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][9]) == false)
                                {
                                    mytable.cycletime = (float) Convert.ToDouble(synctable.Rows[n][9]);
                                }
                                if (Convert.IsDBNull(synctable.Rows[n][10]) == false)
                                {
                                    mytable.storetime = Convert.ToDateTime(synctable.Rows[n][10]);
                                }
                                mytable.systus = Convert.ToString(synctable.Rows[n][11]);
                                mysql.Save(mytable);
                                mysql.Flush();
                                mysql.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("同步出错", ex);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("同步查询阶段出错", ex);
                }
            }
            sefactory.Dispose();
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
                if (restartbutton == false)
                {
                    //if (tcpuplink_dataprocess_thread.IsAlive)
                    //{
                    //    tcpuplink_dataprocess_thread.Abort();
                    //    log.Info("tcpuplink_dataprocess_thread Abort");
                    //}

                    //if (tcpdownlink_dataprocess_thread.IsAlive)
                    //{
                    //    tcpdownlink_dataprocess_thread.Abort();
                    //    log.Info("tcpdownlink_dataprocess_thread Abort");
                    //}

                    if (plccommunication_thread.IsAlive)
                    {
                        plccommunication_thread.Abort();
                        log.Info("plccommunication_thread Abort");
                    }
                    if (plcdatahandler_thread.IsAlive)
                    {
                        plcdatahandler_thread.Abort();
                        log.Info("plcdatahandler_thread Abort");
                    }
                    if (datacenter_storage_thread.IsAlive)
                    {
                        datacenter_storage_thread.Abort();
                        log.Info("datacenter_storage_thread Abort");
                    }
                    if (local_storage_thread.IsAlive)
                    {
                        local_storage_thread.Abort();
                        log.Info("local_storage_thread Abort");
                    }
                    if (S7SNAP.Connected())
                    {
                        S7SNAP.Disconnect();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Abort Error!", ex);
            }
            finally
            {
                //schedule.Dispose();
                log.Info("Application is about to exit.");
                Application.ExitThread();
            }
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
                    log.Error("tcp发送线程出错！", ex);
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
                    log.Error("tcp发送线程出错！", ex);
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
        public void PlcReadCycle()
        {
            bool readflag = true;
            var sw = new Stopwatch();
            Thread.Sleep(1500);
            while (true)
            {
                if (S7SNAP.Connected() == false)
                {
                    log.Warn("Plc not connected!");
                    Invoke(new TextUI(topbarupdata), "PLC连接中断");
                    Thread.Sleep(1000);
                    S7SNAP.Connect();
                    continue;
                }
                try
                {
                    sw.Reset();
                    sw.Start();
                    var daq_data = new PlcDAQCommunicationObject();
                    foreach (var item in fangpu_config.addr)
                    {
                        string[] range;
                        var buffer = new byte[2048];
                        var start = 0;
                        var end = 0;
                        var size = 0;
                        var Wordlen = 0;
                        if (item.Key.Contains("_"))
                        {
                            range = item.Value.Split('-');
                            start = Convert.ToInt32(range[0]);
                            end = Convert.ToInt32(range[1]);
                            size = end - start + 1;
                        }
                        else
                        {
                            start = Convert.ToInt32(item.Value);
                            size = 1;
                        }
                        if (item.Key.Contains("W"))
                        {
                            Wordlen = 2;
                        }
                        else if (item.Key.Contains("B") || item.Key.Contains("I"))
                        {
                            Wordlen = 1;
                        }
                        if (item.Key.Substring(0, 1).Equals("M"))
                        {
                            if (S7SNAP.MBRead(start, size, buffer) == 0)
                            {
                                BufferConverter.BufferDump(daq_data, buffer, start, size, "M", Wordlen);
                                readflag = true;
                            }
                            else
                            {
                                readflag = false;
                                break;
                            }
                            continue;
                        }
                        if (item.Key.Substring(0, 1).Equals("V"))
                        {
                            if (S7SNAP.DBRead(1, start, size, buffer) == 0)
                            {
                                BufferConverter.BufferDump(daq_data, buffer, start, size, "V", Wordlen);
                                readflag = true;
                            }
                            else
                            {
                                readflag = false;
                                break;
                            }
                            continue;
                        }
                        if (item.Key.Substring(0, 1).Equals("I"))
                        {
                            if (S7SNAP.EBRead(start, size, buffer) == 0)
                            {
                                BufferConverter.BufferDump(daq_data, buffer, start, size, "I", Wordlen);
                                readflag = true;
                            }
                            else
                            {
                                readflag = false;
                                break;
                            }
                            continue;
                        }
                    }
                    if (readflag == false)
                    {
                        sw.Reset();
                        continue;
                    }
                    TerminalQueues.plcdataprocessqueue.Enqueue(daq_data);
                    sw.Stop();
                    //Trace.WriteLine(sw.ElapsedMilliseconds);
                    if (sw.ElapsedMilliseconds >= 1000)
                        continue;
                    Thread.Sleep(1000 - Convert.ToInt32(sw.Elapsed.TotalMilliseconds));
                }
                catch (Exception ex)
                {
                    log.Error("读数据线程出错！", ex);
                    Thread.Sleep(600);
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
            while (true)
            {
                try
                {
                    if (S7SNAP.Connected())
                    {
                        if (TerminalQueues.plccommandqueue.Count > 0)
                        {
                            byte[] buffer;
                            byte[] sbuf = new byte[2];
                            int result;
                            enableSync = false;
                            var temp_plccmd = new PlcCommand(TerminalCommon.S7200AreaI,
                                TerminalCommon.S7200DataByte, 0, 0, 0);
                            for (; TerminalQueues.plccommandqueue.Count > 0;)
                            {
                                TerminalQueues.plccommandqueue.TryDequeue(out temp_plccmd);
                                buffer = BitConverter.GetBytes(temp_plccmd.data);
                                if (temp_plccmd.area == "M")
                                {
                                    result = S7SNAP.WriteArea(S7Client.S7AreaMK, 0,
                                        temp_plccmd.addr*8 + temp_plccmd.bitaddr, 1,
                                        S7Client.S7WLBit, buffer);
                                    continue;
                                }
                                //Area V
                                if (temp_plccmd.type == TerminalCommon.S7200DataByte)
                                {
                                    S7SNAP.WriteArea(S7Client.S7AreaDB, 1, temp_plccmd.addr, 1, S7Client.S7WLByte,
                                        buffer);
                                }
                                else if (temp_plccmd.type == TerminalCommon.S7200DataWord)
                                {
                                    sbuf = new byte[2];
                                    sbuf[0] = buffer[1];
                                    sbuf[1] = buffer[0];
                                    S7SNAP.WriteArea(S7Client.S7AreaDB, 1, temp_plccmd.addr, 1, S7Client.S7WLWord,
                                        sbuf);
                                }
                            }
                            enableSync = true;
                        }
                    }
                    else
                    {
                        S7SNAP.Connect();
                    }
                }
                catch (Exception ex)
                {
                    enableSync = true;
                    log.Error("写线程出错", ex);
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
            PPI = new S7_PPI(TerminalParameters.Default.plc_com_port,
                TerminalParameters.Default.plc_com_baudrate);
            var read_count = 0;
            PPI.Connect(0x02);
            while (true)
            {
                try
                {
                    if (PPI.SPort.IsOpen)
                    {
                        //    PPI.Set_Cpu_State("Run");
                        //Trace.WriteLine(PPI.Send_Cmd("6821006802006C32010000D1D1000E00060501120A100200020001840000180004001004B0F216"));
                        //Trace.WriteLine(PPI.Write_Bit(PPI.AreaM, 1, 5, 1));
                        //  Trace.WriteLine(PPI.Write(PPI.AreaV, 3, PPI.LenW, 1234));
                        read_count++;
                        if (read_count > 10)
                        {
                            var aream_data = new Dictionary<string, int>();
                            aream_data["MB0"] = PPI.Read(PPI.AreaM, 0, PPI.LenD);
                            aream_data["MB1"] = PPI.Read(PPI.AreaM, 4, PPI.LenD);

                            aream_data["VB4000"] = PPI.Read(PPI.AreaV, 4000, PPI.LenD);
                            aream_data["VB4004"] = PPI.Read(PPI.AreaV, 4004, PPI.LenD);
                            aream_data["VB4008"] = PPI.Read(PPI.AreaV, 4008, PPI.LenD);
                            aream_data["VW48"] = PPI.Read(PPI.AreaT, 37, PPI.LenW);
                            aream_data["VW50"] = PPI.Read(PPI.AreaT, 38, PPI.LenW);
                            aream_data["VW52"] = PPI.Read(PPI.AreaT, 39, PPI.LenW);
                            aream_data["VW54"] = PPI.Read(PPI.AreaT, 40, PPI.LenW);
                            aream_data["VW56"] = PPI.Read(PPI.AreaT, 41, PPI.LenW);
                            aream_data["VW14"] = PPI.Read(PPI.AreaC, 1, PPI.LenW);
                            aream_data["VW16"] = PPI.Read(PPI.AreaC, 2, PPI.LenW);
                            aream_data["VW20"] = PPI.Read(PPI.AreaC, 4, PPI.LenW);
                            read_count = 0;
                        }

                        if (TerminalQueues.plccommandqueue.Count > 0)
                        {
                            var temp_plccmd = new PlcCommand(TerminalCommon.S7200AreaI,
                                TerminalCommon.S7200DataByte, 0, 0, 0);
                            for (; TerminalQueues.plccommandqueue.Count > 0;)
                            {
                                TerminalQueues.plccommandqueue.TryDequeue(out temp_plccmd);
                                switch (temp_plccmd.area)
                                {
                                    case "M":
                                        PPI.Write_Bit(PPI.AreaM, temp_plccmd.addr, temp_plccmd.bitaddr, temp_plccmd.data);
                                        break;
                                    case "V":
                                        PPI.Write(PPI.AreaV, temp_plccmd.addr, 2, temp_plccmd.data);
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
                catch (Exception ex)
                {
                    log.Error("tcp发送线程出错！", ex);
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
                        var plc_temp_data = new PlcDAQCommunicationObject(); 
                        TerminalQueues.plcdataprocessqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data == null)
                        {
                            continue;
                        }
                        CycleUpdateGuiDisplay(plc_temp_data);
                        WarnInfoProcess(plc_temp_data);               
                        TerminalQueues.localdataqueue.Enqueue(plc_temp_data);
                        TerminalQueues.datacenterprocessqueue.Enqueue(plc_temp_data);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("plc数据处理线程出错！", ex);
                    Thread.Sleep(200);
                }
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
            var cfg = FluentNhibernateHelper.GetSessionConfig();
            ISessionFactory sf;
            ISession session;
            again:
            try
            {
                sf = cfg.BuildSessionFactory();
                session = sf.OpenSession();
            }
            catch
            {
                log.Error("首次连接数据中心失败");
                goto again;
            }
            string tablename = "";
            while (true)
            {
                var tablename_new = "historydata_" + DateTime.Today.ToString("yyyyMMdd");
                if (tablename_new != tablename)
                {
                    tablename = tablename_new;
                    FluentNhibernateHelper.MappingTablenames(cfg, typeof (historydata), tablename);
                    session.Disconnect();
                    session.Close();
                    session.Dispose();
                    sf.Close();
                    sf.Dispose();
                    sf = cfg.BuildSessionFactory(); //日期变化时重新打开连接
                    session = sf.OpenSession(); //建立新的连接         
                }

                if (TerminalQueues.datacenterprocessqueue.Count > 0)
                {
                    try
                    {
                        var plc_temp_data = new PlcDAQCommunicationObject();
                        TerminalQueues.datacenterprocessqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data != null)
                        {
                            var jsonobj = new FangpuTerminalJsonModel();
                            var jsonobj_2 = new FangpuTerminalJsonModel_systus();
                            jsonobj.V4000 = plc_temp_data.aream_data["VB4000"];
                            jsonobj.V4001 = plc_temp_data.aream_data["VB4001"];
                            jsonobj.V4002 = plc_temp_data.aream_data["VB4002"];
                            jsonobj.V4003 = plc_temp_data.aream_data["VB4003"];
                            jsonobj.V4004 = plc_temp_data.aream_data["VB4004"];
                            jsonobj.V4005 = plc_temp_data.aream_data["VB4005"];
                            jsonobj.V4006 = plc_temp_data.aream_data["VB4006"];
                            jsonobj.V4007 = plc_temp_data.aream_data["VB4007"];
                            jsonobj.V4008 = plc_temp_data.aream_data["VB4008"];
                            jsonobj.M53 = (plc_temp_data.aream_data["MB5"] & 0x08) == 0x08;
                            jsonobj_2.M37 = (plc_temp_data.aream_data["MB3"] & 0x80) == 0x80;
                            jsonobj_2.M42 = (plc_temp_data.aream_data["MB4"] & 0x04) == 0x04;
                            jsonobj_2.M52 = (plc_temp_data.aream_data["MB5"] & 0x04) == 0x04;
                            jsonobj_2.M44 = (plc_temp_data.aream_data["MB4"] & 0x10) == 0x10;
                            jsonobj_2.M67 = (plc_temp_data.aream_data["MB6"] & 0x80) == 0x80;
                            jsonobj_2.M00 = (plc_temp_data.aream_data["MB0"] & 0x01) == 0x01;
                            jsonobj_2.M01 = (plc_temp_data.aream_data["MB0"] & 0x02) == 0x02;

                            historydata historyDb = new historydata
                            {
                                deviceid = TerminalParameters.Default.terminal_name,
                                value = JsonConvert.SerializeObject(jsonobj),
                                shuayou_consume_seconds = plc_temp_data.aream_data["VW50"] / 10.0f,
                                kaomo_consume_seconds = plc_temp_data.aream_data["VW52"] / 10.0f,
                                kaoliao_consume_seconds = plc_temp_data.aream_data["VW54"] / 10.0f,
                                lengque_consume_seconds = plc_temp_data.aream_data["VW48"] / 10.0f,
                                jinliao_consume_seconds = plc_temp_data.aream_data["VW56"] / 10.0f,
                                kaomo_temp = 0,
                                kaoliao_temp = 0,
                                cycletime = (float)zuomotime,
                                storetime = plc_temp_data.daq_time,
                                systus = JsonConvert.SerializeObject(jsonobj_2)
                            };
                            var historydata_json = new Dictionary<string, object>();
                            historydata_json.Add("刷油时间", plc_temp_data.aream_data["VW50"] / 10.0f);
                            historydata_json.Add("烤模时间", plc_temp_data.aream_data["VW52"] / 10.0f);
                            historydata_json.Add("烤料时间", plc_temp_data.aream_data["VW54"] / 10.0f);
                            historydata_json.Add("浸料时间", plc_temp_data.aream_data["VW56"] / 10.0f);
                            historydata_json.Add("冷却时间", plc_temp_data.aream_data["VW48"] / 10.0f);
                            historydata_json.Add("一板模时间", (float)zuomotime);
                            historydata_jsoncopy historydatajsonDb = new historydata_jsoncopy();
                            historydatajsonDb.deviceid = TerminalParameters.Default.terminal_name;
                            historydatajsonDb.data_json = JsonConvert.SerializeObject(historydata_json);
                            historydatajsonDb.storetime = plc_temp_data.daq_time;
                            historydatajsonDb.systus = JsonConvert.SerializeObject(jsonobj_2);

                            var realtimedata = new realtimedata();
                            realtimedata =
                                session.QueryOver<realtimedata>().Where(p => p.deviceid == "D17").SingleOrDefault();
                            realtimedata.deviceid = TerminalParameters.Default.terminal_name;
                            realtimedata.value = JsonConvert.SerializeObject(jsonobj);
                            realtimedata.storetime = plc_temp_data.daq_time;
                            realtimedata.shuayou_consume_seconds = plc_temp_data.aream_data["VW50"] / 10.0f;
                            realtimedata.kaomo_consume_seconds = plc_temp_data.aream_data["VW52"] / 10.0f;
                            realtimedata.kaoliao_consume_seconds = plc_temp_data.aream_data["VW54"] / 10.0f;
                            realtimedata.jinliao_consume_seconds = plc_temp_data.aream_data["VW56"] / 10.0f;
                            realtimedata.lengque_consume_seconds = plc_temp_data.aream_data["VW48"] / 10.0f;
                            realtimedata.device_on_time = plc_temp_data.aream_data["VW16"] + "小时" +
                                                          plc_temp_data.aream_data["VW14"] + "分钟";
                            realtimedata.furnace_on_time = plc_temp_data.aream_data["VW24"] + "小时" +
                                                           plc_temp_data.aream_data["VW22"] + "分钟";
                            realtimedata.produce_time = plc_temp_data.aream_data["VW20"] + "小时" +
                                                        plc_temp_data.aream_data["VW18"] + "分钟";
                            realtimedata.cycletime = (float)zuomotime;
                            realtimedata.systus = JsonConvert.SerializeObject(jsonobj_2);

                            using (var tran = session.BeginTransaction())
                            {
                                session.Save(historydatajsonDb);
                                session.Save(historyDb);
                                session.SaveOrUpdate(realtimedata);
                                tran.Commit();
                            }
                        }             
                    }
                    catch (Exception ex)
                    {
                        if (!session.IsConnected)
                        {
                            try
                            {
                                session.Reconnect();
                            }
                            catch
                            {
                            }
                        }
                        log.Error("数据中心存储线程出错！", ex);
                    }
                }

                if (TerminalQueues.warninfoqueue.Count > 0)
                {
                    var plc_warn_data = new PLCWarningObject();
                    TerminalQueues.warninfoqueue.TryDequeue(out plc_warn_data);
                    if (plc_warn_data != null)
                    {
                        foreach (var warninfo in plc_warn_data.warndata)
                            try
                            {
                                warn_info warn_info = new warn_info();
                                warn_info.device_name = TerminalParameters.Default.terminal_name;
                                warn_info.warn_message = warninfo.Key;
                                warn_info.warn_level = warninfo.Value;
                                warn_info.storetime = plc_warn_data.warn_time;
                                using (var tran = session.BeginTransaction())
                                {
                                    session.Save(warn_info);
                                    tran.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("报警信息存储出错", ex);
                            }
                    }
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
        public void SendCommandToPlc(string area, string type, int data, int addr, int bitaddr = 0)
        {
            var cmd = new PlcCommand(area, type, data, addr, bitaddr);
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
            BeginInvoke(S1, daq_input);
        }

        //==================================================================
        //模块名： GetWarnInfo
        //作者：    Yang Chuan
        //日期：    2015.12.02
        //功能：    报警信息处理
        //输入参数：PLC数据字典
        //返回值：  相应的报警信息
        //修改记录：
        //==================================================================
        public Dictionary<string, string> GetWarnInfo(Dictionary<string, int> info)
        {
            var results = new Dictionary<string, string>();//key 报警信息 value 报警等级
            var base_zero = 0;
            var i = 0;

            for (var j = 0; j <= 8; j++)
            {
                if (info["VB400" + j] > 0)
                {
                    for (i = 0; i <= 7; i++)
                    {
                        if ((info["VB400" + j] & ((1 << i))) == 1 << i)
                        {
                            results[(TerminalCommon.warn_info["400" + j + "_" + i])] = null;
                            if (j == 8 && i == 5)
                                results[(TerminalCommon.warn_info["400" + j + "_" + i])] = "脱模报警";
                            else
                            foreach (string key in TerminalCommon.warn_stop_info)
                            {
                                if (key.Equals("400" + j + "_" + i))
                                {
                                    results[(TerminalCommon.warn_info["400" + j + "_" + i])] = "停机报警"; //是否是停机报警
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }

        //==================================================================
        //模块名： WarnInfoProcess
        //作者：    Yufei Zhang
        //日期：    2016.05.03
        //功能：    报警信息处理
        //输入参数： 读数据实例
        //返回值：  
        //修改记录：
        //==================================================================
        public void WarnInfoProcess(PlcDAQCommunicationObject plc_data)
        {
            if (warnflag && (plc_data.aream_data["MB5"] & 0x08) == 0x08)
            {
                PLCWarningObject plcwarn = new PLCWarningObject();
                plcwarn.warn_time = plc_data.daq_time;
                var results = GetWarnInfo(plc_data.aream_data);
                plcwarn.warndata = results;
                TerminalQueues.warninfoqueue.Enqueue(plcwarn);
                TerminalQueues.warninfoqueue_local.Enqueue(plcwarn);
            }
            if (tuomuflag && (plc_data.aream_data["I5"] & 0x40) == 0x40)
            {
                PLCWarningObject plcwarn = new PLCWarningObject();
                plcwarn.warn_time = plc_data.daq_time;
                plcwarn.warndata["这一板模没有脱掉"] = "脱模报警";
                TerminalQueues.warninfoqueue.Enqueue(plcwarn);
                TerminalQueues.warninfoqueue_local.Enqueue(plcwarn);
            }
            warnflag = (plc_data.aream_data["MB5"] & 0x08) != 0x08;//True if M5.3=0
        }

        /// <summary>
        /// 本地存储警告数据
        /// </summary>
        /// <param name="warninfos"></param>
        /// <param name="warntime"></param>
        public void WarnInfoLocalStorage(Dictionary<string, string> warninfos, DateTime warntime)
        {
            var strSql = new StringBuilder();
            strSql.Append("insert into warninfo (");
            strSql.Append("warninfo,warntime,warnlevel)");
            strSql.Append(" values(");
            strSql.Append("@warninfo,@warntime,@warnlevel)");
            foreach (var item in warninfos)
            {
                SQLiteParameter[] parameters =
                {
                    TerminalLocalDataStorage.MakeSQLiteParameter("@warninfo", DbType.String, 200, item.Key),
                    TerminalLocalDataStorage.MakeSQLiteParameter("@warntime", DbType.DateTime, 30, warntime),
                    TerminalLocalDataStorage.MakeSQLiteParameter("@warnlevel", DbType.String, 30, item.Value)
                };
                TerminalLocalDataStorage.ExecuteSql(strSql.ToString(), parameters);
            }
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
                        var plc_temp_data = new PlcDAQCommunicationObject();
                        TerminalQueues.localdataqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data == null)
                        {
                            continue;
                        }
                        var jsonobj = new FangpuTerminalJsonModel();
                        var jsonobj_2 = new FangpuTerminalJsonModel_systus();
                        jsonobj.V4000 = plc_temp_data.aream_data["VB4000"];
                        jsonobj.V4001 = plc_temp_data.aream_data["VB4001"];
                        jsonobj.V4002 = plc_temp_data.aream_data["VB4002"];
                        jsonobj.V4003 = plc_temp_data.aream_data["VB4003"];
                        jsonobj.V4004 = plc_temp_data.aream_data["VB4004"];
                        jsonobj.V4005 = plc_temp_data.aream_data["VB4005"];
                        jsonobj.V4006 = plc_temp_data.aream_data["VB4006"];
                        jsonobj.V4007 = plc_temp_data.aream_data["VB4007"];
                        jsonobj.V4008 = plc_temp_data.aream_data["VB4008"];
                        jsonobj.M53 = (plc_temp_data.aream_data["MB5"] & 0x08) == 0x08;

                        jsonobj_2.M37 = (plc_temp_data.aream_data["MB3"] & 0x80) == 0x80;
                        jsonobj_2.M42 = (plc_temp_data.aream_data["MB4"] & 0x04) == 0x04;
                        jsonobj_2.M52 = (plc_temp_data.aream_data["MB5"] & 0x04) == 0x04;
                        jsonobj_2.M44 = (plc_temp_data.aream_data["MB4"] & 0x10) == 0x10;
                        jsonobj_2.M67 = (plc_temp_data.aream_data["MB6"] & 0x80) == 0x80;
                        jsonobj_2.M00 = (plc_temp_data.aream_data["MB0"] & 0x01) == 0x01;
                        jsonobj_2.M01 = (plc_temp_data.aream_data["MB0"] & 0x02) == 0x02;
                        fangpu_terminal.Ultility.Nhibernate.LiteGroup.historydata_lite n =
                            new fangpu_terminal.Ultility.Nhibernate.LiteGroup.historydata_lite();
                        var strSql = new StringBuilder();
                        strSql.Append("insert into historydata(");
                        strSql.Append(
                            "data,systus,recordtime,shuayou_consume_seconds,kaomo_consume_seconds,kaoliao_consume_seconds,jinliao_consume_seconds,lengque_consume_seconds,cycletime)");
                        strSql.Append("values(");
                        strSql.Append(
                            "@data,@systus,@recordtime,@shuayou_consume_seconds,@kaomo_consume_seconds,@kaoliao_consume_seconds,@jinliao_consume_seconds,@lengque_consume_seconds,@cycletime)");

                        SQLiteParameter[] parameters =
                        {
                            TerminalLocalDataStorage.MakeSQLiteParameter("@data", DbType.String, 100,
                                JsonConvert.SerializeObject(jsonobj)),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@systus", DbType.String, 100,
                                JsonConvert.SerializeObject(jsonobj_2)),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@recordtime", DbType.DateTime, 30,
                                plc_temp_data.daq_time),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@shuayou_consume_seconds", DbType.Double, 100,
                                plc_temp_data.aream_data["VW50"]/10.0),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@kaomo_consume_seconds", DbType.Double, 100,
                                plc_temp_data.aream_data["VW52"]/10.0),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@kaoliao_consume_seconds", DbType.Double, 100,
                                plc_temp_data.aream_data["VW54"]/10.0),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@jinliao_consume_seconds", DbType.Double, 100,
                                plc_temp_data.aream_data["VW56"]/10.0),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@lengque_consume_seconds", DbType.Double, 100,
                                plc_temp_data.aream_data["VW48"]/10.0),
                            TerminalLocalDataStorage.MakeSQLiteParameter("@cycletime", DbType.Double, 100,
                                (float) zuomotime)
                        };

                        if (TerminalLocalDataStorage.ExecuteSql(strSql.ToString(), parameters) >= 1)
                        {
                            ret = 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("本地数据存储线程出错!" + ex);
                }
                try
                {
                    if (TerminalQueues.warninfoqueue_local.Count > 0)
                    {
                        var plc_temp_warn = new PLCWarningObject();
                        TerminalQueues.warninfoqueue_local.TryDequeue(out plc_temp_warn);
                        WarnInfoLocalStorage(plc_temp_warn.warndata, plc_temp_warn.warn_time);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("警告存储出错！" + ex);
                }
            }
        }



        /// <summary>
        /// Get web command and execute
        /// </summary>
        public void WebCommand()
        {
            ISession session;
            again:
            try
            {
                session = FluentNhibernateHelper.GetSession();
            }
            catch
            {
                goto again;
            }

            try
            {
                while (true)
                {
                    var results = session.CreateCriteria<terminalcmd>()
                        .Add(Restrictions.Eq("device_name", "D17"))
                        .Add(Restrictions.Eq("status", "pending"))
                        .AddOrder(Order.Desc("idterminalcmd"))
                        .List<terminalcmd>();
                    foreach (var result in results)
                    {
                        WebCommandUI cmdproc = new WebCommandUI(WebCommandExecute); //创建委托显示提示
                        switch (result.command)
                        {
                            case ("restart"):
                            {
                                result.status = "processed";
                                result.time = DateTime.Now;
                                session.SaveOrUpdate(result);
                                session.Flush();
                                string msg = "接收到堡垒机下达的程序重启指令\n程序即将重启...";
                                this.BeginInvoke(new MessageBoxShow(MessageBoxShow_F), new object[] {msg});
                                Thread.Sleep(5000);
                                log.Info("收到重启程序指令");
                                this.BeginInvoke(cmdproc, "restart");
                                break;
                            }
                            case ("reboot"):
                            {
                                result.status = "processed";
                                result.time = DateTime.Now;
                                session.SaveOrUpdate(result);
                                session.Flush();
                                string msg = "接收到堡垒机下达的重启指令\n终端即将重启...";
                                this.BeginInvoke(new MessageBoxShow(MessageBoxShow_F), new object[] {msg});
                                Thread.Sleep(5000);
                                log.Info("收到重启终端指令");
                                this.BeginInvoke(cmdproc, "reboot");
                                break;
                            }
                            case ("shutdown"):
                            {
                                result.status = "processed";
                                result.time = DateTime.Now;
                                session.SaveOrUpdate(result);
                                session.Flush();
                                string msg = "接收到堡垒机下达的关机指令\n终端即将关闭...";
                                this.BeginInvoke(new MessageBoxShow(MessageBoxShow_F), new object[] {msg});
                                Thread.Sleep(5000);
                                log.Info("收到关机指令");
                                this.BeginInvoke(cmdproc, "shutdown");
                                break;
                            }
                            case ("update"):
                            {
                                string msg = "接收到堡垒机下达的更新指令\n是否进行更新？...";
                                if ((bool) this.Invoke(new MessageBoxShow(MessageBoxShow_F), new object[] {msg}) ==
                                    false)
                                {
                                    result.status = "refused";
                                    result.time = DateTime.Now;
                                    session.SaveOrUpdate(result);
                                    session.Flush();
                                    break;
                                }
                                else
                                {
                                    result.status = "processing";
                                    result.time = DateTime.Now;
                                    session.SaveOrUpdate(result);
                                    session.Flush();
                                    log.Info("尝试进行更新");
                                    Thread.Sleep(5000);
                                    this.BeginInvoke(cmdproc, "update");
                                }
                                break;
                            }
                            case ("update_f"):
                            {
                                string msg = "接收到堡垒机下达的强制更新指令\n程序即将更新...更新过程将关闭程序";
                                BeginInvoke(new MessageBoxShow(MessageBoxShow_F), new object[] {msg});
                                result.status = "processing";
                                result.time = DateTime.Now;
                                session.SaveOrUpdate(result);
                                session.Flush();
                                log.Info("尝试进行强制更新");
                                this.BeginInvoke(cmdproc, "update");
                                break;
                            }
                            default:
                                continue;
                        }
                    }
                    Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                log.Error("数据中心指令获取失败", ex);
                if (!session.IsConnected)
                {
                    try
                    {
                        FluentNhibernateHelper.ResetSession(ref session);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// 执行WEB指令
        /// </summary>
        /// <param name="cmd"></param>
        public void WebCommandExecute(string cmd)
        {
            switch (cmd)
            {
                case ("restart"):
                    TerminalCommon.AppRestart(this);
                    break;
                case ("reboot"):
                    TerminalCommon.SystemReboot(this);
                    break;
                case ("shutdown"):
                    TerminalCommon.SystemShutdown(this);
                    break;
                case ("update"):
                    TerminalCommon.ProcessStart(@"C:/UpdateService/UpdateService.exe", @"C:/UpdateService/)");
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// 委托显示messagebox
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="info"></param>
        /// <param name="buttontype"></param>
        /// <param name="icontype"></param>
        /// <returns></returns>
        delegate bool MessageBoxShow(
            string msg, string info = "提示信息", MessageBoxButtons buttontype = MessageBoxButtons.OK,
            MessageBoxIcon icontype = MessageBoxIcon.Information);

        /// <summary>
        /// 委托显示MessageBox
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="info"></param>
        /// <param name="buttontype"></param>
        /// <param name="icontype"></param>
        void DelegateMessagebox(string msg, string info = "提示信息", MessageBoxButtons buttontype = MessageBoxButtons.OK,
            MessageBoxIcon icontype = MessageBoxIcon.Information)
        {
            this.Invoke(new MessageBoxShow(MessageBoxShow_C), new object[] {msg, info, buttontype, icontype});
        }

        /// <summary>
        /// 显示消息窗口
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="info"></param>
        /// <param name="buttontype"></param>
        /// <param name="icontype"></param>
        /// <returns></returns>
        bool MessageBoxShow_C(string msg, string info = "提示信息", MessageBoxButtons buttontype = MessageBoxButtons.OK,
            MessageBoxIcon icontype = MessageBoxIcon.Information)
        {
            MessageBox.Show(msg, info, buttontype, icontype);
            return true;
        }
        /// <summary>
        /// 显示消息窗口
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="info"></param>
        /// <param name="buttontype"></param>
        /// <param name="icontype"></param>
        /// <returns></returns>
        bool MessageBoxShow_F(string msg, string info = "提示信息", MessageBoxButtons buttontype = MessageBoxButtons.OK,
            MessageBoxIcon icontype = MessageBoxIcon.Information)
        {
            if (DialogResult.OK ==
                MessageBox.Show(this, msg, "提示信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
                return true;
            else
                return false;
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
        public void UploadFormTable(object a)
        {
            try
            {
                using (var mysql = FluentNhibernateHelper.GetSession())
                {
                    mysql.Save(a);
                    mysql.Flush();
                    DelegateMessagebox("上传成功");
                }
            }
            catch (Exception ex)
            {
                DelegateMessagebox("上传失败");
            }
        }

        #region 读取PLC状态

        //==================================================================
        //模块名： SwitchSync
        //日期：    2015.12.11
        //功能：    GUI同步PLC开关状态
        //输入参数：PLC读数据通信实例
        //返回值：  无
        //修改记录：
        //==================================================================
        public void SwitchSync(PlcDAQCommunicationObject temp)
        {
            stopSendCmdtoPLC = true;
            if ((temp.aream_data["MB0"] & 0x01) == 0x01) //自动
            {
                switchRotary_runmode.Value = 1;
            }
            else
            {
                switchRotary_runmode.Value = 0;
            }
            if ((temp.aream_data["MB0"] & 0x02) == 0x02) //启动
            {
                switchRotary_runstatus.Value = 1;
            }
            else
            {
                switchRotary_runstatus.Value = 0;
            }

            if (tabControl_terminal.SelectedIndex == 2)
            {
                if ((temp.aream_data["MB3"] & 0x80) == 0x80) //刷油机
                {
                    switchRotary_pg3_shuayouji.Value = 1;
                }
                else
                {
                    switchRotary_pg3_shuayouji.Value = 0;
                }
                if ((temp.aream_data["MB4"] & 0x04) == 0x04) //水箱
                {
                    switchRotary_pg3_shuixiang.Value = 1;
                }
                else
                {
                    switchRotary_pg3_shuixiang.Value = 0;
                }
                if ((temp.aream_data["MB5"] & 0x04) == 0x04) //浸料气缸
                {
                    switchRotary_pg3_jinliaoqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg3_jinliaoqigang.Value = 0;
                }
                if ((temp.aream_data["MB4"] & 0x10) == 0x10) //脱模机
                {
                    switchRotary_pg3_tuomoji.Value = 1;
                }
                else
                {
                    switchRotary_pg3_tuomoji.Value = 0;
                }
                if ((temp.aream_data["MB6"] & 0x80) == 0x80) //炉子电源
                {
                    switchRotary_pg3_luzidianyuan.Value = 1;
                }
                else
                {
                    switchRotary_pg3_luzidianyuan.Value = 0;
                }
            }

            if (tabControl_terminal.SelectedIndex == 1)
            {
                if ((temp.aream_data["MB4"] & 0x08) == 0x08) //水箱
                {
                    switchSlider_pg2_shuixiang.Value = 1;
                }
                else
                {
                    switchSlider_pg2_shuixiang.Value = 0;
                }
                if ((temp.aream_data["MB5"] & 0x80) == 0x80) //浸料气缸
                {
                    switchSlider_pg2_jinliaoqigang.Value = 1;
                }
                else
                {
                    switchSlider_pg2_jinliaoqigang.Value = 0;
                }
                if ((temp.aream_data["MB5"] & 0x02) == 0x02) //脱模气缸
                {
                    switchRotary_pg2_tuomoqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_tuomoqigang.Value = 0;
                }
                if ((temp.aream_data["MB5"] & 0x40) == 0x40) //抽风机
                {
                    switchRotary_pg2_choufengji.Value = 1;
                }
                else
                {
                    switchRotary_pg2_choufengji.Value = 0;
                }
            }

            if (tabControl_terminal.SelectedIndex == 0)
            {
                if ((temp.aream_data["MB2"] & 0x02) == 0x02) //电机
                {
                    switchSlider_pg1_yureludianji.Value = 0;
                }
                else
                {
                    switchSlider_pg1_yureludianji.Value = 1;
                }
                if ((temp.aream_data["MB3"] & 0x08) == 0x08) //电机
                {
                    switchSlider_pg1_kaoliaoludianji.Value = 0;
                }
                else
                {
                    switchSlider_pg1_kaoliaoludianji.Value = 1;
                }
                if ((temp.aream_data["MB1"] & 0x80) == 0x80) //一号钳销
                {
                    switchSlider_pg1_no1qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no1qianxiao.Value = 0;
                }
                if ((temp.aream_data["MB1"] & 0x08) == 0x08) //二号钳销
                {
                    switchSlider_pg1_no2qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no2qianxiao.Value = 0;
                }
                if ((temp.aream_data["MB2"] & 0x40) == 0x40) //三号钳销
                {
                    switchSlider_pg1_no3qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no3qianxiao.Value = 0;
                }
                if ((temp.aream_data["MB2"] & 0x20) == 0x20) //四号钳销
                {
                    switchSlider_pg1_no4qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no4qianxiao.Value = 0;
                }
                if ((temp.aream_data["MB0"] & 0x08) == 0x08) //前门
                {
                    switchSlider_pg1_yureluqianmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_yureluqianmen.Value = 0;
                }
                if ((temp.aream_data["MB0"] & 0x10) == 0x10) //后门
                {
                    switchSlider_pg1_yureluhoumen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_yureluhoumen.Value = 0;
                }
                if ((temp.aream_data["MB0"] & 0x20) == 0x20) //前门
                {
                    switchSlider_pg1_kaoliaoluqianmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_kaoliaoluqianmen.Value = 0;
                }
                if ((temp.aream_data["MB0"] & 0x40) == 0x40) //后门
                {
                    switchSlider_pg1_kaoliaoluhoumen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_kaoliaoluhoumen.Value = 0;
                }
            }

            stopSendCmdtoPLC = false;
        }

        #endregion

        #region 按键定义

        //==================================================================
        //模块名： button_resetwarn_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================

        private void button_pg1_shuayoujidaowei_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 4);
            label11.ForeColor = Color.Red;
        }

        private void button_pg1_shuayoujidaowei_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 4);
            label11.ForeColor = Color.Blue;
        }

        private void button_pg1_yureludaowei_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 5);
        }

        private void button_pg1_yureludaowei_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 5);
        }

        private void button_pg1_shengliaojidaowei_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 6);
        }

        private void button_pg1_shengliaojidaowei_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 6);
        }

        private void button_pg1_yureluzidonghuiling_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 2);
        }

        private void button_pg1_yureluzidonghuiling_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 2);
        }

        private void button_pg1_yurelushedinglingdian_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 0);
        }

        private void button_pg1_yurelushedinglingdian_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 0);
        }

        private void button_pg1_yureludiandongqianjin_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 3);
        }

        private void button_pg1_yureludiandongqianjin_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 3);
        }

        private void button_pg1_yureludiandonghoutui_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 4);
        }

        private void button_pg1_yureludiandonghoutui_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 4);
        }

        private void button_pg1_tuomojidaowei_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 1);
        }

        private void button_pg1_tuomojidaowei_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 1);
        }

        private void button_pg1_kaoliaoludaowei_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 0);
        }

        private void button_pg1_kaoliaoludaowei_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 0);
        }

        private void button_pg1_kongweidaowei_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 7);
        }

        private void button_pg1_kongweidaowei_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 7);
        }

        private void button_pg1_kaoliaoluzidonghuiling_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 4);
        }

        private void button_pg1_kaoliaoluzidonghuiling_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 4);
        }

        private void button_pg1_kaoliaolushedinglingdian_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 2);
        }

        private void button_pg1_kaoliaolushedinglingdian_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 2);
        }

        private void button_pg1_kaoliaoludiandongqianjin_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 5);
        }

        private void button_pg1_kaoliaoludiandongqianjin_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 5);
        }

        private void button_pg1_kaoliaoludiandonghoutui_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 6);
        }

        private void button_pg1_kaoliaoludiandonghoutui_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 6);
        }

        private void button_pg2_tuomoxiaocheqianjin_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 7);
            label13.ForeColor = Color.Red;
        }

        private void button_pg2_tuomoxiaocheqianjin_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 7);
            label13.ForeColor = Color.Blue;
        }

        private void button_pg2_tuomoxiaochehoutui_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 0);
        }

        private void button_pg2_tuomoxiaochehoutui_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 0);
        }

        private void button_pg2_jinliaoxiaocheqianjin_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 1);
        }

        private void button_pg2_jinliaoxiaocheqianjin_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 1);
        }

        private void button_pg2_jinliaoxiaochehoutui_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 2);
        }

        private void button_pg2_jinliaoxiaochehoutui_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 2);
        }

        private void button_pg2_tuomojiqianjin_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 5);
        }

        private void button_pg2_tuomojiqianjin_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 5);
        }

        private void button_pg2_tuomojihoutui_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 6);
        }

        private void button_pg2_tuomojihoutui_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 6);
        }

        private void button_pg2_tuomoyici_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 5, 4);
        }

        private void button_pg2_tuomoyici_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 5, 4);
        }

        private void button_pg2_shuayoujiqianjin_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 0);
        }

        private void button_pg2_shuayoujiqianjin_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 0);
        }

        private void button_pg2_shuayoujihoutui_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 1);
        }

        private void button_pg2_shuayoujihoutui_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 1);
        }

        private void button_resetwarn_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 6, 2);
        }

        private void button_resetwarn_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 6, 2);
        }

        private void button_system_init_TouchDown(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 6, 4);
        }

        private void button_system_init_TouchUp(object sender, EventArgs e)
        {
            SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 6, 4);
        }

        #endregion

        #region 按键定义

        //==================================================================
        //模块名： button_pg3_shuayoushijianadd_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================

        private void button_pg3_shuayoushijianadd_Click(object sender, EventArgs e)
        {
            if (vw68 < Convert.ToInt32(shuayou_upper)*10)
            {
                vw68 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw68, 68);
                shuayou_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_shuayoushijianminus_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_shuayoushijianminus_Click(object sender, EventArgs e)
        {
            if (vw68 > -Convert.ToInt32(shuayou_lower)*10)
            {
                vw68 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw68, 2010);
                shuayou_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_kaoliaoshijianadd_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_kaoliaoshijianadd_Click(object sender, EventArgs e)
        {
            if (vw74 < Convert.ToInt32(kaoliao_consume_upper)*10)
            {
                vw74 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw74, 74);
                kaoliao_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_kaoliaoshijianminus_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_kaoliaoshijianminus_Click(object sender, EventArgs e)
        {
            if (vw74 > -Convert.ToInt32(kaoliao_consume_lower)*10)
            {
                vw74 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw74, 74);
                kaoliao_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_jinliaoshijianadd_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_jinliaoshijianadd_Click(object sender, EventArgs e)
        {
        }

        //==================================================================
        //模块名： button_pg3_jinliaoshijianminus_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_jinliaoshijianminus_Click(object sender, EventArgs e)
        {
        }

        //==================================================================
        //模块名： button_pg3_kaomushijianadd_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_kaomushijianadd_Click(object sender, EventArgs e)
        {
            if (vw72 < Convert.ToInt32(kaomo_consume_upper)*10)
            {
                vw72 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw72, 72);
                kaomo_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_kaomushijianminus_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_kaomushijianminus_Click(object sender, EventArgs e)
        {
            if (vw72 > -Convert.ToInt32(kaomo_consume_lower)*10)
            {
                vw72 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw72, 2014);
                kaomo_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_lengqueshijianadd_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：   
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_lengqueshijianadd_Click(object sender, EventArgs e)
        {
            if (vw70 < Convert.ToInt32(lengque_consume_upper)*10)
            {
                vw70 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw70, 70);
                lengque_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： button_pg3_lengqueshijianminus_Click
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void button_pg3_lengqueshijianminus_Click(object sender, EventArgs e)
        {
            if (vw70 > -Convert.ToInt32(lengque_consume_lower)*10)
            {
                vw70 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw70, 80);
                lengque_consume_fudong = true;
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_yureludianji_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_yureludianji_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_yureludianji.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 1);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 1);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_no1qianxiao_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_no1qianxiao_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_no1qianxiao.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 7);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 7);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_yureluqianmen_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_yureluqianmen_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_yureluqianmen.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 3);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 3);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_no2qianxiao_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_no2qianxiao_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_no2qianxiao.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 1, 3);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 1, 3);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_yureluhoumen_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_yureluhoumen_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_yureluhoumen.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 4);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 4);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_kaoliaoludianji_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_kaoliaoludianji_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_kaoliaoludianji.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 3);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 3);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_no3qianxiao_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_no3qianxiao_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_no3qianxiao.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 6);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 6);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_kaoliaoluqianmen_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_kaoliaoluqianmen_ValueChanged(object sender,
            ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_kaoliaoluqianmen.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 5);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 5);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_no4qianxiao_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_no4qianxiao_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_no4qianxiao.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 2, 5);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 2, 5);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg1_kaoliaoluhoumen_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg1_kaoliaoluhoumen_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg1_kaoliaoluhoumen.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 6);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 6);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg2_tuomoqigang_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg2_tuomoqigang_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchRotary_pg2_tuomoqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 5, 1);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 5, 1);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg2_choufengji_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg2_choufengji_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchRotary_pg2_choufengji.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 5, 6);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 5, 6);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg2_shuixiang_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg2_shuixiang_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg2_shuixiang.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 3);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 3);
            }
        }

        //==================================================================
        //模块名： switchSlider_pg2_jinliaoqigang_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchSlider_pg2_jinliaoqigang_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchSlider_pg2_jinliaoqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 5, 7);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 5, 7);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg3_shuayouji_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg3_shuayouji_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (switchRotary_pg3_shuayouji.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 3, 7);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 3, 7);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg3_shuixiang_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg3_shuixiang_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (switchRotary_pg3_shuixiang.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 2);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 2);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg3_jinliaoqigang_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg3_jinliaoqigang_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (switchRotary_pg3_jinliaoqigang.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 5, 2);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 5, 2);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg3_tuomoji_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg3_tuomoji_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchRotary_pg3_tuomoji.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 4, 4);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 4, 4);
            }
        }

        //==================================================================
        //模块名： switchRotary_pg3_luzidianyuan_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_pg3_luzidianyuan_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchRotary_pg3_luzidianyuan.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 6, 7);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 6, 7);
            }
        }

        //==================================================================
        //模块名： switchRotary_runmode_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_runmode_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchRotary_runmode.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 0);
                mode = "manual";
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 0);
                mode = "auto";
            }
        }

        //==================================================================
        //模块名： switchRotary_runstatus_ValueChanged
        //作者：    Yang Chuan
        //日期：    2015.12.01
        //功能：    
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void switchRotary_runstatus_ValueChanged(object sender, ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC)
            {
                return;
            }
            if (switchRotary_runstatus.Value.AsInteger == 0)
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 0, 0, 1);
            }
            else
            {
                SendCommandToPlc(TerminalCommon.S7200AreaM, TerminalCommon.S7200DataBit, 1, 0, 1);
            }
        }

        #endregion

        #region 辅助功能定义

        private int shuayou_module;
        private int kaoliao_module;
        private int jinliao_module;
        private int tuomu_module;
        private int lengque_module;

        private void module_process(PlcDAQCommunicationObject data)
        {
            if ((data.aream_data["I5"] & 0x40) == 0x40)
            {
                tuomu_module = 1;
                lengque_module = 2;
                kaoliao_module = 3;
            }
            else
            {
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
        private Thread cloudaquire;

        private void cloudpara_Click(object sender, EventArgs e)
        {
            if (cloudaquire == null)
                cloudaquire = new Thread(get_cloudpara);
            if (cloudaquire.IsAlive)
            {
                MessageBox.Show("下载已启动，结束之前请勿重复操作！");
                return;
            }
            cloudaquire = new Thread(get_cloudpara);
            cloudaquire.IsBackground = true;
            cloudaquire.Priority = ThreadPriority.Normal;
            cloudaquire.Start();
        }

        private void get_cloudpara()
        {
            using (var mysql = FluentNhibernateHelper.GetSession())
            {
                try
                {
                    var para = mysql.QueryOver<proceduretechnologybase>()
                        .Where(x => x.device_name == TerminalParameters.Default.terminal_name)
                        .List();
                    try
                    {
                        var strSql2 = "delete from proceduretechnologybase";
                        TerminalLocalDataStorage.ExecuteSql(strSql2);
                        foreach (var d in para)
                        {
                            var ret = 1;
                            var strSql = new StringBuilder();
                            strSql.Append("insert into proceduretechnologybase(");
                            strSql.Append(
                                "product_id,material,shuayou_base,shuayou_upper,shuayou_lower,yurelu_temp_base,yurelu_temp_upper,yurelu_temp_lower,kaomo_consume_base,kaomo_consume_upper,kaomo_consume_lower,");
                            strSql.Append(
                                "kaoliaolu_temp_base,kaoliaolu_temp_upper,kaoliaolu_temp_lower,kaoliao_consume_base,kaoliao_consume_upper,kaoliao_consume_lower,");
                            strSql.Append(
                                "qigangjinliao_consume_base,qigangjinliao_consume_upper,qigangjinliao_consume_lower,lengque_consume_base,lengque_consume_upper,lengque_consume_lower,");
                            strSql.Append(
                                "jinliao_consume_base,jinliao_consume_upper,jinliao_consume_lower,shangsheng_speed_base,shangsheng_speed_upper,shangsheng_speed_lower,");
                            strSql.Append(
                                "xiajiang_speed_base,xiajiang_speed_upper,xiajiang_speed_lower,hutao_length_base,hutao_length_upper,hutao_length_lower,yemian_distance_base,yemian_distance_upper,yemian_distance_lower)");
                            strSql.Append(" values(");
                            strSql.Append(
                                "@product_id,@material,@shuayou_base,@shuayou_upper,@shuayou_lower,@yurelu_temp_base,@yurelu_temp_upper,@yurelu_temp_lower,@kaomo_consume_base,@kaomo_consume_upper,@kaomo_consume_lower,");
                            strSql.Append(
                                "@kaoliaolu_temp_base,@kaoliaolu_temp_upper,@kaoliaolu_temp_lower,@kaoliao_consume_base,@kaoliao_consume_upper,@kaoliao_consume_lower,");
                            strSql.Append(
                                "@qigangjinliao_consume_base,@qigangjinliao_consume_upper,@qigangjinliao_consume_lower,@lengque_consume_base,@lengque_consume_upper,@lengque_consume_lower,");
                            strSql.Append(
                                "@jinliao_consume_base,@jinliao_consume_upper,@jinliao_consume_lower,@shangsheng_speed_base,@shangsheng_speed_upper,@shangsheng_speed_lower,");
                            strSql.Append(
                                "@xiajiang_speed_base,@xiajiang_speed_upper,@xiajiang_speed_lower,@hutao_length_base,@hutao_length_upper,@hutao_length_lower,@yemian_distance_base,@yemian_distance_upper,@yemian_distance_lower)");


                            SQLiteParameter[] parameters =
                            {
                                new SQLiteParameter("@product_id", d.product_id),
                                new SQLiteParameter("@material", d.material),
                                new SQLiteParameter("@shuayou_base", d.shuayou_base),
                                new SQLiteParameter("@shuayou_upper", d.shuayou_upper),
                                new SQLiteParameter("@shuayou_lower", d.shuayou_lower),
                                new SQLiteParameter("@yurelu_temp_base", d.yurelu_temp_base),
                                new SQLiteParameter("@yurelu_temp_upper", d.yurelu_temp_upper),
                                new SQLiteParameter("@yurelu_temp_lower", d.yurelu_temp_lower),
                                new SQLiteParameter("@kaomo_consume_base", d.kaomo_consume_base),
                                new SQLiteParameter("@kaomo_consume_upper", d.kaomo_consume_upper),
                                new SQLiteParameter("@kaomo_consume_lower", d.kaomo_consume_lower),
                                new SQLiteParameter("@kaoliaolu_temp_base", d.kaoliaolu_temp_base),
                                new SQLiteParameter("@kaoliaolu_temp_upper", d.kaoliaolu_temp_upper),
                                new SQLiteParameter("@kaoliaolu_temp_lower", d.kaoliaolu_temp_lower),
                                new SQLiteParameter("@kaoliao_consume_base", d.kaoliao_consume_base),
                                new SQLiteParameter("@kaoliao_consume_upper", d.kaoliao_consume_upper),
                                new SQLiteParameter("@kaoliao_consume_lower", d.kaoliao_consume_lower),
                                new SQLiteParameter("@qigangjinliao_consume_base", d.qigangjinliao_consume_base),
                                new SQLiteParameter("@qigangjinliao_consume_upper", d.qigangjinliao_consume_upper),
                                new SQLiteParameter("@qigangjinliao_consume_lower", d.qigangjinliao_consume_lower),
                                new SQLiteParameter("@lengque_consume_base", d.lengque_consume_base),
                                new SQLiteParameter("@lengque_consume_upper", d.lengque_consume_upper),
                                new SQLiteParameter("@lengque_consume_lower", d.lengque_consume_lower),
                                new SQLiteParameter("@jinliao_consume_base", d.jinliao_consume_base),
                                new SQLiteParameter("@jinliao_consume_upper", d.jinliao_consume_upper),
                                new SQLiteParameter("@jinliao_consume_lower", d.jinliao_consume_lower),
                                new SQLiteParameter("@shangsheng_speed_base", d.shangsheng_speed_base),
                                new SQLiteParameter("@shangsheng_speed_upper", d.shangsheng_speed_upper),
                                new SQLiteParameter("@shangsheng_speed_lower", d.shangsheng_speed_lower),
                                new SQLiteParameter("@xiajiang_speed_base", d.xiajiang_speed_base),
                                new SQLiteParameter("@xiajiang_speed_upper", d.xiajiang_speed_upper),
                                new SQLiteParameter("@xiajiang_speed_lower", d.xiajiang_speed_lower),
                                new SQLiteParameter("@hutao_length_base", d.hutao_length_base),
                                new SQLiteParameter("@hutao_length_upper", d.hutao_length_upper),
                                new SQLiteParameter("@hutao_length_lower", d.hutao_length_lower),
                                new SQLiteParameter("@yemian_distance_base", d.yemian_distance_base),
                                new SQLiteParameter("@yemian_distance_upper", d.yemian_distance_upper),
                                new SQLiteParameter("@yemian_distance_lower", d.yemian_distance_lower)
                            };
                            if (TerminalLocalDataStorage.ExecuteSql(strSql.ToString(), parameters) >= 1)
                            {
                                ret = 1;
                            }
                        }
                        DelegateMessagebox("从中央数据库下载成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        DelegateMessagebox("数据库连接出错!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        log.Error("下载功能出错", ex);
                    }

                    try
                    {
                        SelectUpdate();
                    }
                    catch
                    {
                        DelegateMessagebox("本地列表刷新失败", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("下载工艺参数时无法查询", ex);
                }
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
                if (typeexist)
                {
                    product_id = Convert.ToString(typeselect.SelectedItem);
                    var strSql = string.Format("select * from proceduretechnologybase where product_id='{0}' ",
                        product_id);
                    var ds = new DataSet();
                    var dTable = new DataTable();
                    ds = TerminalLocalDataStorage.Query(strSql);
                    dTable = ds.Tables[0];
                    if (dTable.Rows.Count != 0)
                    {
                        material = Convert.ToString(dTable.Rows[0]["material"]);
                        shuayou_base = dTable.Rows[0]["shuayou_base"];
                        shuayou_upper = dTable.Rows[0]["shuayou_upper"];
                        shuayou_lower = dTable.Rows[0]["shuayou_lower"];
                        yurelu_temp_base = dTable.Rows[0]["yurelu_temp_base"];
                        yurelu_temp_upper = dTable.Rows[0]["yurelu_temp_upper"];
                        yurelu_temp_lower = dTable.Rows[0]["yurelu_temp_lower"];
                        kaomo_consume_base = dTable.Rows[0]["kaomo_consume_base"];
                        kaomo_consume_upper = dTable.Rows[0]["kaomo_consume_upper"];
                        kaomo_consume_lower = dTable.Rows[0]["kaomo_consume_lower"];
                        kaoliaolu_temp_base = dTable.Rows[0]["kaoliaolu_temp_base"];
                        kaoliaolu_temp_upper = dTable.Rows[0]["kaoliaolu_temp_upper"];
                        kaoliaolu_temp_lower = dTable.Rows[0]["kaoliaolu_temp_lower"];
                        kaoliao_consume_base = dTable.Rows[0]["kaoliao_consume_base"];
                        kaoliao_consume_upper = dTable.Rows[0]["kaoliao_consume_upper"];
                        kaoliao_consume_lower = dTable.Rows[0]["kaoliao_consume_lower"];
                        qigangjinliao_consume_base = dTable.Rows[0]["qigangjinliao_consume_base"];
                        qigangjinliao_consume_upper = dTable.Rows[0]["qigangjinliao_consume_upper"];
                        qigangjinliao_consume_lower = dTable.Rows[0]["qigangjinliao_consume_lower"];
                        lengque_consume_base = dTable.Rows[0]["lengque_consume_base"];
                        lengque_consume_upper = dTable.Rows[0]["lengque_consume_upper"];
                        lengque_consume_lower = dTable.Rows[0]["lengque_consume_lower"];
                        jinliao_consume_base = dTable.Rows[0]["jinliao_consume_base"];
                        jinliao_consume_upper = dTable.Rows[0]["jinliao_consume_upper"];
                        jinliao_consume_lower = dTable.Rows[0]["jinliao_consume_lower"];
                        shangsheng_speed = dTable.Rows[0]["shangsheng_speed_base"];
                        xiajiang_speed = dTable.Rows[0]["xiajiang_speed_base"];
                        hutao_total_length = dTable.Rows[0]["hutao_length_base"];
                        yemian_height = dTable.Rows[0]["yemian_distance_base"];

                        kaomuluwen_text.Text = Convert.ToString(yurelu_temp_base);
                        kaoliaoluwen_text.Text = Convert.ToString(kaoliaolu_temp_base);
                        productlabel.Text = "预备:" + product_id;
                    }
                    else
                    {
                        MessageBox.Show("数据不存在", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        typeexist = false;
                        typeselect.Text = null;
                        product_id = null;
                        kaoliaoluwen_text.Text = "";
                        kaomuluwen_text.Text = "";
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
        public bool restartbutton;

        private void restart_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要重启吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                AbortAllThread();
                restartbutton = true;
                //tcpobject.socket_stop_connect();
                // Application.ExitThread();
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
        private Reasonform reason_frm;

        private void paraupload_Click(object sender, EventArgs e)
        {
            try
            {
                reason_frm = new Reasonform();
                //MessageBox.Show("上传设置参数？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK
                if (reason_frm.ShowDialog() == DialogResult.OK)
                {
                    using (var mysql = FluentNhibernateHelper.GetSession())
                    {
                        var d = new proceduretechnologybase_work();
                        d.device_name = TerminalParameters.Default.terminal_name;
                        d.product_id = Convert.ToString(product_id);
                        d.material = Convert.ToString(material);
                        d.reason = Reasonform.upload_reason;
                        if (Convert.IsDBNull(kaomo_consume_base) == false)
                        {
                            d.kaomo_consume_base = vw72/10.0f + (float) Convert.ToDouble(kaomo_consume_base);
                        }
                        if (Convert.IsDBNull(lengque_consume_base) == false)
                        {
                            d.lengque_consume_base = vw70/10.0f + (float) Convert.ToDouble(lengque_consume_base);
                        }
                        if (Convert.IsDBNull(kaoliao_consume_base) == false)
                        {
                            d.kaoliao_consume_base = vw74/10.0f + (float) Convert.ToDouble(kaoliao_consume_base);
                        }
                        if (Convert.IsDBNull(jinliao_consume_base) == false)
                        {
                            d.jinliao_consume_base = (float) Convert.ToDouble(jinliao_consume_base);
                        }
                        if (Convert.IsDBNull(shuayou_base) == false)
                        {
                            d.shuayou_base = vw68/10.0f + (float) Convert.ToDouble(shuayou_base);
                        }
                        d.storetime = DateTime.Now;
                        mysql.Save(d);
                        mysql.Flush();
                        MessageBox.Show("上传成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        log.Info("上传工艺参数成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("上传失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                log.Error("上传工艺参数失败", ex);
            }
        }

        //==================================================================
        //模块名： formupload_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    打开日常检查表界面
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public bool dailycheck_open = false;
        private Dailycheck daily_frm;

        private void formupload_Click(object sender, EventArgs e)
        {
            if (dailycheck_open)
            {
                daily_frm.TopMost = true;
                return;
            }
            daily_frm = new Dailycheck(this);
            daily_frm.Show();
        }

        //==================================================================
        //模块名： typeselect_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    点击弹开选项
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        private void typeselect_Click(object sender, EventArgs e)
        {
            typeselect.DroppedDown = true;
        }

        //打开条码点检表格
        public bool bandcheck_open = false;
        private Bandcheck band_frm;

        private void formupload2_Click(object sender, EventArgs e)
        {
            if (bandcheck_open)
            {
                band_frm.TopMost = true;
                return;
            }

            band_frm = new Bandcheck(this);
            band_frm.Show();
        }

        //打开现场点检表
        public bool fieldcheck_open = false;
        private Fieldcheck field_frm;

        private void formupload3_Click(object sender, EventArgs e)
        {
            if (fieldcheck_open)
            {
                field_frm.TopMost = true;
                return;
            }

            field_frm = new Fieldcheck(this);
            field_frm.Show();
        }

        private void buzuoguan_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (buzuoguan.Text != "")
                {
                    SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord,
                        (int) (Convert.ToDouble(buzuoguan.Text)*10), 2070);
                    if (numberBoard != null && numberBoard.IsDisposed == false)
                        numberBoard.Close();
                    yincang.Focus();
                }

                else
                {
                    MessageBox.Show("请输入有效值后再提交", "警告", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
                e.Handled = isgoodnumber(buzuoguan, e);
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
                int kc = e.KeyChar;
                if ((kc < 48 || kc > 57) && kc != 8 && kc != 46)
                {
                    return true;
                }
                if (kc == 46)
                {
                    if (box.Text.Length <= 0)
                    {
                        return true;
                    }
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse(box.Text, out oldf);
                    b2 = float.TryParse(box.Text + e.KeyChar, out f);
                    if (b2 == false)
                    {
                        if (b1)
                            return true;
                        return false;
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
        private void jinliaoshezhi_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (jinliaoshezhi.Text != "")
                {
                    SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord,
                        (int) (Convert.ToDouble(jinliaoshezhi.Text)*10), 2008);
                    if (numberBoard != null && numberBoard.IsDisposed == false)
                        numberBoard.Close();
                    yincang.Focus();
                }
                else
                {
                    MessageBox.Show("请输入有效值后再提交", "警告", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
                e.Handled = isgoodnumber(jinliaoshezhi, e);
        }

        #region 键盘窗口

        private void tabPage_pg1_Click(object sender, EventArgs e)
        {
        }

        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("User32.dll ", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName); //关键方法  

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessage(IntPtr HWnd, uint Msg, int WParam, int LParam);


        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_RESTORE = 0xF120;
        public const int SC_CLOSE = 0xF060;

        public void HideInputPanel()
        {
            var TouchhWnd = new IntPtr(0);
            TouchhWnd = FindWindow("IPTip_Main_Window", null);
            if (TouchhWnd == IntPtr.Zero)
                return;
            PostMessage(TouchhWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
        }

        private void MiniMizeAppication(string processName)
        {
            var processs = Process.GetProcessesByName(processName);
            if (processs != null)
            {
                foreach (var p in processs)
                {
                    var handle = FindWindow(null, p.MainWindowTitle);
                    //IntPtr handle = FindWindow("YodaoMainWndClass",null);  
                    PostMessage(handle, WM_SYSCOMMAND, SC_MINIMIZE, 0);
                }
            }
        }

        private void MaxMizeAppication(string processName)
        {
            var processs = Process.GetProcessesByName(processName);
            if (processs != null)
            {
                foreach (var p in processs)
                {
                    var handle = FindWindow(null, p.MainWindowTitle);
                    //IntPtr handle = FindWindow("YodaoMainWndClass",null);  
                    PostMessage(handle, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
                }
            }
        }

        private void RestoreAppication(string processName)
        {
            var processs = Process.GetProcessesByName(processName);
            if (processs != null)
            {
                foreach (var p in processs)
                {
                    var handle = FindWindow(null, p.MainWindowTitle);
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
                if (!File.Exists(file))
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
            var thisproc = Process.GetProcessesByName("TabTip");
            if (thisproc.Length == 0)
            {
                try
                {
                    Process.Start("tabtip.exe");
                }
                catch
                {
                    MessageBox.Show("打开键盘失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        #endregion

        //点击不做管弹出键盘
        private void buzuoguan_Click(object sender, EventArgs e)
        {
            buzuoguan.Text = "";
            if (numberBoard == null)
            {
                numberBoard = new numberboard(this);
                numberBoard.Show();
            }
            else if (numberBoard.IsDisposed)
            {
                numberBoard = new numberboard(this);
                numberBoard.Show();
            }


            buzuoguan.Focus();

            //ShowInputPanel();
        }

        //单击自动弹出键盘输入界面
        private void jinliaoshezhi_Click(object sender, EventArgs e)
        {
            jinliaoshezhi.Text = "";
            if (numberBoard == null)
            {
                numberBoard = new numberboard(this);
                numberBoard.Show();
            }
            else if (numberBoard.IsDisposed) //打开过一次后不等于null
            {
                numberBoard = new numberboard(this);
                numberBoard.Show();
            }
            jinliaoshezhi.Focus();
        }

        //重启系统
        private void restartsystem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要重启吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    //if (tcpuplink_dataprocess_thread.IsAlive)
                    //{
                    //    tcpuplink_dataprocess_thread.Abort();
                    //}

                    //if (tcpdownlink_dataprocess_thread.IsAlive)
                    //{
                    //    tcpdownlink_dataprocess_thread.Abort();
                    //}

                    if (plccommunication_thread.IsAlive)
                    {
                        plccommunication_thread.Abort();
                    }


                    if (plcdatahandler_thread.IsAlive)
                    {
                        plcdatahandler_thread.Abort();
                    }

                    if (datacenter_storage_thread.IsAlive)
                    {
                        datacenter_storage_thread.Abort();
                    }

                    if (local_storage_thread.IsAlive)
                    {
                        local_storage_thread.Abort();
                    }
                }
                catch
                {
                }
                restartbutton = true;
                //tcpobject.socket_stop_connect();
                var startinfo = new ProcessStartInfo("shutdown.exe",
                    "-r -t 00");
                Process.Start(startinfo);
                //System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        //选择产品工艺参数，并输入plc
        private void type_accept_Click(object sender, EventArgs e)
        {
            try
            {
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord,
                    Convert.ToInt16(Convert.ToDouble(shuayou_base)*10), 60);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord,
                    Convert.ToInt16(Convert.ToDouble(kaomo_consume_base)*10), 64);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord,
                    Convert.ToInt16(Convert.ToDouble(kaoliao_consume_base)*10), 66);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord,
                    Convert.ToInt16(Convert.ToDouble(lengque_consume_base)*10), 62);
                productlabel.Text = Convert.ToString(typeselect.SelectedItem);
                vw68 = 0;
                vw70 = 0;
                vw72 = 0;
                vw74 = 0;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw68, 68);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw70, 70);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw72, 72);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, vw74, 74);
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

        private stoptable stoptable_frm;
        public bool stoptable_open = false;
        private bool poweroff;

        private void button_halttable_Click(object sender, EventArgs e)
        {
            try
            {
                if (button_halttable.BackColor == SystemColors.Control)
                {
                    if (TextCommand.Exists("haltinfo.txt"))
                    {
                        TextCommand.DeleteFile("haltinfo.txt");
                    }
                    if (stoptable_open)
                    {
                        stoptable_frm.TopMost = true;
                        return;
                    }
                    stoptable_frm = new stoptable(this);
                    if (stoptable_frm.ShowDialog() != DialogResult.OK)
                        return;
                    sevenSegmentClock_start.Visible = true;
                    sevenSegmentClock_end.Visible = true;
                    ledArrow1.Visible = true;
                    button_halttable.BackColor = Color.Salmon;
                    button_halttable.Text = "停机结束";
                    sevenSegmentClock_start.Value = DateTime.Now;
                    TextCommand.CreateFile("haltinfo.txt");
                    DateTime timestart = sevenSegmentClock_start.Value;
                    TextCommand.WriteLine("haltinfo.txt", timestart.ToString());
                    sevenSegmentClock_end.Value = sevenSegmentClock_start.Value;
                    poweroff = true;
                    sevenSegmentClock_end.AutoUpdate = true;
                }
                else if (button_halttable.BackColor == Color.Salmon)
                {
                    button_halttable.Text = "停机结束";
                    var halt_uploader = new StoptableSqlUpload(this);
                    if (TextCommand.Exists("haltinfo.txt") && (poweroff == false))
                    {
                        var aFile = new FileStream("haltinfo.txt", FileMode.Open);
                        var sr = new StreamReader(aFile);
                        halt_uploader.reason = sr.ReadLine();
                        halt_uploader.start = DateTime.Parse(sr.ReadLine());
                        halt_uploader.end = DateTime.Now;
                        sr.Close();
                    }
                    else
                    {
                        halt_uploader.reason = stoptable.haltreason;
                        halt_uploader.start = sevenSegmentClock_start.Value;
                        halt_uploader.end = sevenSegmentClock_end.Value;
                        sevenSegmentClock_end.AutoUpdate = false;
                    }
                    button_halttable.Enabled = false;
                    var t = new Thread(halt_uploader.HaltReasonUpload);
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error("打开停机表出现错误");
            }
        }

        private void typeselect_Click_1(object sender, EventArgs e)
        {
            typeselect.DroppedDown = true;
        }

        private void FangpuTerminal_Load(object sender, EventArgs e)
        {
            Init();
            //检测是否有停机操作
            if (TextCommand.Exists("haltinfo.txt") == false)
                return;
            button_halttable.BackColor = Color.Salmon;
            button_halttable.PerformClick();
        }

        #region [U盘检测]

        public const int WM_DEVICECHANGE = 0x219;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;
        public const int DBT_CONFIGCHANGED = 0x0018;
        public const int DBT_CUSTOMEVENT = 0x8006;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;
        public const int DBT_DEVNODES_CHANGED = 0x0007;
        public const int DBT_QUERYCHANGECONFIG = 0x0017;
        public const int DBT_USERDEFINED = 0xFFFF;
        public string UsbName;
        public bool isInsert;

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_DEVICECHANGE)
                {
                    switch (m.WParam.ToInt32())
                    {
                        case WM_DEVICECHANGE:
                            break;
                        case DBT_DEVICEARRIVAL: //U盘插入
                            var s = DriveInfo.GetDrives();
                            foreach (var drive in s)
                            {
                                if (drive.DriveType == DriveType.Removable)
                                {
                                    log.Info(DateTime.Now + "--> U盘已插入，盘符为:" + drive.Name);
                                    UsbName = drive.Name;
                                    if (outputform != null && !outputform.IsDisposed)
                                    {
                                        outputform.label_info.Text = "U盘已插入，盘符为:" + UsbName;
                                        outputform.button_accept.Enabled = true;
                                    }
                                    isInsert = true;
                                    break;
                                }
                            }
                            break;
                        case DBT_CONFIGCHANGECANCELED:
                            break;
                        case DBT_CONFIGCHANGED:
                            break;
                        case DBT_CUSTOMEVENT:
                            break;
                        case DBT_DEVICEQUERYREMOVE:
                            break;
                        case DBT_DEVICEQUERYREMOVEFAILED:
                            break;
                        case DBT_DEVICEREMOVECOMPLETE: //U盘卸载
                            log.Info(DateTime.Now + "--> U盘已卸载！");
                            if (outputform != null && !outputform.IsDisposed)
                            {
                                outputform.label_info.Text = "U盘已卸载！";
                                outputform.button_accept.Enabled = false;
                            }
                            isInsert = false;
                            break;
                        case DBT_DEVICEREMOVEPENDING:
                            break;
                        case DBT_DEVICETYPESPECIFIC:
                            break;
                        case DBT_DEVNODES_CHANGED:
                            break;
                        case DBT_QUERYCHANGECONFIG:
                            break;
                        case DBT_USERDEFINED:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            base.WndProc(ref m);
        }

        #endregion

        private OutputForm outputform;

        private void button_localdataoutput_Click(object sender, EventArgs e)
        {
            if (outputform == null || outputform.IsDisposed)
            {
                outputform = new OutputForm(this);
                outputform.Show();
            }
            else
            {
                return;
            }
        }

        public void AbortAllThread()
        {
            try
            {
                //if (tcpuplink_dataprocess_thread.IsAlive)
                //{
                //    tcpuplink_dataprocess_thread.Abort();
                //}

                //if (tcpdownlink_dataprocess_thread.IsAlive)
                //{
                //    tcpdownlink_dataprocess_thread.Abort();
                //}
                S7SNAP.Disconnect();

                if (plccommunication_thread.IsAlive)
                {
                    plccommunication_thread.Abort();
                }


                if (plcdatahandler_thread.IsAlive)
                {
                    plcdatahandler_thread.Abort();
                }

                if (datacenter_storage_thread.IsAlive)
                {
                    datacenter_storage_thread.Abort();
                }

                if (local_storage_thread.IsAlive)
                {
                    local_storage_thread.Abort();
                }
            }
            catch (Exception ex)
            {
                log.Error("Abort Thread failure", ex);
            }
        }

        public void topbarupdata(string str)
        {
            displayWarninfo.Value = str;
        }

        private void tuomucount_clear_Click(object sender, EventArgs e)
        {
            tuomucount.Text = "0";
            youguanno = 0;
        }


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