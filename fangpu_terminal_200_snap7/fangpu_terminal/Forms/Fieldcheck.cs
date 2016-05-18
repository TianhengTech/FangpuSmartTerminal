using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using fangpu_terminal.Ultility.Nhibernate;

namespace fangpu_terminal
{
    public partial class Fieldcheck : Form
    {
        FangpuTerminal FangpuTerminal;
        public Fieldcheck(FangpuTerminal Terminal)
        {
            InitializeComponent();
            FangpuTerminal=Terminal;
            if (FangpuTerminal.typeselect.SelectedItem!=null)
                guige.Text = FangpuTerminal.typeselect.SelectedItem.ToString();
            dateTimePicker1.Value = DateTime.Now;
            shijian.Text = DateTime.Now.ToString();
        }

        private void accept_Click(object sender, EventArgs e)
        {
            try
            {
                FangpuTerminal.HideInputPanel();
                using (var mysql = FluentNhibernateHelper.GetSession())
                {
                    var workerlist = new fieldchecklist();
                    workerlist.deviceid = Properties.TerminalParameters.Default.terminal_name;
                    workerlist.sn = biaodanbianhao.Text;
                    workerlist.producer = shengchanzhe.Text;
                    workerlist.type = guige.Text;
                    workerlist.datetime = shijian.Text;
                    workerlist.material = cailiao.Text;
                    if (danzhong.Text.Trim() == String.Empty)
                        workerlist.singleweight = null;
                    else
                        workerlist.singleweight = Convert.ToDouble(danzhong.Text);
                    if (changdubiaozhun.Text.Trim() == String.Empty)
                        workerlist.standardlength = null;
                    else
                        workerlist.standardlength = Convert.ToDouble(changdubiaozhun.Text);
                    if (changdushice.Text.Trim() == String.Empty)
                        workerlist.actuallength = null;
                    else
                        workerlist.actuallength = Convert.ToDouble(changdushice.Text);
                    if (houdubiaozhun.Text.Trim() == String.Empty)
                        workerlist.standardthickness = null;
                    else
                        workerlist.standardthickness = Convert.ToDouble(houdubiaozhun.Text);
                    if (houdushice.Text.Trim() == String.Empty)
                        workerlist.actualthickness = null;
                    else
                        workerlist.actualthickness = Convert.ToDouble(houdushice.Text);
                    workerlist.burn = shaojiao.Checked;
                    workerlist.bubble = qipao.Checked;
                    workerlist.irregular = aotu.Checked;
                    workerlist.impurity = zazhi.Checked;
                    workerlist.deformation = bianxing.Checked;
                    workerlist.injure = tuoshang.Checked;
                    workerlist.raw = bushu.Checked;
                    workerlist.band = hengwen.Checked;
                    workerlist.spot = madian.Checked;
                    workerlist.internalfail = neibiaomianhua.Checked;
                    workerlist.@double = shuangceng.Checked;
                    workerlist.judge = shengchanpanduan.Checked;
                    workerlist.handle = chulifangshi.Text;
                    workerlist.monitorcheck = banzhangqueren.Text;
                    workerlist.name = jianyan.Text;
                    workerlist.datetime2 = dateTimePicker1.Value;
                    workerlist.confirm = queren.Text;
                    mysql.Save(workerlist);
                    mysql.Flush();
                    MessageBox.Show("上传成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);        
                }
               
            }
            catch(Exception ex)
            {
                TerminalLogWriter.WriteErroLog(typeof(Fieldcheck), "现场点检表上传失败", ex);
                MessageBox.Show("上传失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            FangpuTerminal.HideInputPanel();
            this.Close();
        }

        private void Fieldcheck_Load(object sender, EventArgs e)
        {
            FangpuTerminal.fieldcheck_open = true;
            this.Location = new Point(FangpuTerminal.Location.X + (FangpuTerminal.Size.Width - this.Size.Width) / 2, FangpuTerminal.Location.Y);
        }

        private void Fieldcheck_FormClosed(object sender, FormClosedEventArgs e)
        {
            FangpuTerminal.fieldcheck_open = false;
        }
    }
}
