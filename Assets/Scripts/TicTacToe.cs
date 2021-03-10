using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToe : MonoBehaviour
{
    public const int GridSize = 3;

    public Texture Neutral;
    public Texture Cross;
    public Texture Circle;
    public GameObject tilePrefab;

    public GameState gameState;

    public delegate void OnVictory(int player);
    public event OnVictory victory;

    public bool gameEnd = false;
    
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
            get
            {
                return this.Grid[x, y];
            }
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
                    GameObject go = GameObject.Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity, parent);
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
            Grid = new Tile[g.GetLength(0),g.GetLength(1)];
            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    Grid[i,j] = new Tile(null/*g[i,j].visual*/, g[i,j].state);
                }
            }
            
            N = n;
            Returns = r;
        }

        public GameState Clone()
        {
            var gs = new GameState();
            gs.Grid = new Tile[GridSize,GridSize];
            gs.Grid = this.Grid.Clone() as Tile[,];

            gs.N = N;
            gs.Returns = Returns;

            return gs;
        }
        
        public List<(int, int)> GetAvailableCell()
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

    public int playerTurn;

    public AgentTicTacToe agent;

    public GameObject[,] visualGrid;
    
    public bool SetCell(int playerTurn, int x, int y)
    {
        var state = PlayerNumberToState(playerTurn);
        //if (this.gameState.GetAvailableCell().Contains((x, y)))
        {
            this.gameState.Grid[x, y].SetState(state); //= new GameState.Tile(null, State.CROSS); //
            //this.gameState.Grid[x, y].visual.GetComponent<Renderer>().material.mainTexture = GetTextureFromState(state);
            visualGrid[x, y].GetComponent<Renderer>().material.mainTexture = GetTextureFromState(state);
            return true;
        }
        //else
        {
            //return false;
        }
    }

    public bool SetCellWithoutChangeGraphics(int playerTurn, int x, int y, ref GameState gs)
    {
        var state = PlayerNumberToState(playerTurn);
        if (gs.GetAvailableCell().Contains((x, y)))
        {
            gs.Grid[x, y].SetState(state);
            return true;
        }
        else
        {
            return false;
        }
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
        
        
        agent.Simulate(ref this.gameState, 100);
    }

    public bool CheckNullMatch(ref GameState gs)
    {
        if (gs.GetAvailableCell().Count == 0)
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
        if (Input.GetKeyDown(KeyCode.Mouse0) && !gameEnd)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Plane plane = new Plane(Vector3.back, Vector3.zero);
            float dist = 0;
            plane.Raycast(ray, out dist);
            var intersectPos = ray.GetPoint(dist);
            var coord = new Vector2Int(Mathf.RoundToInt(intersectPos.x), Mathf.RoundToInt(intersectPos.y));

            Debug.Log(coord);
            if (coord.x < 0 || coord.x >= GridSize || coord.y < 0 || coord.y >= GridSize)
                return;
            if (SetCell(this.playerTurn, coord.x, coord.y))
            {
                var victoryState = CheckVictory(ref gameState);
                if (!CheckVictory(ref gameState).Item1)
                {
                    if (!CheckNullMatch(ref gameState))
                        NextTurn(ref playerTurn);
                    else
                        gameEnd = true;
                }
                else
                {
                    this.victory?.Invoke(victoryState.Item2);
                    gameEnd = true;
                }
            }
        }

        if (gameEnd && Input.GetKeyDown(KeyCode.R))
        {
            // Reset
        }
    }
}
