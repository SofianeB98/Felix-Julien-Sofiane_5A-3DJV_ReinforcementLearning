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
        Caisse,
        Player
    }

    class GameStateActionComparer : EqualityComparer<(SokobanGameState, IAction)>
    {
        public override bool Equals((SokobanGameState, IAction) a, (SokobanGameState, IAction) b)
        {
            bool t1 = a.Item1.Equals(b.Item1);
            bool t2 = false;

            if (a.Item2 == null && b.Item2 == null)
                t2 = true;
            else if ((a.Item2 == null && b.Item2 != null) || (a.Item2 != null && b.Item2 == null))
                t2 = false;
            else if (a.Item2 != null && b.Item2 != null)
                t2 = a.Item2.Equals(b.Item2);

                
            return t1 && t2;
        }

        public override int GetHashCode((SokobanGameState, IAction) gs)
        {
            return base.GetHashCode();
        }
    }

    class GameStateComparer : EqualityComparer<SokobanGameState>
    {
        public override bool Equals(SokobanGameState a, SokobanGameState b)
        {
            return a.Equals(b);
        }

        public override int GetHashCode(SokobanGameState gs)
        {
            return base.GetHashCode();
        }
    }

    public class Caisse
    {
        public GameObject visual;
        public Vector2Int position;

        public Caisse(Vector2Int pos, GameObject visual)
        {
            this.visual = visual;
            this.position = pos;
        }

        public void Move(Vector2Int direction)
        {
            this.position += direction;
        }

        public Caisse Clone()
        {
            var b = new Caisse(this.position, this.visual);
            return b;
        }

        public override bool Equals(object obj)
        {
            var b = obj as Caisse;
            if (b.position != this.position)
                return false;
            return true;
        }

        public bool CanMoveInDirection(Vector2Int direction, SokobanGameState gs)
        {
            var newPos = this.position + direction;
            switch (gs.Grid[newPos.x, newPos.y].state)
            {
                case State.Walkable:
                    return true;
                case State.Caisse:
                    return false;
                case State.Objective:
                    return true;
                case State.ObjectiveAccomplish:
                    return false;
                case State.Unwalkable:
                    return false;
                default:
                    return false;
            }
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
        public List<Caisse> caisses;

        public List<IAction> allActions;

        public float r = 0.0f;
        public float v = 0.0f;

        public float N = 0.0f;
        public float Returns = 0.0f;
        
        public (int, int) GridSize
        {
            get { return (Grid.GetLength(0), Grid.GetLength(1)); }
        }


        public SokobanGameState(Tile[,] grid, List<Caisse> caisses, List<IAction> allActions)
        {
            this.allActions = allActions;
            // Required for initialization
            this.Grid = grid;
            this.playerPosition = (from Tile item in this.Grid where item.state == State.Player select item).FirstOrDefault().position;
            this.caisses = caisses;
        }

        public SokobanGameState() { }

        public override bool Equals(object obj)
        {
            var gs = obj as SokobanGameState;
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    if (!Grid[i, j].Equals(gs.Grid[i, j]))
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < caisses.Count; i++)
            {
                if (!caisses[i].Equals(gs.caisses[i]))
                    return false;
            }

            if (!playerPosition.Equals(gs.playerPosition))
                return false;
            
            return true;
        }

        public List<IAction> GetAvailableActions()
        {
            if (CheckFinish())
                return new List<IAction>();

            List<IAction> actions = new List<IAction>();

            foreach (var item in allActions)
            {
                if (item.IsAvailable(this))
                {
                    actions.Add(item);
                }
            }
            return actions;
        }

        public bool CheckFinish()
        {
            foreach (var item in Grid)
            {
                if (item.state == State.Objective)
                    return false;
            }
            return true;
        }

        public SokobanGameState Clone()
        {
            var gs = new SokobanGameState();
            gs.caisses = new List<Caisse>();
            foreach (var item in this.caisses)
            {
                gs.caisses.Add(item.Clone());
            }
            gs.Grid = new Tile[Grid.GetLength(0), Grid.GetLength(1)];
            for (int i = 0; i < Grid.GetLength(0); i++)
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

        int GetRemainingObjectif()
        {
            int remainingObjective = 0;
            for(int i = 0; i < Grid.GetLength(0); i++)
            {
                for(int j = 0; j < Grid.GetLength(1); j++) 
                {
                    if(Grid[i, j].state == State.Objective) 
                    {
                        remainingObjective++;
                    }
                }
            }
            return remainingObjective;
        }

        

        public bool CheckGameOver()
        {
            int caisseBloque = 0;
            int objectifRestant = GetRemainingObjectif();
            foreach (var item in this.caisses)
            {
                // Si la caisse valide un objectif, on la passe. (M�me si elle est bloqu�e, elle est valide)
                if (Grid[item.position.x, item.position.y].state == State.ObjectiveAccomplish)
                    continue;
                var pos = item.position;
                // Check de tous les d�placements possibles
                bool[] movement = new bool[4];
                movement[0] = item.CanMoveInDirection(new Vector2Int(-1, 0), this);
                movement[1] = item.CanMoveInDirection(new Vector2Int(0, 1), this);
                movement[2] = item.CanMoveInDirection(new Vector2Int(1, 0), this);
                movement[3] = item.CanMoveInDirection(new Vector2Int(0, -1), this);

                
                for (int i = 0; i < movement.Length; i++)
                {  
                    if (!movement[i] && !movement[(i + 1) % 4])
                    {
                        caisseBloque++;
                        break;
                    }
                }
            }
            var caisseOk = this.caisses.Count - caisseBloque;
            // Game Over si il ne reste plus assez de caisse pouvant se d�placer
            if (caisseOk < objectifRestant)
                return true;
            return false;
        }
    }
}
