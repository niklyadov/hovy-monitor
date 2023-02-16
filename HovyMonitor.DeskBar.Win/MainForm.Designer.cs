namespace HovyMonitor.DeskBar.Win
{
    partial class MainForm
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
            this.plotView1 = new OxyPlot.WindowsForms.PlotView();
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.sensorsLB = new System.Windows.Forms.ListBox();
            this.fetchBTN = new System.Windows.Forms.Button();
            this.lastDaysNUD = new System.Windows.Forms.NumericUpDown();
            this.maxForSelectedValue = new System.Windows.Forms.Label();
            this.minForSelectedValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.lastDaysNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // plotView1
            // 
            this.plotView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plotView1.Location = new System.Drawing.Point(0, 0);
            this.plotView1.Name = "plotView1";
            this.plotView1.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotView1.Size = new System.Drawing.Size(785, 420);
            this.plotView1.TabIndex = 0;
            this.plotView1.Text = "plotView1";
            this.plotView1.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotView1.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotView1.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.Location = new System.Drawing.Point(12, 426);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.Size = new System.Drawing.Size(298, 29);
            this.dateTimePicker.TabIndex = 1;
            this.dateTimePicker.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // sensorsLB
            // 
            this.sensorsLB.AccessibleName = "";
            this.sensorsLB.FormattingEnabled = true;
            this.sensorsLB.ItemHeight = 21;
            this.sensorsLB.Location = new System.Drawing.Point(12, 461);
            this.sensorsLB.Name = "sensorsLB";
            this.sensorsLB.Size = new System.Drawing.Size(206, 88);
            this.sensorsLB.TabIndex = 2;
            this.sensorsLB.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // fetchBTN
            // 
            this.fetchBTN.Location = new System.Drawing.Point(222, 461);
            this.fetchBTN.Name = "fetchBTN";
            this.fetchBTN.Size = new System.Drawing.Size(154, 32);
            this.fetchBTN.TabIndex = 3;
            this.fetchBTN.Text = "Fetch";
            this.fetchBTN.UseVisualStyleBackColor = true;
            this.fetchBTN.Click += new System.EventHandler(this.button1_Click);
            // 
            // lastDaysNUD
            // 
            this.lastDaysNUD.Location = new System.Drawing.Point(316, 426);
            this.lastDaysNUD.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
            this.lastDaysNUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lastDaysNUD.Name = "lastDaysNUD";
            this.lastDaysNUD.Size = new System.Drawing.Size(60, 29);
            this.lastDaysNUD.TabIndex = 4;
            this.lastDaysNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // maxForSelectedValue
            // 
            this.maxForSelectedValue.AutoSize = true;
            this.maxForSelectedValue.Location = new System.Drawing.Point(227, 504);
            this.maxForSelectedValue.Name = "maxForSelectedValue";
            this.maxForSelectedValue.Size = new System.Drawing.Size(0, 21);
            this.maxForSelectedValue.TabIndex = 5;
            // 
            // minForSelectedValue
            // 
            this.minForSelectedValue.AutoSize = true;
            this.minForSelectedValue.Location = new System.Drawing.Point(227, 525);
            this.minForSelectedValue.Name = "minForSelectedValue";
            this.minForSelectedValue.Size = new System.Drawing.Size(0, 21);
            this.minForSelectedValue.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.minForSelectedValue);
            this.Controls.Add(this.maxForSelectedValue);
            this.Controls.Add(this.lastDaysNUD);
            this.Controls.Add(this.fetchBTN);
            this.Controls.Add(this.sensorsLB);
            this.Controls.Add(this.dateTimePicker);
            this.Controls.Add(this.plotView1);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "HovyMonitor.Win";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lastDaysNUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OxyPlot.WindowsForms.PlotView plotView1;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.ListBox sensorsLB;
        private System.Windows.Forms.Button fetchBTN;
        private System.Windows.Forms.NumericUpDown lastDaysNUD;
        private System.Windows.Forms.Label maxForSelectedValue;
        private System.Windows.Forms.Label minForSelectedValue;
    }
}