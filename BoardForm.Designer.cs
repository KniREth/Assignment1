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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BoardForm));
            menuStrip1 = new MenuStrip();
            gameToolStripMenuItem = new ToolStripMenuItem();
            newGameToolStripMenuItem = new ToolStripMenuItem();
            saveGameToolStripMenuItem = new ToolStripMenuItem();
            overwriteSaveToolStripMenuItem = new ToolStripMenuItem();
            loadGameToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            speakToolStripMenuItem = new ToolStripMenuItem();
            informationPanelToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            txtBoxP1Name = new TextBox();
            txtBoxP2Name = new TextBox();
            lblP1Val = new Label();
            lblP2Val = new Label();
            pictureBox1 = new PictureBox();
            picBoxP1Token = new PictureBox();
            picBoxP2Token = new PictureBox();
            picBoxPlayerToMove = new PictureBox();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxP1Token).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxP2Token).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoxPlayerToMove).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { gameToolStripMenuItem, settingsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(5, 2, 0, 2);
            menuStrip1.Size = new Size(677, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            gameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newGameToolStripMenuItem, saveGameToolStripMenuItem, overwriteSaveToolStripMenuItem, loadGameToolStripMenuItem, exitToolStripMenuItem });
            gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            gameToolStripMenuItem.Size = new Size(50, 20);
            gameToolStripMenuItem.Text = "Game";
            // 
            // newGameToolStripMenuItem
            // 
            newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            newGameToolStripMenuItem.Size = new Size(152, 22);
            newGameToolStripMenuItem.Text = "New Game";
            newGameToolStripMenuItem.Click += NewGameToolStripMenuItem_Click;
            // 
            // saveGameToolStripMenuItem
            // 
            saveGameToolStripMenuItem.Name = "saveGameToolStripMenuItem";
            saveGameToolStripMenuItem.Size = new Size(152, 22);
            saveGameToolStripMenuItem.Text = "Save Game";
            saveGameToolStripMenuItem.Click += SaveGameToolStripMenuItem_Click;
            // 
            // overwriteSaveToolStripMenuItem
            // 
            overwriteSaveToolStripMenuItem.Name = "overwriteSaveToolStripMenuItem";
            overwriteSaveToolStripMenuItem.Size = new Size(152, 22);
            overwriteSaveToolStripMenuItem.Text = "Overwrite Save";
            // 
            // loadGameToolStripMenuItem
            // 
            loadGameToolStripMenuItem.Name = "loadGameToolStripMenuItem";
            loadGameToolStripMenuItem.Size = new Size(152, 22);
            loadGameToolStripMenuItem.Text = "Restore Game";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(152, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { speakToolStripMenuItem, informationPanelToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // speakToolStripMenuItem
            // 
            speakToolStripMenuItem.CheckOnClick = true;
            speakToolStripMenuItem.Name = "speakToolStripMenuItem";
            speakToolStripMenuItem.Size = new Size(169, 22);
            speakToolStripMenuItem.Text = "Speak";
            speakToolStripMenuItem.CheckedChanged += SpeakToolStripMenuItem_CheckedChanged;
            // 
            // informationPanelToolStripMenuItem
            // 
            informationPanelToolStripMenuItem.Checked = true;
            informationPanelToolStripMenuItem.CheckOnClick = true;
            informationPanelToolStripMenuItem.CheckState = CheckState.Checked;
            informationPanelToolStripMenuItem.Name = "informationPanelToolStripMenuItem";
            informationPanelToolStripMenuItem.Size = new Size(169, 22);
            informationPanelToolStripMenuItem.Text = "Information Panel";
            informationPanelToolStripMenuItem.CheckedChanged += InformationPanelToolStripMenuItem_CheckedChanged;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(107, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
            // 
            // txtBoxP1Name
            // 
            txtBoxP1Name.Location = new Point(137, 536);
            txtBoxP1Name.Margin = new Padding(3, 2, 3, 2);
            txtBoxP1Name.Name = "txtBoxP1Name";
            txtBoxP1Name.PlaceholderText = "Enter name";
            txtBoxP1Name.Size = new Size(110, 23);
            txtBoxP1Name.TabIndex = 1;
            txtBoxP1Name.TextChanged += TxtBoxP1Name_TextChanged;
            // 
            // txtBoxP2Name
            // 
            txtBoxP2Name.Location = new Point(541, 536);
            txtBoxP2Name.Margin = new Padding(3, 2, 3, 2);
            txtBoxP2Name.Name = "txtBoxP2Name";
            txtBoxP2Name.PlaceholderText = "Enter name";
            txtBoxP2Name.Size = new Size(110, 23);
            txtBoxP2Name.TabIndex = 2;
            txtBoxP2Name.TextChanged += TxtBoxP2Name_TextChanged;
            // 
            // lblP1Val
            // 
            lblP1Val.AutoSize = true;
            lblP1Val.BackColor = Color.FromArgb(255, 192, 192);
            lblP1Val.Font = new Font("Segoe UI", 20F, FontStyle.Regular, GraphicsUnit.Point);
            lblP1Val.Location = new Point(15, 524);
            lblP1Val.Name = "lblP1Val";
            lblP1Val.Size = new Size(51, 37);
            lblP1Val.TabIndex = 3;
            lblP1Val.Text = "2 x";
            // 
            // lblP2Val
            // 
            lblP2Val.AutoSize = true;
            lblP2Val.BackColor = Color.FromArgb(255, 192, 192);
            lblP2Val.Font = new Font("Segoe UI", 20F, FontStyle.Regular, GraphicsUnit.Point);
            lblP2Val.Location = new Point(420, 524);
            lblP2Val.Name = "lblP2Val";
            lblP2Val.Size = new Size(51, 37);
            lblP2Val.TabIndex = 4;
            lblP2Val.Text = "2 x";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(255, 192, 192);
            pictureBox1.Location = new Point(0, 517);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(693, 64);
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // picBoxP1Token
            // 
            picBoxP1Token.Location = new Point(83, 528);
            picBoxP1Token.Margin = new Padding(3, 2, 3, 2);
            picBoxP1Token.Name = "picBoxP1Token";
            picBoxP1Token.Size = new Size(35, 30);
            picBoxP1Token.TabIndex = 6;
            picBoxP1Token.TabStop = false;
            // 
            // picBoxP2Token
            // 
            picBoxP2Token.BackColor = Color.Black;
            picBoxP2Token.Location = new Point(493, 528);
            picBoxP2Token.Margin = new Padding(3, 2, 3, 2);
            picBoxP2Token.Name = "picBoxP2Token";
            picBoxP2Token.Size = new Size(35, 30);
            picBoxP2Token.TabIndex = 7;
            picBoxP2Token.TabStop = false;
            // 
            // picBoxPlayerToMove
            // 
            picBoxPlayerToMove.Image = Properties.Resources.left;
            picBoxPlayerToMove.Location = new Point(276, 528);
            picBoxPlayerToMove.Margin = new Padding(3, 2, 3, 2);
            picBoxPlayerToMove.Name = "picBoxPlayerToMove";
            picBoxPlayerToMove.Size = new Size(127, 46);
            picBoxPlayerToMove.TabIndex = 8;
            picBoxPlayerToMove.TabStop = false;
            // 
            // BoardForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 576);
            Controls.Add(picBoxPlayerToMove);
            Controls.Add(picBoxP2Token);
            Controls.Add(picBoxP1Token);
            Controls.Add(lblP2Val);
            Controls.Add(lblP1Val);
            Controls.Add(txtBoxP2Name);
            Controls.Add(txtBoxP1Name);
            Controls.Add(menuStrip1);
            Controls.Add(pictureBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MaximumSize = new Size(702, 615);
            MinimizeBox = false;
            MinimumSize = new Size(702, 615);
            Name = "BoardForm";
            Padding = new Padding(0, 0, 9, 0);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "O'Neillo Game";
            FormClosing += BoardForm_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxP1Token).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxP2Token).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoxPlayerToMove).EndInit();
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
        private PictureBox picBoxP1Token;
        private PictureBox picBoxP2Token;
        private PictureBox picBoxPlayerToMove;
        private ToolStripMenuItem newGameToolStripMenuItem;
        private ToolStripMenuItem saveGameToolStripMenuItem;
        private ToolStripMenuItem loadGameToolStripMenuItem;
        private ToolStripMenuItem speakToolStripMenuItem;
        private ToolStripMenuItem informationPanelToolStripMenuItem;
        private ToolStripMenuItem overwriteSaveToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
    }
}