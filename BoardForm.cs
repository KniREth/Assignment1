using GameboardGUI;
using Microsoft.VisualBasic.Devices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.ComponentModel;

namespace Assignment1
{
    /// <summary>
    ///         This class represents the form which is displayed to the player. It acts as a "middle-man"
    ///         between the game logic class and the gameboard image array class as an act of integrity.
    ///         The board form will be able to get and set values which can be displayed onto the screen as
    ///         well as listen for events from the user such as menu buttons being clicked or game tiles.
    /// </summary>
    public partial class BoardForm : Form
    {
        #region Variable intialisation

        // Initialise bool for player entering name into txt box
        bool p1NameEntered = false;
        bool p2NameEntered = false;

        // Initialise the number of rows and columns for the board
        const int numRows = 8;
        const int numCols = 8;

        // boolean to check whether game has been saved when user tries to leave game
        internal bool isGameSaved = false;

        // Initialise an array of pic boxes for board
        readonly GameboardImageArray? gameGUIData;
        internal int[,] gameValueData;
        readonly string tileImagesDirPath = Directory.GetCurrentDirectory() + @"\images\";

        // New game logic class
        readonly GameLogic? gameLogic;

        // Initialise speech synthesis and the string array of voices for use by different players
        internal readonly SpeechSynthesizer? speechSynth;
        internal readonly string[]? voices;



        public delegate void MenuItemClickedEventDelegate(object sender, EventArgs e);
        public event MenuItemClickedEventDelegate? SaveButtonClicked;
        public event MenuItemClickedEventDelegate? LoadButtonClicked;
        public event MenuItemClickedEventDelegate? OverwriteButtonClicked;
        public event MenuItemClickedEventDelegate? NewGameButtonClicked;
        public event MenuItemClickedEventDelegate? GameboardTileClicked;

        #endregion

        #region Constructor

        // Constructor for the class
        public BoardForm()
        {
            InitializeComponent();

            // Initialise the corners of the board for drawing its position
            Point topLeftCorner = new(50, 30);
            Point bottomRightCorner = new(50, 65);

            // Initialise the board and get the current data of each tile
            gameValueData = InitialiseBoard();

            // Try to load the gameboard image array with the data and position 
            try
            {
                gameGUIData = new GameboardImageArray(this, gameValueData, topLeftCorner, bottomRightCorner, 0, tileImagesDirPath);
                gameGUIData.TileClicked += new GameboardImageArray.TileClickedEventDelegate(GameTileClicked);
                gameGUIData.UpdateBoardGui(gameValueData);
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Game board size problem", MessageBoxButtons.OK);
                this.Close();
            }

            // Try to load the game's logic 
            try
            {
                gameLogic = new GameLogic(gameGUIData!, this);
            }
            catch(Exception ex) 
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Cannot load game logic", MessageBoxButtons.OK);
                this.Close();
            }

            // Try to fetch the save game file
            try
            {
                gameLogic!.GetSaveGames();
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Cannot fetch save games");
                this.Close();
            }

            // Try to load the speech synthesizer for text to speech
            try
            {
                speechSynth = new SpeechSynthesizer();
                speechSynth.SetOutputToDefaultAudioDevice();
                voices = speechSynth.GetInstalledVoices().Where(v => v.Enabled).Select(v => v.VoiceInfo.Name).ToArray();
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Cannot load speech synthesizer");
                this.Close();
            }

        }

        /// <summary>
        ///         Creates an array with default map values. All squares will be clear other than the middle 4.
        /// </summary>
        /// <returns></returns>
        internal int[,] InitialiseBoard()
        {
            // Create a new 2d board array and set the length to the number of rows and cols in the board
            int[,] boardArray = new int[numRows, numCols];

            // Initialise 0 as the default board value
            int boardVal;

            // For each row
            for (int row = 0; row < numRows; row++)
            {
                // For each col
                for (int col = 0; col < numCols; col++)
                {
                    // Set initial white board pieces
                    if ((row == 4 && col == 4) || (row == 3 && col == 3))
                    { boardVal = 0; }
                    // Set initial black board pieces
                    else if ((row == 3 && col == 4) || (row == 4 && col == 3))
                    { boardVal = 1; }
                    // Set rest of board as clear pieces
                    else boardVal = 10;

                    // Update the array at the current position to the corrosponding value as found above
                    boardArray[row, col] = boardVal;

                    gameGUIData?.SetTile(row, col, boardVal.ToString());
                }
            }

            return boardArray;
        }

        #endregion

        #region Event handlers

