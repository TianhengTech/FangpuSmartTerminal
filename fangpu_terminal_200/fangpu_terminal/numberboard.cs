using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace fangpu_terminal
{
    
    public partial class numberboard : Form
    {
        FangpuTerminal FangpuTerminal;
        //设置为不获取焦点的浮动窗口
        public numberboard(FangpuTerminal Terminal)
        {
            InitializeComponent();
            FangpuTerminal = Terminal;
            SetControlStyleMethod = typeof(Button).GetMethod("SetStyle",
              BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            SetChildControlNoFocus(this);
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.Selectable, false);
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            SetBtnStyle(button_backspace);
            SetBtnStyle(button_cancel);
            SetBtnStyle(button0);
            SetBtnStyle(button1);
            SetBtnStyle(button2);
            SetBtnStyle(button3);
            SetBtnStyle(button4);
            SetBtnStyle(button5);
            SetBtnStyle(button6);
            SetBtnStyle(button7);
            SetBtnStyle(button8);
            SetBtnStyle(button9);
            SetBtnStyle(button_del);
            SetBtnStyle(button_enter);
            SetBtnStyle(button_CE);           
        }

        private const int WM_MOUSEACTIVATE = 0x21;
        private const int MA_NOACTIVATE = 3;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = new IntPtr(MA_NOACTIVATE);
                return;
            }
            base.WndProc(ref m);
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return false;
            }
        }
        void SetChildControlNoFocus(Control ctrl)
        {
            if (ctrl.HasChildren)
                foreach (Button c in ctrl.Controls)
                {
                    SetControlNoFocus(c);
                }
        }

        MethodInfo SetControlStyleMethod;
        object[] SetControlStyleArgs = new object[] { ControlStyles.Selectable, false };
        private void SetControlNoFocus(Button ctrl)
        {
            SetControlStyleMethod.Invoke(ctrl, SetControlStyleArgs);
            SetChildControlNoFocus(ctrl);
        }

        //让按钮透明
        private void SetBtnStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;//样式  
            btn.ForeColor = Color.Transparent;//前景  
            btn.BackColor = Color.Transparent;//去背景  
            btn.FlatAppearance.BorderSize = 0;//去边线  
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标经过  
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;//鼠标按下 
            btn.Text = "";
        }  
        private void button1_Click(object sender, EventArgs e)
        {
            SendKeys.Send("1");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendKeys.Send("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendKeys.Send("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendKeys.Send("4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendKeys.Send("5");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SendKeys.Send("6");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SendKeys.Send("7");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SendKeys.Send("8");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SendKeys.Send("9");
        }

        private void button_backspace_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{BACKSPACE}");
        }

        private void button0_Click(object sender, EventArgs e)
        {
            SendKeys.Send("0");
        }

        private void button_del_Click(object sender, EventArgs e)
        {
            SendKeys.Send(".");
        }

        private void button_CE_Click(object sender, EventArgs e)
        {
            if (FangpuTerminal.buzuoguan.Focused==true)
            FangpuTerminal.buzuoguan.Text = "";
            if (FangpuTerminal.jinliaoshezhi.Focused == true)
            FangpuTerminal.jinliaoshezhi.Text = "";
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
            FangpuTerminal.yincang.Focus();
            button_CE.PerformClick();
        }

        private void button_enter_Click(object sender, EventArgs e)
        {
            SendKeys.Send("{ENTER}");
        }

        private void numberboard_Load(object sender, EventArgs e)
        {
            this.Location = new Point(FangpuTerminal.Location.X+200, FangpuTerminal.Location.Y+100);
        }
    }
}
