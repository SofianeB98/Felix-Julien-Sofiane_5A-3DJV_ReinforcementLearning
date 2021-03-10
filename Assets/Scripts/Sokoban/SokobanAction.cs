using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban
{
    public interface IAction
    {
        bool Perform(ref SokobanGameState gameState);
        bool IsAvailable(SokobanGameState gameState);

        object DebugAction();
        
    }

    public class MoveAction : IAction
    {
        public readonly Vector2Int direction;

        public MoveAction(Vector2Int direction)
        {
            this.direction = direction;
        }

        public bool Perform(ref SokobanGameState gameState)
        {
            // Move
            var nextPos = gameState.playerPosition + this.direction;
            var nextTile = gameState.Grid[nextPos.x, nextPos.y];
            var res = false;
            switch (nextTile.state)
            {
                case State.Caisse:
                    if (IsWalkableState(TestNextTileAfterBloc(nextPos, ref gameState)))
                    {
                        for (int i = 0; i < gameState.caisses.Count; i++)
                        {
                            if (gameState.caisses[i].position == nextPos)
                            {
                                gameState.Grid[gameState.playerPosition.x, gameState.playerPosition.y].state = State.Walkable;
                                gameState.caisses[i].Move(direction);
                                gameState.Grid[gameState.caisses[i].position.x, gameState.caisses[i].position.y].state = State.Caisse;
                                gameState.playerPosition = nextPos;
                                gameState.Grid[nextPos.x, nextPos.y].state = State.Player;
                                var blocPos = gameState.caisses[i].position;
                                var caisse = gameState.caisses[i];
                                if (gameState.Grid[caisse.position.x, caisse.position.y].state == State.Objective)
                                {
                                    gameState.Grid[caisse.position.x, caisse.position.y].state = State.ObjectiveAccomplish;
                                    res = true;
                                }

                                if (gameState.CheckFinish())
                                {
                                    Debug.Log("FINISH");
                                }
                            }
                        }

                    }
                    break;
                case State.Walkable:
                    gameState.Grid[gameState.playerPosition.x, gameState.playerPosition.y].state = State.Walkable;
                    gameState.playerPosition += direction;
                    gameState.Grid[gameState.playerPosition.x, gameState.playerPosition.y].state = State.Player;
                    break;
                case State.Unwalkable:
                    break;


            }
            return res;
        }

        public bool IsAvailable(SokobanGameState gameState)
        {
            var pos = gameState.playerPosition + direction;
            var t = gameState.Grid[pos.x, pos.y];
            switch (t.state)
            {
                case State.Walkable:
                    return true;
                case State.Objective:
                    return true;
                case State.Unwalkable | State.ObjectiveAccomplish:
                    return false;
                case State.Caisse:
                    var nextPos = pos + direction;
                    var nextTile = gameState.Grid[nextPos.x, nextPos.y];
                    if (nextTile.state == State.Objective || nextTile.state == State.Walkable)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        State TestNextTileAfterBloc(Vector2Int pos, ref SokobanGameState gameState)
        {
            return gameState.Grid[pos.x + this.direction.x, pos.y + this.direction.y].state;
        }

        bool IsWalkableState(State state)
        {
            if (state == State.Walkable || state == State.Objective)
                return true;
            return false;
        }

        void CheckObjectif(ref SokobanGameState gs)
        {
            foreach (var item in gs.Grid)
            {
                if (item.state == State.Objective)
                {
                    foreach (var bloc in gs.caisses)
                    {
                        if (bloc.position == item.position)
                        {
                            item.state = State.ObjectiveAccomplish;
                            break;
                        }
                    }
                }
                if (item.state == State.ObjectiveAccomplish)
                {
                    var test = false;
                    foreach (var bloc in gs.caisses)
                    {
                        if (item.position == bloc.position)
                        {
                            test = true;
                            break;
                        }
                    }
                    if (!test)
                    {
                        item.state = State.Objective;
                    }
                }
            }

        }

        public object DebugAction()
        {
            return this.direction;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is MoveAction)) 
            {
                return false;
            }
            var m = obj as MoveAction;
            if(this.direction == m.direction)
            {
                return true;
            }
            return false;
        }

    }
}