        /// <summary>
        ///         Event hanlder for when the player clicks a game tile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTileClicked(object sender, EventArgs e)
        {
            // If either of the player hasn't entered a name, give them a default name
            if (!p1NameEntered) { txtBoxP1Name.Text = "Player #1"; }
            if (!p2NameEntered) { txtBoxP2Name.Text = "Player #2"; }

            GameboardTileClicked!(sender, e);
        }  

        /// <summary>
        ///     Event handler for the text box of player 1 entering their name. 
        ///     Function sets the boolean for the player entering their name as true, this boolean will
        ///     then be later used for preventing the user from being able to enter text to change their name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBoxP1Name_TextChanged(object sender, EventArgs e)
        {
            p1NameEntered = true;
        }

        /// <summary>
        ///     Event handler for the text box of player 2 entering their name. 
        ///     Function sets the boolean for the player entering their name as true, this boolean will
        ///     then be later used for preventing the user from being able to enter text to change their name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtBoxP2Name_TextChanged(object sender, EventArgs e)
        {
            p2NameEntered = true;
        }

        /// <summary>
        ///     Event handler for when the player presses to start a new game. It will warn the player for data loss and
        ///     then reset the board 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this != null) 
            { 
                NewGameButtonClicked!(sender, e);
            }
        }

        /// <summary>
        ///     Event handler for when the player presses to save the current game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this != null)
            {
                SaveButtonClicked!(sender, e);
            }
        }

        /// <summary>
        ///         Event handler for when the user presses the load game button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadGame_Click(object sender, EventArgs e)
        {
            if (this != null)
            {
                LoadButtonClicked!(sender, e);               
            }
        }

        /// <summary>
        ///     Handler to catch when the player presses the overwrite save button. Prompts for a new save name and if it already exists, set as default, then call OverwriteSave();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverwriteSave_Click(object sender, EventArgs e)
        {                
            OverwriteButtonClicked!(sender, e);
        }

        /// <summary>
        ///     Make the information panel visible or not when the player checks/unchecks the button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InformationPanelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            // Initialise the bool as the value of the information panel check button to change visibility to
            bool isChecked = informationPanelToolStripMenuItem.Checked;

            // Change visibility of information panel items
            lblP1Val.Visible = isChecked;
            lblP2Val.Visible = isChecked;
            picBoxP1Token.Visible = isChecked;
            picBoxP2Token.Visible = isChecked;
            txtBoxP1Name.Visible = isChecked;
            txtBoxP2Name.Visible = isChecked;
            picBoxPlayerToMove.Visible = isChecked;

        }

        /// <summary>
        ///         When the player presses the speak button, it will tell the player whether they have turned speech synthesis on or off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpeakToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (speakToolStripMenuItem.Checked)
            {
                speechSynth!.Speak("Speech synthesis turned on.");
            }
            else { speechSynth!.Speak("Speech synthesis turned off"); }
        }

        /// <summary>
        ///         When the player clicks the about button, the About form will be
        ///         shown onto the screen. This screen will be modal so that they cannot 
        ///         access the main form until this form is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm form1 = new();
            form1.ShowDialog();
        }

        /// <summary>
        ///         When the user presses the exit game button in the game menu, check if the 
        ///         game is saved and then close the game. If the game isn't saved, prompt the user 
        ///         to save the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///         When the user presses the close button on the top right of screen, check if the 
        ///         game is saved and then close the game. If the game isn't saved, prompt the user 
        ///         to save the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isGameSaved)
            {
                DialogResult result = MessageBox.Show("The current game instance is not saved. Continue?", "Exit Game", MessageBoxButtons.YesNo);
                if (result == DialogResult.No) { e.Cancel = true; }
            }
        }

        #endregion

        #region Getter functions

        /// <summary>
        ///         Getter function for the names that the players enter into the text box.
        /// </summary>
        /// <returns>Returns the names of each player as a string array. [Player 1 Name, Player 2 Name].</returns>
        internal string[] GetPlayerNames()
        {
            // Returns the value stored in the text box for each player.
            return new string[] {txtBoxP1Name.Text, txtBoxP2Name.Text};
        }

        /// <summary>
        ///         Getter function to check whether text to speech is activated or not.
        /// </summary>
        /// <returns>Returns a boolean of whether text to speech is checked. True = Active. False = Not Active.</returns>
        internal bool GetIsTextToSpeechActive()
        {
            return speakToolStripMenuItem.Checked;
        }

        /// <summary>
        ///         Checks whether the information panel is checked or not in the menu.
        /// </summary>
        /// <returns>Returns True = Visible or False = Not Visible.</returns>
        internal bool GetIsInformationPanelVisible()
        {
            return informationPanelToolStripMenuItem.Checked;
        }

        #endregion

        #region Setter Functions        

        /// <summary>
        ///         Setter function for the label which displays the total for each player. 
        /// </summary>
        /// <param name="totalP1">The amount which needs to be displayed for player 1</param>
        /// <param name="totalP2">The amount which needs to be displayed for player 2</param>
        internal void SetPlayerTotalString(string totalP1, string totalP2)
        {
            // Update the display text for the totals of each player
            lblP1Val.Text = totalP1 + " x";
            lblP2Val.Text = totalP2 + " x";
        }

        /// <summary>
        ///         Setter function for whether the players should be able to 
        ///         edit their name in the text box.
        /// </summary>
        /// <param name="enabled">Boolean to set if the text boxes should be editable. True = Editable. False = Not editable.</param>
        internal void SetPlayerNameAccessibility(bool enabled)
        {
            // Set the text box accessibility as the param passed through
            txtBoxP1Name.Enabled = enabled;
            txtBoxP2Name.Enabled = enabled;
        }

        /// <summary>
        ///         Setter function for the text box holding the player names.
        /// </summary>
        /// <param name="player1Name">String value for the name of Player 1.</param>
        /// <param name="player2Name">String value for the name of Player 2.</param>
        internal void SetPlayerNames(string player1Name, string player2Name)
        {
            // Set the text box to the value in the parameters
            txtBoxP1Name.Text = player1Name;
            txtBoxP2Name.Text = player2Name;
        }

        /// <summary>
        ///         Set whether the menu item should be visible or not for the load game button 
        ///         and the overwrite save button. This should be set as false if there is no current save games.
        /// </summary>
        /// <param name="visible">The value which will determine the menu button visibility. True = Visible. False = Not visible</param>
        internal void SetDropDownMenuVisibility(bool visible)
        {
            // Set the visibility as the param entered.
            loadGameToolStripMenuItem.Visible = visible;
            overwriteSaveToolStripMenuItem.Visible = visible;
        }

        /// <summary>
        ///         Setter function for the picture box which indicates the next player to move.
        /// </summary>
        /// <param name="player">Player determines which picture box should be displayed. Player 1 = Left, Player 2 = Right.</param>
        internal void SetPlayerToMoveIcon(int player)
        {
            // Swap arrow img for next player move
            if (player == 0) { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "left.PNG"; }
            else { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "right.PNG"; }
        }

        /// <summary>
        ///         Sets whether the text to speech should be turned on or off
        /// </summary>
        /// <param name="active">True = TTS Active. False = TTS Not Active.</param>
        internal void SetTextToSpeech(bool active)
        {
            speakToolStripMenuItem.Checked = active;
        }

        /// <summary>
        ///         Sets whether the information panel should be visible or not.
        /// </summary>
        /// <param name="active">True = Visible. False = Not Visible</param>
        internal void SetInformationPanelVisible(bool active)
        {
            informationPanelToolStripMenuItem.Checked = active;
        }

        /// <summary>
        ///         Clear the items which are displayed in the drop down menus for overwriteSaveToolStripMenuItem 
        ///         and loadGameToolStripMenuItem.DropDownItems
        /// </summary>
        internal void ClearDropDownMenus()
        {
            // Clear menu dropdown items.
            loadGameToolStripMenuItem.DropDownItems.Clear();
            overwriteSaveToolStripMenuItem.DropDownItems.Clear();
        }

        /// <summary>
        ///         Create new drop down menu items for the overwrite save button and the load game button. 
        ///         These will be all of the save games that are loaded.
        /// </summary>
        /// <param name="saveGame">The save game which needs to be loaded onto the drop down menu</param>
        /// <param name="index">The position in the drop down where the current save game is being loaded.</param>
        internal void CreateDropDownMenu(SaveGame saveGame, int index)
        {
            // Create a new drop down item for the load game drop down and Overwrite save and insert it
            ToolStripMenuItem newItem = new() { Name = "New save " + index.ToString(), Text = saveGame.saveName };
            loadGameToolStripMenuItem.DropDownItems.Insert(index, newItem);

            ToolStripMenuItem newOverwrite = new() { Name = "Overwrite Save" + index.ToString(), Text = saveGame.saveName };
            overwriteSaveToolStripMenuItem.DropDownItems.Insert(index, newOverwrite);

            // Event handler for the new drop down item, when clicked, LoadGame function will run
            loadGameToolStripMenuItem.DropDownItems[index].Click += new EventHandler(LoadGame_Click!);

            // Event handler for overwrite save game
            overwriteSaveToolStripMenuItem.DropDownItems[index].Click += new EventHandler(OverwriteSave_Click!);
        }

        #endregion
    }
}