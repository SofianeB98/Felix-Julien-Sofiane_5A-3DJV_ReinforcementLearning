using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AgentTicTacToe
{
    public TicTacToe ticTacToe;

    public bool isFirstVisit = false;
    public bool isOnPolicy = false;

    // Policy ? Ou le jeu donne la policy ?
    public Dictionary<TicTacToe.GameState, Vector2Int> policy;

    // Faut un tableau d'etat et leur action possible ?


    public void Simulate(ref TicTacToe.GameState gs, int episodeCount = 10)
    {
        var V = 0.0f;

        var copyGs = new TicTacToe.GameState(gs.Grid, gs.N, gs.Returns);

        // Selection - Expansion - Simulation - Retropopagation

        // GS - 1
        // ==> N state possible (avec N Action)
        // ==> Selection un state N
        // ==> On lance MC Prediction

        if (isFirstVisit)
        {
            V = FirstVisitMCPrediction(ref policy, copyGs, episodeCount);
        }
        else
        {
            V = EveryVisitMCPrediction(ref policy, copyGs, episodeCount);
        }

        var b = 0;
    }

    public float FirstVisitMCPrediction(ref Dictionary<TicTacToe.GameState, Vector2Int> policy,
        TicTacToe.GameState gs_copy, int episodeCount = 10)
    {
        float V = 0.0f;

        //list ou dico de chaque etat
        Dictionary<TicTacToe.GameState, Vector2Int> explored = new Dictionary<TicTacToe.GameState, Vector2Int>();

        for (int i = 0; i < episodeCount; i++)
        {
            // Copy du GS
            var copyGs = gs_copy.Clone();//new TicTacToe.GameState(gs_copy.Grid, gs_copy.N, gs_copy.Returns);

            // Generer une simulation de jeu
            // On va récupérer N state possible
           // SimulateGameState(ref explored, ref gs_copy);

            float G = 0;

            // On retropopage tout
            for (int t = 1; t >= 0; t++)
            {
            }
        }

        // Avec cette list ou dico on met dans policy

        return V;
    }

    public float EveryVisitMCPrediction(ref Dictionary<TicTacToe.GameState, Vector2Int> policy,
        TicTacToe.GameState gs_copy, int episodeCount = 10)
    {
        float V = 0.0f;

        //list ou dico de chaque etat
        //Pouvoir récupérer chaque etat pour un etat donné
        //Get all posibilitie ==> Regarder dans la policy le meilleure V (V = Return / N)
        
        //List< Dictionary<TicTacToe.GameState, Vector2Int> > explored0 = new List<Dictionary<TicTacToe.GameState, Vector2Int>>();
        //Dictionary<TicTacToe.GameState, Vector2Int> exploredTmp = new Dictionary<TicTacToe.GameState, Vector2Int>();
        List<(TicTacToe.GameState, Vector2Int)> explored = new List<(TicTacToe.GameState, Vector2Int)>();

        for (int i = 0; i < episodeCount; i++)
        {
            Dictionary<TicTacToe.GameState, Vector2Int> exploredTmp = new Dictionary<TicTacToe.GameState, Vector2Int>();
            
            // Copy du GS
            var copyGs = gs_copy.Clone();
            
            // Generer une simulation de jeu
            // On va récupérer N state possible
            float R = SimulateGameState(ref policy, ref exploredTmp, ref copyGs);
            float G = 0;

            // Checker si le state qu'on va incrémenté est contenu dans Explored
            // Si c'est le cas on l'incrémenter lui
            // Sinon on ajoute un nouveau et on incrémente
            var list = exploredTmp.Keys.ToList();
            
            // On retropopage tout
            for (int t = list.Count - 1; t >= 0; t--)
            {
                G = G + R;

                var strct = list[t];

                if (explored.Contains(
                    explored.First(x => x.Item1.Grid == strct.Grid)
                    ))
                {
                    // On incrémente celui deja existant
                }
                else
                {
                    //Sinon on incrément celui qui existe pas et on l'ajoute
                    strct.SetReturns(list[t].Returns + G);
                    strct.SetN(list[t].N + 1);

                    list[t] = strct;
                }
                
                
                // On Policy
                // Epsilon greedy = range entre 0 - 10, et on tire RDM, si en dessous de Epsi action rdm, sinon policy
                // var policyGS = policy.FirstOrDefault(x => x.Key.Grid == e.Grid);
                // if (policy.ContainsKey(policyGS.Key))
                // {
                //     policyGS.Key.SetN(e.N);
                //     policyGS.Key.SetReturns(e.Returns);
                // }
            }

            //int cIdx = 0;
            // foreach (var e in exploredTmp.Keys)
            // {
            //     e.SetN(list[cIdx].N);
            //     e.SetReturns(list[cIdx++].Returns);
            //
            
            // }
            
            //explored0.Add(exploredTmp);
            
            // VS = Return / N
        }
        
        // Off policy, on passe sur Explored, Pour un etat S donne, on cherche toutes les Actions atteignable
        // Pour ces action atteignable on va prendre la meilleure possible V = (Returns / N) et assigner 
        // exemple : Etat null => je cherche tout les etat atteignable (= action)
        // Parmis ces actions, je regarde celui qui a le meilleure V
        // Et j'applique la value à la policy[key]
        
        // ajouter la policy RDM en init, puis ajouter les GS manquant au fur et a mpesure
        
        // Avec cette list ou dico on met a jour dans policy
        

        return V;
    }

    public float SimulateGameState(ref Dictionary<TicTacToe.GameState, Vector2Int> policy, ref Dictionary<TicTacToe.GameState, Vector2Int> exploredState,
        ref TicTacToe.GameState gs)
    {
        int playerTurn = 0;
        bool gameEnd = false;

        // IA = Rond, donc playerWinner = 1
        int playerWinner = -1;

        
        while (!gameEnd)
        {
            //Je prend l'action possible
            var c = gs.GetAvailableCell();
            var selectedCell = c[Random.Range(0, c.Count)];
            
            var clone = gs.Clone();
            var gridToTest = policy.ToList().FirstOrDefault(x => x.Key.Grid == clone.Grid).Key;
            if (policy.ContainsKey(gridToTest))
            {
                selectedCell = (policy[gridToTest].x, policy[gridToTest].y);
            }
            else
            {
                policy.Add(clone, new Vector2Int(selectedCell.Item1, selectedCell.Item2));
            }
            // Grid, N, R, ...

            exploredState.Add(clone, new Vector2Int(selectedCell.Item1, selectedCell.Item2));
            
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

        return playerWinner == 1 ? 1.0f : 0.0f;
    }
}