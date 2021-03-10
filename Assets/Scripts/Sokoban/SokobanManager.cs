using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban
{
    public class SokobanManager : MonoBehaviour
    {
        public Sokoban.SokobanGameState gameState;
        IAction moveUp = new MoveAction(Vector2Int.up);
        IAction moveDown = new MoveAction(Vector2Int.down);
        IAction moveLeft = new MoveAction(Vector2Int.left);
        IAction moveRight = new MoveAction(Vector2Int.right);
        public GameObject Player;

        void Start()
        {
            var grid = LoadLevel();
            this.gameState = new SokobanGameState(grid.Item1, grid.Item2);
            GameStateComparer comparer = new GameStateComparer();
            Dictionary<SokobanGameState, float> dic = new Dictionary<SokobanGameState, float>(comparer);
            dic.Add(this.gameState, 0);
            var clone = this.gameState.Clone();
            clone.Grid[0, 0].state = State.Walkable;
            dic.Add(clone, 1);
            Debug.Log(dic[clone]);
            Debug.Log(dic[this.gameState]);
            Debug.Log(dic.Count);

        }

        public (Tile[,], List<Bloc>) LoadLevel()
        {
            var walls = GameObject.FindGameObjectsWithTag("Wall");
            var walkables = GameObject.FindGameObjectsWithTag("Walkable");
            var player = GameObject.FindGameObjectWithTag("Player");
            var voids = GameObject.FindGameObjectsWithTag("Void");
            var blocs = GameObject.FindGameObjectsWithTag("Bloc");
            var targets = GameObject.FindGameObjectsWithTag("Target");

            // x = XMin 
            // y = XMax
            // z = YMin
            // w = ZMax

            var bounds = new Vector4(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            // Walls defines bounds
            foreach (var item in walls)
            {
                if (item.transform.position.x < bounds.x)
                {
                    bounds.x = item.transform.position.x;
                }

                if (item.transform.position.x > bounds.y)
                {
                    bounds.y = item.transform.position.x;
                }

                if (item.transform.position.y < bounds.z)
                {
                    bounds.z = item.transform.position.y;
                }

                if (item.transform.position.y > bounds.w)
                {
                    bounds.w = item.transform.position.y;
                }
            }

            var grid = new Tile[Mathf.RoundToInt(bounds.y) + 1, Mathf.RoundToInt(bounds.w) + 1];
            var b = new List<Bloc>();
            // Initialize Walls
            foreach (var item in walls)
            {
                var pos = new Vector2Int(
                    Mathf.RoundToInt(item.transform.position.x),
                    Mathf.RoundToInt(item.transform.position.y)
                    );
                var t = new Tile(pos, State.Unwalkable, item);
                Debug.Log(pos);
                Debug.Log(grid.GetLength(0));
                grid[pos.x, pos.y] = t;
            }

            // Initialize Floor
            foreach (var item in walkables)
            {
                var pos = new Vector2Int(
                    Mathf.RoundToInt(item.transform.position.x),
                    Mathf.RoundToInt(item.transform.position.y)
                    );
                var t = new Tile(pos, State.Walkable, item);
                grid[pos.x, pos.y] = t;
            }

            // Initialize Blocs
            foreach (var item in blocs)
            {
                var pos = new Vector2Int(
                    Mathf.RoundToInt(item.transform.position.x),
                    Mathf.RoundToInt(item.transform.position.y)
                    );
                grid[pos.x, pos.y].state = State.Bloc;
                b.Add(new Bloc(pos, item));
            }

            // Initialise Player
            {
                var playerPos = player.transform.position;
                var pos = new Vector2Int(
                    Mathf.RoundToInt(playerPos.x),
                    Mathf.RoundToInt(playerPos.y)
                    );
                grid[pos.x, pos.y].state = State.Player;
                this.Player = player;
            }

            foreach (var item in targets)
            {
                var pos = new Vector2Int(
                    Mathf.RoundToInt(item.transform.position.x),
                    Mathf.RoundToInt(item.transform.position.y)
                    );
                grid[pos.x, pos.y].state = State.Objective;
            }

            foreach (var item in voids)
            {
                var pos = new Vector2Int(
                    Mathf.RoundToInt(item.transform.position.x),
                    Mathf.RoundToInt(item.transform.position.y)
                    );
                var t = new Tile(pos, State.Unwalkable, item);
                grid[pos.x, pos.y] = t;
            }
            return (grid, b);

        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.moveUp.Perform(ref this.gameState);
                UpdatePlayerPosition();
                UpdateBlocPosition();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.moveDown.Perform(ref this.gameState);
                UpdatePlayerPosition();
                UpdateBlocPosition();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.moveLeft.Perform(ref this.gameState);
                UpdatePlayerPosition();
                UpdateBlocPosition();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.moveRight.Perform(ref this.gameState);
                UpdatePlayerPosition();
                UpdateBlocPosition();
            }
        }

        void UpdatePlayerPosition()
        {

            this.Player.transform.position = new Vector3(this.gameState.playerPosition.x, this.gameState.playerPosition.y, 0);
        }

        void UpdateBlocPosition()
        {
            foreach (var item in this.gameState.blocs)
            {
                item.visual.transform.position = new Vector3(item.position.x, item.position.y, 0);
            }
        }
    }
}
