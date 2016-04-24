namespace fangpu_terminal
{
    partial class stoptable
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
            this.label1 = new System.Windows.Forms.Label();
            this.checkedListBox_stopreason = new System.Windows.Forms.CheckedListBox();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 20F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Coral;
            this.label1.Location = new System.Drawing.Point(175, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "机台停机原因";
            // 
            // checkedListBox_stopreason
            // 
            this.checkedListBox_stopreason.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.checkedListBox_stopreason.CheckOnClick = true;
            this.checkedListBox_stopreason.Font = new System.Drawing.Font("宋体", 20F);
            this.checkedListBox_stopreason.FormattingEnabled = true;
            this.checkedListBox_stopreason.Items.AddRange(new object[] {
            "计划停机",
            "洗模",
            "故障",
            "材料问题",
            "模具问题",
            "工艺问题",
            "其他"});
            this.checkedListBox_stopreason.Location = new System.Drawing.Point(47, 34);
            this.checkedListBox_stopreason.Name = "checkedListBox_stopreason";
            this.checkedListBox_stopreason.Size = new System.Drawing.Size(424, 301);
            this.checkedListBox_stopreason.TabIndex = 1;
            // 
            // button_stop
            // 
            this.button_stop.BackColor = System.Drawing.SystemColors.Control;
            this.button_stop.Font = new System.Drawing.Font("宋体", 20F, System.Drawing.FontStyle.Bold);
            this.button_stop.ForeColor = System.Drawing.Color.Red;
            this.button_stop.Location = new System.Drawing.Point(180, 347);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(150, 53);
            this.button_stop.TabIndex = 5;
            this.button_stop.Text = "开始停机";
            this.button_stop.UseVisualStyleBackColor = false;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.BackColor = System.Drawing.SystemColors.Control;
            this.button_cancel.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold);
            this.button_cancel.ForeColor = System.Drawing.Color.Black;
            this.button_cancel.Location = new System.Drawing.Point(448, 382);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(69, 28);
            this.button_cancel.TabIndex = 8;
            this.button_cancel.Text = "返回";
            this.button_cancel.UseVisualStyleBackColor = false;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // stoptable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Olive;
            this.ClientSize = new System.Drawing.Size(520, 412);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.checkedListBox_stopreason);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "stoptable";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "stoptable";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.stoptable_FormClosed);
            this.Load += new System.EventHandler(this.stoptable_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.stoptable_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox checkedListBox_stopreason;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_cancel;
    }
}