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


namespace Assignment1
{

    // TODO: Create a new class for game logic, makes everything more readable and modular
    public partial class BoardForm : Form
    {
        // Initialise bool for player entering name into txt box
        bool p1NameEntered = false;
        bool p2NameEntered = false;

        // Initialise the number of rows and columns for the board
        const int numRows = 8;
        const int numCols = 8;

        // Player 0 is white, player 1 is black. White plays first
        int player = 0;

        // The initial number of tiles each player has on the board
        int blackTiles = 2;
        int whiteTiles = 2;

        // All of the SaveGame objects which have been deserialised from the SaveGame.JSON file
        List<SaveGame> saveGames = new List<SaveGame>();

        // Valid tiles for each player
        List<Point> validTiles = new List<Point>();

        // The directory for the save game file
        string saveDataDirPath = Directory.GetCurrentDirectory() + @"\saves\game_data.JSON";

        // Offsets are the tiles that surround the current tile
        List<Point> offsets = new List<Point>
        {
            new Point(-1, -1), // Diag up left
            new Point(-1, 0), // Up
            new Point(-1, 1), // Diag up right
            new Point(0, -1), // Left
            new Point(0, 1), // Right
            new Point(1, -1), // Diag down left
            new Point(1, 0), // Down 
            new Point(1, 1) // Diag down right
        };


        // Initialise an array of pic boxes for board
        GameboardImageArray _gameBoardGui;
        int[,] gameBoardData;
        string tileImagesDirPath = Directory.GetCurrentDirectory() + @"\images\";

        // Initialise speech synthesis and the string array of voices for use by different players
        SpeechSynthesizer speechSynth;
        string[] voices;

        public BoardForm()
        {
            InitializeComponent();

            Point top = new Point(10, 30);
            Point bottom = new Point(10, 65);
            gameBoardData = this.MakeBoardArray();


            try
            {
                _gameBoardGui = new GameboardImageArray(this, gameBoardData, top, bottom, 0, tileImagesDirPath);
                _gameBoardGui.TileClicked += new GameboardImageArray.TileClickedEventDelegate(GameTileClicked);
                _gameBoardGui.UpdateBoardGui(gameBoardData);
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Game board size problem", MessageBoxButtons.OK);
                this.Close();
            }

            try
            {
                GetSaveGames();
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Cannot fetch save games");
                this.Close();
            }

            validTiles = GetValidTiles();

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
        private int[,] MakeBoardArray()
        {
            // Create a new 2d board array and set the length to the number of rows and cols in the board
            int[,] boardArray = new int[numRows, numCols];

            // Initialise 0 as the default board value
            int boardVal = 0;

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

                    if (_gameBoardGui != null)
                    {
                        _gameBoardGui.SetTile(row, col, boardVal.ToString());
                    }
                }
            }

            return boardArray;
        }

        // TODO: Possibly break down GameTileClicked into more functions for readability. This function is quite packed atm
        // Such as creating a CheckPath function maybe?

