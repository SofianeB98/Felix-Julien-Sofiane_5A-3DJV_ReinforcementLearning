using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * List d'action
 * coeff de reduction => gamma
 * ensemble d'�tat
 * Reward appliqu� en passant d'un �tat � l'autre grace a une action
 *
 * Un agent
 * PolicyEvaluation()
 * PolicyImprovement()
 * ValueIteration()
 *
 */

[System.Serializable]
public class GridCell
{
    public enum GridState
    {
        Walkable,
        End,
        Start,
        Unwalkable,
        Stars
    }

    public Vector2Int position;
    public GridState state;

    public GameObject visual;

    public float r;
}

[System.Serializable]
public class GridParameter
{
    public Vector2Int gridSize;
    public GameObject gridCellPrefab;
    public int unwalkableCellCount = 1;
    public int bonusCount = 1;
    public Vector2Int startState;
    public Vector2Int targetState;
}

[System.Serializable]
public class Agent
{
    public List<Action> actions;
    
    public Vector2Int actualState;
    public Vector2Int targetState;

    public GameObject visual;

    public float gamma;

    public float reward = 0.0f;

    public void Init(List<Action> availableActions, Vector2Int startState, Vector2Int targetState, float gamma)
    {
        reward = 0.0f;
        this.gamma = gamma;
        this.actions = availableActions;
        this.actualState = startState;
        this.targetState = targetState;

        
    }
}

[System.Serializable]
public abstract class Action
{
    public string actionName;
    public float actionProbability;

    public abstract void Perform(Agent agent, GridParameter gridParams, GridWorld env);
}

public class GridWorld : MonoBehaviour
{
    public GridParameter gridParameter;
    public GridCell[,] grid; // = tout les etats

    public GameObject agentPrefab;
    public Agent agent;

    public Transform gridParent;

    public Camera cam; 

    private void Start()
    {
        if ((gridParameter.gridSize.x == 0 || gridParameter.gridSize.y == 0) ||
            (gridParameter.gridSize.x == 1 && gridParameter.gridSize.y == 1))
        {
            gridParameter.gridSize = new Vector2Int(3, 3);
        }

        InitGrid();
        InitAgent();
    }

    private void InitGrid()
    {
        // Initialisation de la grid
        grid = new GridCell[gridParameter.gridSize.x, gridParameter.gridSize.y];

        for (int i = 0; i < gridParameter.gridSize.x; i++)
        {
            for (int j = 0; j < gridParameter.gridSize.y; j++)
            {
                grid[i, j] = new GridCell()
                {
                    position = new Vector2Int(i, j),
                    state = GridCell.GridState.Walkable
                };

                grid[i, j].visual = Instantiate(gridParameter.gridCellPrefab, new Vector3(i, j, 0.0f),
                    Quaternion.identity);

                grid[i, j].visual.transform.SetParent(gridParent);

                switch (grid[i,j].state)
                {
                    case GridCell.GridState.Unwalkable:
                        grid[i, j].r = -1000.0f;
                        break;

                    case GridCell.GridState.Start:
                    case GridCell.GridState.Walkable:
                        grid[i, j].r = -1.0f;
                        break;

                    case GridCell.GridState.Stars:
                        grid[i, j].r = 10.0f;
                        break;

                    case GridCell.GridState.End:
                        grid[i, j].r = 1000.0f;
                        break;
                }
            }
        }

        // Definition du start state & de l'end state
        gridParameter.startState = grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;

        gridParameter.targetState = grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;

        if(gridParameter.targetState.Equals(gridParameter.startState))
            while(gridParameter.targetState.Equals(gridParameter.startState))
                gridParameter.targetState = grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;

        grid[gridParameter.targetState.x, gridParameter.targetState.y].visual.GetComponent<Renderer>().material.color = Color.green;

        // Cr�ation des obstacles

        // Cr�ation des Bonus

        // Setup de la camera
        cam.transform.position = new Vector3(gridParameter.gridSize.x * 0.5f, gridParameter.gridSize.y * 0.5f, -5);
        cam.orthographic = true;
        cam.orthographicSize = ((gridParameter.gridSize.x + gridParameter.gridSize.y) * 0.5f + 5f) * 0.5f;
    }

    private void InitAgent()
    {
        var actions = new List<Action>();
        actions.Add(new MoveAction(new Vector2Int(1, 0)));
        actions.Add(new MoveAction(new Vector2Int(-1, 0)));
        actions.Add(new MoveAction(new Vector2Int(0, 1)));
        actions.Add(new MoveAction(new Vector2Int(0, -1)));
        this.agent.Init(actions, gridParameter.startState, grid[0,0].position, 0.9f);

        this.agent.visual = Instantiate(agentPrefab, new Vector3(this.agent.actualState.x, this.agent.actualState.y, 0.0f), Quaternion.identity);
    }
}

