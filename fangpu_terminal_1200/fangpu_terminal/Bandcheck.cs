using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fangpu_terminal
{
    public partial class Bandcheck : Form
    {
        FangpuTerminal FangpuTerminal;
        public Bandcheck(FangpuTerminal Terminal)
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now;
            luwei.Text = Properties.TerminalParameters.Default.terminal_name;
            FangpuTerminal=Terminal;
            if (FangpuTerminal.typeselect.SelectedItem!=null)
            chanpinguige.Text = FangpuTerminal.typeselect.SelectedItem.ToString();

            
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
            FangpuTerminal.HideInputPanel();

        }

        private void accept_Click(object sender, EventArgs e)
        {
            try
            {
                FangpuTerminal.HideInputPanel();
                var mysql = new FangpuDatacenterModelEntities();
                var workerlist = new bandchecklist();
                workerlist.deviceid = Properties.TerminalParameters.Default.terminal_name;
                workerlist.sn=biaodanbianhao.Text;
                workerlist.date=dateTimePicker1.Value;
                workerlist.position=luwei.Text;       
                workerlist.name=xingmingbianhao.Text;
                workerlist.producesn=shengchandanhao.Text;
                workerlist.cardnumber = liuzhuankahao.Text;
                workerlist.weight = zhongliang.Text;
                workerlist.singleweight = danzhong.Text;
                workerlist.materialnumber = liaopihao.Text;
                workerlist.waste = feipin.Text;
                workerlist.producttype = chanpinguige.Text;
                workerlist.remark = beizhu.Text;
                if (xiaoji.Text.Trim() == String.Empty)
                    workerlist.subtotal = null;
                else
                workerlist.subtotal = Convert.ToDouble(xiaoji.Text);
                if (zhongliang.Text.Trim() == String.Empty)
                    workerlist.total = null;
                else
                workerlist.total = Convert.ToDouble(zhongliang.Text);
                mysql.bandchecklist.Add(workerlist);
                mysql.SaveChanges();
                MessageBox.Show("上传成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
               
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("上传失败", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Bandcheck_Load(object sender, EventArgs e)
        {
            this.Location = new Point(FangpuTerminal.Location.X + (FangpuTerminal.Size.Width - this.Size.Width) / 2, FangpuTerminal.Location.Y);
        }
    }
}
