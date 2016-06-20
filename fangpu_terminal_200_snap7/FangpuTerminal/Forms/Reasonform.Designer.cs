namespace fangpu_terminal
{
    partial class Reasonform
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_accept = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkedListBox_reason = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // button_accept
            // 
            this.button_accept.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_accept.Location = new System.Drawing.Point(371, 433);
            this.button_accept.Name = "button_accept";
            this.button_accept.Size = new System.Drawing.Size(84, 33);
            this.button_accept.TabIndex = 0;
            this.button_accept.Text = "确认上传";
            this.button_accept.UseVisualStyleBackColor = true;
            this.button_accept.Click += new System.EventHandler(this.button_accept_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_cancel.Location = new System.Drawing.Point(475, 433);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(78, 33);
            this.button_cancel.TabIndex = 1;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("华文楷体", 14F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.label1.Location = new System.Drawing.Point(215, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "请选择原因";
            // 
            // checkedListBox_reason
            // 
            this.checkedListBox_reason.BackColor = System.Drawing.Color.YellowGreen;
            this.checkedListBox_reason.CheckOnClick = true;
            this.checkedListBox_reason.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Bold);
            this.checkedListBox_reason.FormattingEnabled = true;
            this.checkedListBox_reason.Items.AddRange(new object[] {
            "烧焦",
            "油泡",
            "横纹",
            "麻点",
            "炉温不正常",
            "不熟",
            "凹口",
            "料浓",
            "熟料",
            "没有对应工艺",
            "薄膜口",
            "其他"});
            this.checkedListBox_reason.Location = new System.Drawing.Point(39, 49);
            this.checkedListBox_reason.Name = "checkedListBox_reason";
            this.checkedListBox_reason.Size = new System.Drawing.Size(478, 364);
            this.checkedListBox_reason.TabIndex = 4;
            // 
            // Reasonform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(565, 478);
            this.Controls.Add(this.checkedListBox_reason);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_accept);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Reasonform";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reasonform";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_accept;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox checkedListBox_reason;
    }
}