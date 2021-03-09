using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToe : MonoBehaviour
{
    public const int GridSize = 3;

    public Sprite Neutral;
    public Sprite Cross;
    public Sprite Circle;
    public GameObject tilePrefab;

    public GameState gameState;
    public enum State
    {
        NEUTRAL,
        CROSS,
        CIRCLE
    }

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
        public class Tile 
        {
            public GameObject visual;
            public State state;

            public static bool operator== (Tile a, State s) 
            {
                return a.state == s;
            }

            public static bool operator!= (Tile a, State s) 
            {
                return a.state != s;
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

    public bool CheckEnd()
    {
        // Vertical
        for (int i = 0; i < GridSize; i++)
        {
            // Vertical Check
            var state = this.gameState[i, 0];
            bool ok = true;
            for (int j = 1; j < GridSize; j++)
            {
                if (this.gameState[i, j] != state)
                    ok = false;
            }
            if (ok)
                return true;

        }
        // Horizontal
        for (int i = 0; i < GridSize; i++)
        {
            // Vertical Check
            var state = this.gameState[0, i];
            bool ok = true;
            for (int j = 1; j < GridSize; j++)
            {
                if (this.gameState[j, i] != state)
                    ok = false;
            }
            if (ok)
                return true;

        }

        // Diagonal 1
        if(this.gameState[0, 0] == this.gameState[1, 1] && this.gameState[0, 0] == this.gameState[2, 2]) 
        {
            return true;
        }
        // Diagonal 2
        if (this.gameState[0, 2] == this.gameState[1, 1] && this.gameState[0, 2] == this.gameState[2, 0])
        {
            return true;
        }

        return false;
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

    public Sprite GetSpriteFromState(State state)
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
}
