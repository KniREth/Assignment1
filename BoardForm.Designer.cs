namespace Assignment1
{
    public partial class BoardForm : Form
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            gameToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            txtBoxP1Name = new TextBox();
            txtBoxP2Name = new TextBox();
            lblP1Val = new Label();
            lblP2Val = new Label();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { gameToolStripMenuItem, settingsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(782, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            gameToolStripMenuItem.Size = new Size(62, 24);
            gameToolStripMenuItem.Text = "Game";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(76, 24);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "Help";
            // 
            // txtBoxP1Name
            // 
            txtBoxP1Name.Location = new Point(205, 715);
            txtBoxP1Name.Name = "txtBoxP1Name";
            txtBoxP1Name.PlaceholderText = "Enter name";
            txtBoxP1Name.Size = new Size(125, 27);
            txtBoxP1Name.TabIndex = 1;
            txtBoxP1Name.TextChanged += textBox1_TextChanged;
            // 
            // txtBoxP2Name
            // 
            txtBoxP2Name.Location = new Point(595, 714);
            txtBoxP2Name.Name = "txtBoxP2Name";
            txtBoxP2Name.PlaceholderText = "Enter name";
            txtBoxP2Name.Size = new Size(125, 27);
            txtBoxP2Name.TabIndex = 2;
            txtBoxP2Name.TextChanged += txtBoxP2Name_TextChanged;
            // 
            // lblP1Val
            // 
            lblP1Val.AutoSize = true;
            lblP1Val.BackColor = Color.FromArgb(255, 192, 192);
            lblP1Val.Font = new Font("Segoe UI", 20F, FontStyle.Regular, GraphicsUnit.Point);
            lblP1Val.Location = new Point(26, 698);
            lblP1Val.Name = "lblP1Val";
            lblP1Val.Size = new Size(63, 46);
            lblP1Val.TabIndex = 3;
            lblP1Val.Text = "2 x";
            // 
            // lblP2Val
            // 
            lblP2Val.AutoSize = true;
            lblP2Val.BackColor = Color.FromArgb(255, 192, 192);
            lblP2Val.Font = new Font("Segoe UI", 20F, FontStyle.Regular, GraphicsUnit.Point);
            lblP2Val.Location = new Point(425, 698);
            lblP2Val.Name = "lblP2Val";
            lblP2Val.Size = new Size(63, 46);
            lblP2Val.TabIndex = 4;
            lblP2Val.Text = "2 x";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(255, 192, 192);
            pictureBox1.Location = new Point(0, 689);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(782, 71);
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new Point(121, 704);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(40, 40);
            pictureBox2.TabIndex = 6;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Black;
            pictureBox3.Location = new Point(520, 704);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(40, 40);
            pictureBox3.TabIndex = 7;
            pictureBox3.TabStop = false;
            // 
            // BoardForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 753);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(lblP2Val);
            Controls.Add(lblP1Val);
            Controls.Add(txtBoxP2Name);
            Controls.Add(txtBoxP1Name);
            Controls.Add(menuStrip1);
            Controls.Add(pictureBox1);
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            MaximumSize = new Size(800, 800);
            MinimizeBox = false;
            MinimumSize = new Size(800, 800);
            Name = "BoardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "O'Spookio";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem gameToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private TextBox txtBoxP1Name;
        private TextBox txtBoxP2Name;
        private Label lblP1Val;
        private Label lblP2Val;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
    }
}