using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class Reward
    {
        public float Value { get; set; }
        public int PlayerID { get; set; }

		public float GetNodeReward(MCTSNode node)
		{
			MCTSNode parent = node.Parent;
			int player = parent.PlayerID;

			//only for the initial node
			if(parent == null)
			{
				return this.Value;
			}

			if(player == this.PlayerID)
			{
				return this.Value;
			}
			else
			{
				return 1-this.Value;
			}
		}
    }
}
