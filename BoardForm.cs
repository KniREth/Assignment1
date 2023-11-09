using GameboardGUI;
using Microsoft.VisualBasic.Devices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.VisualBasic;

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

        // Player 0 is white, player 1 is black. White plays first
        int player = 0;

        // The initial number of tiles each player has on the board
        int blackTiles = 2;
        int whiteTiles = 2;

        // All of the SaveGame objects which have been deserialised from the SaveGame.JSON file
        List<SaveGame> saveGames = new List<SaveGame>();

        // Check if the game is over
        bool isGameOver = false;

        // Valid tiles for each player
        List<Point> validTiles = new List<Point>();

        // The directory for the save game file
        string saveDataDirPath = Directory.GetCurrentDirectory() + @"\saves\SaveGame.JSON";

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
                GetSaveGames();
                validTiles = GetValidTiles();
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show(ex.ToString(), "Game board size problem", MessageBoxButtons.OK);
                this.Close();
            }


        }
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
                }
            }

            return boardArray;
        }

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
                    var TileCheck = IsTileValid(selectionRow, selectionCol, offsets[x], new List<Point>(), 0);
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
                                updatePlayerTotals(1, 1);

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
                    updatePlayerTotals(1, 0);
                    SwapPlayer();
                }
            }

            // Check to see if the game has ended
            CheckGameOver();

            // TODO: Place the check "stalemate" into its own function
            // TODO: Create a function to prompt the user to start a new game when the current game is over
            // If the player has no tiles left to choose from, but the game isn't over, switch players
            if (GetValidTiles().Count <= 0 && !isGameOver)
            {
                MessageBox.Show("No valid tiles, swapping player");
                SwapPlayer();

                // If the other player is in the same situation, the game is ended
                if (GetValidTiles().Count <= 0)
                {
                    MessageBox.Show("No more valid tiles for either players.");
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

            validTiles = GetValidTiles();

        }

        private void SwapPlayer()
        {
            // Swap to next player's turn
            player = (player + 1) % 2;

            // Swap arrow img for next player move
            if (player == 0) { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "left.PNG"; }
            else { picBoxPlayerToMove.ImageLocation = tileImagesDirPath + "right.PNG"; }
        }

        private void CheckGameOver()
        {
            // Check game over, 8x8 grid = 64 tiles, if all are filled, game is over
            if (blackTiles + whiteTiles >= 64)
            {
                // Check which player wins, whoever has most tiles
                if (whiteTiles > blackTiles)
                {
                    MessageBox.Show("Game over, White wins!");
                }
                else { MessageBox.Show("Game over, Black wins!"); }
                isGameOver = true;
            }
        }

        private List<Point> GetValidTiles()
        {
            List<Point> validTiles = new List<Point>();
            for (int i = 0; i < gameBoardData.GetLength(0); i++)
            {
                for (int j = 0; j < gameBoardData.GetLength(1); j++)
                {
                    if (gameBoardData[i, j] == 10)
                    {
                        for (int x = 0; x < offsets.Count; x++)
                        {
                            var isValid = IsTileValid(i, j, offsets[x], new List<Point>(), 0);
                            if (isValid.Item1.Count > 0 && isValid.Item2)
                            {
                                validTiles.Add(new Point(i, j));
                            }
                        }
                    }
                }
            }
            for (int y = 0; y < validTiles.Count; y++)
            {
                _gameBoardGui.SetTile(validTiles[y].X, validTiles[y].Y, "Available");
            }
            return validTiles;
        }
        private (List<Point>, bool) IsTileValid(int offsetRow, int offsetCol, Point currentOffset, List<Point> visitedTiles, int iterations)
        {
            offsetRow += currentOffset.X;
            offsetCol += currentOffset.Y;

            // Is the value out of range?
            if (offsetRow < 0 || offsetCol < 0 || offsetRow >= 8 || offsetCol >= 8)
            {
                return (new List<Point>(), false);
            }

            // Is the offset value empty?
            if (gameBoardData[offsetRow, offsetCol] == 10)
            {
                return (new List<Point>(), false);
            }
            // Is the offset value the player?
            if (gameBoardData[offsetRow, offsetCol] != player)
            {
                // Add the tile to the visited tile array to be changed if the path is valid
                visitedTiles.Add(new Point(offsetRow, offsetCol));
                var valid = IsTileValid(offsetRow, offsetCol, currentOffset, visitedTiles, iterations++);
                return (valid.Item1, true);
            }

            return (visitedTiles, false);
        }

        private void updateTiles(int moves, int offsetRow, int offsetCol, int iterations, List<Point> visitedTiles)
        {
            // Update the visited tiles, this will be the tiles between the clicked tile and the end tile
            if (moves > 0 && gameBoardData[offsetRow, offsetCol] != 10 && iterations == 0)
            {
                for (int i = 0; i < visitedTiles.Count(); i++)
                {
                    gameBoardData[visitedTiles[i].X, visitedTiles[i].Y] = player;
                    _gameBoardGui.SetTile(visitedTiles[i].X, visitedTiles[i].Y, player.ToString());
                    updatePlayerTotals(1, 1);
                }
            }
        }

        private void updatePlayerTotals(int valueToAdd, int valueToRemove)
        {
            if (player == 0)
            {
                whiteTiles += valueToAdd;
                blackTiles -= valueToRemove;
            }
            else
            {
                blackTiles += valueToAdd;
                whiteTiles -= valueToRemove;
            }
            lblP1Val.Text = whiteTiles.ToString() + " x";
            lblP2Val.Text = blackTiles.ToString() + " x";

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            p1NameEntered = true;
        }

        private void txtBoxP2Name_TextChanged(object sender, EventArgs e)
        {
            p2NameEntered = true;
        }

        // Pressing new game button
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult choice = MessageBox.Show("Warning, any unsaved progress will be lost.\nContinue?", "New Game", MessageBoxButtons.YesNo);
            if (choice == DialogResult.Yes)
            {
                int[,] gameData = MakeBoardArray();
                for (int i = 0; i < gameBoardData.GetLength(0); i++)
                {
                    for (int j = 0; j < gameBoardData.GetLength(1); j++)
                    {
                        _gameBoardGui.SetTile(i, j, gameData[i, j].ToString());
                        gameBoardData[i, j] = gameData[i, j];
                    }
                }
                GetValidTiles();
            }
        }

        private void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool nameExists = false;
            if (saveGames.Count < 5)
            {
                // TODO: Create a new default string to place in the input box that will be unique. I.E. Date&Time
                string saveName = Microsoft.VisualBasic.Interaction.InputBox("Enter name of save", "Save game");
                if (!String.IsNullOrEmpty(saveName))
                {
                    for (int i = 0; i < saveGames.Count; i++)
                    {
                        if (saveGames[i].saveName == saveName)
                        {
                            nameExists = true;
                        }
                    }
                    if (nameExists) 
                    {
                        DialogResult choice = MessageBox.Show("Warning, game name already exists.\nOverwrite??", "Game Exists", MessageBoxButtons.YesNo);
                        if (choice == DialogResult.Yes) { OverwriteSave(saveName); }
                    }
                    if (!nameExists)
                    {
                        int[][] gameData = new int[8][];
                        for (int i = 0; i < gameBoardData.GetLength(0); i++)
                        {
                            gameData[i] = new int[8];
                            for (int j = 0; j < gameBoardData.GetLength(1); j++)
                            {
                                gameData[i][j] = gameBoardData[i, j];
                            }
                        }
                        SaveGame newSave = new SaveGame(saveName, txtBoxP1Name.Text, txtBoxP2Name.Text, gameData, player);
                        File.AppendAllText(saveDataDirPath, newSave.Serialise() + "\n");
                    }
                }
            }
            GetSaveGames();
        }

        private void OverwriteSave(string saveName)
        {
            // Delete the initial file
            File.Delete(saveDataDirPath);

            // Iterate through all of the SaveGame objects in the list
            for (int i = 0; i < saveGames.Count; i++)
            {
                // If the saveGame object's saveName is the same as the param, overwrite that object
                if (saveGames[i].saveName == saveName)
                {
                    saveGames[i].player1Name = txtBoxP1Name.Text;
                    saveGames[i].player2Name = txtBoxP2Name.Text;
                    saveGames[i].playerTurn = player;
                    for (int j = 0;j < gameBoardData.GetLength(0); j++)
                    {
                        for (int x = 0; x < gameBoardData.GetLength(1);  x++)
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

        private int GetSaveGames()
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

            // Read the save game file and save data to the string array
            // Each line in the file will be a separate index in the array
            string[] saveData = File.ReadAllLines(saveDataDirPath);

            // Iterate through the string array, hence iterate through the data in the file
            for (int i = 0; i < saveData.Length; i++)
            {
                // Serialise each line in the array into a Object and add objects intoa list of objects
                saveGames.Add(JsonSerializer.Deserialize<SaveGame>(saveData[i]));

                // Create a new drop down item for the load game drop down and insert it
                ToolStripMenuItem newItem = new ToolStripMenuItem { Name = "New save " + i.ToString(), Text = saveGames[i].saveName };
                loadGameToolStripMenuItem.DropDownItems.Insert(i, newItem);

                // Event handler for the new drop down item, when clicked, LoadGame function will run
                loadGameToolStripMenuItem.DropDownItems[i].Click += new EventHandler(LoadGame);
            }

            // Return back the amount of items in the array for iteration purposes
            // TODO: This may not be needed, could possibly instead use saveGames.Count which should have same value but
            // instead would be a pulic variable which therefore negates the need of a return
            return saveData.Length;
        }

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
                // Iterate through the game board and change the tiles to game to load's tiles
                for (int i = 0; i < gameBoardData.GetLength(0); i++)
                {
                    for (int j = 0; j < gameBoardData.GetLength(1); j++)
                    {
                        gameBoardData[i, j] = saveGames[indexToLoad].gameData[i][j];
                        _gameBoardGui.SetTile(i, j, gameBoardData[i,j].ToString());
                    }
                }

                // Display all of the current valid tiles to the screen
                GetValidTiles();
            }
        }
    }
}