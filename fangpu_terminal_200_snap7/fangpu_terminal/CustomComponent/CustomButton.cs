using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CustomGUI.Forms
{
    public class CustomButton : System.Windows.Forms.Button
    {
        public delegate void ButtonTouchDownHandler(object sender,EventArgs e);
        public event ButtonTouchDownHandler ButtonTouchDownEvent;
        public delegate void ButtonTouchUpHandler(object sender, EventArgs e);
        public event ButtonTouchUpHandler ButtonTouchUpEvent;
        public event Action<object, EventArgs> customerevent;
        
        public Color baseColor;
        bool _first = true;

        public CustomButton()
        {
            
        }
        protected override void WndProc(ref System.Windows.Forms.Message msg)
        {
            const int WM_POINTERDOWN = 0x0246;
            const int WM_POINTERUP = 0x247;
            const int WM_LBUTTONDOWN = 0x201;
            const int WM_LBUTTONUP = 0x202;
            const int WM_MOUSEMOVE = 0x200;
   
            switch (msg.Msg)
            {
                case WM_POINTERDOWN:
                    Debug.WriteLine("DOWN");
                    if (_first)
                    {
                        this.baseColor = this.BackColor;
                        _first = false;
                    }
                    this.BackColor = Color.Red;
                    this.ButtonTouchDownEvent(this, new EventArgs());                   
                    break;
                case WM_POINTERUP:
                    Debug.WriteLine("UP");
                    this.BackColor = this.baseColor;
                    this.ButtonTouchUpEvent(this, new EventArgs());
                    break;

            }
            base.WndProc(ref msg);
        }
    }
}
