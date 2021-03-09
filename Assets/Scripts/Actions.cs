using UnityEngine;

[System.Serializable]
public abstract class Action
{
    public string actionName;

    public abstract void Perform(AgentGridWorld agentGridWorld, GridParameter gridParams, GridWorld env);
}

[System.Serializable]
public class MoveAction : Action 
{
    public readonly Vector2Int direction;

    public MoveAction(Vector2Int direction, string name)
    {
        this.actionName = name;
        this.direction = direction;
    }

    public bool OutOfGridBound(Vector2Int newPos, ref GridParameter gridParams)
    {
        if(newPos.x < 0 || newPos.x > gridParams.gridSize.x - 1
            || newPos.y < 0 || newPos.y > gridParams.gridSize.y - 1)
            return true;
        

        return false;
    }

    public override void Perform(AgentGridWorld agentGridWorld, GridParameter gridParams, GridWorld env) 
    {
        var newPos = agentGridWorld.actualState + direction;
        if (OutOfGridBound(newPos, ref gridParams))
            return;
        agentGridWorld.actualState = newPos;
    }
}
