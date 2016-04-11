using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fangpu_terminal
{
    public partial class Passwordinput : Form
    {
        FangpuTerminal fangputerminal;
        public static int iOperCount;
        public Passwordinput(FangpuTerminal mainterminal)
        {
            InitializeComponent();
            fangputerminal = mainterminal;    

        }
        //点击输入密码框则打开键盘，键入密码
        private void accept_Click(object sender, EventArgs e)
        {
            //if(pswd.Text=="123")
            //{
            //    fangputerminal.Enabled = true;
            //    fangputerminal.unlockScreen();
            //    fangputerminal.timer1.Enabled = true;
            //    this.Close();
            //    Process[] thisproc = Process.GetProcessesByName("tabtip");
            //    for (int i = 0; i < thisproc.Length; i++)
            //    {
            //        thisproc[i].Kill();
            //    }
                
            //}
        }
        private void clear_Click(object sender, EventArgs e)
        {    
            pswd.Text = "";
        }
        private void pswd_Click(object sender, EventArgs e)
        {
            Process[] thisproc = Process.GetProcessesByName("tabtip");
            if(thisproc.Length==0)
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            //iOperCount++;
            //if (iOperCount > 5)
            //{
            //    timer1.Enabled = false;
            //    this.Close();
            //    FangpuTerminal.mainenable = false;
            //}
        }

    }
}
