using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace fangpu_terminal
{
    public partial class Dailycheck : Form
    {
        FangpuTerminal FangpuTerminal;
        public Dailycheck(FangpuTerminal Terminal)
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now;
            FangpuTerminal = Terminal;
            textBox2.Text = Properties.TerminalParameters.Default.terminal_name;
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FangpuTerminal.HideInputPanel();
            this.Close();
        }

        private void accept_Click(object sender, EventArgs e)
        {
            try
            {
            FangpuTerminal.HideInputPanel();
            var mysql = new FangpuDatacenterModelEntities();
            var workerlist = new dailychecklist();
            workerlist.deviceid = Properties.TerminalParameters.Default.terminal_name;
            workerlist.airleak_check = true;
            try
            {
                workerlist.airpressure_check = checkedListBox1.GetItemChecked(0);
                workerlist.gaspressure_check = checkedListBox1.GetItemChecked(1);
                workerlist.airleak_check = checkedListBox1.GetItemChecked(2);
                workerlist.gasleak_check = checkedListBox1.GetItemChecked(3);
                workerlist.belt_check = checkedListBox1.GetItemChecked(4);
                workerlist.furnacecylinder_check = checkedListBox1.GetItemChecked(5);
                workerlist.surfacesensor_check = checkedListBox1.GetItemChecked(6);
                workerlist.demouldcylinder_check = checkedListBox1.GetItemChecked(7);
                workerlist.airtap_check = checkedListBox1.GetItemChecked(8);
                workerlist.ventilator_check = checkedListBox1.GetItemChecked(9);
                workerlist.screen_check = checkedListBox1.GetItemChecked(10);
                workerlist.groundclean_check = checkedListBox1.GetItemChecked(11);
                workerlist.tableclean_check = checkedListBox1.GetItemChecked(12);
                if (zhengchang.Text.Trim() == String.Empty)
                { 
                    workerlist.normal = null;
                }
                else
                { 
                    workerlist.normal = (float)Convert.ToDouble(zhengchang.Text) / 1.0f;
                }
                if (tiaoji.Text.Trim() == String.Empty)
                { 
                    workerlist.debug = null;
                }
                else
                { 
                    workerlist.debug = (float)Convert.ToDouble(tiaoji.Text) / 1.0f;
                }
                if (shengwen.Text.Trim() == String.Empty)
                { 
                    workerlist.tempup = null;
                }
                else
                { 
                    workerlist.tempup = (float)Convert.ToDouble(shengwen.Text) / 1.0f;
                }
                if (ximuju.Text.Trim() == String.Empty)
                { 
                    workerlist.cleanmould = null;
                }
                else
                { 
                    workerlist.cleanmould = (float)Convert.ToDouble(ximuju.Text) / 1.0f;
                }
                if (huanliao.Text.Trim() == String.Empty)
                { 
                    workerlist.changematerial = null;
                }
                else
                { 
                    workerlist.changematerial = (float)Convert.ToDouble(huanliao.Text) / 1.0f;
                }
                if (shebeiguzhang.Text.Trim() == String.Empty)
                { 
                    workerlist.device_error = null;
                }
                else
                { 
                    workerlist.device_error = (float)Convert.ToDouble(shebeiguzhang.Text) / 1.0f;
                }
                if (dailiao.Text.Trim() == String.Empty)
                { 
                    workerlist.wait = null;
                }
                else
                {
                    workerlist.wait = (float)Convert.ToDouble(dailiao.Text) / 1.0f;
                }
                if (qita.Text.Trim() == String.Empty)
                {
                    workerlist.@else = null;
                }
                else
                {
                    workerlist.@else = (float)Convert.ToDouble(qita.Text) / 1.0f;
                }
                if (huanmuju.Text.Trim() == String.Empty)
                {
                    workerlist.changemoud = null;
                }
                else
                {
                    workerlist.changemoud = (float)Convert.ToDouble(huanmuju.Text) / 1.0f;
                }

                workerlist.shift = comboBox1.Text;
                workerlist.checkdate = dateTimePicker1.Value;
                workerlist.name = textBox1.Text;
                workerlist.tablename = textBox2.Text;
                workerlist.procedure = gongyimingcheng.Text;
            }
            catch(Exception ex)
            {
                MessageBox.Show("填写错误!请确认填写时间");
            }
                mysql.dailychecklist.Add(workerlist);
                mysql.SaveChanges();
                MessageBox.Show("上传成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.Close();
            }
                 catch(Exception ex)
                 {
                     Trace.WriteLine(ex);
                     MessageBox.Show("上传失败!");
                  }
            this.Close();
        }

        private void tiaoji_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(tiaoji, e);
 
        }
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
            catch(Exception)
            {
                return true;
            }

        }

        private void zhengchang_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(zhengchang, e);
        }

        private void shengwen_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(shengwen, e);
        }

        private void ximuju_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(ximuju, e);
        }

        private void huanmuju_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(huanmuju, e);
        }

        private void huanliao_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(huanliao, e);
        }

        private void dailiao_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(dailiao, e);
        }

        private void qita_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(qita, e);
        }

        private void shebeiguzhang_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = isgoodnumber(shebeiguzhang, e);
        }


        private void comboBox1_Click(object sender, EventArgs e)
        {
            comboBox1.DroppedDown = true;
        }

        private void Dailycheck_Load(object sender, EventArgs e)
        {
            FangpuTerminal.dailycheck_open = true;
            this.Location = new Point(FangpuTerminal.Location.X + (FangpuTerminal.Size.Width-this.Size.Width)/2, FangpuTerminal.Location.Y);
        }

        private void Dailycheck_FormClosed(object sender, FormClosedEventArgs e)
        {
            FangpuTerminal.dailycheck_open = false;
        }


    }
}
