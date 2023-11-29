using GameboardGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assignment1
{
    /// <summary>
    ///         This class represents all of the logic that is required to run the game. For example, checking
    ///         if a move is valid, updating player values, resetting the map and being able to save or load a 
    ///         game instance into the current game.
    /// </summary>
    internal class GameLogic
    {
        #region Variable Initialisation

        // Player 0 is white, player 1 is black. White plays first
        internal int player = 0;

        // Initialise player names
        internal string[] playerNames = new string[2];

        // The initial number of tiles each player has on the board
        internal int blackTiles = 2;
        internal int whiteTiles = 2;

        // Valid tiles for each player
        internal List<Point> validTiles = new();

        // All of the SaveGame objects which have been deserialised from the SaveGame.JSON file
        internal readonly List<SaveGame> saveGames = new();

        // boolean to check whether game has been saved when user tries to leave game
        internal bool isGameSaved = false;

        // The directory for the save game file
        readonly string saveDataDirPath = Directory.GetCurrentDirectory() + @"\saves\game_data.JSON";

        // List of points which represent the positions of all of the tiles that surround the current tile
        readonly List<Point> offsets = new()
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
        internal int[,] gameValueData;


        public bool isInfoPanelVisible = true;

        // Initialise speech synthesis and the string array of voices for use by different players
        internal readonly SpeechSynthesizer? speechSynth;
        internal readonly string[]? voices;
        internal bool textToSpeechActive;

        #endregion

        #region Delegate Initialisation

        public delegate void GameLogicDelegate();
        public event GameLogicDelegate? PlayerTotalsChanged;
        public event GameLogicDelegate? PlayerTurnChanged;
        public event GameLogicDelegate? DropDownMenuToClear;
        public event GameLogicDelegate? BoardValuesToDefault;

        public delegate void GameLogicBoolDelegate(bool b);
        public event GameLogicBoolDelegate? PlayerNameAccessibilityChanged;
        public event GameLogicBoolDelegate? InformationPanelVisibilityChanged;
        public event GameLogicBoolDelegate? TextToSpeechActiveChanged;
        public event GameLogicBoolDelegate? DropDownMenuVisibilityChanged;

        public delegate void GameLogicStringDelegate(string str1, string str2);
        public event GameLogicStringDelegate? PlayerNamesChanged;

        public delegate void GameLogicSaveDelegate(SaveGame saveGame, int i);
        public event GameLogicSaveDelegate? DropDownMenuCreated;

        public delegate void GameLogicTileSetterDelegate(int r, int c, string s);
        public event GameLogicTileSetterDelegate TileChanged;

        public delegate bool GameLogicTileClearDelegate(int r, int c);
        public event GameLogicTileClearDelegate TileCleared;

        #endregion

        #region Constructor

        // Constructor for class
        public GameLogic(int[,] gameValueData) 
        {
            this.gameValueData = gameValueData;
            
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
            }
        }

        #endregion

        #region Tile checks

        /// <summary>
        ///         When the player clicks on the tile, it checks if the current tile is valid and then
        ///         swaps all of the necessary tiles to the correct colour and updates the player totals.
        ///         It will then switch to the next player's turn.
        /// </summary>
        /// <param name="rowClicked">Integer value representing the row position clicked, 0 indexed.</param>
        /// <param name="colClicked">Integer value representing the col position clicked, 0 indexed.</param>
        internal void CheckPath(int rowClicked, int colClicked)
        {
            // Flag to check whether values need to be updated
            bool moveCheck = false;

            // Initial validity check, if the tile is 10 or 11, it is a clear tile and hence may be valid
            if (gameValueData![rowClicked, colClicked] == 10 || gameValueData![rowClicked, colClicked] == 11)
            {
                // The game has now started so make it so the players cannot change their name anymore
                PlayerNameAccessibilityChanged(false);

                // Iterate though the offsets, checking the path
                for (int x = 0; x < offsets.Count; x++)
                {
                    // See if the current tile clicked is valid for the current offset
                    var TileCheck = IsTileValid(rowClicked, colClicked, offsets[x], new List<Point>());
                    // Item 1 is a list of points which will be the visited tiles, Item 2 is a bool to see if the path reaches a clear tile
                    if (TileCheck.Item2 == true && TileCheck.Item1.Count > 0)
                    {
                        // Iterate through the list of points
                        for (int y = 0; y < TileCheck.Item1.Count; y++)
                        {
                            // If the point is not currently the player's tile, change it to the players tile and update totals
                            if (gameValueData[TileCheck.Item1[y].X, TileCheck.Item1[y].Y] != player)
                            {
                                gameValueData[TileCheck.Item1[y].X, TileCheck.Item1[y].Y] = player;

                                TileChanged(TileCheck.Item1[y].X, TileCheck.Item1[y].Y, player.ToString());
                                UpdatePlayerTotals(1, 1);

                                moveCheck = true;

                                // The game instance is changing, so therefore isn't saved.
                                isGameSaved = false;
                            }

                        }

                    }
                }

                // If the player has completed a valid move, update the current index and update player values then switch player
                if (moveCheck)
                {
                    gameValueData[rowClicked, colClicked] = player;
                    TileChanged(rowClicked, colClicked, player.ToString());

                    UpdatePlayerTotals(1, 0);

                    // If speech synthesis is on, say the tile which has been taken
                    if (textToSpeechActive)
                    {
                        // Add 1 to each due to 0 indexing
                       speechSynth!.Speak("Player" + (player + 1).ToString() + " has placed a token at " + (rowClicked + 1).ToString() + " " + (colClicked + 1).ToString());
                    }
                    SwapPlayer();
                    // Get all of the valid tiles for the player
                    ClearValidTiles();
                    validTiles = GetValidTiles();
                }

                // TODO: Move this to the game logic class function for check path
                if (CheckGameOver() || CheckStalemate())
                {
                    // Speech if required
                    if (textToSpeechActive)
                    {
                       speechSynth!.Speak("Start a new game?");
                    }

                    // Prompt the user that they will lose unsaved data if they continue
                    DialogResult choice = MessageBox.Show("Start a new game?", "New Game", MessageBoxButtons.YesNo);


                    // Check if the user presses to continue
                    if (choice == DialogResult.Yes)
                    {
                        ResetMap();
                    }
                }
            }
        }


        private void ClearValidTiles()
        {
            // Clear valid tiles so that they can be reset 
            for (int y = 0; y < validTiles.Count; y++)
            {
                if (TileCleared(validTiles[y].X, validTiles[y].Y))
                {                    
                    TileChanged(validTiles[y].X, validTiles[y].Y, "10");                    
                }
            }
        }

        /// <summary>
        ///     GetValidTiles iterates through the gameBoard and checks each position for its value.
        ///     If the value of the position is an empty square, then check if it is a valid tile for all of the offsets
        ///     If the tile is valid, add it's position to a list of Points and return.
        /// </summary>
        /// <returns>
        ///     A list of Points which are valid tiles that the player is able to press on
        /// </returns>
        internal List<Point> GetValidTiles()
        {
            // Initialise the list of points, this will be returned at the end
            List<Point> validTiles = new();

            // Iterate through the rows
            for (int i = 0; i < gameValueData!.GetLength(0); i++)
            {
                // Iterate through the columns
                for (int j = 0; j < gameValueData.GetLength(1); j++)
                {
                    // Check if the current position is clear
                    if (gameValueData[i, j] == 10)
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
                                TileChanged(i, j, "11");
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
            if (gameValueData![currentRow, currentCol] == 10)
            {
                return (new List<Point>(), false);
            }

            // Check if the current offset tile is taken by the opposing player, if so then add the tile to the list and start recursion
            if (gameValueData[currentRow, currentCol] != player)
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
        ///         Checks if the current player has no moves, if so swap player and check the other player. 
        ///         If the other player has no moves, game is stalemate
        /// </summary>
        internal bool CheckStalemate()
        {
            // If the player has no tiles left to choose from, but the game isn't over, switch players
            if (GetValidTiles().Count <= 0 && !CheckGameOver())
            {

                // Speech if required
                if (textToSpeechActive)
                {
                   speechSynth!.Speak("No valid tiles, swapping player.");
                }

                MessageBox.Show("No valid tiles, swapping player.");
                SwapPlayer();

                // If the other player is in the same situation, the game is ended
                if (GetValidTiles().Count <= 0)
                {
                    // Speech if required
                    if (textToSpeechActive)
                    {
                       speechSynth!.Speak("No more valid tiles for either players.");
                    }
                    MessageBox.Show("No more valid tiles for either players.");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// Check whether the game is over by seeing if all of the tiles are taken by the players.
        /// 
        /// </summary>
        internal bool CheckGameOver()
        {
            // Check game over, 8x8 grid = 64 tiles, if all are filled, game is over
            if (blackTiles + whiteTiles >= 64)
            {
                // Check which player wins, whoever has most tiles
                if (whiteTiles > blackTiles)
                {
                    // Speech if required
                    if (textToSpeechActive)
                    {
                       speechSynth!.Speak("Game over, White wins!");
                    }

                    MessageBox.Show("Game over, White wins!");
                }
                else
                {
                    // Speech if required
                    if (textToSpeechActive)
                    {
                       speechSynth!.Speak("Game over, Black wins!");
                    }

                    MessageBox.Show("Game over, Black wins!");
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Player data

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

            PlayerTurnChanged();
            
            // Set the speech synthesiser's voice to the specified player
            speechSynth!.SelectVoice(voices![player]);
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
            PlayerTotalsChanged();
        }

        /// <summary>
        ///         Looks through the game array to see how many tiles each player currently has
        ///         and sets the variable storing the amount for each player. 
        ///         Also sets the string which is displayed on the screen for each player's totals.
        /// </summary>
        internal void GetPlayerTotals()
        {
            whiteTiles = 0;
            blackTiles = 0;
            for (int i = 0; i < gameValueData!.GetLength(0); i++)
            {
                for (int j = 0; j < gameValueData.GetLength(1); j++)
                {
                    if (gameValueData[i, j] == 0)
                    {
                        whiteTiles += 1;
                    }
                    else if (gameValueData[i, j] == 1)
                    {
                        blackTiles += 1;
                    }
                }
            }

            // Update the display text for the totals of each player
            PlayerTotalsChanged();
        }

        #endregion

        #region Resetting the map

        /// <summary>
        ///         Resets all of the maps values to the base values so that the players can start again.
        /// </summary>
        internal void ResetMap()
        {
            // Reset board data
            BoardValuesToDefault();

            if (player == 1) { SwapPlayer(); }

            // Fetch all of the current valid tiles to be displayed
            validTiles = GetValidTiles();

            // Get player totals to load to screen
            GetPlayerTotals();

            PlayerNameAccessibilityChanged(true);

        }

        #endregion

        #region Game storage
        /// <summary>
        ///         Rewrites all of the saves in the save file with the new data. 
        ///         Deletes all data in the save game file and rewrite all data with changes.
        /// </summary>
        /// <param name="saveName">The name of the save to overwrite</param>
        internal void OverwriteSave(string saveToOverwrite)
        {
            // The default name for the save game, this ensures it will be unique everytime
            string defaultSaveName = DateTime.Now.ToString();

            // Speech if required
            if (textToSpeechActive)
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
                for (int i = 0; i < saveGames.Count; i++)
                {
                    if (saveGames[i].saveName == saveName)
                    {
                        // Speech if required
                        if (textToSpeechActive)
                        {
                           speechSynth!.Speak("Save name already exists, setting name as default name.");
                        }
                        MessageBox.Show("Save name already exists, setting name as default name");
                        saveName = defaultSaveName;
                        break;
                    }
                }

                // Delete the initial file
                File.Delete(saveDataDirPath);  

                // Iterate through all of the SaveGame objects in the list
                for (int i = 0; i < saveGames.Count; i++)
                {
                    // If the saveGame object's saveName is the same as the param, overwrite that object
                    if (saveGames[i].saveName == saveToOverwrite)
                    {
                        saveGames[i].saveName = saveName;
                        saveGames[i].player1Name = playerNames[0];
                        saveGames[i].player2Name = playerNames[1];
                        saveGames[i].playerTurn = player;
                        saveGames[i].isSpeechActivated = textToSpeechActive;
                        saveGames[i].isInformationPanelVisible = isInfoPanelVisible;
                        for (int j = 0; j < gameValueData!.GetLength(0); j++)
                        {
                            for (int x = 0; x < gameValueData.GetLength(1); x++)
                            {
                                saveGames[i].gameData[j][x] = gameValueData[j, x];
                            }
                        }
                    }
                    // Rewrite the file with all new objects
                    File.AppendAllText(saveDataDirPath, saveGames[i].Serialise() + "\n");
                }
                // Game has just been saved, so set isGameSaved as true
                isGameSaved = true;

                // Load all of the new save games back to the menu
                GetSaveGames();

            }
        }

        /// <summary>
        ///         Loads all of the save games onto the menu.
        ///         If a save file doesn't currently exist, create one.
        /// </summary>
        internal void GetSaveGames()
        {
            // Error check for if the file doesn't exist in the folder, could be due to accidental deletion by user
            if (!File.Exists(saveDataDirPath))
            {
                // If the file doesn't exist, create it
                File.Create(saveDataDirPath);
            }

            // Clear the list of SaveGame objects from the list and clear the menu drop down items
            saveGames.Clear();
            DropDownMenuToClear();

            // Read the save game file and save data to the string array
            // Each line in the file will be a separate index in the array
            string[] saveData = File.ReadAllLines(saveDataDirPath);

            // Check if there are any save games
            if (saveData.Length > 0)
            {
                // Ensure that the player can see the load and save games
                DropDownMenuVisibilityChanged(true);

                // Iterate through the string array, hence iterate through the data in the file
                for (int i = 0; i < saveData.Length; i++)
                {
                    // Serialise each line in the array into a Object and add objects intoa list of objects
                    saveGames.Add(JsonSerializer.Deserialize<SaveGame>(saveData[i])!);

                    DropDownMenuCreated(saveGames[i], i);
                }
            }

            // Make the load game and overwrite save buttons not visible as they are not needed
            else
            {
                DropDownMenuVisibilityChanged(false);
            }
        }

        /// <summary>
        ///         Loads the game at the specified index into the current instance.
        /// </summary>
        /// <param name="indexToLoad">The index of the save in the save file to load.</param>
        internal void LoadGame(string saveName)
        {
            // Initialising a default value for indexToLoad, if not changed, the game will not be loaded
            int indexToLoad = -1;

            // Iterates through all of the objects in the saveGames list
            for (int i = 0; i < saveGames.Count; i++)
            {
                // Check if the object's saveName is equal to the sender object's name
                if (saveGames[i].saveName == saveName)
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
                PlayerNamesChanged(saveGames[indexToLoad].player1Name, saveGames[indexToLoad].player2Name);

                if (saveGames[indexToLoad].playerTurn != player) { SwapPlayer(); }
                // Iterate through the game board and change the tiles to game to load's tiles
                for (int i = 0; i < gameValueData!.GetLength(0); i++)
                {
                    for (int j = 0; j < gameValueData.GetLength(1); j++)
                    {
                        gameValueData[i, j] = saveGames[indexToLoad].gameData[i][j];
                        TileChanged(i, j, gameValueData[i, j].ToString());
                    }
                }

                // Display all of the current valid tiles to the screen
                GetValidTiles();

                // Get the player totals to display to screen
                GetPlayerTotals();

                // Set the settings of the game to the saved value
                InformationPanelVisibilityChanged(saveGames[indexToLoad].isInformationPanelVisible);
                TextToSpeechActiveChanged(saveGames[indexToLoad].isSpeechActivated);

                // Loading an active game so names shouldn't be editable
                PlayerNameAccessibilityChanged(false);

                // This game has just been loaded, so is saved, therefore set isGameSaved as true
                isGameSaved = true;
            }
        }

        /// <summary>
        ///         Checks there is not too many saves and whether the current save name is already taken.
        ///         If so, prompt to overwrite. Otherwise, save to new save slot.
        /// </summary>
        /// <param name="saveName">String value of the name to give to the save game</param>
        internal void CreateNewSave()
        {

            // Initialise boolean for whether the save name currently exists. 
            bool nameExists = false;


            // Check how many saveGame objects there as there can only be 5 save slots in the requirements
            if (saveGames.Count < 5)
            {
                // The default name for the save game, this ensures it will be unique everytime
                string defaultSaveName = DateTime.Now.ToString();

                // Speech if required
                if (textToSpeechActive)
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
                    for (int i = 0; i < saveGames.Count; i++)
                    {
                        if (saveGames[i].saveName == saveName)
                        {
                            nameExists = true;
                        }
                    }

                    // If the save name doesn't exist, save the game to a new slot
                    if (!nameExists)
                    {
                        // Create a jagged array as 2d arrays cannot be serialised using this serialiser
                        int[][] gameData = new int[8][];

                        // Copy the data from gameboard data to the new jagged array
                        for (int i = 0; i < gameValueData!.GetLength(0); i++)
                        {
                            gameData[i] = new int[8];
                            for (int j = 0; j < gameValueData.GetLength(1); j++)
                            {
                                gameData[i][j] = gameValueData[i, j];
                            }
                        }

                        // Create a new SaveGame object with the new data 
                        SaveGame newSave = new(saveName, playerNames[0], playerNames[1], gameData, player, textToSpeechActive, isInfoPanelVisible);

                        // Serialise and append this data to the save game file
                        File.AppendAllText(saveDataDirPath, newSave.Serialise() + "\n");

                        isGameSaved = true;
                    }
                }

                // Load the save games to the menu
                GetSaveGames();
            }
            else
            {
                // Speech if required
                if (textToSpeechActive)
                {
                   speechSynth!.Speak("You can only have 5 save game slots, choose a game to overwrite.");
                }
                MessageBox.Show("You can only have 5 save game slots, choose a game to overwrite.");
            }              
        }

        #endregion

    }
}  
