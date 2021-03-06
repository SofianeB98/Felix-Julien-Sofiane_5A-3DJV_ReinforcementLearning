using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class AgentGridWorld
{
    [Header("Debug Grid")]
    public Texture upArrow;
    public Texture downArrow;
    public Texture rightArrow;
    public Texture leftArrow;

    //Stock toutes les actions possible pour chaque case de la map
    public Dictionary<Vector2Int, List<Action>> actions;
    //Stock les meilleures actions pour un state donne
    public Dictionary<Vector2Int, Action> policy;
    

    public GameObject visual;

    public float gamma = 0.9f;
    public float theta = 0.005f;

    //Initialise l'agent
    public void Init(Dictionary<Vector2Int, List<Action>> availableActions, Vector2Int startState, Vector2Int targetState, float gamma = 0.9f, float theta = 0.005f)
    {
        this.gamma = gamma;
        this.actions = availableActions;

        this.theta = theta;

        InitializeRandomPolicy();
    }

    //Lance le remplissage random de la policy, elle se mettra a jour lorsque l'agent apprendra
    public void InitializeRandomPolicy()
    {
        //On initiase la policy avec du RDM
        policy = new Dictionary<Vector2Int, Action>();
        foreach (var key in actions.Keys)
        {
            if (actions[key].Count > 0)
                policy.Add(key, actions[key][Random.Range(0, actions[key].Count)]);
            else
                policy.Add(key, null);
        }
    }

    // Fonction de Policy Evaluation qui evalue la fonction de valeur
    public void PolicyEvaluation(ref GridCell[,] allStates)
    {
        float delta = theta + 1;
        int iteration = 0;
        //Fonction appelé plusieurs fois dans une coroutine afin de voir l'amélioration progressive de la policy
        while (delta > theta && iteration < 100000000)
        {
            delta = 0.0f;
            iteration++;

            // Pour chaque s in S
            for (int i = 0; i < allStates.GetLength(0); i++)
            {
                for (int j = 0; j < allStates.GetLength(1); j++)
                {
                    float tmp = allStates[i, j].v;

                    //Perform une action
                    var moveAct = policy[allStates[i, j].position] as MoveAction;

                    if (moveAct != null)
                    {
                        var nxt = allStates[i, j].position + moveAct.direction;

                        allStates[i, j].v = allStates[nxt.x, nxt.y].r + gamma * allStates[nxt.x, nxt.y].v;
                    }

                    delta = Mathf.Max(delta, Mathf.Abs(tmp - allStates[i, j].v));
                }
            }
        }

        // Display les action actuelle de la policy
        DisplayDirection(ref allStates);
    }

    // Met a jour la policy
    public bool PolicyImprovement(ref GridCell[,] allStates)
    {
        bool policyStable = true;
        for (int i = 0; i < allStates.GetLength(0); i++)
        {
            for (int j = 0; j < allStates.GetLength(1); j++)
            {
                var tmpAct = policy[allStates[i, j].position];

                // Je cherche la meilleure action
                float tmp = allStates[i, j].v;
                float maxV = 0.0f;
                foreach (var act in actions[allStates[i, j].position])
                {
                    var moveAct = act as MoveAction;
                    var nxt = allStates[i, j].position + moveAct.direction;

                    maxV = allStates[nxt.x, nxt.y].r + gamma * allStates[nxt.x, nxt.y].v;
                    if (maxV > allStates[i, j].v)
                    {
                        allStates[i, j].v = maxV;
                        policy[allStates[i, j].position] = act;
                    }
                }

                if (tmpAct != policy[allStates[i, j].position])
                {
                    policyStable = false;
                    break;
                }
            }
        }

        if (!policyStable)
            PolicyEvaluation(ref allStates);
        else
        {
            Debug.LogError("Policy Stable");
        }

        return policyStable;
    }
    
    // Lance le training par value iteration
    public void ValueIteration(ref GridCell[,] allStates)
    {
        float delta = theta + 1;
        int iteration = 0;
        while (delta > theta && iteration < 100000000)
        {
            delta = 0.0f;
            iteration++;
            Debug.Log(iteration);

            for (int i = 0; i < allStates.GetLength(0); i++)
            {
                for (int j = 0; j < allStates.GetLength(1); j++)
                {
                    float tmp = allStates[i, j].v;
                    float maxV = float.MinValue;

                    //Perform une action
                    foreach (var act in actions[allStates[i, j].position])
                    {
                        var moveAct = act as MoveAction;
                        var nxt = allStates[i, j].position + moveAct.direction;

                        float v = allStates[nxt.x, nxt.y].r + gamma * allStates[nxt.x, nxt.y].v;
                        if (v > maxV)
                        {
                            maxV = v;
                            policy[allStates[i, j].position] = act;
                        }
                    }

                    allStates[i, j].v = maxV < -10000.0f ? allStates[i, j].v : maxV;
                    delta = Mathf.Max(delta, Mathf.Abs(tmp - allStates[i, j].v));
                }
            }
        }

        string val = "";
        for (int j = allStates.GetLength(1) - 1; j >= 0; j--)
        {
            for (int i = 0; i < allStates.GetLength(0); i++)
            {
                val += "[ " + allStates[i, j].v + " ] ";
            }

            val += "\n";
        }

        Debug.Log(val);

        DisplayDirection(ref allStates);
    }

    private void DisplayDirection(ref GridCell[,] allStates)
    {
        var minV = float.MaxValue;
        var maxV = float.MinValue;


        //for (int i = 0; i < allStates.GetLength(0); i++)
        //{
        //    for (int j = 0; j < allStates.GetLength(1); j++)
        //    {
        //        float v = allStates[i, j].v;
        //        if (v < minV)
        //            minV = v;
        //        if (v > maxV)
        //            maxV = v;
        //    }
        //}

        for (int i = 0; i < allStates.GetLength(0); i++)
        {
            for (int j = 0; j < allStates.GetLength(1); j++)
            {
                if (allStates[i, j].state.Equals(GridCell.GridState.End) ||
                    allStates[i, j].state.Equals(GridCell.GridState.Bloc))
                    continue;

                var rd = allStates[i, j].visual.GetComponent<Renderer>();
                var act = policy[allStates[i, j].position] as MoveAction;

                if (act.direction.Equals(Vector2Int.up))
                {
                    rd.material.mainTexture = upArrow; //.color = Color.red;
                }
                if (act.direction.Equals(Vector2Int.down))
                {
                    rd.material.mainTexture = downArrow; //.color = Color.magenta;
                }
                if (act.direction.Equals(Vector2Int.left))
                {
                    rd.material.mainTexture = leftArrow; //.color = Color.blue;
                }
                if (act.direction.Equals(Vector2Int.right))
                {
                    rd.material.mainTexture = rightArrow; //.color = Color.cyan;
                }
                rd.material.color = Color.Lerp(Color.red, Color.green, allStates[i, j].v / 1000);
            }
        }
    }
}