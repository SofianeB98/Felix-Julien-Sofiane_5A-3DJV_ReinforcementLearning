using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        GameStateActionComparer gameStateActionComparer = new GameStateActionComparer();
        public Dictionary<(SokobanGameState, IAction), float> q_sa;

        private float epsilonGreedy = 0.6f; // entre 0 et 1
        private int maxIteration = 1000;

        private float theta = 0.005f;


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

        // Pour Qlearning, ignorer ligne 7, testr toutes les actions possible et garde le q(s', a') maximum pour le calcule

        public void Init(ref SokobanGameState gs, ref List<IAction> allActions, Algo agent, float alpha = 0.1f,
            float gamma = 0.9f, int episodeCount = 50, bool useOnPolicy = false, float eps = 0.5f, float theta = 0.005f)
        {
            this.algo = agent;

            this.epsilonGreedy = eps;
            this.theta = theta;

            q_sa = new Dictionary<(SokobanGameState, IAction), float>(gameStateActionComparer);
            policy = new Dictionary<SokobanGameState, IAction>(gameStateComparer);

            Simulate(ref gs, ref allActions, alpha, gamma, episodeCount, useOnPolicy);
        }

        public void Simulate(ref SokobanGameState gs, ref List<IAction> allActions, float alpha = 0.1f,
            float gamma = 0.9f, int episodeCount = 10, bool useOnPolicy = false)
        {
            Debug.LogWarning($"Agent entraine sur : {algo.ToString()}");
            
            switch (algo)
            {
                case Algo.Sarsa:
                    Simulate_SARSA(ref gs, ref allActions, alpha, gamma, episodeCount);
                    break;

                case Algo.MC_ES_EveryVisit:
                    Simulate_MonteCarlo_EV(ref policy, gs.Clone(), episodeCount, useOnPolicy);
                    break;

                case Algo.MC_ES_FirstVisit:
                    Simulate_MonteCarlo_FV(ref policy, gs.Clone(), episodeCount, useOnPolicy);
                    break;

                case Algo.QLearning:
                    Simulate_QLearning(ref gs, ref allActions, alpha, gamma, episodeCount);
                    break;

                case Algo.ValueIteration:
                    ValueIteration();
                    break;

                case Algo.PolicyIteration:
                    //Lancer une coroutine qui call toutes les N seconde PolicyEvalutaion
                    break;
            }
        }

        #region TemporalDifferenceLearning

        // Appeler pour SARSA
        private void Simulate_SARSA(ref SokobanGameState gs, ref List<IAction> allActions, float alpha = 0.1f,
            float gamma = 0.9f, int episodeCount = 10)
        {
            for (int e = 0; e < episodeCount; e++)
            {
                Debug.LogWarning($"Episode {e + 1} - Sarsa");
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

                int iteration = 0;
                bool gameFinish = false;
                while (iteration < maxIteration && !gameFinish)
                {
                    iteration++;

                    var sPrime = s.Clone();
                    var objectifComplete = a.Perform(ref sPrime);

                    gameFinish = sPrime.CheckFinish();
                    var gameOver = sPrime.CheckGameOver();

                    // Choisir a prime
                    var availableActionsPrime = sPrime.GetAvailableActions();
                    var aPrime = availableActionsPrime.Count > 0
                        ? availableActionsPrime[Random.Range(0, availableActionsPrime.Count)]
                        : null;

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
                    // Ajouter s et a
                    if (!q_sa.ContainsKey((s, a)))
                        q_sa.Add((s, a), 0.0f);

                    if (!q_sa.ContainsKey((sPrime, aPrime)))
                        q_sa.Add((sPrime, aPrime), 0.0f);


                    sPrime.r = gameFinish ? 1000.0f : objectifComplete ? 10.0f : -1.0f;

                    // Update de Q
                    q_sa[(s, a)] += alpha * sPrime.r + gamma * q_sa[(sPrime, aPrime)] - q_sa[(s, a)];

                    if (gameFinish)
                        break;

                    // Application de s et a pour la prochaine itération
                    s = sPrime;
                    a = aPrime;
                }

                // Mettre a jour la policy
                List<(SokobanGameState, IAction)> newActions = new List<(SokobanGameState, IAction)>();
                foreach (var key in policy.Keys)
                {
                    var common = q_sa.Keys.ToList().FindAll(x => x.Item1.Equals(key));
                    if (common.Count <= 0)
                        continue;

                    float bestQ = float.MinValue;
                    var bestAction = policy[key];
                    for (int i = 0; i < common.Count; i++)
                    {
                        if (q_sa[common[i]] > bestQ)
                        {
                            bestQ = q_sa[common[i]];
                            bestAction = common[i].Item2;
                        }
                    }

                    //policy[key] = bestAction;
                    newActions.Add((key, bestAction));
                }

                foreach (var act in newActions)
                {
                    policy[act.Item1] = act.Item2;
                }

                Debug.Log($"Episode {e + 1} / {episodeCount} fini");
            }
        }

        // Q Learning
        private void Simulate_QLearning(ref SokobanGameState gs, ref List<IAction> allActions, float alpha = 0.1f,
            float gamma = 0.9f, int episodeCount = 10)
        {
            for (int e = 0; e < episodeCount; e++)
            {
                Debug.LogWarning($"Episode {e + 1} - QL");
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

                int iteration = 0;
                bool gameFinish = false;
                while (iteration < maxIteration && !gameFinish)
                {
                    iteration++;

                    var sPrime = s.Clone();
                    var objectifComplete = a.Perform(ref sPrime);

                    gameFinish = sPrime.CheckFinish();
                    //var gameOver = sPrime.CheckGameOver();

                    //Ajout a q_sa ou deja contenu donc on incremente
                    //Checker si existant, sinon ajouter
                    // Ajouter s et a
                    if (!q_sa.ContainsKey((s, a)))
                        q_sa.Add((s, a), 0.0f);


                    // Choisir a prime (Ici on doit tester tout les aPrime et selectionner celui qui maximise la recompense (on prend Q[ sP aP] qui est le mieux))
                    // Ajouter au fur et a mesure dans Q
                    var availableActionsPrime = sPrime.GetAvailableActions();
                    //var aPrime = availableActionsPrime.Count > 0
                    //   ? availableActionsPrime[Random.Range(0, availableActionsPrime.Count)]
                    //    : null;
                    IAction aPrime = availableActionsPrime.Count > 0 ? availableActionsPrime[0] : null;
                    float bestAPrime = 0.0f;
                    foreach (var actPrime in availableActionsPrime)
                    {
                        if (!policy.ContainsKey(sPrime))
                        {
                            policy.Add(sPrime, actPrime);
                        }

                        if (!q_sa.ContainsKey((sPrime, actPrime)))
                            q_sa.Add((sPrime, actPrime), 0.0f);

                        if (aPrime == null)
                            aPrime = actPrime;
                        
                        if (q_sa[(sPrime, actPrime)] > bestAPrime)
                        {
                            bestAPrime = q_sa[(sPrime, actPrime)];
                            aPrime = actPrime;
                        }
                    }


                    sPrime.r = gameFinish ? 1000.0f : objectifComplete ? 10.0f : -1.0f;

                    // Update de Q
                    q_sa[(s, a)] += alpha * sPrime.r + gamma * bestAPrime - q_sa[(s, a)];

                    if (gameFinish)
                        break;

                    // Application de s et a pour la prochaine itération
                    s = sPrime;
                    a = aPrime;
                }

                // Mettre a jour la policy
                List<(SokobanGameState, IAction)> newActions = new List<(SokobanGameState, IAction)>();
                foreach (var key in policy.Keys)
                {
                    var common = q_sa.Keys.ToList().FindAll(x => x.Item1.Equals(key));
                    if (common.Count <= 0)
                        continue;

                    float bestQ = float.MinValue;
                    var bestAction = policy[key];
                    for (int i = 0; i < common.Count; i++)
                    {
                        if (q_sa[common[i]] > bestQ)
                        {
                            bestQ = q_sa[common[i]];
                            bestAction = common[i].Item2;
                        }
                    }

                    //policy[key] = bestAction;
                    newActions.Add((key, bestAction));
                }

                foreach (var act in newActions)
                {
                    policy[act.Item1] = act.Item2;
                }

                Debug.Log($"Episode {e + 1} / {episodeCount} fini");
            }
        }

        #endregion

        #region MonteCarloES

        private void Simulate_MonteCarlo_FV(ref Dictionary<SokobanGameState, IAction> pi, SokobanGameState gs_copy,
            int episodeCount = 50, bool useOnPolicy = false)
        {
            List<SokobanGameState> exploredGameStates = new List<SokobanGameState>();
            for (int e = 0; e < episodeCount; e++)
            {
                Debug.LogWarning($"Episode {e + 1} - MC ES");
                List<SokobanGameState> serie = new List<SokobanGameState>();
                List<SokobanGameState> processedEpisodes = new List<SokobanGameState>();

                var copyGs = gs_copy.Clone();

                float R = SimulateGameState(ref pi, ref serie, ref copyGs);
                float G = 0;

                for (int t = serie.Count - 2; t >= 0; t--)
                {
                    G += serie[t + 1].r; //a remplacer par le reward de T + 1
                    var GSt = serie[t];

                    if (processedEpisodes.FindIndex(x => x.Equals(GSt)) == -1)
                    {
                        int idxInExplo = exploredGameStates.FindIndex(x => x.Equals(GSt));
                        if (idxInExplo >= 0)
                        {
                            GSt = exploredGameStates[idxInExplo];
                            GSt.Returns += G;
                            GSt.N += 1;

                            exploredGameStates[idxInExplo] = GSt;
                        }
                        else
                        {
                            GSt.Returns += G;
                            GSt.N += 1;
                            serie[t] = GSt;

                            exploredGameStates.Add(GSt);
                        }

                        processedEpisodes.Add(GSt);
                    }
                }

                if (useOnPolicy)
                {
                    List<(SokobanGameState, IAction)> newActions = new List<(SokobanGameState, IAction)>();
                    foreach (var key in pi.Keys)
                    {
                        float best = float.MinValue;
                        IAction bestAction = pi[key];

                        var availableActions = key.GetAvailableActions();
                        foreach (var act in availableActions)
                        {
                            var copy = key.Clone();
                            act.Perform(ref copy);

                            int idxInExplo = exploredGameStates.FindIndex(x => x.Equals(copy));
                            if (idxInExplo >= 0)
                            {
                                float Vs = exploredGameStates[idxInExplo].Returns / exploredGameStates[idxInExplo].N;

                                if (Vs > best)
                                {
                                    best = Vs;
                                    bestAction = act;
                                }
                            }
                        }

                        newActions.Add((key, bestAction));
                    }

                    Debug.Log($"Nombre d etat connu : {exploredGameStates.Count}");

                    foreach (var act in newActions)
                    {
                        pi[act.Item1] = act.Item2;
                    }
                }
            }

            if (!useOnPolicy)
            {
                List<(SokobanGameState, IAction)> newActions = new List<(SokobanGameState, IAction)>();
                foreach (var key in pi.Keys)
                {
                    float best = float.MinValue;
                    IAction bestAction = pi[key];

                    var availableActions = key.GetAvailableActions();
                    foreach (var act in availableActions)
                    {
                        var copy = key.Clone();
                        act.Perform(ref copy);

                        int idxInExplo = exploredGameStates.FindIndex(x => x.Equals(copy));
                        if (idxInExplo >= 0)
                        {
                            float Vs = exploredGameStates[idxInExplo].Returns / exploredGameStates[idxInExplo].N;

                            if (Vs > best)
                            {
                                best = Vs;
                                bestAction = act;
                            }
                        }
                    }

                    newActions.Add((key, bestAction));
                }

                Debug.Log($"Nombre d etat connu : {exploredGameStates.Count}");

                foreach (var act in newActions)
                {
                    pi[act.Item1] = act.Item2;
                }
            }
        }

        private void Simulate_MonteCarlo_EV(ref Dictionary<SokobanGameState, IAction> pi, SokobanGameState gs_copy,
            int episodeCount = 50, bool useOnPolicy = false)
        {
            List<SokobanGameState> exploredGameStates = new List<SokobanGameState>();
            for (int e = 0; e < episodeCount; e++)
            {
                Debug.LogWarning($"Episode {e + 1} - MC ES");
                List<SokobanGameState> serie = new List<SokobanGameState>();
                var copyGs = gs_copy.Clone();

                float R = SimulateGameState(ref pi, ref serie, ref copyGs);
                float G = 0;

                for (int t = serie.Count - 2; t >= 0; t--)
                {
                    G += serie[t + 1].r; //a remplacer par le reward de T + 1
                    var GSt = serie[t];

                    int idxInExplo = exploredGameStates.FindIndex(x => x.Equals(GSt));
                    if (idxInExplo >= 0)
                    {
                        GSt = exploredGameStates[idxInExplo];
                        GSt.Returns += G;
                        GSt.N += 1;

                        exploredGameStates[idxInExplo] = GSt;
                    }
                    else
                    {
                        GSt.Returns += G;
                        GSt.N += 1;
                        serie[t] = GSt;

                        exploredGameStates.Add(GSt);
                    }
                }

                if (useOnPolicy)
                {
                    int GSConnu = 0;

                    List<(SokobanGameState, IAction)> newActions = new List<(SokobanGameState, IAction)>();
                    foreach (var key in pi.Keys)
                    {
                        float best = float.MinValue;
                        IAction bestAction = pi[key];

                        var availableActions = key.GetAvailableActions();
                        foreach (var act in availableActions)
                        {
                            var copy = key.Clone();
                            act.Perform(ref copy);

                            int idxInExplo = exploredGameStates.FindIndex(x => x.Equals(copy));
                            if (idxInExplo >= 0)
                            {
                                GSConnu++;
                                float Vs = exploredGameStates[idxInExplo].Returns / exploredGameStates[idxInExplo].N;

                                if (Vs > best)
                                {
                                    best = Vs;
                                    bestAction = act;
                                }
                            }
                        }

                        newActions.Add((key, bestAction));
                    }

                    foreach (var act in newActions)
                    {
                        pi[act.Item1] = act.Item2;
                    }
                }
            }

            if (!useOnPolicy)
            {
                List<(SokobanGameState, IAction)> newActions = new List<(SokobanGameState, IAction)>();
                foreach (var key in pi.Keys)
                {
                    float best = float.MinValue;
                    IAction bestAction = pi[key];

                    var availableActions = key.GetAvailableActions();
                    foreach (var act in availableActions)
                    {
                        var copy = key.Clone();
                        act.Perform(ref copy);

                        int idxInExplo = exploredGameStates.FindIndex(x => x.Equals(copy));
                        if (idxInExplo >= 0)
                        {
                            float Vs = exploredGameStates[idxInExplo].Returns / exploredGameStates[idxInExplo].N;

                            if (Vs > best)
                            {
                                best = Vs;
                                bestAction = act;
                            }
                        }
                    }

                    newActions.Add((key, bestAction));
                }

                Debug.Log($"Nombre d etat connu : {exploredGameStates.Count}");

                foreach (var act in newActions)
                {
                    pi[act.Item1] = act.Item2;
                }
            }
        }

        private float SimulateGameState(ref Dictionary<SokobanGameState, IAction> pi,
            ref List<SokobanGameState> exploredGS, ref SokobanGameState gs)
        {
            Debug.LogWarning("Start MC ES Simulation");
            // On simule le GS
            // Ne pas oublier la selection pas Epsilon Greedy
            bool gameWin = false;
            bool gameOver = false;
            int iteration = 0;

            while (!gameWin && !gameOver && iteration < 10000)
            {
                iteration++;

                var acts = gs.GetAvailableActions();
                var selectedAct = acts[Random.Range(0, acts.Count)];

                var gsCopy = gs.Clone();
                gsCopy.r = -1.0f;

                if (policy.ContainsKey(gsCopy))
                {
                    if (Random.Range(0.0f, 1.0f) > epsilonGreedy)
                        selectedAct = pi[gsCopy];
                }
                else
                {
                    pi.Add(gsCopy, selectedAct);
                }

                exploredGS.Add(gsCopy);

                bool objectifComplete = selectedAct.Perform(ref gs);
                if (objectifComplete)
                    gs.r = 10.0f;
                else
                    gs.r = -1.0f;

                gameOver = false; //a changer
                if (gameOver)
                    gs.r = -1000.0f;

                gameWin = gs.CheckFinish();
                if (gameWin)
                    gs.r = 1000.0f;
            }

            return gameWin ? 1.0f : 0.0f;
        }

        #endregion

        #region DynamicProgramming

        private void PolicyImprovement()
        {
            // Regarder si il est possible d'accéder à des etat en fonction de notre etat
            // Etre Myope

            // Pos du joueur peut etre pas utile
        }

        private void PolicyEvaluation()
        {
        }

        private void ValueIteration()
        {
        }

        #endregion


        public IAction GetBestAction(ref SokobanGameState gs)
        {
            var available = gs.GetAvailableActions();

            if (policy.ContainsKey(gs))
            {
                var idx = policy.Keys.ToList().IndexOf(gs);
                var act = (MoveAction) policy[gs];
                Debug.Log($"Yes je connais cet etat !!! {act.direction}");
                return act;
            }


            Debug.Log("Cet etat ne fait pas parti de ma policy bande de fou !");

            return available[Random.Range(0, available.Count)];
        }
    }
}