using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * List d'action
 * coeff de reduction => gamma
 * ensemble d'état
 * Reward appliqué en passant d'un état à l'autre grace a une action
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
}

[System.Serializable]
public class GridParameter
{
    public Vector2Int gridSize;
    public GameObject gridCellPrefab;
    public int unwalkableCellCount = 1;
    public Vector2Int startState;
}

[System.Serializable]
public class Agent
{
    public List<Action> actions;
    
    public Vector2Int actualState;
    public Vector2Int targetState;


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

public class Action
{
    public string actionName;
    public float actionProbability;

    public void Perform()
    {

    }
}

public class GridWorld : MonoBehaviour
{
    public GridParameter gridParameter;
    public GridCell[,] grid; // = tout les etats
    public Agent agent;

    public Transform gridParent;

    public Camera cam; 

    private void Start()
    {
        if (gridParameter.gridSize.x == 0 || gridParameter.gridSize.y == 0)
        {
            gridParameter.gridSize = new Vector2Int(3, 3);
        }



        InitGrid();
        InitAgent();
    }

    private void InitGrid()
    {
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
            }
        }

        cam.transform.position = new Vector3(gridParameter.gridSize.x * 0.5f, gridParameter.gridSize.y * 0.5f, -5);
        cam.orthographic = true;
        cam.orthographicSize = ((gridParameter.gridSize.x + gridParameter.gridSize.y) * 0.5f + 5f) * 0.5f;
    }

    private void InitAgent()
    {
        this.agent.Init(new List<Action>(), gridParameter.startState, grid[0,0].position, 0.9f);
    }
}

