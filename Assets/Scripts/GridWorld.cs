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
        Bloc,
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
    [HideInInspector]public int bonusCount = 1;
    public Vector2Int startState;
    public Vector2Int targetState;
}

public enum GridWorld_Algo
{
    VALUE_ITERATION,
    POLICY_ITERATION
}

public class GridWorld : MonoBehaviour
{
    [Header("Grid")] 
    public GridParameter gridParameter;
    public GridCell[,] grid; // = tout les etats
    public Transform gridParent;

    [Header("Agent")] public GameObject agentPrefab;
    public AgentGridWorld agentGridWorld;

    [Header("World Parameter")]
    public float stepTime = 0.1f;

    [Header("Debug Grid")]
    public Texture upArrow;
    public Texture downArrow;
    public Texture rightArrow;
    public Texture leftArrow;

    [Space(10)] 
    
    public Camera cam;
    
    [Header("Agent Settings")]
    public GridWorld_Algo algo;
    public float theta = 0.005f;
    public float gamma = 0.9f;

    private void Start()
    {
        if ((gridParameter.gridSize.x == 0 || gridParameter.gridSize.y == 0) ||
            (gridParameter.gridSize.x == 1 && gridParameter.gridSize.y == 1))
        {
            gridParameter.gridSize = new Vector2Int(3, 3);
        }

        Initialisation();
    }

    // Initialise la map + l'agent
    private void Initialisation()
    {
        this.agentGridWorld = new AgentGridWorld();
        
        // Initialisation des actions possible
        var rightAction = new MoveAction(new Vector2Int(1, 0), "DR");
        var leftAction = new MoveAction(new Vector2Int(-1, 0), "DL");
        var upAction = new MoveAction(new Vector2Int(0, 1), "DU");
        var downAction = new MoveAction(new Vector2Int(0, -1), "DD");

        // On les ajoutes dans une liste afin de definir tout les actions possible pour un etat donne
        var actions = new List<Action>();
        actions.Add(rightAction);
        actions.Add(leftAction);
        actions.Add(upAction);
        actions.Add(downAction);

        // On declare un dictionnaire afin d'y stocker les action possible pour un etat donne
        Dictionary<Vector2Int, List<Action>> actionsDic = new Dictionary<Vector2Int, List<Action>>();

        // Initialisation de la grid
        grid = new GridCell[gridParameter.gridSize.x, gridParameter.gridSize.y];
        for (int i = 0; i < gridParameter.gridSize.x; i++)
        {
            for (int j = 0; j < gridParameter.gridSize.y; j++)
            {
                // Initialisation du state par defaut et de la position de la grid
                grid[i, j] = new GridCell()
                {
                    position = new Vector2Int(i, j),
                    state = GridCell.GridState.Walkable
                };

                // Instanciation du visuel de la case
                grid[i, j].visual = Instantiate(gridParameter.gridCellPrefab, new Vector3(i, j, 0.0f),
                    Quaternion.identity);
                grid[i, j].visual.transform.SetParent(gridParent);

                // Definition du reward initial
                grid[i, j].r = -1.0f;
                grid[i, j].v = 0.0f;

                // Initialisation vide des actions pour le state [i, j]
                var availableActions = new List<Action>();
                actionsDic.Add(grid[i, j].position, availableActions);
            }
        }

        // Definition du start state (pas utile pour le training de l'agent)
        gridParameter.startState = grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;
        grid[gridParameter.startState.x, gridParameter.startState.y].state = GridCell.GridState.Start;

        // Definition du end state (décommenter la ligne 132 et commenter la 133 pour que ce soit aleatoire)
        // gridParameter.targetState = grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;
        gridParameter.targetState = grid[0, 0].position;
        if (gridParameter.targetState.Equals(gridParameter.startState))
            while (gridParameter.targetState.Equals(gridParameter.startState))
                gridParameter.targetState = grid[Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y)].position;

        // Setup du visuel du end state
        grid[gridParameter.targetState.x, gridParameter.targetState.y].visual.GetComponent<Renderer>().material.color = Color.green;
        grid[gridParameter.targetState.x, gridParameter.targetState.y].state = GridCell.GridState.End;

        // Set du reward du end state
        grid[gridParameter.targetState.x, gridParameter.targetState.y].r = 0.0f;
        grid[gridParameter.targetState.x, gridParameter.targetState.y].v = 1000.0f;

        // Reset des action possible au end state, on est a la fin donc on ne fait plus rien !
        actionsDic[gridParameter.targetState] = new List<Action>();

