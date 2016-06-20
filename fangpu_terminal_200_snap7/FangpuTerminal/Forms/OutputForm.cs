using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace fangpu_terminal
{
    public partial class OutputForm : Form
    {
        public delegate void OutputFormUI(int i); 
        public FangpuTerminal FangpuTerminal;
        public string selectedtype;
        
        public OutputForm(FangpuTerminal Terminal)
        {
            InitializeComponent();
            DateTime now = DateTime.Now;
            dataout_dateTimePickerstart.Value = now;
            dataout_dateTimePickerstart.Value = now.AddHours(1);
            comboBox_type.SelectedIndex=0;
            FangpuTerminal = Terminal;
            if(FangpuTerminal.isInsert==true)
            {
                label_info.Text = "U盘已插入，盘符为:" + FangpuTerminal.UsbName;
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();           
        }

        private Thread t;
        private void button_accept_Click(object sender, EventArgs e)
        {
            if ((FangpuTerminal.isInsert == true) && (FangpuTerminal.UsbName != null))
            {
                button_accept.Enabled = true;
                OutputThread Excelexecuter = new OutputThread(this);
                selectedtype = comboBox_type.SelectedItem.ToString();
                if(t==null)
                    t = new Thread(Excelexecuter.output2excel);
                if (t.IsAlive)
                {
                    t.Abort();
                }
                t = new Thread(Excelexecuter.output2excel);
                t.IsBackground = true;
                t.Start();
            }
        }
    }
    public class OutputThread
    {
        OutputForm OutputForm;
        DateTime start, end;
        public OutputThread(OutputForm outform)
        {
            OutputForm = outform;
            start = outform.dataout_dateTimePickerstart.Value;
            end = outform.dataout_dateTimePickerend.Value;            
        }
        public void output2excel()
        {
            try
            {
                OutputForm.OutputFormUI S1 = new OutputForm.OutputFormUI(updateui);
                OutputForm.Invoke(S1, -1);
                OutputForm.Invoke(S1, 0);
                StringBuilder strSql = new StringBuilder();
                string a = OutputForm.FangpuTerminal.UsbName + Properties.TerminalParameters.Default.terminal_name + OutputForm.selectedtype + "_" + string.Format("{0:yyyyMMddHHmmss}_{1:yyyyMMddHHmmss}.xls", start, end);
                if (OutputForm.selectedtype == "报警信息")
                {
                    //string sql = "SELECT COUNT(*) FROM historydata";
                    //DataSet ds0 = TerminalLocalDataStorage.Query(sql);
                    //int rows = (int)ds0.Tables[0].Rows[0][0];
                    strSql.Append("select * from warninfo where warntime<=@end and warntime>=@start");
                    SQLiteParameter[] parameters = {  
                        TerminalLocalDataStorage.MakeSQLiteParameter("@end", DbType.DateTime,30,end), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@start", DbType.DateTime,30,start),
                        };
                    DataSet ds = TerminalLocalDataStorage.Query(strSql.ToString(), parameters);
                    OutputForm.Invoke(S1, 0);
                    ExcelExport.SaveToFile(ExcelExport.RenderToExcel(ds.Tables[0]), a);
                    OutputForm.Invoke(S1, 100);
                }
                else if (OutputForm.selectedtype == "历史数据")
                {
                    strSql.Append("select * from historydata where recordtime<=@end and recordtime>=@start");
                    SQLiteParameter[] parameters = {  
                        TerminalLocalDataStorage.MakeSQLiteParameter("@end", DbType.DateTime,30,end), 
                        TerminalLocalDataStorage.MakeSQLiteParameter("@start", DbType.DateTime,30,start),
                        };
                    DataSet ds = TerminalLocalDataStorage.Query(strSql.ToString(), parameters);
                    OutputForm.Invoke(S1, 0);
                    ExcelExport.SaveToFile(ExcelExport.RenderToExcel(ds.Tables[0]), a);
                    OutputForm.Invoke(S1, 100);
                }          
            }
            catch(Exception ex)
            {
                MessageBox.Show("出现错误");
            }
            
       }
        public void updateui(int i)
        {
            if(i==-1)
            {
                OutputForm.progressBarControl1.Position = 0;
                return;
            }
            OutputForm.progressBarControl1.PerformStep() ;
            if (i == 100)
                OutputForm.label_info.Text = "导出完毕！";
            else
                OutputForm.label_info.Text = "导出中...请勿插拔U盘或切断电源";
        }

    }
}
