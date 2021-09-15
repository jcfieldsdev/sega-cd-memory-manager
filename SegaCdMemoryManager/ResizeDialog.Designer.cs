
namespace SegaCdMemoryManager
{
    partial class ResizeDialog
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
            this.buttonRename = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton16 = new System.Windows.Forms.RadioButton();
            this.radioButton32 = new System.Windows.Forms.RadioButton();
            this.radioButton64 = new System.Windows.Forms.RadioButton();
            this.radioButton128 = new System.Windows.Forms.RadioButton();
            this.radioButton256 = new System.Windows.Forms.RadioButton();
            this.radioButton512 = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonRename
            // 
            this.buttonRename.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRename.Location = new System.Drawing.Point(176, 136);
            this.buttonRename.Name = "buttonRename";
            this.buttonRename.Size = new System.Drawing.Size(75, 23);
            this.buttonRename.TabIndex = 7;
            this.buttonRename.Text = "Resize";
            this.buttonRename.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(257, 136);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select a new size for the BRAM file:";
            // 
            // radioButton8
            // 
            this.radioButton8.AutoSize = true;
            this.radioButton8.Location = new System.Drawing.Point(3, 3);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(90, 17);
            this.radioButton8.TabIndex = 0;
            this.radioButton8.TabStop = true;
            this.radioButton8.Tag = "3";
            this.radioButton8.Text = "8 KB (64 Kbit)";
            this.radioButton8.UseVisualStyleBackColor = true;
            this.radioButton8.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // radioButton16
            // 
            this.radioButton16.AutoSize = true;
            this.radioButton16.Location = new System.Drawing.Point(3, 26);
            this.radioButton16.Name = "radioButton16";
            this.radioButton16.Size = new System.Drawing.Size(102, 17);
            this.radioButton16.TabIndex = 1;
            this.radioButton16.TabStop = true;
            this.radioButton16.Tag = "4";
            this.radioButton16.Text = "16 KB (128 Kbit)";
            this.radioButton16.UseVisualStyleBackColor = true;
            this.radioButton16.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // radioButton32
            // 
            this.radioButton32.AutoSize = true;
            this.radioButton32.Location = new System.Drawing.Point(3, 49);
            this.radioButton32.Name = "radioButton32";
            this.radioButton32.Size = new System.Drawing.Size(102, 17);
            this.radioButton32.TabIndex = 2;
            this.radioButton32.TabStop = true;
            this.radioButton32.Tag = "5";
            this.radioButton32.Text = "32 KB (256 Kbit)";
            this.radioButton32.UseVisualStyleBackColor = true;
            this.radioButton32.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // radioButton64
            // 
            this.radioButton64.AutoSize = true;
            this.radioButton64.Location = new System.Drawing.Point(3, 72);
            this.radioButton64.Name = "radioButton64";
            this.radioButton64.Size = new System.Drawing.Size(101, 17);
            this.radioButton64.TabIndex = 3;
            this.radioButton64.TabStop = true;
            this.radioButton64.Tag = "6";
            this.radioButton64.Text = "64 KB (512 KBit)";
            this.radioButton64.UseVisualStyleBackColor = true;
            this.radioButton64.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // radioButton128
            // 
            this.radioButton128.AutoSize = true;
            this.radioButton128.Location = new System.Drawing.Point(161, 3);
            this.radioButton128.Name = "radioButton128";
            this.radioButton128.Size = new System.Drawing.Size(117, 17);
            this.radioButton128.TabIndex = 4;
            this.radioButton128.TabStop = true;
            this.radioButton128.Tag = "7";
            this.radioButton128.Text = "128 KB (1,024 Kbit)";
            this.radioButton128.UseVisualStyleBackColor = true;
            this.radioButton128.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // radioButton256
            // 
            this.radioButton256.AutoSize = true;
            this.radioButton256.Location = new System.Drawing.Point(161, 26);
            this.radioButton256.Name = "radioButton256";
            this.radioButton256.Size = new System.Drawing.Size(117, 17);
            this.radioButton256.TabIndex = 5;
            this.radioButton256.TabStop = true;
            this.radioButton256.Tag = "8";
            this.radioButton256.Text = "256 KB (2,048 Kbit)";
            this.radioButton256.UseVisualStyleBackColor = true;
            this.radioButton256.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // radioButton512
            // 
            this.radioButton512.AutoSize = true;
            this.radioButton512.Location = new System.Drawing.Point(161, 49);
            this.radioButton512.Name = "radioButton512";
            this.radioButton512.Size = new System.Drawing.Size(117, 17);
            this.radioButton512.TabIndex = 6;
            this.radioButton512.TabStop = true;
            this.radioButton512.Tag = "9";
            this.radioButton512.Text = "512 KB (4,096 Kbit)";
            this.radioButton512.UseVisualStyleBackColor = true;
            this.radioButton512.CheckedChanged += new System.EventHandler(this.ChangeRadioButton);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.radioButton8, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioButton16, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.radioButton32, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.radioButton64, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.radioButton512, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.radioButton128, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioButton256, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 35);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(317, 95);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // ResizeDialog
            // 
            this.AcceptButton = this.buttonRename;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(345, 170);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonRename);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResizeDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resize File";
            this.Load += new System.EventHandler(this.ResizeDialog_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonRename;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton16;
        private System.Windows.Forms.RadioButton radioButton32;
        private System.Windows.Forms.RadioButton radioButton64;
        private System.Windows.Forms.RadioButton radioButton128;
        private System.Windows.Forms.RadioButton radioButton256;
        private System.Windows.Forms.RadioButton radioButton512;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}