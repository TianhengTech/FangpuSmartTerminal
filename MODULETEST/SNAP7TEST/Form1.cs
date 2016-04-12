using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Snap7;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using MySql.Data.MySqlClient;



namespace SNAP7TEST
{
    public partial class Mainform : Form
    {
        byte[] buffer = new byte[2048];
        S7Client Client;
        DataTable dataTable;
        Thread ReadThread,ChartThread;
        public delegate void delegete1(DataTable CurveData, Mainform series);
        


        public Mainform()
        {
            InitializeComponent();
            Init();            
        }
        private void Init()
        {
            Client = new S7Client();
            Client.SetConnectionParams("192.168.1.51", 0x1000, 0x1001);
            int result = Client.Connect();
            //int result=Client.ConnectTo("192.168.1.50", 0, 0);
            Trace.WriteLine(result);
            dataTable = new DataTable();
            dataTable.Columns.Add("时间");
            dataTable.Columns.Add("温度");
            ReadThread = new Thread(S7200Read);
            ReadThread.IsBackground = true;
            ChartThread = new Thread(ChartRefresh);
            ChartThread.IsBackground = true;           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //int result=Client.ReadArea(S7Client.S7AreaDB, 1, 0, 1, S7Client.S7WLReal, buffer);
            //byte[] b=new byte[1];
            //b[0]=0xFF;

            //int r = Client.WriteArea(S7Client.S7AreaDB, 1, 2000, 1, S7Client.S7WLByte, b);
            //Trace.WriteLine(buffer);

            if (ReadThread.IsAlive == false)
            {
                ReadThread.Start();
            }
            if (ChartThread.IsAlive == false)
            {
                ChartThread.Start();
            }

            
            

        }
        private void ChartRefresh()
        {
            while(true)
            {
                if(TerminalQueues.plcreadqueue.Count>0)
                {
                    TempObject tempObject = new TempObject();
                    TerminalQueues.plcreadqueue.TryDequeue(out tempObject);
                    if(tempObject==null)
                    {
                        continue;
                    }
                    if (dataTable.Rows.Count > 100)
                        dataTable.Rows.RemoveAt(0);
                    Random rd = new Random();
                    double rand = rd.Next(-5, 5) + rd.NextDouble();
                    dataTable.Rows.Add(tempObject.data_time.ToLongTimeString(),tempObject.temp+(float)rand);
                    delegete1 task = new delegete1(ChartCurve.ChartCurveUpdate);
                    this.Invoke(task,dataTable,this);      
                    UploadToSQL(tempObject.data_time, tempObject.temp);                    
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Client.Disconnect();
            if(ReadThread.IsAlive)
            {
                ReadThread.Abort();
            }
            if(ChartThread.IsAlive)
            {
                ReadThread.Abort();
            }
        }
        private void S7200Read()
        {
            while(true)
            {
                Client.DBRead(1, 4000, 2, buffer);
                byte[] bytes = new byte[2];
                Array.Copy(buffer, 0, bytes, 0, 2);
                Array.Reverse(bytes);
                float temp = BitConverter.ToInt16(bytes, 0) / 10.0f;
                TempObject tempObject = new TempObject();
                tempObject.temp=temp;
                tempObject.data_time = DateTime.Now;
                TerminalQueues.plcreadqueue.Enqueue(tempObject);                
                Thread.Sleep(1500);
                //string json = Newtonsoft.Json.JsonConvert.SerializeObject();
            }

        }
        private void UploadToSQL(DateTime time,float temp)
        {
            MySqlConnection dbConn = new MySql.Data.MySqlClient.MySqlConnection("Persist Security Info=False;server=syspeed123.mysql.rds.aliyuncs.com;database=tianheng_rd;uid=tianheng_rd;password=tianheng123");
            MySqlCommand cmd = dbConn.CreateCommand();
            dbConn.Open();
            cmd.CommandText = "insert into lab_temperature(TEMP,TIME) values(@temp,@time)";
            MySqlParameter[] parameters = new MySqlParameter[2];
            parameters[1] = new MySqlParameter("@time", MySqlDbType.DateTime);
            parameters[1].Value = time;
            parameters[0] = new MySqlParameter("@temp", MySqlDbType.Double);
            parameters[0].Value = temp;
            cmd.Parameters.AddRange(parameters);
            cmd.ExecuteNonQuery();
            dbConn.Close();
            
        }


    }
}
