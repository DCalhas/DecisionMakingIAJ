using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
	public class MCTSBiasedPlayout : MCTS
	{
		public MCTSBiasedPlayout(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
		{
		}

		public override Reward Playout(WorldModel initialPlayoutState)
		{ 
			GOB.Action action = null;
			GOB.Action[] actions;

			WorldModel state = initialPlayoutState.GenerateChildWorldModel ();

			while (!state.IsTerminal()) {
				//should choose randomly
				actions = state.GetExecutableActions ();


				float best = float.MinValue;
				foreach (var a in actions) {
					WorldModel w = state.GenerateChildWorldModel ();
					a.ApplyActionEffects (w);
					var heuristic = w.GetGoalValue ("BeQuick") + 1/w.GetGoalValue ("GainXP") + w.GetGoalValue ("Survive") + w.GetGoalValue ("GetRich");

					if (heuristic > best) {
						best = heuristic;
						action = a;
					}
				}





				action.ApplyActionEffects (state);

			}

			Reward r = new Reward ();
			r.Value = state.GetScore ();
			r.PlayerID = state.GetNextPlayer ();

			return r;
		}
	}
}
