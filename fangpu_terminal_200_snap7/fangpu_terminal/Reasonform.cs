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
    public partial class Reasonform : Form
    {
       public static string upload_reason = "";

       public Reasonform()
       {
           InitializeComponent();
           upload_reason = "";
       }
            

        private void button_accept_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            foreach (string item in checkedListBox_reason.CheckedItems)
            {
                if (upload_reason.Equals(""))
                {
                    upload_reason = item;
                }
                else
                    upload_reason = upload_reason + "," + item;
            }
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            upload_reason = "";
            this.Close();
        }




    }
}
