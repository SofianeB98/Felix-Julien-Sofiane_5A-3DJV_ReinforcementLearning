using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban
{
    public class SokobanManager : MonoBehaviour
    {
        private Sokoban.SokobanGameState gameState;
        private IAction moveUp = new MoveAction(Vector2Int.up);
        private IAction moveDown = new MoveAction(Vector2Int.down);
        private IAction moveLeft = new MoveAction(Vector2Int.left);
        private IAction moveRight = new MoveAction(Vector2Int.right);
        private GameObject Player;
        private List<IAction> actions;

        [Header("Player Control")] public bool playerCanControl = false;

        [Header("Learning Parameter")] [SerializeField]
        private SokobanAgent.Algo selectedAlgo = SokobanAgent.Algo.QLearning;

        [SerializeField] private int episodeCount = 100;
        [SerializeField, Range(0.0f, 1.0f)] private float epsilonGreedy = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] private float gamma = 0.9f;

        [Header("SARSA - QLearning")] [SerializeField]
        private float alpha = 0.2f;

        [Header("Value-Policy Iteration")] [SerializeField, Range(0.0f, 1.0f)]
        private float theta = 0.005f;

        [Header("MC ES")] [SerializeField] private bool useOnPolicy = false;

        private SokobanAgent agent = new SokobanAgent();

        void Start()
        {
            this.actions = new List<IAction>();
            actions.Add(moveUp);
            actions.Add(moveDown);
            actions.Add(moveLeft);
            actions.Add(moveRight);

            var grid = LoadLevel();
            this.gameState = new SokobanGameState(grid.Item1, grid.Item2, actions);

            agent.Init(ref gameState, ref actions, selectedAlgo, alpha, gamma, episodeCount, useOnPolicy, epsilonGreedy,
                theta);

            StartCoroutine(PlayWithIA());
        }

        private IEnumerator PlayWithIA()
        {
            int iteration = 0;
            Debug.Log("Start Playing IA");
            while (!gameState.CheckFinish() && iteration < 100000)
            {
                if (!playerCanControl)
                {
                    iteration++;
                    var act = agent.GetBestAction(ref gameState);

                    act.Perform(ref this.gameState);
                    UpdatePlayerPosition();
                    UpdateBlocPosition();
                }

                yield return new WaitForSeconds(0.5f);
            }

            yield break;
        }

        public (Tile[,], List<Caisse>) LoadLevel()
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
            var b = new List<Caisse>();
            // Initialize Walls
            foreach (var item in walls)
            {
                var pos = new Vector2Int(
                    Mathf.RoundToInt(item.transform.position.x),
                    Mathf.RoundToInt(item.transform.position.y)
                );
                var t = new Tile(pos, State.Unwalkable, item);
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
                grid[pos.x, pos.y].state = State.Caisse;
                b.Add(new Caisse(pos, item));
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
            if (!playerCanControl)
                return;

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
            this.Player.transform.position =
                new Vector3(this.gameState.playerPosition.x, this.gameState.playerPosition.y, 0);
        }

        void UpdateBlocPosition()
        {
            foreach (var item in this.gameState.caisses)
            {
                item.visual.transform.position = new Vector3(item.position.x, item.position.y, 0);
            }
        }
    }
}