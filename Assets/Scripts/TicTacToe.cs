using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToe : MonoBehaviour
{
    [Header("Grid Size")]
    public const int GridSize = 3;

    [Header("Visual")]
    public Texture Neutral;
    public Texture Cross;
    public Texture Circle;
    public GameObject tilePrefab;

    private GameState gameState;

    public delegate void OnVictory(int player);
    public event OnVictory victory;

    private bool gameEnd = false;

    public enum State
    {
        NEUTRAL = -1,
        CROSS = 0,
        CIRCLE = 1
    }

    [System.Serializable]
    public struct GameState
    {
        // Data informative
        public Tile[,] Grid;

        // Data QLearning
        public float N;
        public float Returns;

        public void SetReturns(float ret)
        {
            this.Returns = ret;
        }

        public void SetN(float n)
        {
            this.N = n;
        }

        public Tile this[int x, int y]
        {
            get { return this.Grid[x, y]; }
        }

        [System.Serializable]
        public struct Tile
        {
            //public GameObject visual;
            public State state;

            public static bool operator ==(Tile a, State s)
            {
                return a.state.Equals(s);
            }

            public static bool operator !=(Tile a, State s)
            {
                return !(a.state.Equals(s));
            }

            public Tile(GameObject visual, State s)
            {
                //this.visual = visual;
                this.state = s;
            }

            public void SetState(State s)
            {
                this.state = s;
            }
        }

        public GameState(GameObject tilePrefab, Transform parent, out GameObject[,] visualGrid)
        {
            // Initialize Grid wit Neutral state
            this.Grid = new Tile[GridSize, GridSize];
            visualGrid = new GameObject[GridSize, GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    GameObject go =
                        GameObject.Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity, parent);
                    visualGrid[i, j] = go;
                    this.Grid[i, j] = new Tile(go, State.NEUTRAL);
                }
            }

            N = 0.0f;
            Returns = 0.0f;
        }

        public GameState(Tile[,] g, float n, float r)
        {
            // Initialize Grid wit Neutral state
            Grid = new Tile[g.GetLength(0), g.GetLength(1)];
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    Grid[i, j] = new Tile(null /*g[i,j].visual*/, g[i, j].state);
                }
            }

            N = n;
            Returns = r;
        }

        public GameState Clone()
        {
            var gs = new GameState();
            gs.Grid = new Tile[GridSize, GridSize];
            gs.Grid = this.Grid.Clone() as Tile[,];

            gs.N = N;
            gs.Returns = Returns;

            return gs;
        }

        public List<(int, int)> GetAvailableCells()
        {
            var availableCells = new List<(int, int)>();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (this[i, j] == State.NEUTRAL)
                    {
                        availableCells.Add((i, j));
                    }
                }
            }

            return availableCells;
        }
    }

    private int playerTurn = 0;

    private AgentTicTacToe agent;
    private GameObject[,] visualGrid;

    [Header("Agent Parameter")] 
    [SerializeField] private int episodeCount = 1000;
    [SerializeField, Range(0.0f, 1.0f)] private float epsilonGreedy = 0.3f;
    [SerializeField] private bool useFirstVisit = false;
    [SerializeField] private bool useOnPolicy = false;
    [SerializeField] private float winR = 10.0f;
    [SerializeField] private float nulR = 0.0f;
    [SerializeField] private float loseR = -10.0f;
    
    public bool SetCell(int playerTurn, int x, int y)
    {
        var state = PlayerNumberToState(playerTurn);
        if (this.gameState.GetAvailableCells().Contains((x, y)))
        {
            this.gameState.Grid[x, y].SetState(state); //= new GameState.Tile(null, State.CROSS); //
            //this.gameState.Grid[x, y].visual.GetComponent<Renderer>().material.mainTexture = GetTextureFromState(state);
            visualGrid[x, y].GetComponent<Renderer>().material.mainTexture = GetTextureFromState(state);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool SetCellWithoutChangeGraphics(int playerTurn, int x, int y, ref GameState gs)
    {
        var state = PlayerNumberToState(playerTurn);
        //if (gs.GetAvailableCells().Contains((x, y)))
        {
            gs.Grid[x, y].SetState(state);
            return true;
        }
        //else
        //{
        //    return false;
        //}
    }

    public void NextTurn(ref int player)
    {
        player = (player + 1) % 2;
    }

    public void Initialize()
    {
        this.gameState = new GameState(this.tilePrefab, this.gameObject.transform, out visualGrid);
        this.agent = new AgentTicTacToe();
        agent.ticTacToe = this;
        agent.policy = new Dictionary<GameState, Vector2Int>();


        agent.Simulate(ref this.gameState, episodeCount, useFirstVisit, useOnPolicy, epsilonGreedy, winR, nulR, loseR);

        StartCoroutine(GameController());
    }

    public bool CheckNullMatch(ref GameState gs)
    {
        if (gs.GetAvailableCells().Count == 0)
        {
            return true;
        }

        return false;
    }

    public (bool, int) CheckVictory(ref GameState gs)
    {
        // Vertical
        for (int i = 0; i < GridSize; i++)
        {
            // Vertical Check
            var tile = gs[i, 0];
            if (tile.state == State.NEUTRAL)
                continue;

            bool ok = true;

            for (int j = 1; j < GridSize; j++)
            {
                if (gs[i, j] != tile.state)
                    ok = false;
            }

            if (ok)
                return (true, StateToPlayerNumber(tile.state));
        }

        // Horizontal
        for (int i = 0; i < GridSize; i++)
        {
            // Vertical Check
            var tile = gs[0, i];
            if (tile.state == State.NEUTRAL)
                continue;

            bool ok = true;
            for (int j = 1; j < GridSize; j++)
            {
                if (gs[j, i] != tile.state)
                    ok = false;
            }

            if (ok)
                return (true, StateToPlayerNumber(tile.state));
        }

        // Diagonal 1
        if (gs[0, 0] == gs[1, 1].state && gs[0, 0] == gs[2, 2].state)
        {
            if (gs[0, 0] != State.NEUTRAL)
            {
                return (true, StateToPlayerNumber(gs[0, 0].state));
            }
        }

        // Diagonal 2
        if (gs[0, 2] == gs[1, 1].state && gs[0, 2] == gs[2, 0].state)
        {
            if (gs[0, 2] != State.NEUTRAL)
            {
                return (true, StateToPlayerNumber(gs[0, 0].state));
            }
        }

        return (false, -1);
    }


    public State PlayerNumberToState(int playerNumber)
    {
        return playerNumber == 0 ? State.CROSS : State.CIRCLE;
    }

    public int StateToPlayerNumber(State state)
    {
        if (state == State.NEUTRAL)
            return -1;
        return state == State.CROSS ? 0 : 1;
    }

    public Texture GetTextureFromState(State state)
    {
        switch (state)
        {
            case State.NEUTRAL:
                return this.Neutral;
            case State.CROSS:
                return this.Cross;
            default:
                return this.Circle;
        }
    }

    public void Start()
    {
        this.Initialize();
    }

    public void Update()
    {
        if (gameEnd && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Je Reset");
            for (int i = 0; i < this.gameState.Grid.GetLength(0); i++)
            {
                for (int j = 0; j < this.gameState.Grid.GetLength(0); j++)
                {
                    this.gameState.Grid[i, j].SetState(State.NEUTRAL); //= new GameState.Tile(null, State.CROSS); //
                    //this.gameState.Grid[x, y].visual.GetComponent<Renderer>().material.mainTexture = GetTextureFromState(state);
                    visualGrid[i, j].GetComponent<Renderer>().material.mainTexture = GetTextureFromState(State.NEUTRAL);
                }
            }

            this.gameEnd = false;
            this.playerTurn = 0;

            StopAllCoroutines();
            StartCoroutine(GameController());
        }
    }

    private IEnumerator GameController()
    {
        Debug.Log("LA partie commence !");
        while (!gameEnd)
        {
            switch (playerTurn)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        Plane plane = new Plane(Vector3.back, Vector3.zero);
                        float dist = 0;
                        plane.Raycast(ray, out dist);
                        var intersectPos = ray.GetPoint(dist);
                        var coord = new Vector2Int(Mathf.RoundToInt(intersectPos.x), Mathf.RoundToInt(intersectPos.y));

                        //Debug.Log(coord);
                        if (coord.x < 0 || coord.x >= GridSize || coord.y < 0 || coord.y >= GridSize)
                        {
                            //return;
                        }
                        else
                        {
                            if (SetCell(this.playerTurn, coord.x, coord.y))
                            {
                                var victoryState = CheckVictory(ref gameState);
                                if (!CheckVictory(ref gameState).Item1)
                                {
                                    if (!CheckNullMatch(ref gameState))
                                    {
                                        NextTurn(ref playerTurn);
                                        yield return new WaitForSeconds(0.5f);
                                    }
                                    else
                                        gameEnd = true;
                                }
                                else
                                {
                                    this.victory?.Invoke(victoryState.Item2);
                                    gameEnd = true;
                                    // yield break;
                                }
                            }
                        }
                    }

                    break;

                case 1:
                    // if (Input.GetKeyDown(KeyCode.Mouse0))
                    // {
                    //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    //     RaycastHit hit;
                    //     Plane plane = new Plane(Vector3.back, Vector3.zero);
                    //     float dist = 0;
                    //     plane.Raycast(ray, out dist);
                    //     var intersectPos = ray.GetPoint(dist);
                    //     var coord = new Vector2Int(Mathf.RoundToInt(intersectPos.x), Mathf.RoundToInt(intersectPos.y));
                    //
                    //     Debug.Log(coord);
                    //     if (coord.x < 0 || coord.x >= GridSize || coord.y < 0 || coord.y >= GridSize)
                    //         return;
                    //     
                    //     if (SetCell(this.playerTurn, coord.x, coord.y))
                    //     {
                    //         var victoryState = CheckVictory(ref gameState);
                    //         if (!CheckVictory(ref gameState).Item1)
                    //         {
                    //             if (!CheckNullMatch(ref gameState))
                    //                 NextTurn(ref playerTurn);
                    //             else
                    //                 gameEnd = true;
                    //         }
                    //         else
                    //         {
                    //             this.victory?.Invoke(victoryState.Item2);
                    //             gameEnd = true;
                    //         }
                    //     }
                    // }

                    var agentAction = agent.GetBestAction(ref gameState);
                    if (SetCell(this.playerTurn, agentAction.x, agentAction.y))
                    {
                        var victoryState = CheckVictory(ref gameState);
                        if (!CheckVictory(ref gameState).Item1)
                        {
                            if (!CheckNullMatch(ref gameState))
                            {
                                NextTurn(ref playerTurn);
                                yield return new WaitForSeconds(0.5f);
                            }
                            else
                                gameEnd = true;
                        }
                        else
                        {
                            this.victory?.Invoke(victoryState.Item2);
                            gameEnd = true;
                        }
                    }

                    break;
            }

            yield return null;
        }

        Debug.Log($"La partie est fini !!");

        yield break;
    }
}