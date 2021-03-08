using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridCell
{
    public enum GridState
    {
        Walkable,
        End,
        Start,
        Unwalkable,
        Bonus
    }

    public Vector2Int position;
    public GridState state;

    public GameObject visual;

    public float r;
    public float v;
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


public class GridWorld : MonoBehaviour
{
    [Header("Grid")] public GridParameter gridParameter;
    public GridCell[,] grid; // = tout les etats
    public Transform gridParent;

    [Header("Agent")] public GameObject agentPrefab;
    public Agent agent;

    [Header("World Parameter")] public float stepTime = 1.0f;

    [Space(10)] public Camera cam;

    private void Start()
    {
        if ((gridParameter.gridSize.x == 0 || gridParameter.gridSize.y == 0) ||
            (gridParameter.gridSize.x == 1 && gridParameter.gridSize.y == 1))
        {
            gridParameter.gridSize = new Vector2Int(3, 3);
        }

        Initialisation();
    }

    private void Initialisation()
    {
        // Agent
        var rightAction = new MoveAction(new Vector2Int(1, 0));
        var leftAction = new MoveAction(new Vector2Int(-1, 0));
        var upAction = new MoveAction(new Vector2Int(0, 1));
        var downAction = new MoveAction(new Vector2Int(0, -1));

        var actions = new List<Action>();
        actions.Add(rightAction);
        actions.Add(leftAction);
        actions.Add(upAction);
        actions.Add(downAction);

        Dictionary<Vector2Int, List<Action>> actionsDic = new Dictionary<Vector2Int, List<Action>>();

        // Initialisation de la grid
        grid = new GridCell[gridParameter.gridSize.x, gridParameter.gridSize.y];
        for (int i = 0; i < gridParameter.gridSize.x; i++)
        {
            for (int j = 0; j < gridParameter.gridSize.y; j++)
            {
                // Initialisation du state
                grid[i, j] = new GridCell()
                {
                    position = new Vector2Int(i, j),
                    state = GridCell.GridState.Walkable
                };

                grid[i, j].visual = Instantiate(gridParameter.gridCellPrefab, new Vector3(i, j, 0.0f),
                    Quaternion.identity);
                grid[i, j].visual.transform.SetParent(gridParent);

                // Définition du reward initial
                switch (grid[i, j].state)
                {
                    case GridCell.GridState.Unwalkable:
                        grid[i, j].r = -1.0f;
                        grid[i, j].v = 0.0f;
                        break;

                    case GridCell.GridState.Start:
                    case GridCell.GridState.Walkable:
                        grid[i, j].r = 0.0f;
                        grid[i, j].v = 0.0f;
                        break;

                    case GridCell.GridState.Bonus:
                        grid[i, j].r = 1.0f;
                        grid[i, j].v = 0.0f;
                        break;

                    case GridCell.GridState.End:
                        grid[i, j].r = 10.0f;
                        grid[i, j].v = 0.0f;
                        break;
                }

                // Init actions
                var availableActions = new List<Action>();
                foreach (var act in actions)
                {
                    MoveAction a = act as MoveAction;
                    if (a.OutOfGridBound(grid[i, j].position + a.direction, ref gridParameter))
                        continue;

                    availableActions.Add(act);
                }
                actionsDic.Add(grid[i, j].position, availableActions);
            }
        }

        // Definition du start state & de l'end state
        gridParameter.startState =
            grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;
        grid[gridParameter.startState.x, gridParameter.startState.y].state = GridCell.GridState.Start;

        gridParameter.targetState =
            grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;

        if (gridParameter.targetState.Equals(gridParameter.startState))
            while (gridParameter.targetState.Equals(gridParameter.startState))
                gridParameter.targetState = grid[Random.Range(0, gridParameter.gridSize.x),
                    Random.Range(0, gridParameter.gridSize.y)].position;

        grid[gridParameter.targetState.x, gridParameter.targetState.y].visual.GetComponent<Renderer>().material
            .color = Color.green;
        grid[gridParameter.targetState.x, gridParameter.targetState.y].state = GridCell.GridState.End;
        grid[gridParameter.targetState.x, gridParameter.targetState.y].r = 10.0f;

        // Creation des obstacles


        // Creation des Bonus


        // Setup de la camera
        cam.transform.position = new Vector3(gridParameter.gridSize.x * 0.5f, gridParameter.gridSize.y * 0.5f, -5);
        cam.orthographic = true;
        cam.orthographicSize = ((gridParameter.gridSize.x + gridParameter.gridSize.y) * 0.5f + 5f) * 0.5f;

        // Initialisation de l'agent
        this.agent.Init(actionsDic, gridParameter.startState, gridParameter.targetState, 0.9f);

        this.agent.visual = Instantiate(agentPrefab,
            new Vector3(this.agent.actualState.x, this.agent.actualState.y, 0.0f), Quaternion.identity);

        this.agent.ValueIteration(ref grid);
    }




    private IEnumerator UpdateWorld()
    {
        while (true)
        {


            UpdateGridState(default);
            yield return new WaitForSeconds(stepTime);
        }


        yield break;
    }

    public void UpdateGridState(Vector2Int newAgentState)
    {
        // Update de la position de l'agent

        // Update des bonus si récolté

        // End de la simulation si agent == target


    }
}

