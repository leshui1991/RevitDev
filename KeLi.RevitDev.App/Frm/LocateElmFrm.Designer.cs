namespace KeLi.RevitDev.App.Frm
{
    partial class LocateElmFrm
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
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbId3 = new System.Windows.Forms.TextBox();
            this.tbId5 = new System.Windows.Forms.TextBox();
            this.tbId4 = new System.Windows.Forms.TextBox();
            this.tbId1 = new System.Windows.Forms.TextBox();
            this.tbId2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLocate = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(142, 206);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(52, 23);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbId3);
            this.groupBox1.Controls.Add(this.tbId5);
            this.groupBox1.Controls.Add(this.tbId4);
            this.groupBox1.Controls.Add(this.tbId1);
            this.groupBox1.Controls.Add(this.tbId2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(207, 169);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Element Info";
            // 
            // tbId3
            // 
            this.tbId3.Location = new System.Drawing.Point(89, 77);
            this.tbId3.Name = "tbId3";
            this.tbId3.Size = new System.Drawing.Size(96, 21);
            this.tbId3.TabIndex = 6;
            // 
            // tbId5
            // 
            this.tbId5.Location = new System.Drawing.Point(89, 129);
            this.tbId5.Name = "tbId5";
            this.tbId5.Size = new System.Drawing.Size(96, 21);
            this.tbId5.TabIndex = 10;
            // 
            // tbId4
            // 
            this.tbId4.Location = new System.Drawing.Point(89, 103);
            this.tbId4.Name = "tbId4";
            this.tbId4.Size = new System.Drawing.Size(96, 21);
            this.tbId4.TabIndex = 8;
            // 
            // tbId1
            // 
            this.tbId1.Location = new System.Drawing.Point(89, 25);
            this.tbId1.Name = "tbId1";
            this.tbId1.Size = new System.Drawing.Size(96, 21);
            this.tbId1.TabIndex = 4;
            // 
            // tbId2
            // 
            this.tbId2.Location = new System.Drawing.Point(89, 51);
            this.tbId2.Name = "tbId2";
            this.tbId2.Size = new System.Drawing.Size(96, 21);
            this.tbId2.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "Element3 Id: ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "Element5 Id: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "Element4 Id:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Element2 Id: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Element1 Id:";
            // 
            // btnLocate
            // 
            this.btnLocate.Location = new System.Drawing.Point(75, 206);
            this.btnLocate.Name = "btnLocate";
            this.btnLocate.Size = new System.Drawing.Size(52, 23);
            this.btnLocate.TabIndex = 7;
            this.btnLocate.Text = "Locate";
            this.btnLocate.UseVisualStyleBackColor = true;
            this.btnLocate.Click += new System.EventHandler(this.BtnLocate_Click);
            // 
            // LocateElmFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 241);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnLocate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LocateElmFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Element Tool";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbId5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbId4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbId3;
        private System.Windows.Forms.TextBox tbId1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbId2;
        private System.Windows.Forms.Button btnLocate;
    }
}