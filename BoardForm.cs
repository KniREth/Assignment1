using GameboardGUI;

namespace Assignment1
{
    public partial class BoardForm : Form
    {
        // Initialise the number of rows and columns for the board
        const int numRows = 8;
        const int numCols = 8;

        int player = 0;

        int blackTiles = 2;
        int whiteTiles = 2;

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

        // Initialise array of images for gameboard
        GameboardImageArray _gameBoardGui;
        int[,] gameBoardData;
        string tileImagesDirPath = Directory.GetCurrentDirectory() + @"\images\";
        public BoardForm()
        {
            InitializeComponent();

            Point top = new Point(10, 30);
            Point bottom = new Point(10, 60);

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

        }
        private int[,] MakeBoardArray()
        {
            int[,] boardArray = new int[numRows, numCols];
            int boardVal = 0;

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    if ((row == 4 && col == 4) || (row == 3 && col == 3))
                    { boardVal = 0; }
                    else if ((row == 3 && col == 4) || (row == 4 && col == 3))
                    { boardVal = 1; }
                    else boardVal = 10;

                    boardArray[row, col] = boardVal;
                }
            }

            return boardArray;
        }

        private void GameTileClicked(object sender, EventArgs e)
        {
            bool moveCheck = false;

            int selectionRow = _gameBoardGui.GetCurrentRowIndex(sender);
            int selectionCol = _gameBoardGui.GetCurrentColumnIndex(sender);
            // MessageBox.Show($"You just clicked the square at row {selectionRow} and col {selectionCol}");

            // Iterate though the offsets, checking the path
            for (int x = 0; x < offsets.Count; x++)
            {
                if (IsTileValid(selectionRow, selectionCol, offsets[x], 0, new List<Point>()) > 0)
                {
                    gameBoardData[selectionRow, selectionCol] = player;

                    _gameBoardGui.SetTile(selectionRow, selectionCol, player.ToString());
                    // updatePlayerTotals(1,0);

                    moveCheck = true;

                }
            }

            if (moveCheck) { player = (player + 1) % 2; }

        }

        private int IsTileValid(int offsetRow, int offsetCol, Point currentOffset, int moves, List<Point> visitedTiles)
        {
            offsetRow += currentOffset.X;
            offsetCol += currentOffset.Y;

            // MessageBox.Show("Row val: " + offsetRow.ToString() + " Col val" + offsetCol.ToString()
            //    + "\n Board val: " + gameBoardData[offsetRow, offsetCol]);

            // Is the value out of range?
            if (offsetRow < 0 || offsetCol < 0 || offsetRow > 8 || offsetCol > 8)
            {
                return 0;
            }

            // Is the offset value empty?
            if (gameBoardData[offsetRow, offsetCol] == 10)
            {
                return 0;
            }
            // Is the offset value the player?
            if (gameBoardData[offsetRow, offsetCol] != player)
            {
                // Add the tile to the visited tile array to be changed if the path is valid
                visitedTiles.Add(new Point(offsetRow, offsetCol));
                // MessageBox.Show("New visited tile" + visitedTiles[visitedTiles.Count() - 1]);
                moves++;
                moves = IsTileValid(offsetRow, offsetCol, currentOffset, moves, visitedTiles);
            }

            // MessageBox.Show(moves.ToString());
            // Update the visited tiles, this will be the tiles between the clicked tile and the end tile
            if (moves > 0 && gameBoardData[offsetRow, offsetCol] != 10)
            {
                // MessageBox.Show(offsetRow + " " + offsetCol);
                // MessageBox.Show(gameBoardData[offsetRow, offsetCol].ToString());
                for (int i = 0; i < visitedTiles.Count(); i++)
                {
                    gameBoardData[visitedTiles[i].X, visitedTiles[i].Y] = player;
                    _gameBoardGui.SetTile(visitedTiles[i].X, visitedTiles[i].Y, player.ToString());
                    updatePlayerTotals(1,1);
                }
            }

            return moves;
        }

        private void updatePlayerTotals(int valueToAdd, int valueToRemove)
        {
            if (player == 0)
            {
                MessageBox.Show(valueToAdd.ToString() + " added white. Removed " + valueToRemove.ToString());
                whiteTiles += valueToAdd;
                blackTiles -= valueToRemove;
            }
            else
            {
                MessageBox.Show(valueToAdd.ToString() + " added black. Removed " + valueToRemove.ToString());
                blackTiles += valueToAdd;
                whiteTiles -= valueToRemove;
            }
            lblP1Val.Text = whiteTiles.ToString();
            lblP2Val.Text = blackTiles.ToString();
        }
    }
}