namespace fangpu_terminal
{
    partial class OutputForm
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
            this.dataout_dateTimePickerstart = new System.Windows.Forms.DateTimePicker();
            this.dataout_dateTimePickerend = new System.Windows.Forms.DateTimePicker();
            this.button_accept = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label_info = new System.Windows.Forms.Label();
            this.progressBarControl1 = new DevExpress.XtraEditors.ProgressBarControl();
            this.comboBox_type = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // dataout_dateTimePickerstart
            // 
            this.dataout_dateTimePickerstart.CustomFormat = "yyyy/MM/dd HH:mm:ss";
            this.dataout_dateTimePickerstart.Font = new System.Drawing.Font("宋体", 12F);
            this.dataout_dateTimePickerstart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dataout_dateTimePickerstart.Location = new System.Drawing.Point(27, 24);
            this.dataout_dateTimePickerstart.Name = "dataout_dateTimePickerstart";
            this.dataout_dateTimePickerstart.Size = new System.Drawing.Size(195, 26);
            this.dataout_dateTimePickerstart.TabIndex = 0;
            // 
            // dataout_dateTimePickerend
            // 
            this.dataout_dateTimePickerend.CustomFormat = "yyyy/MM/dd HH:mm:ss";
            this.dataout_dateTimePickerend.Font = new System.Drawing.Font("宋体", 12F);
            this.dataout_dateTimePickerend.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dataout_dateTimePickerend.Location = new System.Drawing.Point(270, 24);
            this.dataout_dateTimePickerend.Name = "dataout_dateTimePickerend";
            this.dataout_dateTimePickerend.Size = new System.Drawing.Size(192, 26);
            this.dataout_dateTimePickerend.TabIndex = 1;
            // 
            // button_accept
            // 
            this.button_accept.Enabled = false;
            this.button_accept.Font = new System.Drawing.Font("宋体", 12F);
            this.button_accept.Location = new System.Drawing.Point(56, 161);
            this.button_accept.Name = "button_accept";
            this.button_accept.Size = new System.Drawing.Size(156, 89);
            this.button_accept.TabIndex = 2;
            this.button_accept.Text = "导出";
            this.button_accept.UseVisualStyleBackColor = true;
            this.button_accept.Click += new System.EventHandler(this.button_accept_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Font = new System.Drawing.Font("宋体", 12F);
            this.button_cancel.Location = new System.Drawing.Point(270, 161);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(156, 89);
            this.button_cancel.TabIndex = 3;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label_info
            // 
            this.label_info.Font = new System.Drawing.Font("宋体", 12F);
            this.label_info.ForeColor = System.Drawing.Color.Red;
            this.label_info.Location = new System.Drawing.Point(87, 93);
            this.label_info.Name = "label_info";
            this.label_info.Size = new System.Drawing.Size(319, 37);
            this.label_info.TabIndex = 4;
            this.label_info.Text = "U盘已卸载！";
            this.label_info.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBarControl1
            // 
            this.progressBarControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.progressBarControl1.Location = new System.Drawing.Point(139, 133);
            this.progressBarControl1.Name = "progressBarControl1";
            this.progressBarControl1.Properties.Step = 33;
            this.progressBarControl1.Size = new System.Drawing.Size(205, 18);
            this.progressBarControl1.TabIndex = 5;
            // 
            // comboBox_type
            // 
            this.comboBox_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_type.Font = new System.Drawing.Font("宋体", 12F);
            this.comboBox_type.FormattingEnabled = true;
            this.comboBox_type.Items.AddRange(new object[] {
            "报警信息",
            "历史数据"});
            this.comboBox_type.Location = new System.Drawing.Point(182, 66);
            this.comboBox_type.Name = "comboBox_type";
            this.comboBox_type.Size = new System.Drawing.Size(121, 24);
            this.comboBox_type.TabIndex = 6;
            // 
            // OutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 262);
            this.Controls.Add(this.comboBox_type);
            this.Controls.Add(this.progressBarControl1);
            this.Controls.Add(this.label_info);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_accept);
            this.Controls.Add(this.dataout_dateTimePickerend);
            this.Controls.Add(this.dataout_dateTimePickerstart);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "OutputForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_cancel;
        public System.Windows.Forms.Label label_info;
        public System.Windows.Forms.DateTimePicker dataout_dateTimePickerstart;
        public System.Windows.Forms.DateTimePicker dataout_dateTimePickerend;
        public DevExpress.XtraEditors.ProgressBarControl progressBarControl1;
        public System.Windows.Forms.ComboBox comboBox_type;
        public System.Windows.Forms.Button button_accept;
    }
}