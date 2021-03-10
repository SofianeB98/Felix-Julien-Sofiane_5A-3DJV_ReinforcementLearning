using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sokoban
{
    public class SokobanAgent
    {
        public enum Algo
        {
            PolicyIteration,
            ValueIteration,
            MC_ES_FirstVisit,
            MC_ES_EveryVisit,
            Sarsa,
            QLearning
        }

        public Algo algo;

        // Pour jouer
        public Dictionary<SokobanGameState, IAction> policy = new Dictionary<SokobanGameState, IAction>();
        
        // Q(S, A) = Valeur
        // On associe un couple GS - Action, pour une valeur q
        // Pour l'entrainement
        public Dictionary<(SokobanGameState, IAction), float> q_sa = new Dictionary<(SokobanGameState, IAction), float>();

        
        // Sarsa peut etre alimenter QSA au fur et a mesure
        // Seul un couple Action State est possible !!!!
        // Donc q sa mis sur GAme state et non comme ci dessus !
        
        // R a mettre au fur et a mesure est une bonne piste
        // GROS GROS Reward quand toute les caisse sont sur les points (100)
        // Deplacement -1
        // Caisse sur un point petit reward (1)
        
        // Action dispo = On choisit, mais préférable de donner les vrai action dispo
        
        // Q_SA global et Q_SA_Temporaire qui copy qsa, mais qui peux choisir une action différente
        // Donc on set la nouvelle action dans le temp, et on reset sont q_sa, et on accroit
        // Ce qui nous donne, pour 1 meme GS donnee, 4 Action maximum, et on prend la meilleure
        
        public void Init(ref SokobanGameState gs, ref List<IAction> allActions)
        {
            
        }

        public void Simulate()
        {
            switch (algo)
            {
                case Algo.Sarsa:
                    
                    break;
            }
        }

        // Appeler pour SARSA
        private void Simulate_SARSA(ref SokobanGameState gs, ref List<IAction> allActions, float alpha = 0.1f, float gamma = 0.9f, int episodeCount = 10)
        {
            for (int e = 0; e < episodeCount; e++)
            {
                var initialGS = gs.Clone();
                
                // Choisir une action parmis celle available
                // Selon Epsilon Greedy (donc soit RDM, soit celui de la "policy")
            }
        }
    }
}