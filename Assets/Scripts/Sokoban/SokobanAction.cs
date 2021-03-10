using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban
{
    public interface IAction
    {
        public void Perform(ref SokobanGameState gameState);
    }

    public class MoveAction : IAction
    {
        public readonly Vector2Int direction;

        public MoveAction(Vector2Int direction)
        {
            this.direction = direction;
        }

        public void Perform(ref SokobanGameState gameState)
        {
            // Move
            var nextPos = gameState.playerPosition + this.direction;
            var nextTile = gameState.Grid[nextPos.x, nextPos.y];

            switch (nextTile.state)
            {
                case State.Bloc:
                    if (IsWalkableState(TestNextTileAfterBloc(nextPos, ref gameState)))
                    {
                        for (int i = 0; i < gameState.blocs.Count; i++)
                        {
                            if (gameState.blocs[i].position == nextPos)
                            {
                                gameState.Grid[gameState.playerPosition.x, gameState.playerPosition.y].state = State.Walkable;
                                gameState.blocs[i].Move(direction);
                                gameState.Grid[gameState.blocs[i].position.x, gameState.blocs[i].position.y].state = State.Bloc;
                                gameState.playerPosition = nextPos;
                                gameState.Grid[nextPos.x, nextPos.y].state = State.Player;
                                var blocPos = gameState.blocs[i].position;
                                CheckObjectif(ref gameState);

                                if(CheckFinish(ref gameState))
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
                    foreach (var bloc in gs.blocs)
                    {
                        if (bloc.position == item.position)
                        {
                            item.state = State.ObjectiveAccomplish;
                            break;
                        }
                    }
                }
                if(item.state == State.ObjectiveAccomplish) 
                {
                    var test = false;
                    foreach(var bloc in gs.blocs)
                    {
                        if(item.position == bloc.position)
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

        bool CheckFinish(ref SokobanGameState gs)
        {
            foreach(var item in gs.Grid)
            {
                if (item.state == State.Objective)
                    return false;
            }
            return true;
        }
    }
}
