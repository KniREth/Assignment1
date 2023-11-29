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

        // Initialise an array of pic boxes for board
        readonly GameboardImageArray? gameGUIData;
        internal int[,] gameValueData;
        readonly string tileImagesDirPath = Directory.GetCurrentDirectory() + @"\images\";

        // New game logic class
        readonly GameLogic? gameLogic;

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
                gameLogic = new GameLogic(gameValueData);
                gameLogic.PlayerTotalsChanged += new GameLogic.GameLogicDelegate(SetPlayerTotalString);
                gameLogic.PlayerTurnChanged += new GameLogic.GameLogicDelegate(SetPlayerToMoveIcon);
                gameLogic.DropDownMenuToClear += new GameLogic.GameLogicDelegate(ClearDropDownMenus);
                gameLogic.DropDownMenuVisibilityChanged += new GameLogic.GameLogicBoolDelegate(SetDropDownMenuVisibility);
                gameLogic.BoardValuesToDefault += new GameLogic.GameLogicDelegate(ResetMapDelegate);

                gameLogic.PlayerNameAccessibilityChanged += new GameLogic.GameLogicBoolDelegate(SetPlayerNameAccessibility);
                gameLogic.InformationPanelVisibilityChanged += new GameLogic.GameLogicBoolDelegate(SetInformationPanelVisible);
                gameLogic.TextToSpeechActiveChanged += new GameLogic.GameLogicBoolDelegate(SetTextToSpeech);

                gameLogic.PlayerNamesChanged += new GameLogic.GameLogicStringDelegate(SetPlayerNames);

                gameLogic.DropDownMenuCreated += new GameLogic.GameLogicSaveDelegate(CreateDropDownMenu);

                gameLogic.TileChanged += new GameLogic.GameLogicTileSetterDelegate(SetGuiTile);

                gameLogic.TileCleared += new GameLogic.GameLogicTileClearDelegate(IsTileSetAvailable);

                gameLogic.validTiles = gameLogic.GetValidTiles();
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
                    { 
                        boardVal = 0;
                    }
                    // Set initial black board pieces
                    else if ((row == 3 && col == 4) || (row == 4 && col == 3))
                    { 
                        boardVal = 1;
                    }
                    // Set rest of board as clear pieces
                    else boardVal = 10;

                    gameGUIData?.SetTile(row, col, boardVal.ToString());
                   
                    // Update the array at the current position to the corrosponding value as found above
                    boardArray[row, col] = boardVal;                    
                }
            }

            return boardArray;
        }

        #endregion

        #region Event handlers

        /// <summary>
        ///         Checks if a tile is set to Available or not
        /// </summary>
        /// <param name="row">Defines the row position to check</param>
        /// <param name="col">Defines the col position to check</param>
        /// <returns>True if the file is set to available, false otherwise</returns>
        private bool IsTileSetAvailable(int row, int col)
        {
            return Path.GetFileNameWithoutExtension(gameGUIData!.GetTile(row,col).ImageLocation) == "11";
        }

        /// <summary>
        ///         Delegate for when a tile is trying to be changed on the GUI.
        /// </summary>
        /// <param name="row">Defines the row position to be changed</param>
        /// <param name="col">Defines the col position to be changed</param>
        /// <param name="tileName">The value to change the tile to</param>
        private void SetGuiTile(int row, int col, string tileName)
        {
            gameGUIData.SetTile(row, col, tileName);
        }

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

            gameLogic.playerNames[0]  = txtBoxP1Name.Text;
            gameLogic.playerNames[1] = txtBoxP2Name.Text;

            // Set the row and col into variables for readability
            int rowClicked = gameGUIData!.GetCurrentRowIndex(sender);
            int colClicked = gameGUIData!.GetCurrentColumnIndex(sender);
            gameLogic.CheckPath(rowClicked, colClicked);
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
            // Speech if required
            if (gameLogic.textToSpeechActive)
            {
                gameLogic.speechSynth!.Speak("Warning, any unsaved progress will be lost. Do you want to continue?");
            }

            // Prompt the user that they will lose unsaved data if they continue
            DialogResult choice = MessageBox.Show("Warning, any unsaved progress will be lost.\nContinue?", "New Game", MessageBoxButtons.YesNo);

            // Check if the user presses to continue
            if (choice == DialogResult.Yes)
            {
                gameLogic.ResetMap();
            }
        }

        /// <summary>
        ///     Event handler for when the player presses to save the current game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gameLogic.CreateNewSave();
        }

        /// <summary>
        ///         Event handler for when the user presses the load game button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadGame_Click(object sender, EventArgs e)
        {
            string? saveName = sender.ToString();
            if (saveName != null) { gameLogic.LoadGame(saveName); }
        }

        /// <summary>
        ///     Handler to catch when the player presses the overwrite save button. Prompts for a new save name and if it already exists, set as default, then call OverwriteSave();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverwriteSave_Click(object sender, EventArgs e)
        {
            string? saveToOverwrite = sender.ToString();
            if (saveToOverwrite != null) { gameLogic.OverwriteSave(saveToOverwrite); }
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
            gameLogic.isInfoPanelVisible = isChecked;
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
                gameLogic.speechSynth!.Speak("Speech synthesis turned on.");
                gameLogic.textToSpeechActive = true;
            }
            else { gameLogic.speechSynth!.Speak("Speech synthesis turned off"); gameLogic.textToSpeechActive = false; }
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
            if (!gameLogic.isGameSaved)
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

        #endregion

        #region Setter Functions        

        /// <summary>
        ///         Resets the value of the game data in game logic back to default
        /// </summary>
        private void ResetMapDelegate()
        {
            gameLogic.gameValueData = InitialiseBoard();
        }

        /// <summary>
        ///         Setter function for the label which displays the total for each player. 
        /// </summary>
        /// <param name="totalP1">The amount which needs to be displayed for player 1</param>
        /// <param name="totalP2">The amount which needs to be displayed for player 2</param>
        private void SetPlayerTotalString()
        {
            // Update the display text for the totals of each player
            lblP1Val.Text = gameLogic!.whiteTiles.ToString() + " x";
            lblP2Val.Text = gameLogic.blackTiles.ToString() + " x";
        }

        /// <summary>
        ///         Setter function for whether the players should be able to 
        ///         edit their name in the text box.
        /// </summary>
        /// <param name="enabled">Boolean to set if the text boxes should be editable. True = Editable. False = Not editable.</param>
        private void SetPlayerNameAccessibility(bool enabled)
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
        private void SetPlayerNames(string player1Name, string player2Name)
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
        private void SetDropDownMenuVisibility(bool visible)
        {
            // Set the visibility as the param entered.
            loadGameToolStripMenuItem.Visible = visible;
            overwriteSaveToolStripMenuItem.Visible = visible;
        }

        /// <summary>
        ///         Setter function for the picture box which indicates the next player to move.
        /// </summary>
        /// <param name="player">Player determines which picture box should be displayed. Player 1 = Left, Player 2 = Right.</param>
        private void SetPlayerToMoveIcon()
        {
            // Swap arrow img for next player move
            if (gameLogic.player == 0) { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "left.PNG"; }
            else { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "right.PNG"; }
        }

        /// <summary>
        ///         Sets whether the text to speech should be turned on or off
        /// </summary>
        /// <param name="active">True = TTS Active. False = TTS Not Active.</param>
        private void SetTextToSpeech(bool active)
        {
            speakToolStripMenuItem.Checked = active;
        }

        /// <summary>
        ///         Sets whether the information panel should be visible or not.
        /// </summary>
        /// <param name="active">True = Visible. False = Not Visible</param>
        private void SetInformationPanelVisible(bool active)
        {
            informationPanelToolStripMenuItem.Checked = active;
        }

        /// <summary>
        ///         Clear the items which are displayed in the drop down menus for overwriteSaveToolStripMenuItem 
        ///         and loadGameToolStripMenuItem.DropDownItems
        /// </summary>
        private void ClearDropDownMenus()
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
        private void CreateDropDownMenu(SaveGame saveGame, int index)
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