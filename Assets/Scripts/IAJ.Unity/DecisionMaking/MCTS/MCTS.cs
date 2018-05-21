using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<GOB.Action> BestActionSequence { get; private set; }


        public int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        public MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }     
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 500;
            this.MaxIterationsProcessedPerFrame = 50;
            this.RandomGenerator = new System.Random();

        }


        public void InitializeMCTSSearch()
        {

            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
			this.InitialNode = new MCTSNode (this.CurrentStateWorldModel) {
				Action = null,
				Parent = null,
				PlayerID = 0
			};
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<GOB.Action>();

        }

        public GOB.Action Run()
        {
			MCTSNode selectedNode;
            Reward reward;

            var startTime = Time.realtimeSinceStartup;
            this.CurrentIterationsInFrame = 0;

			while (this.CurrentIterationsInFrame < this.MaxIterationsProcessedPerFrame) {
				//selection and expansion
				selectedNode = Selection(this.InitialNode);

				//playout or simulation
				reward = Playout (selectedNode.State);

				//backpropagate
				Backpropagate (selectedNode, reward);

				this.CurrentIterations++;
				this.CurrentIterationsInFrame++;

			}

			if (this.CurrentIterations >= this.MaxIterations) {
				this.InProgress = false;
			} else {
				return null;
			}

			this.BestFirstChild = BestChild (InitialNode);
			this.BestActionSequence.Clear ();
			this.BestActionSequence.Add (this.BestFirstChild.Action);

			var stopTime = Time.realtimeSinceStartup;
			TotalProcessingTime = startTime - stopTime;

			return this.BestFirstChild.Action;
        }




        private MCTSNode Selection(MCTSNode initialNode)
        {
            GOB.Action nextAction;
            MCTSNode currentNode = initialNode;

			while (!currentNode.State.IsTerminal()) {

				nextAction = currentNode.State.GetNextAction ();

				if (nextAction != null) {
					return Expand(currentNode, nextAction);
				} else {
					this.CurrentDepth++;
					currentNode = BestUCTChild (currentNode);
				}
			}
			return currentNode;
        }



        public virtual Reward Playout(WorldModel initialPlayoutState)
        {
			GOB.Action action;
			GOB.Action[] actions;
			int random;

			WorldModel state = initialPlayoutState.GenerateChildWorldModel ();

			while (!state.IsTerminal()) {
				//should choose randomly
				actions = state.GetExecutableActions ();

				if (actions.Length == 0)
					continue;
				
				random = RandomGenerator.Next(0, actions.Length);
				action = actions [random];
				action.ApplyActionEffects (state);

			}

			Reward r = new Reward ();
			r.Value = state.GetScore ();
			r.PlayerID = state.GetNextPlayer ();

			return r;
        }

        private void Backpropagate(MCTSNode node, Reward reward)
        {
			while (node != null) {
				node.N++;
				node.Q += reward.Value;
				
				node = node.Parent;
			}
        }

        private MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
			WorldModel state = parent.State.GenerateChildWorldModel();
			MCTSNode expand = new MCTSNode (state);
			expand.Parent = parent;
			action.ApplyActionEffects (state);
			expand.Action = action;
			state.CalculateNextPlayer ();
			expand.PlayerID = state.GetNextPlayer ();
			parent.ChildNodes.Add (expand);

			expand.N = 0;
			expand.Q = 0;

			return expand;
        }

        //gets the best child of a node, using the UCT formula
        private MCTSNode BestUCTChild(MCTSNode node)
        {
			double BestUCT = -1;
			double uct;
			float u;
			List<MCTSNode> children = node.ChildNodes;
			MCTSNode bestNode = children[0];
			float parentVisits = Mathf.Log(node.N);


			foreach (MCTSNode n in children) {
				
				u = n.Q / n.N;
				uct = u + C * Mathf.Sqrt (parentVisits / n.N);

				if (n.N == 0)
					continue;
				if (uct > BestUCT) {
					BestUCT = uct;
					bestNode = n;
				}
			}

			return bestNode;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        private MCTSNode BestChild(MCTSNode node)
        {
			float BestUCT = -1;
			float u;
			List<MCTSNode> children = node.ChildNodes;
			MCTSNode bestNode;
			if (children.Count == 0) {
				bestNode = node;
			} else {
				bestNode = children [0];
			}

			foreach (MCTSNode n in node.ChildNodes) {
				u = n.Q / n.N;
				if (n.N == 0)
					continue;
				if (u > BestUCT) {
					BestUCT = u;
					bestNode = n;
				}
			}

			return bestNode;
        }
    }
}
