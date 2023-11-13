namespace Assignment1
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            lblTitleName = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            btnOk = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // lblTitleName
            // 
            lblTitleName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTitleName.AutoSize = true;
            lblTitleName.Font = new Font("Segoe UI", 19F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitleName.Location = new Point(419, 37);
            lblTitleName.Name = "lblTitleName";
            lblTitleName.Size = new Size(151, 90);
            lblTitleName.TabIndex = 0;
            lblTitleName.Text = "O'Neillo \r\nGame";
            lblTitleName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BorderStyle = BorderStyle.Fixed3D;
            label1.Font = new Font("Segoe UI", 7.2F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(373, 166);
            label1.MaximumSize = new Size(250, 180);
            label1.Name = "label1";
            label1.Size = new Size(244, 152);
            label1.TabIndex = 2;
            label1.Text = resources.GetString("label1.Text");
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(33, 39);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(300, 279);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // btnOk
            // 
            btnOk.Location = new Point(303, 353);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(94, 29);
            btnOk.TabIndex = 4;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += Button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(682, 403);
            Controls.Add(btnOk);
            Controls.Add(pictureBox1);
            Controls.Add(label1);
            Controls.Add(lblTitleName);
            MaximizeBox = false;
            MaximumSize = new Size(700, 450);
            MinimizeBox = false;
            MinimumSize = new Size(700, 450);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitleName;
        private Label label1;
        private PictureBox pictureBox1;
        private Button btnOk;
    }
}