using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Agent
{
    public Dictionary<Vector2Int, List<Action>> actions;
    public Dictionary<Vector2Int, Action> policy;

    public Vector2Int actualState;
    public Vector2Int targetState;

    public GameObject visual;

    public float gamma = 0.9f;
    public float theta = 0.005f;

    public float reward = 0.0f;

    public void Init(Dictionary<Vector2Int, List<Action>> availableActions, Vector2Int startState, Vector2Int targetState, float gamma = 0.9f)
    {
        reward = 0.0f;
        this.gamma = gamma;
        this.actions = availableActions;
        this.actualState = startState;
        this.targetState = targetState;

        this.theta = 0.005f;

        InitializePolicy();
    }

    public void InitializePolicy()
    {
        //On initiase la policy avec du RDM
        policy = new Dictionary<Vector2Int, Action>();
        foreach (var key in actions.Keys)
        {
            policy.Add(key, actions[key][Random.Range(0, actions[key].Count)]);
        }
    }

    public void PolicyEvaluation(ref GridCell[,] allStates)
    {
        float delta = 0.0f;
        while (delta < theta)
        {
            // Pour chaque s in S
            for (int i = 0; i < allStates.GetLength(0); i++)
            {
                for (int j = 0; j < allStates.GetLength(1); j++)
                {
                    if (!policy.ContainsKey(allStates[i, j].position))
                        continue;

                    float tmp = allStates[i, j].v;

                    //Perform une action
                    var moveAct = policy[allStates[i, j].position] as MoveAction;
                    var nxt = allStates[i, j].position + moveAct.direction;

                    allStates[i, j].v = allStates[nxt.x, nxt.y].r + gamma * allStates[nxt.x, nxt.y].v;

                    delta = Mathf.Max(delta, Mathf.Abs(tmp - allStates[i, j].v));

                    if (delta > theta)
                        break;
                }

                if (delta > theta)
                    break;
            }
        }
    }

    public void PolicyImprovement(ref GridCell[,] allStates)
    {
        bool policyStable = true;
        for (int i = 0; i < allStates.GetLength(0); i++)
        {
            for (int j = 0; j < allStates.GetLength(1); j++)
            {
                if (!policy.ContainsKey(allStates[i, j].position))
                    continue;

                var tmpAct = policy[allStates[i, j].position];

                // Je cherche la meilleure action
                float tmp = allStates[i, j].v;
                float newV = 0.0f;
                foreach (var act in actions[allStates[i, j].position])
                {
                    var moveAct = act as MoveAction;
                    var nxt = allStates[i, j].position + moveAct.direction;

                    newV = allStates[nxt.x, nxt.y].r + gamma * allStates[nxt.x, nxt.y].v;
                    if (newV > tmp)
                    {
                        allStates[i, j].v = newV;
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

    }


    public void ValueIteration(ref GridCell[,] allStates)
    {
        float delta = 0.0f;
        int iteration = 0;
        while (delta < theta && iteration < 100000)
        {
            iteration++;

            for (int i = 0; i < allStates.GetLength(0); i++)
            {
                for (int j = 0; j < allStates.GetLength(1); j++)
                {
                    if (!policy.ContainsKey(allStates[i, j].position))
                        continue;

                    float tmp = allStates[i, j].v;

                    var tmpState = allStates[i, j].position;

                    //Perform une action
                    foreach (var act in actions[tmpState])
                    {
                        var moveAct = act as MoveAction;
                        var nxt = allStates[i, j].position + moveAct.direction;

                        float newV = allStates[nxt.x, nxt.y].r + gamma * allStates[nxt.x, nxt.y].v;
                        if (newV > allStates[i, j].v)
                        {
                            allStates[i, j].v = newV;
                            policy[allStates[i, j].position] = act;
                        }
                    }

                    delta = Mathf.Max(delta, Mathf.Abs(tmp - allStates[i, j].v));
                }
            }
        }

        for (int i = 0; i < allStates.GetLength(0); i++)
        {
            for (int j = 0; j < allStates.GetLength(1); j++)
            {
                if (allStates[i, j].state.Equals(GridCell.GridState.End))
                    continue;

                var rd = allStates[i, j].visual.GetComponent<Renderer>();
                var act = policy[allStates[i, j].position] as MoveAction;

                allStates[i, j].visual.transform.localScale = new Vector3(act.direction.x, act.direction.y, 0.25f) + Vector3.one * 0.25f;

                if (act.direction.Equals(Vector2Int.up))
                {
                    rd.material.color = Color.red;
                }
                if (act.direction.Equals(Vector2Int.down))
                {
                    rd.material.color = Color.magenta;
                }
                if (act.direction.Equals(Vector2Int.left))
                {
                    rd.material.color = Color.blue;
                }
                if (act.direction.Equals(Vector2Int.right))
                {
                    rd.material.color = Color.cyan;
                }
            }
        }
    }
}