        /// <summary>
        ///         When the player clicks on the tile, it checks if the current tile is valid and then
        ///         swaps all of the necessary tiles to the correct colour and updates the player totals.
        ///         It will then switch to the next player's turn.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTileClicked(object sender, EventArgs e)
        {
            // Flag to check whether values need to be updated
            bool moveCheck = false;

            // Set the row and col into variables for readability
            int selectionRow = _gameBoardGui.GetCurrentRowIndex(sender);
            int selectionCol = _gameBoardGui.GetCurrentColumnIndex(sender);

            // If either of the player hasn't entered a name, give them a default name
            if (!p1NameEntered) { txtBoxP1Name.Text = "Player #1"; }
            if (!p2NameEntered) { txtBoxP2Name.Text = "Player #2"; }

            // Initial validity check, if the tile is 10, it is a clear tile and hence may be valid
            if (gameBoardData[selectionRow, selectionCol] == 10)
            {
                // The game has now started so make it so the players cannot change their name anymore
                txtBoxP1Name.Enabled = false;
                txtBoxP2Name.Enabled = false;

                // Iterate though the offsets, checking the path
                for (int x = 0; x < offsets.Count; x++)
                {
                    // See if the current tile clicked is valid for the current offset
                    var TileCheck = IsTileValid(selectionRow, selectionCol, offsets[x], new List<Point>());
                    // Item 1 is a list of points which will be the visited tiles, Item 2 is a bool to see if the path reaches a clear tile
                    if (TileCheck.Item2 == true && TileCheck.Item1.Count > 0)
                    {
                        // Iterate through the list of points
                        for (int y = 0; y < TileCheck.Item1.Count; y++)
                        {
                            // If the point is not currently the player's tile, change it to the players tile and update totals
                            if (gameBoardData[TileCheck.Item1[y].X, TileCheck.Item1[y].Y] != player)
                            {
                                gameBoardData[TileCheck.Item1[y].X, TileCheck.Item1[y].Y] = player;

                                _gameBoardGui.SetTile(TileCheck.Item1[y].X, TileCheck.Item1[y].Y, player.ToString());
                                UpdatePlayerTotals(1, 1);

                                moveCheck = true;
                            }

                        }

                    }
                }

                // If the player has completed a valid move, update the current index and update player values then switch player
                if (moveCheck)
                {
                    gameBoardData[selectionRow, selectionCol] = player;
                    _gameBoardGui.SetTile(selectionRow, selectionCol, player.ToString());
                    UpdatePlayerTotals(1, 0);

                    // If speech synthesis is on, say the tile which has been taken
                    if (speakToolStripMenuItem.Checked)
                    {
                        // Add 1 to each due to 0 indexing
                        speechSynth.Speak("Player" + (player + 1).ToString() + " has placed a token at " + (selectionRow + 1).ToString() + " " + (selectionCol + 1).ToString());
                    }
                    SwapPlayer();
                }
            }

            if (CheckGameOver() || CheckStalemate())
            {
                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth.Speak("Start a new game?");
                }

                // Prompt the user that they will lose unsaved data if they continue
                DialogResult choice = MessageBox.Show("Start a new game?", "New Game", MessageBoxButtons.YesNo);


                // Check if the user presses to continue
                if (choice == DialogResult.Yes)
                {
                    ResetMap();
                }
            }

            // TODO: See if the below loop can be placed into the top of the GetValidTiles function to help clear up this funciton

            // Clear valid tiles so that they can be reset 
            for (int y = 0; y < validTiles.Count; y++)
            {
                if (Path.GetFileNameWithoutExtension(_gameBoardGui.GetTile(validTiles[y].X, validTiles[y].Y).ImageLocation) == "Available")
                {
                    _gameBoardGui.SetTile(validTiles[y].X, validTiles[y].Y, "10");
                }
            }

