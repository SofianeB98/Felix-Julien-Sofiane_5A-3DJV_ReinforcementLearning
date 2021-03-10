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

    class GameStateComparer : EqualityComparer<SokobanGameState> 
    {
        public override bool Equals(SokobanGameState a, SokobanGameState b)
        {
            Debug.Log("Compare");
            for (int i = 0; i < a.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < b.Grid.GetLength(1); j++)
                {
                    if (!a.Grid[i, j].Equals(b.Grid[i, j]))
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < a.blocs.Count; i++)
            {
                if (!a.blocs[i].Equals(b.blocs[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode(SokobanGameState gs)
        {
            return base.GetHashCode();
        }
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

        public override bool Equals(object obj)
        {
            var b = obj as Bloc;
            if (b.position != this.position)
                return false;
            return true;
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

        public override bool Equals(object obj)
        {
            var tile = obj as Tile;
            if (tile.position != this.position)
                return false;
            if (tile.state != this.state)
                return false;
            return true;
        }
    }

    [System.Serializable]
    public class SokobanGameState
    {
        [Header("Game State Data")]
        public Tile[,] Grid;
        public Vector2Int playerPosition;
        public List<Bloc> blocs;
        public List<IAction> allActions;
        public (int, int) GridSize
        {
            get { return (Grid.GetLength(0), Grid.GetLength(1)); }
        }


        public SokobanGameState(Tile[,] grid, List<Bloc> blocs, List<IAction> allActions)
        {
            this.allActions = allActions;
            // Required for initialization
            this.Grid = grid;
            this.playerPosition = (from Tile item in this.Grid where item.state == State.Player select item).FirstOrDefault().position;
            this.blocs = blocs;

            
        }

        public SokobanGameState() { }

        public override bool Equals(object obj)
        {
            var gs = obj as SokobanGameState;
            for(int i = 0; i < Grid.GetLength(0); i++)
            {
                for(int j = 0; j < Grid.GetLength(1); j++) 
                {
                    if(!Grid[i, j].Equals(gs.Grid[i, j]))
                    {
                        return false;
                    }
                }
            }

            for(int i = 0; i < blocs.Count; i++) 
            {
                if (!blocs[i].Equals(gs.blocs[i]))
                    return false;
            }
            return true;
        }

        public List<IAction> GetAvailableActions() 
        {
            List<IAction> actions = new List<IAction>();
            
            foreach(var item in allActions) 
            {
                if (item.IsAvailable(this)) 
                {
                    actions.Add(item);
                }
            }
            return actions;
        }

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
            gs.allActions = this.allActions;
            return gs;
        }

       
    }
}
