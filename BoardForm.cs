using GameboardGUI;

namespace Assignment1
{
    public partial class BoardForm : Form
    {
        // Initialise the number of rows and columns for the board
        const int numRows = 8;
        const int numCols = 8;

        int player = 0;

        int[,] offsets = { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };
        int movesDownPath;

        // Initialise array of images for gameboard
        GameboardImageArray _gameBoardGui;
        int[,] gameBoardData;
        string tileImagesDirPath = Directory.GetCurrentDirectory() + @"\images\";
        public BoardForm()
        {
            InitializeComponent();

            Point top = new Point(10, 30);
            Point bottom = new Point(10, 50);

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
            int selectionRow = _gameBoardGui.GetCurrentRowIndex(sender);
            int selectionCol = _gameBoardGui.GetCurrentColumnIndex(sender);
            MessageBox.Show($"You just clicked the square at row {selectionRow} and col {selectionCol}");
            if (IsTileValid(selectionRow, selectionCol, 0, new List<Point> ) > 0)
            {
                gameBoardData[selectionRow, selectionCol] = player;
                _gameBoardGui.SetTile(selectionRow, selectionCol, player.ToString());
            }

        }

        private int IsTileValid(int row, int col, int moves, List<Point> visitedTiles)
        {
            int offsetRow = 1 + row;
            int offsetCol = 0 + col;
            MessageBox.Show("Row val: " + offsetRow.ToString() + " Col val" + offsetCol.ToString()
                + "\n Board val: " + gameBoardData[offsetRow, offsetCol]);

            // Is the offset value empty? Is the offset value the player?
            if (gameBoardData[offsetRow, offsetCol] != 10 && gameBoardData[offsetRow, offsetCol] != player)
            {
                visitedTiles.Add(new Point(offsetRow, offsetCol));
                moves++;
                IsTileValid(offsetRow, offsetCol, moves, visitedTiles);
            }

            return moves;
        }
        
    }
}