            // Get all of the valid tiles for the player
            validTiles = GetValidTiles();

        }

        /// <summary>
        ///         Checks if the current player has no moves, if so swap player and check the other player. 
        ///         If the other player has no moves, game is stalemate
        /// </summary>
        private bool CheckStalemate()
        {
            // If the player has no tiles left to choose from, but the game isn't over, switch players
            if (GetValidTiles().Count <= 0 && !CheckGameOver())
            {

                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth.Speak("No valid tiles, swapping player.");
                }

                MessageBox.Show("No valid tiles, swapping player.");
                SwapPlayer();

                // If the other player is in the same situation, the game is ended
                if (GetValidTiles().Count <= 0)
                {
                    // Speech if required
                    if (speakToolStripMenuItem.Checked)
                    {
                        speechSynth.Speak("No more valid tiles for either players.");
                    }
                    MessageBox.Show("No more valid tiles for either players.");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// The SwapPlayer function will take the current player and set it to the  remainder of the current player plus one 
        /// divided by 2. 
        /// For example, player = 1. Player + 1 = 2. 2 % 2 = 0. New player = 0.
        /// player = 0. player + 1 = 1. 1 % 2 = 1. New player = 1.
        /// 
        /// This in my opinion is cleaner then checking and switching
        /// 
        /// The function will then swap the picbox image to show that it is the next player's move
        /// 
        /// </summary>
        private void SwapPlayer()
        {
            // Swap to next player's turn
            player = (player + 1) % 2;

            // TODO: Possibly swap the icon to indicate which player's turn it is. Not essential.
            // Swap arrow img for next player move
            if (player == 0) { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "left.PNG"; }
            else { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "right.PNG"; }

            // Set the speech synthesiser's voice to the specified player
            speechSynth.SelectVoice(voices[player]);
        }

        /// <summary>
        /// 
        /// Check whether the game is over by seeing if all of the tiles are taken by the players.
        /// 
        /// </summary>
        private bool CheckGameOver()
        {
            // Check game over, 8x8 grid = 64 tiles, if all are filled, game is over
            if (blackTiles + whiteTiles >= 64)
            {
                // Check which player wins, whoever has most tiles
                if (whiteTiles > blackTiles)
                {
                    // Speech if required
                    if (speakToolStripMenuItem.Checked)
                    {
                        speechSynth.Speak("Game over, White wins!");
                    }

                    MessageBox.Show("Game over, White wins!");
                }
                else
                {
                    // Speech if required
                    if (speakToolStripMenuItem.Checked)
                    {
                        speechSynth.Speak("Game over, Black wins!");
                    }

                    MessageBox.Show("Game over, Black wins!");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     GetValidTiles iterates through the gameBoard and checks each position for its value.
        ///     If the value of the position is an empty square, then check if it is a valid tile for all of the offsets
        ///     If the tile is valid, add it's position to a list of Points and return.
        /// </summary>
        /// <returns>
        ///     A list of Points which are valid tiles that the player is able to press on
        /// </returns>
        private List<Point> GetValidTiles()
        {
            // Initialise the list of points, this will be returned at the end
            List<Point> validTiles = new List<Point>();

            // Iterate through the rows
            for (int i = 0; i < gameBoardData.GetLength(0); i++)
            {
                // Iterate through the columns
                for (int j = 0; j < gameBoardData.GetLength(1); j++)
                {
                    // Check if the current position is clear
                    if (gameBoardData[i, j] == 10)
                    {
                        // Iterate through all of the offsets around the current position
                        for (int x = 0; x < offsets.Count; x++)
                        {
                            // Check the path for the current position with its corrosponding offset
                            var isValid = IsTileValid(i, j, offsets[x], new List<Point>());

                            // If the tile is valid, add it to the List of Points
                            if (isValid.Item1.Count > 0 && isValid.Item2)
                            {
                                validTiles.Add(new Point(i, j));
                                _gameBoardGui.SetTile(i, j, "Available");
                            }
                        }
                    }
                }
            }

            return validTiles;
        }

        /// <summary>
        ///     Checks if the current tile is valid by checking through the tiles offsets, this will be predetermined before
        ///     the check. If the current tile is valid, it will go through recursion in the same function until it meets a tile
        ///     that is either out of range, or clear, at which point we know that the path is invalid. On the other hand,
        ///     if the tile met is taken by the player, we know the path that it has taken is valid, therefore the path is then 
        ///     returned.
        /// </summary>
        /// <param name="offsetRow">This is the current row pos being checked</param>
        /// <param name="offsetCol">This is the current col pos being checked</param>
        /// <param name="currentOffset">This is the current offset indicating the direction the of the path. This is needed for
        ///                             the recursion. </param>
        /// <param name="visitedTiles">This is all of the tiles that have been visited and checked, if the path is eventually
        ///                            valid, this List will be returned</param>
        /// <returns>A list of valid tiles and a boolean to say whether the path is valid or not</returns>
        private (List<Point>, bool) IsTileValid(int currentRow, int currentCol, Point currentOffset, List<Point> visitedTiles)
        {
            // Separate the offset Point into separate variables for readability
            currentRow += currentOffset.X;
            currentCol += currentOffset.Y;

            // Check if the value is out of range, if so then return that the tile is false
            if (currentRow < 0 || currentCol < 0 || currentRow >= 8 || currentCol >= 8)
            {
                return (new List<Point>(), false);
            }

            // Check if the current offset is a clear tile, if so then retrace the recursion
            if (gameBoardData[currentRow, currentCol] == 10)
            {
                return (new List<Point>(), false);
            }

            // Check if the current offset tile is taken by the opposing player, if so then add the tile to the list and start recursion
            if (gameBoardData[currentRow, currentCol] != player)
            {
                // Add the tile to the visited tile array to be changed if the path is valid
                visitedTiles.Add(new Point(currentRow, currentCol));

                // Check the next offset to see if its valid
                var valid = IsTileValid(currentRow, currentCol, currentOffset, visitedTiles);

                return (valid.Item1, true);
            }

            // If the current tile is taken by the player, the path is not valid
            return (visitedTiles, false);
        }

        /// <summary>
        ///     Updates the retrospective total tiles for each player.
        ///     This will be showcased on the screen and also used to check whether the game is over.
        /// </summary>
        /// <param name="valueToAdd">The value to add to the current player's total</param>
        /// <param name="valueToRemove">The value to remove from the opposing player's total</param>
        private void UpdatePlayerTotals(int valueToAdd, int valueToRemove)
        {
            // Check if it is player 1
            if (player == 0)
            {
                whiteTiles += valueToAdd;
                blackTiles -= valueToRemove;
            }
            // Otherwise it must be player 2
            else
            {
                blackTiles += valueToAdd;
                whiteTiles -= valueToRemove;
            }

            // Update the display text for the totals of each player
            lblP1Val.Text = whiteTiles.ToString() + " x";
            lblP2Val.Text = blackTiles.ToString() + " x";

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
                speechSynth.Speak("Warning, any unsaved progress will be lost. Do you want to continue?");
            }

            // Prompt the user that they will lose unsaved data if they continue
            DialogResult choice = MessageBox.Show("Warning, any unsaved progress will be lost.\nContinue?", "New Game", MessageBoxButtons.YesNo);

            // Check if the user presses to continue
            if (choice == DialogResult.Yes)
            {
                ResetMap();
            }
        }

        /// <summary>
        ///         Resets all of the maps values to the base values so that the players can start again.
        /// </summary>
        private void ResetMap()
        {
            // Reset board data
            gameBoardData = MakeBoardArray();

            // Fetch all of the current valid tiles to be displayed
            GetValidTiles();

            // Get player totals to load to screen
            GetPlayerTotals();

            // Allow the player to enter their name again
            txtBoxP1Name.Enabled = true;
            txtBoxP2Name.Enabled = true;
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
            // Initialise boolean for whether the save name currently exists. 
            bool nameExists = false;

            // Check how many saveGame objects there as there can only be 5 save slots in the requirements
            if (saveGames.Count < 5)
            {
                // The default name for the save game, this ensures it will be unique everytime
                string defaultSaveName = DateTime.Now.ToString();

                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth.Speak("Enter name of save game.");
                }

                // Display a message box for the player to choose the name of their save game
                string saveName = Microsoft.VisualBasic.Interaction.InputBox("Enter name of save", "Save Game", defaultSaveName);

                // If the player presses the cancel button, this will return false, otherwise it will continue with
                // the default value from the input box
                if (!String.IsNullOrEmpty(saveName))
                {
                    // Iterate through all of the current saveGame objects, and check if the current saveName is taken
                    for (int i = 0; i < saveGames.Count; i++)
                    {
                        if (saveGames[i].saveName == saveName)
                        {
                            nameExists = true;
                        }
                    }

                    // If the current save name exists, prompt the user to overwrite the save
                    if (nameExists)
                    {
                        // Speech if required
                        if (speakToolStripMenuItem.Checked)
                        {
                            speechSynth.Speak("Warning, game name already exists. Do you want to overwrite it?");
                        }
                        DialogResult choice = MessageBox.Show("Warning, game name already exists.\nOverwrite??", "Game Exists", MessageBoxButtons.YesNo);
                        if (choice == DialogResult.Yes) { OverwriteSave(saveName, saveName); }
                    }

                    // If the save name doesn't exist, save the game to a new slot
                    else
                    {
                        // Create a jagged array as 2d arrays cannot be serialised using this serialiser
                        int[][] gameData = new int[8][];

                        // Copy the data from gameboard data to the new jagged array
                        for (int i = 0; i < gameBoardData.GetLength(0); i++)
                        {
                            gameData[i] = new int[8];
                            for (int j = 0; j < gameBoardData.GetLength(1); j++)
                            {
                                gameData[i][j] = gameBoardData[i, j];
                            }
                        }

                        // Create a new SaveGame object with the new data 
                        SaveGame newSave = new SaveGame(saveName, txtBoxP1Name.Text, txtBoxP2Name.Text, gameData, player);

                        // Serialise and append this data to the save game file
                        File.AppendAllText(saveDataDirPath, newSave.Serialise() + "\n");
                    }
                }
            }
            else
            {
                // Speech if required
                if (speakToolStripMenuItem.Checked)
                {
                    speechSynth.Speak("You can only have 5 save game slots, choose a game to overwrite.");
                }
                MessageBox.Show("You can only have 5 save game slots, choose a game to overwrite.");
            }

            // Load the save games to the menu
            GetSaveGames();
        }

        /// <summary>
        ///         Rewrites all of the saves in the save file with the new data. 
        ///         Deletes all data in the save game file and rewrite all data with changes.
        /// </summary>
        /// <param name="saveName">The name of the save to overwrite</param>
        private void OverwriteSave(string saveName, string newSaveName)
        {
            // Delete the initial file
            File.Delete(saveDataDirPath);

            // Iterate through all of the SaveGame objects in the list
            for (int i = 0; i < saveGames.Count; i++)
            {
                // If the saveGame object's saveName is the same as the param, overwrite that object
                if (saveGames[i].saveName == saveName)
                {
                    //saveGames[i].saveName = newSaveName;
                    saveGames[i].player1Name = txtBoxP1Name.Text;
                    saveGames[i].player2Name = txtBoxP2Name.Text;
                    saveGames[i].playerTurn = player;
                    for (int j = 0; j < gameBoardData.GetLength(0); j++)
                    {
                        for (int x = 0; x < gameBoardData.GetLength(1); x++)
                        {
                            saveGames[i].gameData[j][x] = gameBoardData[j, x];
                        }
                    }
                }
                // Rewrite the file with all new objects
                File.AppendAllText(saveDataDirPath, saveGames[i].Serialise() + "\n");
            }
            // Load all of the new save games back to the menu
            GetSaveGames();
        }

        /// <summary>
        ///         Loads all of the save games onto the menu.
        ///         If a save file doesn't currently exist, create one.
        /// </summary>
        private void GetSaveGames()
        {
            // Error check for if the file doesn't exist in the folder, could be due to accidental deletion by user
            if (!File.Exists(saveDataDirPath))
            {
                // If the file doesn't exist, create it
                File.Create(saveDataDirPath);
            }

            // Clear the list of SaveGame objects from the list and clear the menu drop down items
            saveGames.Clear();
            loadGameToolStripMenuItem.DropDownItems.Clear();
            overwriteSaveToolStripMenuItem.DropDownItems.Clear();

            // Read the save game file and save data to the string array
            // Each line in the file will be a separate index in the array
            string[] saveData = File.ReadAllLines(saveDataDirPath);

            // Check if there are any save games
            if (saveData.Length > 0)
            {
                // Ensure that the player can see the load and save games
                loadGameToolStripMenuItem.Visible = true;
                overwriteSaveToolStripMenuItem.Visible = true;

                // Iterate through the string array, hence iterate through the data in the file
                for (int i = 0; i < saveData.Length; i++)
                {
                    // Serialise each line in the array into a Object and add objects intoa list of objects
                    saveGames.Add(JsonSerializer.Deserialize<SaveGame>(saveData[i]));

                    // Create a new drop down item for the load game drop down and Overwrite save and insert it
                    ToolStripMenuItem newItem = new ToolStripMenuItem { Name = "New save " + i.ToString(), Text = saveGames[i].saveName };
                    loadGameToolStripMenuItem.DropDownItems.Insert(i, newItem);

                    ToolStripMenuItem newOverwrite = new ToolStripMenuItem { Name = "Overwrite Save" + i.ToString(), Text = saveGames[i].saveName };
                    overwriteSaveToolStripMenuItem.DropDownItems.Insert(i, newOverwrite);

                    // Event handler for the new drop down item, when clicked, LoadGame function will run
                    loadGameToolStripMenuItem.DropDownItems[i].Click += new EventHandler(LoadGame);

                    // Event handler for overwrite save game
                    overwriteSaveToolStripMenuItem.DropDownItems[i].Click += new EventHandler(HandlerOverwriteSave);
                }
            }

            // Make the load game and overwrite save buttons not visible as they are not needed
            else
            {
                loadGameToolStripMenuItem.Visible = false;
                overwriteSaveToolStripMenuItem.Visible = false;
            }
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
                speechSynth.Speak("Enter name of save game.");
            }

            // Display a message box for the player to choose the name of their save game
            string saveName = Microsoft.VisualBasic.Interaction.InputBox("Enter name of save", "Save Game", defaultSaveName);

            // If the player presses the cancel button, this will return false, otherwise it will continue with
            // the default value from the input box
            if (!String.IsNullOrEmpty(saveName))
            {
                // Iterate through all of the current saveGame objects, and check if the current saveName is taken
                for (int i = 0; i < saveGames.Count; i++)
                {
                    if (saveGames[i].saveName == saveName)
                    {
                        // Speech if required
                        if (speakToolStripMenuItem.Checked)
                        {
                            speechSynth.Speak("Save name already exists, setting name as default name.");
                        }
                        MessageBox.Show("Save name already exists, setting name as default name");
                        saveName = defaultSaveName;
                        break;
                    }
                }
                OverwriteSave(sender.ToString(), saveName);
            }

        }

        /// <summary>
        ///         Loads the game at the index which shares a saveName with the sender.ToString().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadGame(object sender, EventArgs e)
        {
            // Initialising a default value for indexToLoad, if not changed, the game will not be loaded
            int indexToLoad = -1;

            // Iterates through all of the objects in the saveGames list
            for (int i = 0; i < saveGames.Count; i++)
            {
                // Check if the object's saveName is equal to the sender object's name
                if (saveGames[i].saveName == sender.ToString())
                {
                    // Set the index to load as the current index, this will mean the save game is found
                    // This means that you can break out of the for loop as there shouldn't be any games with the same saveName
                    indexToLoad = i;
                    break;
                }
            }

            // Check if the index is valid
            if (indexToLoad >= 0)
            {
                // Load the player names and player turn
                txtBoxP1Name.Text = saveGames[indexToLoad].player1Name;
                txtBoxP2Name.Text = saveGames[indexToLoad].player2Name;
                if (saveGames[indexToLoad].playerTurn != player) { SwapPlayer(); }
                // Iterate through the game board and change the tiles to game to load's tiles
                for (int i = 0; i < gameBoardData.GetLength(0); i++)
                {
                    for (int j = 0; j < gameBoardData.GetLength(1); j++)
                    {
                        gameBoardData[i, j] = saveGames[indexToLoad].gameData[i][j];
                        _gameBoardGui.SetTile(i, j, gameBoardData[i, j].ToString());
                    }
                }

                // Display all of the current valid tiles to the screen
                GetValidTiles();

                // Get the player totals to display to screen
                GetPlayerTotals();

                // Loading an active game so names shouldn't be editable
                txtBoxP1Name.Enabled = false;
                txtBoxP2Name.Enabled = false;
            }
        }

        /// <summary>
        ///         Looks through the game array to see how many tiles each player currently has
        /// </summary>
        private void GetPlayerTotals()
        {
            whiteTiles = 0;
            blackTiles = 0;
            for (int i = 0; i < gameBoardData.GetLength(0); i++)
            {
                for (int j = 0; j < gameBoardData.GetLength(1); j++)
                {
                    if (gameBoardData[i, j] == 0)
                    {
                        whiteTiles += 1;
                    }
                    else if (gameBoardData[i, j] == 1)
                    {
                        blackTiles += 1;
                    }
                }
            }

            // Update the display text for the totals of each player
            lblP1Val.Text = whiteTiles.ToString() + " x";
            lblP2Val.Text = blackTiles.ToString() + " x";
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
                speechSynth.Speak("Speech synthesis turned on.");
            }
            else { speechSynth.Speak("Speech synthesis turned off"); }
        }
    }
}