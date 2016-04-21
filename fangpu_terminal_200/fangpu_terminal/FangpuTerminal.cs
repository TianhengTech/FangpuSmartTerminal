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
using System.IO;
using Snap7;
using CustomGUI.Forms;
//using DevExpress.XtraSplashScreen;


namespace fangpu_terminal
{     

    public partial class FangpuTerminal : Form
    {
        #region 成员定义
        
        public static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        TerminalTcpClientAsync tcpobject;
        S7_Socket S7S;
        S7_PPI PPI;
        SynchronizationContext thread_updateui_syncContext = null;
        
        public delegate void UpdateText(PlcDAQCommunicationObject daq_input);
        public delegate void HaltUI();
        Thread tcpuplink_dataprocess_thread;  //tcp数据上行线程队列
        Thread tcpdownlink_dataprocess_thread;  //tcp数据下行处理
        Thread plccommunication_thread;         //plc数据通信处理
        Thread plcdatahandler_thread;         //plc数据处理
        Thread datacenter_storage_thread;     //数据库存储对象
        Thread local_storage_thread;           //本地数据缓存
        Thread plcread_thread;                  //PLC读线程
        Thread proceduretimeread_thread;    //读工艺时间线程
        System.Threading.Timer timer_read_interval_60s;       //60s读一次
        System.Threading.Timer timer_check_interval_60m;
        System.Threading.Timer timer_tcp_heart_connection;    //tcp心跳连接
        string mode = "manual";
        string warntext = "报警信息";


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
        public FangpuTerminal()
        {
            
            InitializeComponent();
            Init();
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
            //SplashScreenManager.ShowForm(typeof(TianhengLogin));
          InitGlobalParameter();
          //UpdateLoadGUIConfig("正在尝试连接...", 30);
          //S7S = new S7_Socket();
          //S7S.Connect();
          ////S7Client S7SNAP = new S7Client();
          ////S7SNAP.SetConnectionParams("192.168.1.51",0x1000,0x1001);
          ////int result =S7SNAP.Connect();
          ////Trace.WriteLine(result);



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

          //// UpdateLoadGUIConfig("载入中", 60);
          //plcread_thread = new Thread(PlcReadCycle);
          //plcread_thread.IsBackground = true;
          //plcread_thread.Priority = ThreadPriority.BelowNormal;
          //plcread_thread.Start();

          //plccommunication_thread = new Thread(PlcCommunicationThread);
          //plccommunication_thread.IsBackground = true;
          //plccommunication_thread.Priority = ThreadPriority.BelowNormal;
          ////plccommunication_thread.Priority = ThreadPriority.Highest;
          //plccommunication_thread.Start();

          //plcdatahandler_thread = new Thread(PlcDataProcessThread);
          //plcdatahandler_thread.IsBackground = true;
          //plcdatahandler_thread.Priority = ThreadPriority.BelowNormal;
          //plcdatahandler_thread.Start();

          ////UpdateLoadGUIConfig("载入中", 80);
          //datacenter_storage_thread = new Thread(DataCenterStorageThread);
          //datacenter_storage_thread.IsBackground = true;
          //datacenter_storage_thread.Priority = ThreadPriority.BelowNormal;
          //datacenter_storage_thread.Start();

          ////UpdateLoadGUIConfig("载入中", 90);
          //local_storage_thread = new Thread(PlcDataLocalStorage);
          //local_storage_thread.IsBackground = true;
          //local_storage_thread.Priority = ThreadPriority.BelowNormal;
          //local_storage_thread.Start();



          //timer_read_interval_60s = new System.Threading.Timer(new TimerCallback(Timer_60s_handler), null, 0, 60000);
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
            try
            {
                SelectUpdate();
            }
            catch { }
            WarnInfoRead();//读取错误记录
            fangpu_config.ReadAddrIniFile("./fangpu_config.ini");//读取地址信息
            fangpu_config.ReadInfoIniFile("./fangpu_warn.ini");//读取报警信息
            foreach(var item in fangpu_config.warnmsg)
            {
                TerminalCommon.warn_info[item.Key] = item.Value;
            }          ;

               
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
            if (enableSync == true)
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

            if(tabControl_terminal.SelectedTab==tabPage_pg3)
            {
                displayDouble_pg3_shuayoushijiandisplay.Value = daq_input.aream_data["T38"] / 10.0;
                displayDouble_pg3_kaomushijiandisplay.Value = daq_input.aream_data["T39"] / 10.0;
                displayDouble_pg3_kaoliaoshijiandisplay.Value = daq_input.aream_data["T40"] / 10.0;
                displayDouble_pg3_lengqueshijiandisplay.Value = daq_input.aream_data["T37"] / 10.0;
                displayDouble_pg3_jinliaoshijiandisplay.Value = daq_input.aream_data["T41"] / 10.0;

                displayDouble_pg3_shuayoushijianset.Value = daq_input.aream_data["VW2062"] / 10.0f;
                displayDouble_pg3_kaomushijianset.Value = daq_input.aream_data["VW2066"] / 10.0f;
                displayDouble_pg3_kaoliaoshijianset.Value = daq_input.aream_data["VW2068"] / 10.0f;
                displayDouble_pg3_lengqueshijianset.Value = daq_input.aream_data["VW2072"] / 10.0f;
                displayDouble_pg3_jinliaoshijianset.Value = daq_input.aream_data["VW2008"] / 10.0f;
                displayDouble_pg3_buzuoguanshijianset.Value = (daq_input.aream_data["VW2070"] / 10.0f);
                displayDouble_pg3_lengqueshijian2.Value = (daq_input.aream_data["VW2064"] / 10.0f);

                if (!buzuoguan.Focused)
                {
                    buzuoguan.Text = (daq_input.aream_data["VW2070"] / 10.0f).ToString();
                }

                if (!jinliaoshezhi.Focused)
                {
                    jinliaoshezhi.Text = (daq_input.aream_data["VW2008"] / 10.0f).ToString();
                }
            }

            //if (tabControl_terminal.SelectedIndex == 3)
            
            if (enableWarn == true && (daq_input.aream_data["M5"] & 0x08) == 0x08)
            {
                List<string> results = WarnInfoProcess(daq_input.aream_data);
                if (results.Count != 0)
                {
                    foreach (string Warn in results)
                    {
                        PLCWarningObject plcwarn = new PLCWarningObject();
                        if (dataGridView_warn.Rows.Count >= 500)
                        {
                            dataGridView_warn.Rows.RemoveAt(499);
                        }
                        int index = dataGridView_warn.Rows.Add();
                        dataGridView_warn.Rows[index].Cells[0].Value = Warn;
                        dataGridView_warn.Rows[index].Cells[1].Value = daq_input.daq_time;
                        warntext = Warn;
                        plcwarn.warndata = Warn;
                        plcwarn.warn_time= daq_input.daq_time;
                        TerminalQueues.warninfoqueue.Enqueue(plcwarn);
                        TerminalQueues.warninfoqueue_local.Enqueue(plcwarn);
                        dataGridView_warn.Sort(dataGridView_warn.Columns[1], ListSortDirection.Descending);
                    }                   
                    displayWarninfo.Value = warntext;
                }
            };


            if (tabControl_terminal.SelectedTab == tabPage_pg5)
            {
                if (daq_input.aream_data.ContainsKey("C2") == true)
                {
                    displayInteger_currentshift_kaijishijian_hour.Value = daq_input.aream_data["C2"];
                    displayInteger_currentshift_kaijishijian_minute.Value = daq_input.aream_data["C1"];
                    displayInteger_currentshift_zuoguanshijian_hour.Value = daq_input.aream_data["C4"];
                    displayInteger_currentshift_zuoguanshijian_minute.Value = daq_input.aream_data["C3"];
                    displayInteger_currentshift_kailushijian_hour.Value = daq_input.aream_data["C6"];
                    displayInteger_currentshift_kailushijian_minute.Value = daq_input.aream_data["C5"];
                    
                }
                displayDouble_chanliangtongji_danmomotou_count.Value=daq_input.aream_data["VW2040"];
                displayDouble_chanliangtongji_jihua_count.Value = daq_input.aream_data["VW2052"];
                displayDouble_chanliangtongji_shiji_count.Value = daq_input.aream_data["VW2048"];

                //2024=1对应写2020,2024=0对应写2022
                if ((daq_input.aream_data["VB2024"] & 0x01) == 0x01)
                {
                    zuomoshijian.Value = daq_input.aream_data["VW2022"] / 10.0f;
                    zuomotime = daq_input.aream_data["VW2022"] / 10.0f;
                    if (onetime == false)
                    {
                        cyclenum = (cyclenum + 1) % 9;
                        onetime = true;
                    }
                }
                else
                {
                    zuomoshijian.Value = daq_input.aream_data["VW2020"] / 10.0f;
                    zuomotime = daq_input.aream_data["VW2020"] / 10.0f;
                    if (onetime == true)
                    {
                        cyclenum = (cyclenum + 1) % 9;
                        onetime = false;
                    }
                }
                switch (cyclenum)
                {
                    case 0:
                        displayDouble_onceprocedure_time_no1.Value = zuomotime;
                        break;
                    case 1:
                        displayDouble_onceprocedure_time_no2.Value = zuomotime;
                        break;
                    case 2:
                        displayDouble_onceprocedure_time_no3.Value = zuomotime;
                        break;
                    case 3:
                        displayDouble_onceprocedure_time_no4.Value = zuomotime;
                        break;
                    case 4:
                        displayDouble_onceprocedure_time_no5.Value = zuomotime;
                        break;
                    case 5:
                        displayDouble_onceprocedure_time_no6.Value = zuomotime;
                        break;
                    case 6:
                        displayDouble_onceprocedure_time_no7.Value = zuomotime;
                        break;
                    case 7:
                        displayDouble_onceprocedure_time_no8.Value = zuomotime;
                        break;
                    case 8:
                        displayDouble_onceprocedure_time_no9.Value = zuomotime;
                        break;
                }
            }                       
            if ((daq_input.aream_data["M5"] & 0x08) == 0x08)
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
            if ((daq_input.aream_data["M0"] & 0x01) == 0x01 && (led_manul.BlinkerEnabled=true))
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
            if ((daq_input.aream_data["M0"] & 0x02) == 0x02 && (led_pause.BlinkerEnabled = true))
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
            string strSql = String.Format("select warninfo,warntime from warninfo order by warninfoid desc limit 500");
            DataSet ds = new DataSet();
            DataTable dTable = new DataTable();
            ds = TerminalLocalDataStorage.Query(strSql);
            dTable = ds.Tables[0];
            for (int i = 0; i < dTable.Rows.Count; i++)
            {
                int index = dataGridView_warn.Rows.Add();
                dataGridView_warn.Rows[index].Cells[0].Value = dTable.Rows[i]["warninfo"];
                dataGridView_warn.Rows[index].Cells[1].Value = dTable.Rows[i]["warntime"];
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
                cmd.CommandText = "select d.storetime,n.storetime from historydata d join historydata n on(n.historydataid=d.historydataid+1) where timediff(n.storetime, d.storetime) >5 AND d.storetime BETWEEN DATE_SUB(now(), INTERVAL 1 HOUR ) AND NOW();";
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
                    dbConn.Close();
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
        public void PlcReadCycle()
        {
            while (true)
            {
                try
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    PlcDAQCommunicationObject daq_data = new PlcDAQCommunicationObject();
                    foreach(var item in fangpu_config.addr)
                    {
                        if(item.Key.Substring(0,1).Equals("M"))
                        {
                            daq_data.aream_data[item.Key] = S7S.Read(S7S.AreaM, item.Value, S7S.LenB);
                        }
                        else if (item.Key.Substring(0, 2).Equals("VW"))
                        {
                            daq_data.aream_data[item.Key] = S7S.Read(S7S.AreaV, item.Value, S7S.LenW);
                        }
                        else if(item.Key.Substring(0,2).Equals("VB"))
                        {
                            daq_data.aream_data[item.Key] = S7S.Read(S7S.AreaV, item.Value, S7S.LenB);
                        }
                        else if (item.Key.Substring(0, 1).Equals("I"))
                        {
                            daq_data.aream_data[item.Key] = S7S.Read(S7S.AreaI, item.Value, S7S.LenB);
                        }
                        else if (item.Key.Substring(0, 1).Equals("C") && read_interval_60s_flag == true)
                        {
                            daq_data.aream_data[item.Key] = S7S.Read(S7S.AreaC, item.Value, S7S.LenW);
                        }
                        else if (item.Key.Substring(0, 1).Equals("T"))
                        {
                            daq_data.aream_data[item.Key] = S7S.Read(S7S.AreaT, item.Value, S7S.LenW);
                        }
                    }
                    read_interval_60s_flag = false;
                    TerminalQueues.plcdataprocessqueue.Enqueue(daq_data);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds >= 950)
                        continue;
                    else
                    Thread.Sleep(950-Convert.ToInt32(sw.Elapsed.TotalMilliseconds));
                    
                    }
                
                catch (Exception e)
                {
                    if (S7S.ClientSocket == null | S7S.ClientSocket.Connected == false)
                    {
                        S7S.Connect();
                    }
                    Thread.Sleep(580);
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
            //S7S = new S7_Socket();
            //S7S.Connect();
            //int read_count = 0;
            while (true)
            {
                try
                {
                    if (S7S.ClientSocket.Connected == true)
                    {                       
                        if (TerminalQueues.plccommandqueue.Count > 0)
                        {
                            enableSync = false;
                            PlcCommand temp_plccmd = new PlcCommand(TerminalCommon.S7200AreaI, TerminalCommon.S7200DataByte, 0, 0, 0);
                            for (; TerminalQueues.plccommandqueue.Count > 0; )
                            {
                                TerminalQueues.plccommandqueue.TryDequeue(out temp_plccmd);
                                if (temp_plccmd.area == "M")
                                {
                                    S7S.Write_Bit(S7S.AreaM, temp_plccmd.addr, temp_plccmd.bitaddr, temp_plccmd.data);
                                    continue;
                                }
                                if (temp_plccmd.type == TerminalCommon.S7200DataByte)
                                {
                                    S7S.Write(S7S.AreaV, temp_plccmd.addr, temp_plccmd.data);
                                }
                                else if (temp_plccmd.type == TerminalCommon.S7200DataWord)
                                {
                                    if (temp_plccmd.data < 0)
                                    { 
                                        temp_plccmd.data &= 0x0000ffff;
                                    }                                   
                                    S7S.Write(S7S.AreaV, temp_plccmd.addr, S7S.LenW,temp_plccmd.data );
                                }
                               // Trace.WriteLine(temp_plccmd.area.ToString() + temp_plccmd.addr.ToString() + temp_plccmd.bitaddr.ToString() + temp_plccmd.data.ToString());
                            }
                            enableSync = true;
                        }
                        
                    }
                    else
                    {
                        S7S.Connect();
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
                            Dictionary<string, int> aream_data = new Dictionary<string, int>();
                            aream_data["M0"] = PPI.Read(PPI.AreaM, 0, PPI.LenD);
                            aream_data["M1"] = PPI.Read(PPI.AreaM, 4, PPI.LenD);

                            aream_data["VB4000"] = PPI.Read(PPI.AreaV, 4000, PPI.LenD);
                            aream_data["VB4004"] = PPI.Read(PPI.AreaV, 4004, PPI.LenD);
                            aream_data["VB4008"] = PPI.Read(PPI.AreaV, 4008, PPI.LenD);
                            aream_data["T37"] = PPI.Read(PPI.AreaT, 37, PPI.LenW);
                            aream_data["T38"] = PPI.Read(PPI.AreaT, 38, PPI.LenW);
                            aream_data["T39"] = PPI.Read(PPI.AreaT, 39, PPI.LenW);
                            aream_data["T40"] = PPI.Read(PPI.AreaT, 40, PPI.LenW);
                            aream_data["T41"] = PPI.Read(PPI.AreaT, 41, PPI.LenW);
                            aream_data["C1"] = PPI.Read(PPI.AreaC, 1, PPI.LenW);
                            aream_data["C2"] = PPI.Read(PPI.AreaC, 2, PPI.LenW);
                            aream_data["C4"] = PPI.Read(PPI.AreaC, 4, PPI.LenW);
                            read_count = 0;
                        }

                        if (TerminalQueues.plccommandqueue.Count > 0)
                        {
                            PlcCommand temp_plccmd = new PlcCommand(TerminalCommon.S7200AreaI, TerminalCommon.S7200DataByte, 0, 0, 0);
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
            PlcDAQCommunicationObject plc_temp_data = new PlcDAQCommunicationObject();
            while (true)
            {
                try
                {
                   
                    while (TerminalQueues.plcdataprocessqueue.Count > 0)
                    {
                        TerminalQueues.plcdataprocessqueue.TryDequeue(out plc_temp_data);
                        if (plc_temp_data == null)
                        {
                            continue;
                        }
                        CycleUpdateGuiDisplay(plc_temp_data);
                        if (S7S.S7SConnected == false)
                        {
                            continue;
                        }                           
                        TerminalQueues.datacenterprocessqueue.Enqueue(plc_temp_data);
                        TerminalQueues.localdataqueue.Enqueue(plc_temp_data);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("plc数据处理线程出错！");
                    Thread.Sleep(200);
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
                    while (TerminalQueues.datacenterprocessqueue.Count > 0)
                    {
                        PlcDAQCommunicationObject plc_temp_data = new PlcDAQCommunicationObject();                      
                        TerminalQueues.datacenterprocessqueue.TryDequeue(out plc_temp_data);                  
                        if (plc_temp_data == null)
                        {
                            continue;
                        }
                        var historydata = new historydata();
                        var realtimedata = new realtimedata();
                        var warn_info=new warn_info();
                        var historydata_jsoncopy = new historydata_jsoncopy();               
                        historydata.deviceid = Properties.TerminalParameters.Default.terminal_name;
                        FangpuTerminalJsonModel jsonobj = new FangpuTerminalJsonModel();
                        FangpuTerminalJsonModel_systus jsonobj_2 = new FangpuTerminalJsonModel_systus();
                        jsonobj.V4000 = plc_temp_data.aream_data["VB4000"];
                        jsonobj.V4001 = plc_temp_data.aream_data["VB4001"];
                        jsonobj.V4002 = plc_temp_data.aream_data["VB4002"];
                        jsonobj.V4003 = plc_temp_data.aream_data["VB4003"];
                        jsonobj.V4004 = plc_temp_data.aream_data["VB4004"];
                        jsonobj.V4005 = plc_temp_data.aream_data["VB4005"];
                        jsonobj.V4006 = plc_temp_data.aream_data["VB4006"];
                        jsonobj.V4007 = plc_temp_data.aream_data["VB4007"];
                        jsonobj.V4008 = plc_temp_data.aream_data["VB4008"];
                        jsonobj.M53 = ((plc_temp_data.aream_data["M5"] & 0x08) == 0x08);

                        jsonobj_2.M37 = ((plc_temp_data.aream_data["M3"] & 0x80) == 0x80);
                        jsonobj_2.M42 = ((plc_temp_data.aream_data["M4"] & 0x04) == 0x04);
                        jsonobj_2.M52 = ((plc_temp_data.aream_data["M5"] & 0x04) == 0x04);
                        jsonobj_2.M44 = ((plc_temp_data.aream_data["M4"] & 0x10) == 0x10);
                        jsonobj_2.M67 = ((plc_temp_data.aream_data["M6"] & 0x80) == 0x80);
                        jsonobj_2.M00 = ((plc_temp_data.aream_data["M0"] & 0x01) == 0x01);
                        jsonobj_2.M01 = ((plc_temp_data.aream_data["M0"] & 0x02) == 0x02);

                        historydata.value = JsonConvert.SerializeObject(jsonobj);
                        historydata.systus = JsonConvert.SerializeObject(jsonobj_2);

                        historydata.storetime = plc_temp_data.daq_time;
                        historydata.shuayou_consume_seconds = plc_temp_data.aream_data["T38"] / 10.0f;
                        historydata.kaomo_consume_seconds = plc_temp_data.aream_data["T39"] / 10.0f;
                        historydata.kaoliao_consume_seconds = plc_temp_data.aream_data["T40"] / 10.0f;
                        historydata.jinliao_consume_seconds = plc_temp_data.aream_data["T41"] / 10.0f;
                        historydata.lengque_consume_seconds = plc_temp_data.aream_data["T37"] / 10.0f;
                        historydata.cycletime = (float)zuomotime;

                        Dictionary<string, object> historydata_json = new Dictionary<string, object>();
                        historydata_json.Add("刷油时间", plc_temp_data.aream_data["T38"] / 10.0f);
                        historydata_json.Add("烤模时间", plc_temp_data.aream_data["T39"] / 10.0f);
                        historydata_json.Add("烤料时间", plc_temp_data.aream_data["T40"] / 10.0f);
                        historydata_json.Add("浸料时间", plc_temp_data.aream_data["T41"] / 10.0f);
                        historydata_json.Add("冷却时间", plc_temp_data.aream_data["T37"] / 10.0f);
                        historydata_json.Add("一板模时间", (float)zuomotime);
                        historydata_jsoncopy.deviceid = Properties.TerminalParameters.Default.terminal_name;
                        historydata_jsoncopy.data_json = JsonConvert.SerializeObject(historydata_json);
                        historydata_jsoncopy.storetime = plc_temp_data.daq_time;
                        historydata_jsoncopy.systus = JsonConvert.SerializeObject(jsonobj_2); 
                        
                        mysql.historydata.Add(historydata);
                        mysql.historydata_jsoncopy.Add(historydata_jsoncopy);
                        try
                        {
                            realtimedata = mysql.realtimedata.SingleOrDefault(a => a.deviceid.Equals(Properties.TerminalParameters.Default.terminal_name));
                            realtimedata.value = JsonConvert.SerializeObject(jsonobj);
                            realtimedata.storetime = plc_temp_data.daq_time;
                            realtimedata.shuayou_consume_seconds = plc_temp_data.aream_data["T38"] / 10.0f;
                            realtimedata.kaomo_consume_seconds = plc_temp_data.aream_data["T39"] / 10.0f;
                            realtimedata.kaoliao_consume_seconds = plc_temp_data.aream_data["T40"] / 10.0f;
                            realtimedata.jinliao_consume_seconds = plc_temp_data.aream_data["T41"] / 10.0f;
                            realtimedata.lengque_consume_seconds = plc_temp_data.aream_data["T37"] / 10.0f;
                            realtimedata.device_on_time = plc_temp_data.aream_data["C2"].ToString() + "小时" + plc_temp_data.aream_data["C1"].ToString() + "分钟";
                            realtimedata.furnace_on_time = plc_temp_data.aream_data["C6"].ToString() + "小时" + plc_temp_data.aream_data["C5"].ToString() + "分钟";
                            realtimedata.produce_time=plc_temp_data.aream_data["C4"].ToString() + "小时" + plc_temp_data.aream_data["C3"].ToString() + "分钟";
                            realtimedata.cycletime = (float)zuomotime;
                            realtimedata.systus = JsonConvert.SerializeObject(jsonobj_2);
                                                       
                        }
                        catch (Exception ex)
                        {     
                        }
                        if(TerminalQueues.warninfoqueue.Count>0)
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
        public void SendCommandToPlc(string area, string type, int data, int addr, int bitaddr=0)
        {
            PlcCommand cmd = new PlcCommand(area, type, data, addr, bitaddr);
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
        //输入参数：PLC数据字典
        //返回值：  相应的报警信息
        //修改记录：
        //==================================================================
        public List<string> WarnInfoProcess(Dictionary<string, int> info)
        {
            List<string> results = new List<string>();
            int base_zero = 0;
            int i = 0;

            for (int j = 0; j <= 8; j++)
            {
                if (info["VB400" + j.ToString()] > 0)
                {
                    for (i = 0; i <= 7; i++)
                    {
                        if ((info["VB400" + j.ToString()] & (base_zero | (1 << i))) == 1 << i)
                        {
                            results.Add(TerminalCommon.warn_info["400" + j.ToString() + "_" + i.ToString()]);
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
                        jsonobj.V4000 = plc_temp_data.aream_data["VB4000"];
                        jsonobj.V4001 = plc_temp_data.aream_data["VB4001"];
                        jsonobj.V4002 = plc_temp_data.aream_data["VB4002"];
                        jsonobj.V4003 = plc_temp_data.aream_data["VB4003"];
                        jsonobj.V4004 = plc_temp_data.aream_data["VB4004"];
                        jsonobj.V4005 = plc_temp_data.aream_data["VB4005"];
                        jsonobj.V4006 = plc_temp_data.aream_data["VB4006"];
                        jsonobj.V4007 = plc_temp_data.aream_data["VB4007"];
                        jsonobj.V4008 = plc_temp_data.aream_data["VB4008"];
                        jsonobj.M53 = ((plc_temp_data.aream_data["M5"] & 0x08) == 0x08);

                        jsonobj_2.M37 = ((plc_temp_data.aream_data["M3"] & 0x80) == 0x80);
                        jsonobj_2.M42 = ((plc_temp_data.aream_data["M4"] & 0x04) == 0x04);
                        jsonobj_2.M52 = ((plc_temp_data.aream_data["M5"] & 0x04) == 0x04);
                        jsonobj_2.M44 = ((plc_temp_data.aream_data["M4"] & 0x10) == 0x10);
                        jsonobj_2.M67 = ((plc_temp_data.aream_data["M6"] & 0x80) == 0x80);
                        jsonobj_2.M00 = ((plc_temp_data.aream_data["M0"] & 0x01) == 0x01);
                        jsonobj_2.M01 = ((plc_temp_data.aream_data["M0"] & 0x02) == 0x02);

                        StringBuilder strSql = new StringBuilder();
                        strSql.Append("insert into historydata(");
                        strSql.Append("data,systus,recordtime,shuayou_consume_seconds,kaomo_consume_seconds,kaoliao_consume_seconds,jinliao_consume_seconds,lengque_consume_seconds,cycletime)");
                        strSql.Append(" values(");
                        strSql.Append("@data,@systus,@recordtime,@shuayou_consume_seconds,@kaomo_consume_seconds,@kaoliao_consume_seconds,@jinliao_consume_seconds,@lengque_consume_seconds,@cycletime)");
                   
                        SQLiteParameter[] parameters = {  
                        TerminalLocalDataStorage.MakeSQLiteParameter("@data", DbType.String,100,JsonConvert.SerializeObject(jsonobj)), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@systus", DbType.String,100,JsonConvert.SerializeObject(jsonobj_2)), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@recordtime", DbType.DateTime,30,plc_temp_data.daq_time),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@shuayou_consume_seconds", DbType.Double,100,plc_temp_data.aream_data["T38"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@kaomo_consume_seconds", DbType.Double,100,plc_temp_data.aream_data["T39"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@kaoliao_consume_seconds", DbType.Double,100,plc_temp_data.aream_data["T40"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@jinliao_consume_seconds", DbType.Double,100,plc_temp_data.aream_data["T41"]/10.0),
                        TerminalLocalDataStorage.MakeSQLiteParameter("@lengque_consume_seconds", DbType.Double,100,plc_temp_data.aream_data["T37"]/10.0),
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
                    log.Error("本地数据存储线程出错！"+DateTime.Now);
                }
                try
                {
                    if (TerminalQueues.warninfoqueue_local.Count > 0)
                    {
                        PLCWarningObject plc_temp_warn = new PLCWarningObject();
                        TerminalQueues.warninfoqueue_local.TryDequeue(out plc_temp_warn);
                        WarnInfoLocalStorage(plc_temp_warn.warndata,plc_temp_warn.warn_time);
                    }
                }
                catch(Exception ex)
                {
                    log.Error("警告存储出错！" + DateTime.Now);
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
        
        private void FangpuTerminal_FormClosing(object sender, FormClosingEventArgs e)
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

                if (S7S.ClientSocket.Connected == true)
                {
                    S7S.ClientSocket.Close();
                }

                tcpobject.socket_stop_connect();
            }
            catch { }
            Application.ExitThread();
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
            if (this.vw2010 < Convert.ToInt32(shuayou_upper) * 10)
            {
                this.vw2010 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2010, 2010);
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
            if (this.vw2010 > -Convert.ToInt32(shuayou_lower) * 10)
            {
                this.vw2010 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2010, 2010);
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
            if (this.vw2016 < Convert.ToInt32(kaoliao_consume_upper) * 10)
            {
                this.vw2016 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2016, 2016);
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
            if (this.vw2016 > -Convert.ToInt32(kaoliao_consume_lower) * 10)
            {
                this.vw2016 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2016, 2016);
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
            if (this.vw2014 < Convert.ToInt32(kaomo_consume_upper) * 10)
            {
                this.vw2014 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2014, 2014);
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
            if (this.vw2014 > -Convert.ToInt32(kaomo_consume_lower) * 10)
            {
                this.vw2014 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2014, 2014);
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
            if (this.vw2012 < Convert.ToInt32(lengque_consume_upper) * 10)
            {
                this.vw2012 += 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2012, 2012);
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
            if (this.vw2012 > -Convert.ToInt32(lengque_consume_lower) * 10)
            {
                this.vw2012 -= 10;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2012, 2012);
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
        private void switchSlider_pg1_yureludianji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_no1qianxiao_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_yureluqianmen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_no2qianxiao_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_yureluhoumen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_kaoliaoludianji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_no3qianxiao_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_kaoliaoluqianmen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_no4qianxiao_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg1_kaoliaoluhoumen_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchRotary_pg2_tuomoqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchRotary_pg2_choufengji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg2_shuixiang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchSlider_pg2_jinliaoqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchRotary_pg3_shuayouji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
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
        private void switchRotary_pg3_shuixiang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
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
        private void switchRotary_pg3_jinliaoqigang_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
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
        private void switchRotary_pg3_tuomoji_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchRotary_pg3_luzidianyuan_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchRotary_runmode_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
        private void switchRotary_runstatus_ValueChanged(object sender, Iocomp.Classes.ValueIntegerEventArgs e)
        {
            if (stopSendCmdtoPLC == true)
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
            if ((temp.aream_data["M0"]&0x01) == 0x01)//自动
            {
                switchRotary_runmode.Value = 1;
              
            }
            else
            {
                switchRotary_runmode.Value = 0;
            }
            if ((temp.aream_data["M0"] & 0x02) == 0x02)//启动
            {
                switchRotary_runstatus.Value = 1;
            
            }
            else
            {
                switchRotary_runstatus.Value = 0;
            }

            if(tabControl_terminal.SelectedIndex==2)
            {
                if ((temp.aream_data["M3"] & 0x80) == 0x80)//刷油机
                {
                    switchRotary_pg3_shuayouji.Value = 1;
                }
                else
                {
                    switchRotary_pg3_shuayouji.Value = 0;
                }
                if ((temp.aream_data["M4"] & 0x04) == 0x04)//水箱
                {
                    switchRotary_pg3_shuixiang.Value = 1;

                }
                else
                {
                    switchRotary_pg3_shuixiang.Value = 0;
                }
                if ((temp.aream_data["M5"] & 0x04) == 0x04)//浸料气缸
                {
                    switchRotary_pg3_jinliaoqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg3_jinliaoqigang.Value = 0;
                }
                if ((temp.aream_data["M4"] & 0x10) == 0x10)//脱模机
                {
                    switchRotary_pg3_tuomoji.Value = 1;
                }
                else
                {
                    switchRotary_pg3_tuomoji.Value = 0;
                }
                if ((temp.aream_data["M6"] & 0x80) == 0x80)//炉子电源
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
                if ((temp.aream_data["M4"] & 0x08) == 0x08)//水箱
                {
                    switchSlider_pg2_shuixiang.Value = 1;
                }
                else
                {
                    switchSlider_pg2_shuixiang.Value = 0;
                }
                if ((temp.aream_data["M5"] & 0x80) == 0x80)//浸料气缸
                {
                    switchSlider_pg2_jinliaoqigang.Value = 1;
                }
                else
                {
                    switchSlider_pg2_jinliaoqigang.Value = 0;
                }
                if ((temp.aream_data["M5"] & 0x02) == 0x02)//脱模气缸
                {
                    switchRotary_pg2_tuomoqigang.Value = 1;
                }
                else
                {
                    switchRotary_pg2_tuomoqigang.Value = 0;
                }
                if ((temp.aream_data["M5"] & 0x40) == 0x40)//抽风机
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
                if ((temp.aream_data["M2"] & 0x02) == 0x02)//电机
                {
                    switchSlider_pg1_yureludianji.Value = 0;
                }
                else
                {
                    switchSlider_pg1_yureludianji.Value = 1;
                }
                if ((temp.aream_data["M3"] & 0x08) == 0x08)//电机
                {
                    switchSlider_pg1_kaoliaoludianji.Value = 0;
                }
                else
                {
                    switchSlider_pg1_kaoliaoludianji.Value = 1;
                }
                if ((temp.aream_data["M1"] & 0x80) == 0x80)//一号钳销
                {
                    switchSlider_pg1_no1qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no1qianxiao.Value = 0;
                }
                if ((temp.aream_data["M1"] & 0x08) == 0x08)//二号钳销
                {
                    switchSlider_pg1_no2qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no2qianxiao.Value = 0;
                }
                if ((temp.aream_data["M2"] & 0x40) == 0x40)//三号钳销
                {
                    switchSlider_pg1_no3qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no3qianxiao.Value = 0;
                }
                if ((temp.aream_data["M2"] & 0x20) == 0x20)//四号钳销
                {
                    switchSlider_pg1_no4qianxiao.Value = 1;
                }
                else
                {
                    switchSlider_pg1_no4qianxiao.Value = 0;
                }
                if ((temp.aream_data["M0"] & 0x08) == 0x08)//前门
                {
                    switchSlider_pg1_yureluqianmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_yureluqianmen.Value = 0;
                }
                if ((temp.aream_data["M0"] & 0x10) == 0x10)//后门
                {
                    switchSlider_pg1_yureluhoumen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_yureluhoumen.Value = 0;
                }
                if ((temp.aream_data["M0"] & 0x20) == 0x20)//前门
                {
                    switchSlider_pg1_kaoliaoluqianmen.Value = 1;
                }
                else
                {
                    switchSlider_pg1_kaoliaoluqianmen.Value = 0;
                }
                if ((temp.aream_data["M0"] & 0x40) == 0x40)//后门
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
        #region 辅助功能定义
        

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

            var mysql = new FangpuDatacenterModelEntities();
            var para = from d in mysql.proceduretechnologybase
                       where d.device_name.Equals(Properties.TerminalParameters.Default.terminal_name)
                       select d;
            
            try
            {
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


                    SQLiteParameter[] parameters = new SQLiteParameter[] 
                    {                     
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
        Reasonform reason_frm;
        private void paraupload_Click(object sender, EventArgs e)
        {
            try
            {
                reason_frm = new Reasonform();
                
                //MessageBox.Show("上传设置参数？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK
                if (reason_frm.ShowDialog() == DialogResult.OK)
                {
                    var mysql = new FangpuDatacenterModelEntities();
                    var d = new proceduretechnologybase_work();
                    d.device_name = Properties.TerminalParameters.Default.terminal_name; 
                    d.product_id = Convert.ToString(product_id);
                    d.material = Convert.ToString(material);
                    d.reason = Reasonform.upload_reason;
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
        //模块名： formupload_Click
        //作者：    Yufei Zhang
        //日期：    2015.12.20
        //功能：    打开日常检查表界面
        //输入参数：
        //返回值：  
        //修改记录：
        //==================================================================
        public bool dailycheck_open=false;
        Dailycheck daily_frm;
        private void formupload_Click(object sender, EventArgs e)
        {
            if (dailycheck_open == true)
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
        Bandcheck band_frm;
        private void formupload2_Click(object sender, EventArgs e)
        {
            if (bandcheck_open == true)
            {
                band_frm.TopMost = true;
                return;
            }

            band_frm = new Bandcheck(this);
            band_frm.Show(); 
        }
        //打开现场点检表
        public bool fieldcheck_open=false;
        Fieldcheck field_frm;
        private void formupload3_Click(object sender, EventArgs e)
        {
            if (fieldcheck_open == true)
            {
                field_frm.TopMost = true;
                return;
            }
                
            field_frm = new Fieldcheck(this);
            field_frm.Show(); 
        }
        private void buzuoguan_KeyPress(object sender, KeyPressEventArgs e)
        {        
            if(e.KeyChar==13)
            {
                if (buzuoguan.Text != "")
                {
                    SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, (int)(Convert.ToDouble(buzuoguan.Text) * 10), 2070);
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
        private void jinliaoshezhi_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (jinliaoshezhi.Text != "")
                {
                    SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, (int)(Convert.ToDouble(jinliaoshezhi.Text) * 10), 2008);
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
        //点击不做管弹出键盘
        private void buzuoguan_Click(object sender, EventArgs e)
        {
            
            buzuoguan.Text = "";     
            if(numberBoard==null)
            {
                numberBoard = new numberboard(this);
                numberBoard.Show();
            }           
            else if (numberBoard.IsDisposed==true)
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
            else if (numberBoard.IsDisposed == true)  //打开过一次后不等于null
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

        //选择产品工艺参数，并输入plc
        private void type_accept_Click(object sender, EventArgs e)
        {
            try
            {
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(shuayou_base) * 10), 2000);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(kaomo_consume_base) * 10), 2004);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(kaoliao_consume_base) * 10), 2006);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, Convert.ToInt16(Convert.ToDouble(lengque_consume_base) * 10), 2002);
                productlabel.Text = Convert.ToString(typeselect.SelectedItem);
                this.vw2010 = 0;
                this.vw2012 = 0;
                this.vw2014 = 0;
                this.vw2016 = 0;
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2010, 2010);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2012, 2012);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2014, 2014);
                SendCommandToPlc(TerminalCommon.S7200AreaV, TerminalCommon.S7200DataWord, this.vw2016, 2016);
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

        stoptable stoptable_frm;
        public bool stoptable_open = false;
        bool poweroff = false;
        private void button_halttable_Click(object sender, EventArgs e)
        {
            try
            {
                if (button_halttable.BackColor == System.Drawing.SystemColors.Control)
                {
                    if (TextCommand.Exists("haltinfo.txt"))
                    {
                        TextCommand.DeleteFile("haltinfo.txt");
                    }
                    if (stoptable_open == true)
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
                    button_halttable.BackColor = System.Drawing.Color.Salmon;
                    button_halttable.Text = "停机结束";
                    sevenSegmentClock_start.Value = DateTime.Now;
                    TextCommand.CreateFile("haltinfo.txt");
                    DateTime timestart = sevenSegmentClock_start.Value;
                    TextCommand.WriteLine("haltinfo.txt", timestart.ToString());
                    sevenSegmentClock_end.Value = sevenSegmentClock_start.Value;
                    poweroff = true;
                    sevenSegmentClock_end.AutoUpdate = true;

                }
                else if (button_halttable.BackColor == System.Drawing.Color.Salmon)
                {
                    button_halttable.Text = "停机结束";
                    StoptableSqlUpload halt_uploader = new StoptableSqlUpload(this);
                    if (TextCommand.Exists("haltinfo.txt") && (poweroff == false))
                    {
                        FileStream aFile = new FileStream("haltinfo.txt", FileMode.Open);
                        StreamReader sr = new StreamReader(aFile);
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
                    Thread t = new Thread(halt_uploader.HaltReasonUpload);
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch(Exception ex)
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
            //检测是否有停机操作
            if (TextCommand.Exists("haltinfo.txt") == false)
                return;
            button_halttable.BackColor = System.Drawing.Color.Salmon;
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
                        case DBT_DEVICEARRIVAL://U盘插入
                            DriveInfo[] s = DriveInfo.GetDrives();
                            foreach (DriveInfo drive in s)
                            {
                                if (drive.DriveType == DriveType.Removable)
                                {
                                    Trace.WriteLine((DateTime.Now.ToString() + "--> U盘已插入，盘符为:" + drive.Name.ToString()));
                                    UsbName = drive.Name;
                                    
                                    if(this.outputform!=null&&!this.outputform.IsDisposed)
                                    {
                                        this.outputform.label_info.Text = "U盘已插入，盘符为:"+UsbName;
                                        this.outputform.button_accept.Enabled = true;
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
                            Trace.WriteLine(DateTime.Now.ToString() + "--> U盘已卸载！");
                            if (this.outputform != null && !this.outputform.IsDisposed)
                            {
                                this.outputform.label_info.Text = "U盘已卸载！";
                                this.outputform.button_accept.Enabled = false;
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
        OutputForm outputform;
        private void button_localdataoutput_Click(object sender, EventArgs e)
        {
            if(outputform==null||outputform.IsDisposed)
            {
                outputform = new OutputForm(this);
                outputform.Show();
            }
            else
            {
                Trace.WriteLine("asd");
            }
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
