using System.Collections;
using System.Linq;
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
        // Une seul action possible pour un etat donne
        GameStateComparer gameStateComparer = new GameStateComparer();
        public Dictionary<SokobanGameState, IAction> policy;
        
        // Q(S, A) = Valeur
        // On associe un couple GS - Action, pour une valeur q
        // Pour l'entrainement
        public Dictionary<(SokobanGameState, IAction), float> q_sa = new Dictionary<(SokobanGameState, IAction), float>();

        public float epsilonGreedy = 0.3f; // entre 0 et 1
        
        
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
            policy = new Dictionary<SokobanGameState, IAction>(gameStateComparer);
            Simulate(ref gs, ref allActions);
        }

        public void Simulate(ref SokobanGameState gs, ref List<IAction> allActions, float alpha = 0.1f, float gamma = 0.9f, int episodeCount = 3)
        {
            switch (algo)
            {
                case Algo.Sarsa:
                    Simulate_SARSA(ref gs, ref allActions);
                    break;
            }
        }

        // Appeler pour SARSA
        private void Simulate_SARSA(ref SokobanGameState gs, ref List<IAction> allActions, float alpha = 0.1f, float gamma = 0.9f, int episodeCount = 3)
        {
            for (int e = 0; e < episodeCount; e++)
            {
                var s = gs.Clone();
                s.r = -1.0f;
                
                // Choisir une action parmis celle available
                // Selon Epsilon Greedy (donc soit RDM, soit celui de la "policy")
                var availableActions = s.GetAvailableActions();
                var a = availableActions[Random.Range(0, availableActions.Count)];
                
                // Point d'interogation la dessus
                if (policy.ContainsKey(s))
                {
                    // Policy
                    if (Random.Range(0.0f, 1.0f) > epsilonGreedy)
                        a = policy[s];
                }
                else
                {
                    policy.Add(s, a);
                }

                // Ajouter s et a
                
                int iteration = 0;
                bool gameFinish = false;
                while (iteration < 2 && !gameFinish)
                {
                    iteration++;
                    
                    var sPrime = s.Clone();
                    a.Perform(ref sPrime);
                    
                    gameFinish = sPrime.CheckFinish();
                    
                    // Choisir a prime
                    var availableActionsPrime = sPrime.GetAvailableActions();
                    var aPrime = availableActionsPrime.Count > 0 ? availableActions[Random.Range(0, availableActions.Count)] : null;

                    if (policy.ContainsKey(sPrime))
                    {
                        // Policy
                        if (Random.Range(0.0f, 1.0f) > epsilonGreedy)
                            aPrime = policy[sPrime];
                    }
                    else
                    {
                        policy.Add(sPrime, aPrime);
                    }
                    
                    //Ajout a q_sa ou deja contenu donc on incremente
                    //Checker si existant, sinon ajouter
                    if(!q_sa.ContainsKey((s, a)))
                        q_sa.Add((s, a), 0.0f);
                    
                    if(!q_sa.ContainsKey((sPrime, aPrime)))
                        q_sa.Add((sPrime, aPrime), 0.0f);
                    
                    
                    sPrime.r = gameFinish ? 100.0f : -1.0f;
                    
                    // Update de Q
                    q_sa[(s, a)] += alpha * sPrime.r + gamma * q_sa[(sPrime, aPrime)] - q_sa[(s, a)];

                    if (gameFinish)
                        break;
                    
                    // Application de s et a pour la prochaine itération
                    s = sPrime;
                    a = aPrime;
                }

            }
        }
    }
}