        // Creation des obstacles
        for (int i = 0; i < gridParameter.unwalkableCellCount; i++)
        {
            Vector2Int idx = Vector2Int.zero;

            do
            {
                idx = new Vector2Int(Random.Range(0, gridParameter.gridSize.x), Random.Range(0, gridParameter.gridSize.y));
            } while (grid[idx.x, idx.y].state.Equals(GridCell.GridState.End)
                     || grid[idx.x, idx.y].state.Equals(GridCell.GridState.Start)
                     || ThereIsAnOtherObstacleSoClose(idx));

            grid[idx.x, idx.y].state = GridCell.GridState.Bloc;
            grid[idx.x, idx.y].r = 0.0f;
            grid[idx.x, idx.y].v = 0.0f;
            grid[idx.x, idx.y].visual.GetComponent<Renderer>().material.color = Color.black;

            // On remove les actions possible sur les bloc
            actionsDic[grid[idx.x, idx.y].position] = new List<Action>();
        }

        // Definition des actions possible pour chaque etat
        for (int i = 0; i < gridParameter.gridSize.x; i++)
        {
            for (int j = 0; j < gridParameter.gridSize.y; j++)
            {
                if (grid[i, j].state.Equals(GridCell.GridState.End) ||
                   grid[i, j].state.Equals(GridCell.GridState.Bloc))
                    continue;

                // Initialisation des actions possible pour le state [i, j]
                foreach (var act in actions)
                {
                    MoveAction a = act as MoveAction;
                    if (a.OutOfGridBound(grid[i, j].position + a.direction, ref gridParameter))
                        continue;

                    var nxt = grid[i, j].position + a.direction;
                    if (grid[nxt.x, nxt.y].state.Equals(GridCell.GridState.Bloc))
                        continue;

                    actionsDic[grid[i, j].position].Add(act);
                }
            }
        }
        
        // Setup de la camera afin d'avoir une vu global de la grid
        cam.transform.position = new Vector3(gridParameter.gridSize.x * 0.5f, gridParameter.gridSize.y * 0.5f, -5);
        cam.orthographic = true;
        cam.orthographicSize = ((gridParameter.gridSize.x + gridParameter.gridSize.y) * 0.5f + 5f) * 0.5f;

        // Initialisation de l'agent
        this.agentGridWorld.Init(actionsDic, gridParameter.startState, gridParameter.targetState, theta:this.theta, gamma:gamma);
        this.agentGridWorld.upArrow = this.upArrow;
        this.agentGridWorld.rightArrow = this.rightArrow;
        this.agentGridWorld.downArrow= this.downArrow;
        this.agentGridWorld.leftArrow = this.leftArrow;

        // Creation du visuel de l'agent
        this.agentGridWorld.visual = Instantiate(agentPrefab, new Vector3(gridParameter.startState.x, gridParameter.startState.y, 0.0f), Quaternion.identity);
        
        // Lancement du Reinforcement Learning
        if (algo == GridWorld_Algo.POLICY_ITERATION)
            StartCoroutine(UpdateWorldWithPolicyIteration());
        else
            this.agentGridWorld.ValueIteration(ref grid);
        

    }

    // Permet de positionner correctement les obstacles lorsqu'on génère la map
    private bool ThereIsAnOtherObstacleSoClose(Vector2Int idx)
    {
        for (int i = idx.x - 1; i <= idx.x + 1; i++)
        {
            for (int j = idx.y - 1; j <= idx.y + 1; j++)
            {
                var newPos = new Vector2Int(i, j);

                if (newPos.Equals(idx))
                    continue;

                if (newPos.x < 0 || newPos.x > gridParameter.gridSize.x - 1
                                 || newPos.y < 0
                                 || newPos.y > gridParameter.gridSize.y - 1)
                    continue;


                if (grid[i, j].state.Equals(GridCell.GridState.Bloc))
                    return true;

            }
        }

        return false;
    }

    // Permet d'update l'agent s'il est en mode Policy Iteration
    private IEnumerator UpdateWorldWithPolicyIteration()
    {
        int ite = 0;
        
        // On met un nombre d'iteration pour sécuriser afin de ne pas rentrer en infinity loop
        while (ite <= 10000)
        {
            // Policy Imporvement renvoie True quand la policy optimal est trouvé !
            bool stable = this.agentGridWorld.PolicyImprovement(ref grid);

            if (stable)
                break;
            
            // On va perform le policy improvement tout les StepTime
            yield return new WaitForSeconds(stepTime);
            
            ite++;
            Debug.Log(ite);
        }


        yield break;
    }

}

