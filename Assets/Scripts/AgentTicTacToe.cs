using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AgentTicTacToe
{
    public TicTacToe ticTacToe;

    public bool isFirstVisit = false;
    public bool isOnPolicy = false;

    public float epsilonGreedy = 0.3f;

    public Dictionary<TicTacToe.GameState, Vector2Int> policy;

    private float winR = 10.0f;
    private float nulR = 0.0f;
    private float loseR = -10.0f;

    // Initialisation
    public void Simulate(ref TicTacToe.GameState gs, int episodeCount = 10, bool useFirstVisit = false,
        bool useOnPolicy = false, float eps = 0.3f, float win = 10.0f, float nul = 0.0f, float lose = -10.0f)
    {
        var V = 0.0f;

        this.winR = win;
        this.loseR = lose;
        this.nulR = nul;
        
        var copyGs = new TicTacToe.GameState(gs.Grid, gs.N, gs.Returns);

        this.isFirstVisit = useFirstVisit;
        this.isOnPolicy = useOnPolicy;
        this.epsilonGreedy = eps;
        // Selection - Expansion - Simulation - Retropopagation

        // GS - 1
        // ==> N state possible (avec N Action)
        // ==> Selection un state N
        // ==> On lance MC Prediction

        // if (isFirstVisit)
        //     V = FirstVisitMCPrediction(ref policy, copyGs, episodeCount);
        // else
        //     V = EveryVisitMCPrediction(ref policy, copyGs, episodeCount);
        
        MCPrediction(ref policy, copyGs, episodeCount);
    }

    // Learning par Monte Carlo
    public void MCPrediction(ref Dictionary<TicTacToe.GameState, Vector2Int> policy,
        TicTacToe.GameState gs_copy, int episodeCount = 10)
    {

        List<TicTacToe.GameState> exploredGameStates = new List<TicTacToe.GameState>();

        for (int i = 0; i < episodeCount; i++)
        {
            List<TicTacToe.GameState> currentGameStatesEpisodes =
                new List<TicTacToe.GameState>();
            
            //Permet de ne pas retraiter les episodes deja traite
            //Pour le cas du tic tac toe, cela n'a pas d'utilité
            List<TicTacToe.GameState> ProcessedEpisodes =
                new List<TicTacToe.GameState>();
            
            // Copy du GS
            var copyGs = gs_copy.Clone();

            // Generer une simulation de jeu
            // On va récupérer N state possible
            float R = SimulateGameState(ref policy, ref currentGameStatesEpisodes, ref copyGs);
            float G = 0;

            // Checker si le state qu'on va incrémenté est contenu dans Explored
            // Si c'est le cas on l'incrémenter lui
            // Sinon on ajoute un nouveau et on incrémente
            // On retropopage tout
            G = G + R; //Vue qu'on a le reward juste de l'issue de la simulation (win - lose - nul) on l'ajoute qu'une fois
            for (int t = currentGameStatesEpisodes.Count - 1; t >= 0; t--)
            {
                //G = G + R;
                
                var currentGS = currentGameStatesEpisodes[t];
                
                //Is First vist
                // Le if du first visit correspond a :
                // Lorsqu'on tombe sur un ETAT de l'episode en cours qui a deja eté exploré, on le zaap
                if (isFirstVisit && GetIndexOf(ref ProcessedEpisodes, ref currentGS.Grid) == -1)
                {
                    //Est ce que l'episode a deja ete traite ?
                    //Si non on le traite
                    //Si oui on zap
                    
                    var indexOfCurrentGS =
                        GetIndexOf(ref exploredGameStates,
                            ref currentGS.Grid); //explored.FindIndex(x => x.Grid == strct.Grid);
                
                    if (indexOfCurrentGS >= 0)
                    {
                        // On incrémente celui deja existant

                        currentGS = exploredGameStates[indexOfCurrentGS];

                        currentGS.SetReturns(currentGS.Returns + G);
                        currentGS.SetN(currentGS.N + 1);

                        //explored[idx].SetReturns(explored[idx].Returns + G);
                        //explored[idx].SetN(explored[idx].N + 1);

                        exploredGameStates[indexOfCurrentGS] = currentGS;
                    }
                    else
                    {
                        //Sinon on incrément celui qui existe pas et on l'ajoute
                        currentGS.SetReturns(currentGS.Returns + G);
                        currentGS.SetN(currentGS.N + 1);

                        currentGameStatesEpisodes[t] = currentGS;

                        exploredGameStates.Add(currentGS);
                    }
                    
                    ProcessedEpisodes.Add(currentGS);
                }
                else//Is Every Visit
                {
                    var indexOfCurrentGS =
                        GetIndexOf(ref exploredGameStates,
                            ref currentGS.Grid); //explored.FindIndex(x => x.Grid == strct.Grid);
                
                    if (indexOfCurrentGS >= 0)
                    {
                        // On incrémente celui deja existant

                        currentGS = exploredGameStates[indexOfCurrentGS];

                        currentGS.SetReturns(currentGS.Returns + G);
                        currentGS.SetN(currentGS.N + 1);

                        exploredGameStates[indexOfCurrentGS] = currentGS;
                    }
                    else
                    {
                        //Sinon on incrément celui qui existe pas et on l'ajoute
                        currentGS.SetReturns(currentGS.Returns + G);
                        currentGS.SetN(currentGS.N + 1);

                        currentGameStatesEpisodes[t] = currentGS;

                        exploredGameStates.Add(currentGS);
                    }
                }
                
                
            }

            if (isOnPolicy)
            {
                // On Policy
                // var policyGS = policy.FirstOrDefault(x => x.Key.Grid == e.Grid);
                // if (policy.ContainsKey(policyGS.Key))
                // {
                //     policyGS.Key.SetN(e.N);
                //     policyGS.Key.SetReturns(e.Returns);
                // }
                
                foreach (var k in policy.Keys)
                {
                    var availableCells = k.GetAvailableCells();

                    float best = 0.0f;
                    (int, int) bestAction = (policy[k].x, policy[k].y);

                    foreach (var act in availableCells)
                    {
                        var copy = k.Clone();

                        // TicTacToe.GameState.Tile[,] tmpGrid = k.Grid.Clone() as TicTacToe.GameState.Tile[,];
                        // tmpGrid[act.Item1, act.Item2].SetState(TicTacToe.State.CIRCLE);

                        ticTacToe.SetCellWithoutChangeGraphics(1, act.Item1, act.Item2, ref copy);

                        var indexOfActionsInExplored = GetIndexOf(ref exploredGameStates, ref copy.Grid);

                        if (indexOfActionsInExplored >= 0)
                        {
                            float Vs = exploredGameStates[indexOfActionsInExplored].Returns /
                                       exploredGameStates[indexOfActionsInExplored].N;

                            if (Vs > best)
                            {
                                best = Vs;
                                bestAction = act;
                                //policy[k] = new Vector2Int(act.Item1, act.Item2);
                            }
                        }
                    }

                    policy[k].Set(bestAction.Item1, bestAction.Item2);
                }
            }
            
        }

        if (!isOnPolicy)
        {
            // Avec cette list ou dico on met a jour dans policy
            // Off policy, on passe sur Explored, Pour un etat S donne, on cherche toutes les Actions atteignable
            // Pour ces action atteignable on va prendre la meilleure possible V = (Returns / N) et assigner 
            // exemple : Etat null => je cherche tout les etat atteignable (= action)
            // Parmis ces actions, je regarde celui qui a le meilleure V
            // Et j'applique la value à la policy[key]

            // Pour update la policy, On va chercher, pour chaque Key de la policy, les Available Cell (= Actions)
            // Ensuite pour ces actions available, je vais regarder dans Explored Game State, si elle est contenu, celle qui a le meilleur V
            // Enfin j'applique l'action à la key

            //int connu = 0;

            foreach (var k in policy.Keys)
            {
                var availableCells = k.GetAvailableCells();

                float best = 0.0f;
                (int, int) bestAction = (policy[k].x, policy[k].y);

                foreach (var act in availableCells)
                {
                    var copy = k.Clone();

                    // TicTacToe.GameState.Tile[,] tmpGrid = k.Grid.Clone() as TicTacToe.GameState.Tile[,];
                    // tmpGrid[act.Item1, act.Item2].SetState(TicTacToe.State.CIRCLE);

                    ticTacToe.SetCellWithoutChangeGraphics(1, act.Item1, act.Item2, ref copy);

                    var indexOfActionsInExplored = GetIndexOf(ref exploredGameStates, ref copy.Grid);

                    if (indexOfActionsInExplored >= 0)
                    {
                        //connu++;

                        float Vs = exploredGameStates[indexOfActionsInExplored].Returns /
                                   exploredGameStates[indexOfActionsInExplored].N;

                        if (Vs > best)
                        {
                            best = Vs;
                            bestAction = act;
                            //policy[k] = new Vector2Int(act.Item1, act.Item2);
                        }
                    }
                }

                policy[k].Set(bestAction.Item1, bestAction.Item2);
            }
        }
    }

    // Fonction d'utilité
    public int GetIndexOf(ref List<TicTacToe.GameState> gsList, ref TicTacToe.GameState.Tile[,] gs)
    {
        int idx = -1;

        for (int i = 0; i < gsList.Count; i++)
        {
            bool areEquals = true;

            //On va tester l'element i
            for (int j = 0; j < gsList[i].Grid.GetLength(0); j++)
            {
                for (int k = 0; k < gsList[i].Grid.GetLength(1); k++)
                {
                    if (gsList[i].Grid[j, k].state.Equals(gs[j, k].state))
                        continue;

                    areEquals = false;
                    break;
                }

                if (!areEquals)
                    break;
            }

            if (areEquals)
            {
                idx = i;
                break;
            }
        }

        return idx;
    }

    //Permet de simuler le GS d'un etat donne jusqu'a un etat final
    public float SimulateGameState(ref Dictionary<TicTacToe.GameState, Vector2Int> policy,
        ref List<TicTacToe.GameState> exploredState,
        ref TicTacToe.GameState gs)
    {
        int playerTurn = 0;
        bool gameEnd = false;

        // IA = Rond, donc playerWinner = 1
        int playerWinner = -1;


        while (!gameEnd)
        {
            //Je prend l'action possible
            var c = gs.GetAvailableCells();
            var selectedCell = c[Random.Range(0, c.Count)];

            var clone = gs.Clone();
            if (playerTurn == 1)
            {
                var test = policy.Keys.ToList();

                // Epsilon greedy = range entre 0 - 10, et on tire RDM, si en dessous de Epsi action rdm, sinon policy
                // a Utiliser quand on fait la simulation d'un episode


                var gridToTest =
                    GetIndexOf(ref test,
                        ref clone.Grid); //policy.Any(x => x.Key.Grid == clone.Grid);//policy.ToList().FirstOrDefault(x => x.Key.Grid == clone.Grid).Key;

                if (gridToTest >= 0)
                {
                    if (Random.Range(0.0f, 1.0f) > epsilonGreedy)
                    {
                        selectedCell = (policy[policy.ElementAt(gridToTest).Key].x,
                            policy[policy.ElementAt(gridToTest).Key].y);
                    }
                }
                else
                {
                    policy.Add(clone, new Vector2Int(selectedCell.Item1, selectedCell.Item2));
                }


                exploredState.Add(clone);
            }
            else
            {
                exploredState.Add(clone); //new Vector2Int(selectedCell.Item1, selectedCell.Item2));
            }
            // Grid, N, R, ...

            if (ticTacToe.SetCellWithoutChangeGraphics(playerTurn, selectedCell.Item1, selectedCell.Item2, ref gs))
            {
                var victoryState = ticTacToe.CheckVictory(ref gs);
                if (!victoryState.Item1)
                {
                    if (!ticTacToe.CheckNullMatch(ref gs))
                        ticTacToe.NextTurn(ref playerTurn);
                    else
                    {
                        gameEnd = true;
                        playerWinner = victoryState.Item2;
                    }
                }
                else
                {
                    gameEnd = true;
                    playerWinner = victoryState.Item2;
                }
            }
        }

        return playerWinner == 1 ? winR : playerWinner == 0 ? nulR : loseR;
    }

    // Fonction utilisé lorsqu'on joue contre l'ia
    // Elle retourne la meilleure action pour un etat donne
    public Vector2Int GetBestAction(ref TicTacToe.GameState gs)
    {
        var list = policy.Keys.ToList();
        var idx = GetIndexOf(ref list, ref gs.Grid);

        if (idx >= 0)
        {
            Debug.LogWarning("Etat connu dans la policy !!");
            return policy.ElementAt(idx).Value;
        }


        Debug.LogWarning("Cette etat n'est pas contenu dans ma policy !");
        
        var available = gs.GetAvailableCells();
        var rdm = Random.Range(0, available.Count);
        
        return new Vector2Int(available[rdm].Item1, available[rdm].Item2);
    }
    
}