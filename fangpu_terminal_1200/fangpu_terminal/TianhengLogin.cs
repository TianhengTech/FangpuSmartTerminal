using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraSplashScreen;

namespace fangpu_terminal
{
    public partial class TianhengLogin : SplashScreen
    {
        public TianhengLogin()
        {
            InitializeComponent();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            //base.ProcessCommand(cmd, arg);
            base.ProcessCommand(cmd, arg);
            SplashScreenCommand command = (SplashScreenCommand)cmd;
            if (command == SplashScreenCommand.Setinfo)
            {
                TianhengLoginInfo pos = (TianhengLoginInfo)arg;
                ProgressBarControl1.Position = pos.Pos;
                labelControl2.Text = pos.LabelText;
            }
        }

        #endregion

        public enum SplashScreenCommand
        {
            Setinfo
        }
    }

    public class TianhengLoginInfo
    {
        //滚动条的位置信息
        public int Pos
        {
            get;
            set;
        }
        //滚动条上对应的文字信息
        public string LabelText
        {
            get;
            set;
        }
    }
}