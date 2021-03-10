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

    public class Bloc 
    {
        public GameObject visual;
        public Vector2Int position;

        public Bloc(Vector2Int pos, GameObject visual)
        {
            this.visual = visual;
            this.position = pos;
        }

        public void Move(Vector2Int direction)
        {
            this.position += direction;
        }

        public Bloc Clone()
        {
            var b = new Bloc(this.position, this.visual);
            return b;
            
        }
    }

    [System.Serializable]
    public class Tile
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

        public Tile Clone()
        {
            var t = new Tile(this.position, this.state, this.visual);
            return t;
        }
    }

    [System.Serializable]
    public class SokobanGameState
    {
        [Header("Game State Data")]
        public Tile[,] Grid;
        public Vector2Int playerPosition;
        public List<Bloc> blocs;

        public (int, int) GridSize
        {
            get { return (Grid.GetLength(0), Grid.GetLength(1)); }
        }


        public SokobanGameState(Tile[,] grid, List<Bloc> blocs)
        {
            // Required for initialization
            this.Grid = grid;
            this.playerPosition = (from Tile item in this.Grid where item.state == State.Player select item).FirstOrDefault().position;
            this.blocs = blocs;

            
        }

        public SokobanGameState() { }

        public SokobanGameState Clone() 
        {
            var gs = new SokobanGameState();
            gs.blocs = new List<Bloc>();
            foreach(var item in this.blocs)
            {
                gs.blocs.Add(item.Clone());
            }
            gs.Grid = new Tile[Grid.GetLength(0), Grid.GetLength(1)];
            for(int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++) 
                {
                    gs.Grid[i, j] = Grid[i, j].Clone();
                }
            }
            gs.playerPosition = playerPosition;
            return gs;
        }

       
    }
}
