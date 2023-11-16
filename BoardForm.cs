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
    public partial class BoardForm : Form
    {
        // Initialise bool for player entering name into txt box
        bool p1NameEntered = false;
        bool p2NameEntered = false;

        // Initialise the number of rows and columns for the board
        const int numRows = 8;
        const int numCols = 8;

        // boolean to check whether game has been saved when user tries to leave game
        readonly bool isGameSaved = false;

        // Valid tiles for each player
        List<Point> validTiles = new();

        // Initialise an array of pic boxes for board
        readonly GameboardImageArray? gameGUIData;
        internal int[,] gameValueData;
        readonly string tileImagesDirPath = Directory.GetCurrentDirectory() + @"\images\";

        // New game logic class
        readonly GameLogic gameLogic;

        // Initialise speech synthesis and the string array of voices for use by different players
        internal readonly SpeechSynthesizer? speechSynth;
        internal readonly string[]? voices;

        public BoardForm()
        {
            InitializeComponent();

            Point topLeftCorner = new(50, 30);
            Point bottomRightCorner = new(50, 65);
            gameValueData = InitialiseBoard();

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

            try
            {
                gameLogic = new GameLogic(gameGUIData!, this);
            }
            catch(Exception ex) 
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Cannot load game logic", MessageBoxButtons.OK);
                this.Close();
            }

            try
            {
                gameLogic!.GetSaveGames();
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Cannot fetch save games");
                this.Close();
            }

            validTiles = gameLogic!.GetValidTiles();

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

        /// <summary>
        ///         When the player clicks a tile, it will ensure they cannot change their names as the
        ///         game has now started and if they haven't yet entered a name they will have it set to default. 
        ///         It will also then send the row and col to CheckPath to change the tile if it is valid.
        ///         It will then check if the game is over and if so give the option to start a new game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTileClicked(object sender, EventArgs e)
        {


            // Set the row and col into variables for readability
            int rowClicked = gameGUIData!.GetCurrentRowIndex(sender);
            int colClicked = gameGUIData!.GetCurrentColumnIndex(sender);

            // If either of the player hasn't entered a name, give them a default name
            if (!p1NameEntered) { txtBoxP1Name.Text = "Player #1"; }
            if (!p2NameEntered) { txtBoxP2Name.Text = "Player #2"; }

            gameLogic.CheckPath(rowClicked, colClicked);

            if (gameLogic.CheckGameOver() || gameLogic.CheckStalemate())
            {
                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth!.Speak("Start a new game?");
                }

                // Prompt the user that they will lose unsaved data if they continue
                DialogResult choice = MessageBox.Show("Start a new game?", "New Game", MessageBoxButtons.YesNo);


                // Check if the user presses to continue
                if (choice == DialogResult.Yes)
                {
                    gameLogic.ResetMap();
                }
            }

            // Clear valid tiles so that they can be reset 
            for (int y = 0; y < validTiles.Count; y++)
            {
                if (Path.GetFileNameWithoutExtension(gameGUIData.GetTile(validTiles[y].X, validTiles[y].Y).ImageLocation) == "Available")
                {
                    gameGUIData.SetTile(validTiles[y].X, validTiles[y].Y, "10");
                }
            }

            // Get all of the valid tiles for the player
            validTiles = gameLogic.GetValidTiles();

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
            if (speakToolStripMenuItem.Checked)
            {
                speechSynth!.Speak("Warning, any unsaved progress will be lost. Do you want to continue?");
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
        ///     Checks there is not too many saves and whether the current save name is already taken.
        ///     If so, prompt to overwrite. Otherwise, save to new save slot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check how many saveGame objects there as there can only be 5 save slots in the requirements
            if (gameLogic.saveGames.Count < 5)
            {
                // The default name for the save game, this ensures it will be unique everytime
                string defaultSaveName = DateTime.Now.ToString();

                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth!.Speak("Enter name of save game.");
                }

                // Display a message box for the player to choose the name of their save game
                string saveName = Microsoft.VisualBasic.Interaction.InputBox("Enter name of save", "Save Game", defaultSaveName);

                // If the player presses the cancel button, this will return false, otherwise it will continue with
                // the default value from the input box
                if (!String.IsNullOrEmpty(saveName))
                {
                    gameLogic.CreateNewSave(saveName);
                }
            }
            else
            {
                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth!.Speak("You can only have 5 save game slots, choose a game to overwrite.");
                }
                MessageBox.Show("You can only have 5 save game slots, choose a game to overwrite.");
            }

            // Load the save games to the menu
            gameLogic.GetSaveGames();
        }

        /// <summary>
        ///         Loads the game at the index which shares a saveName with the sender.ToString().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadGame_Click(object sender, EventArgs e)
        {
            // Initialising a default value for indexToLoad, if not changed, the game will not be loaded
            int indexToLoad = -1;

            // Iterates through all of the objects in the saveGames list
            for (int i = 0; i < gameLogic.saveGames.Count; i++)
            {
                // Check if the object's saveName is equal to the sender object's name
                if (gameLogic.saveGames[i].saveName == sender.ToString())
                {
                    // Set the index to load as the current index, this will mean the save game is found
                    // This means that you can break out of the for loop as there shouldn't be any games with the same saveName
                    indexToLoad = i;
                    break;

                }
            }
            
            // Load the game at the valid index if it is valid
            gameLogic.LoadGame(indexToLoad);

        }

        /// <summary>
        ///     Handler to catch when the player presses the overwrite save button. Prompts for a new save name and if it already exists, set as default, then call OverwriteSave();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlerOverwriteSave(object sender, EventArgs e)
        {
            // The default name for the save game, this ensures it will be unique everytime
            string defaultSaveName = DateTime.Now.ToString();

            // Speech if required
            if (speakToolStripMenuItem.Checked)
            {
                speechSynth!.Speak("Enter name of save game.");
            }

            // Display a message box for the player to choose the name of their save game
            string saveName = Microsoft.VisualBasic.Interaction.InputBox("Enter name of save", "Save Game", defaultSaveName);

            // If the player presses the cancel button, this will return false, otherwise it will continue with
            // the default value from the input box
            if (!String.IsNullOrEmpty(saveName))
            {
                // Iterate through all of the current saveGame objects, and check if the current saveName is taken
                for (int i = 0; i < gameLogic.saveGames.Count; i++)
                {
                    if (gameLogic.saveGames[i].saveName == saveName)
                    {
                        // Speech if required
                        if (speakToolStripMenuItem.Checked)
                        {
                            speechSynth!.Speak("Save name already exists, setting name as default name.");
                        }
                        MessageBox.Show("Save name already exists, setting name as default name");
                        saveName = defaultSaveName;
                        break;
                    }
                }
                gameLogic.OverwriteSave(sender.ToString()!, saveName);
            }

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
        




        internal void SetPlayerTotalString(string totalP1, string totalP2)
        {
            // Update the display text for the totals of each player
            lblP1Val.Text = totalP1 + " x";
            lblP2Val.Text = totalP2 + " x";
        }
        internal void SetPlayerNameAccessibility(bool enabled)
        {
            txtBoxP1Name.Enabled = enabled;
            txtBoxP2Name.Enabled = enabled;
        }
        internal string[] GetPlayerNames()
        {
            return new string[] {txtBoxP1Name.Text, txtBoxP2Name.Text};
        }
        
        internal void SetPlayerNames(string player1Name, string player2Name)
        {
            txtBoxP1Name.Text = player1Name;
            txtBoxP2Name.Text = player2Name;
        }
        internal void ClearDropDownMenus()
        {
            loadGameToolStripMenuItem.DropDownItems.Clear();
            overwriteSaveToolStripMenuItem.DropDownItems.Clear();
        }

        internal void SetDropDownMenuVisibility(bool visible)
        {
            loadGameToolStripMenuItem.Visible = visible;
            overwriteSaveToolStripMenuItem.Visible = visible;
        }

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
            overwriteSaveToolStripMenuItem.DropDownItems[index].Click += new EventHandler(HandlerOverwriteSave!);
        }

        internal void SetPlayerToMoveIcon(int player)
        {
            // Swap arrow img for next player move
            if (player == 0) { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "left.PNG"; }
            else { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "right.PNG"; }
        }

        internal bool IsTextToSpeechActive()
        {
            return speakToolStripMenuItem.Checked;
        }
    }
}