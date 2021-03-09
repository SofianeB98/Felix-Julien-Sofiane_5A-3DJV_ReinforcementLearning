using System.Collections.Generic;
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
            SimulateGameState(ref explored, ref gs_copy);

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
        Dictionary<TicTacToe.GameState, Vector2Int> explored = new Dictionary<TicTacToe.GameState, Vector2Int>();

        for (int i = 0; i < episodeCount; i++)
        {
            // Copy du GS
            var copyGs = gs_copy.Clone();

            // Generer une simulation de jeu
            // On va récupérer N state possible
            SimulateGameState(ref explored, ref copyGs);

            float G = 0;

            // On retropopage tout
            for (int t = 1; t >= 0; t++)
            {
                
            }
        }

        // Avec cette list ou dico on met dans policy

        return V;
    }

    public void SimulateGameState(ref Dictionary<TicTacToe.GameState, Vector2Int> exploredState,
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
            
            if (ticTacToe.SetCellWithoutChangeGraphics(playerTurn, selectedCell.Item1, selectedCell.Item2, ref gs))
            {
                exploredState.Add(gs.Clone(), new Vector2Int(selectedCell.Item1, selectedCell.Item2));
                
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
    }
}