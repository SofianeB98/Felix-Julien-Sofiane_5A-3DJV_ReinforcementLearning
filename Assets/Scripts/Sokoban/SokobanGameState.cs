using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Sokoban
{
    public enum State
    {
        Walkable,
        Unwalkable,
        Objective,
        ObjectiveAccomplish,
        Bloc,
        Player
    }

    [System.Serializable]
    public struct Tile
    {
        public Vector2Int position;
        public State state;
        public GameObject visual;

        public Tile(Vector2Int position, State state, GameObject visual)
        {
            this.position = position;
            this.state = state;
            this.visual = visual;
        }
    }

    [System.Serializable]
    public struct SokobanGameState
    {
        [Header("Game State Data")]
        public Tile[,] Grid;
        public Vector2Int playerPosition;
        
        public (int, int) GridSize
        {
            get { return (Grid.GetLength(0), Grid.GetLength(1)); }
        }


        public SokobanGameState(Tile[,] grid) 
        {
            // Required for initialization
            this.Grid = grid;
            this.playerPosition = (from Tile item in this.Grid where item.state == State.Player select item).FirstOrDefault().position;
        }

       
    }
}
