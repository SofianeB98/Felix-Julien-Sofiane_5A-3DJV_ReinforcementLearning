using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveAction : Action 
{
    public readonly Vector2Int direction;

    public MoveAction(Vector2Int direction) 
    {
        this.direction = direction;
    }

    bool OutOfGridBound(Vector2Int newPos, ref GridParameter gridParams)
    {
        if(newPos.x < 0 || newPos.x > gridParams.gridSize.x - 1
            || newPos.y < 0 || newPos.y > gridParams.gridSize.y - 1)
        {
            return true;
        }
        return false;
    }

    public override void Perform(Agent agent, GridParameter gridParams, GridWorld env) 
    {
        var newPos = agent.actualState + direction;
        if (OutOfGridBound(newPos, ref gridParams))
            return;
        agent.actualState = newPos;
    }
}
