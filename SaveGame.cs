using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assignment1
{
    [Serializable]
    internal class SaveGame
    {
        public string saveName {  get; set; }
        public string player1Name { get; set; }
        public string player2Name { get; set; }
        public int playerTurn { get; set; }
        public int[][] gameData { get; set; }

        public SaveGame(string saveName, string player1Name, string player2Name, int[][] gameData, int playerTurn)
        {
            this.saveName = saveName;
            this.player1Name = player1Name;
            this.player2Name = player2Name;
            this.playerTurn = playerTurn;
            this.gameData = gameData;
        }

        public string Serialise()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
