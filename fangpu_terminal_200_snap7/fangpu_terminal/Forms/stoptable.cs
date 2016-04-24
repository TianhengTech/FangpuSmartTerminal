using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;



namespace fangpu_terminal
{
    public partial class stoptable : Form
    {
        FangpuTerminal FangpuTerminal;
        public static string haltreason = "";
        bool cancelclose = false;
        public stoptable(FangpuTerminal Terminal)
        {
            InitializeComponent();

            FangpuTerminal = Terminal;
            haltreason = "";
            Init();
        }
        public void Init()
        {

        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            foreach (string item in checkedListBox_stopreason.CheckedItems)
            {
                if (haltreason.Equals(""))
                {
                    haltreason = item;
                }
                else
                    haltreason = haltreason + "," + item;
            }
            TextCommand.WriteLine("haltinfo.txt", haltreason.ToString());
            this.Close();
        }



        private void stoptable_Load(object sender, EventArgs e)
        {
            FangpuTerminal.stoptable_open = true;
            //this.Location = new Point(FangpuTerminal.Location.X + (FangpuTerminal.Size.Width - this.Size.Width) / 2, FangpuTerminal.Location.Y);
        }
        private void stoptable_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cancelclose == true)
            {
                cancelclose = false;
                FangpuTerminal.stoptable_open = false;
                FangpuTerminal.button_halttable.Enabled = true;
                this.DialogResult = DialogResult.No;
                return;
            }
            FangpuTerminal.stoptable_open = false;
            FangpuTerminal.button_halttable.Enabled = true;
            this.DialogResult = DialogResult.OK;
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            cancelclose = true;
            this.Close();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private void stoptable_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x0112, 0xF012, 0);
        } 

    }
    class StoptableSqlUpload
    {
        FangpuTerminal FangpuTerminal;
        public StoptableSqlUpload(FangpuTerminal Terminal)
        {
            FangpuTerminal = Terminal;
        }
        public string reason;
        public DateTime start;
        public DateTime end;
        public void HaltReasonUpload()
        {
            bool firsttry = true;
            while(true)
            {               
                try
                {  
                    var mysql = new FangpuDatacenterModelEntities();
                    var haltinfo = new haltinfo();
                    haltinfo.device_name = Properties.TerminalParameters.Default.terminal_name;
                    haltinfo.time_start = start;
                    haltinfo.time_end = end;
                    haltinfo.halt_reason = reason;
                    haltinfo.storetime = DateTime.Now;
                    mysql.haltinfo.Add(haltinfo);
                    mysql.SaveChanges();
                    FangpuTerminal.HaltUI S1 = new FangpuTerminal.HaltUI(updateui);
                    FangpuTerminal.BeginInvoke(S1);                    
                    try
                    {
                        TextCommand.DeleteFile("haltinfo.txt");
                    }
                    catch
                    {

                    }
                    break;
                }
                catch
                {
                    if (firsttry == true)
                    MessageBox.Show("停机信息上传失败!\n与数据中心连接出错!请稍后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    firsttry = false;
                    FangpuTerminal.log.Error("停机原因存储出错");                    
                }
            }
            
        }
        public void updateui()
        {
            MessageBox.Show("停机结束\n" + start.ToString() + " - " +
                            end.ToString() + "\n" +
                            "停机原因:" + reason, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FangpuTerminal.button_halttable.BackColor = System.Drawing.SystemColors.Control;
            FangpuTerminal.button_halttable.Text = "停机";
            FangpuTerminal.sevenSegmentClock_start.Visible = false;
            FangpuTerminal.sevenSegmentClock_end.Visible = false;
            FangpuTerminal.ledArrow1.Visible = false;
            FangpuTerminal.sevenSegmentClock_end.AutoUpdate = false;
            FangpuTerminal.button_halttable.ForeColor = System.Drawing.SystemColors.ControlText;
            FangpuTerminal.button_halttable.Enabled = true;
        }
    }
}




