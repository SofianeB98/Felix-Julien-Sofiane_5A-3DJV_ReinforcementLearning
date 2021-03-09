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

    public enum State
    {
        NEUTRAL,
        CROSS,
        CIRCLE
    }
    [System.Serializable]
    public class GameState
    {
        public Tile[,] Grid;
        public int playerTurn;
        public int turn;
        public Tile this[int x, int y]
        {
            get
            {
                return this.Grid[x, y];
            }
        }
        [System.Serializable]
        public class Tile
        {
            public GameObject visual;
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
                this.visual = visual;
                this.state = s;
            }
        }
        public GameState(GameObject tilePrefab, Transform parent)
        {
            // Initialize Grid wit Neutral state
            this.Grid = new Tile[GridSize, GridSize];
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    GameObject go = GameObject.Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity, parent);
                    this.Grid[i, j] = new Tile(go, State.NEUTRAL);
                }
            }
            this.playerTurn = 0;
            this.turn = 0;
        }

        public void NextTurn()
        {
            this.playerTurn = (playerTurn + 1) % 2;
        }

    }

    public bool SetCell(int playerTurn, int x, int y)
    {
        var state = PlayerNumberToState(playerTurn);
        if (this.GetAvailableCell().Contains((x, y)))
        {
            this.gameState[x, y].state = state;
            this.gameState[x, y].visual.GetComponent<Renderer>().material.mainTexture = GetTextureFromState(state);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Initialize()
    {
        this.gameState = new GameState(this.tilePrefab, this.gameObject.transform);
    }

    public bool CheckNullMatch()
    {
        if (GetAvailableCell().Count == 0)
        {
            return true;
        }
        return false;
    }

    public (bool, int) CheckVictory()
    {
        // Vertical
        for (int i = 0; i < GridSize; i++)
        {
            // Vertical Check
            var tile = this.gameState[i, 0];
            if (tile.state == State.NEUTRAL)
                continue;

            bool ok = true;

            for (int j = 1; j < GridSize; j++)
            {
                if (this.gameState[i, j] != tile.state)
                    ok = false;
            }
            if (ok)
                return (true, StateToPlayerNumber(tile.state));

        }
        // Horizontal
        for (int i = 0; i < GridSize; i++)
        {
            // Vertical Check
            var tile = this.gameState[0, i];
            if (tile.state == State.NEUTRAL)
                continue;

            bool ok = true;
            for (int j = 1; j < GridSize; j++)
            {
                if (this.gameState[j, i] != tile.state)
                    ok = false;
            }
            if (ok)
                return (true, StateToPlayerNumber(tile.state));

        }

        // Diagonal 1
        if (this.gameState[0, 0] == this.gameState[1, 1].state && this.gameState[0, 0] == this.gameState[2, 2].state)
        {
            if (this.gameState[0, 0] != State.NEUTRAL)
            {
                return (true, StateToPlayerNumber(this.gameState[0, 0].state));
            }
        }
        // Diagonal 2
        if (this.gameState[0, 2] == this.gameState[1, 1].state && this.gameState[0, 2] == this.gameState[2, 0].state)
        {
            if (this.gameState[0, 0] != State.NEUTRAL)
            {
                return (true, StateToPlayerNumber(this.gameState[0, 0].state));
            }
        }

        return (false, -1);
    }

    public List<(int, int)> GetAvailableCell()
    {
        var availableCells = new List<(int, int)>();
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                if (this.gameState[i, j] == State.NEUTRAL)
                {
                    availableCells.Add((i, j));
                }
            }
        }
        return availableCells;
    }

    State PlayerNumberToState(int playerNumber)
    {
        return playerNumber == 0 ? State.CROSS : State.CIRCLE;
    }

    int StateToPlayerNumber(State state)
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
        if (Input.GetKeyDown(KeyCode.Mouse0))
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
            if (SetCell(this.gameState.playerTurn, coord.x, coord.y))
            {
                var victoryState = CheckVictory();
                if (!CheckVictory().Item1)
                {
                    CheckNullMatch();
                }
                else
                {
                    this.victory?.Invoke(victoryState.Item2);
                }
                this.gameState.NextTurn();
            }
        }
    }
}
