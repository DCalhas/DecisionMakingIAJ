using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using System;
using UnityEngine;

namespace Assets.Scripts.DecisionMakingActions
{
    public class GetHealthPotion : WalkToTargetAndExecuteAction
    {
        public GetHealthPotion(AutonomousCharacter character, GameObject target) : base("GetHealthPotion",character,target)
        {
        }

		public override bool CanExecute()
		{
			if (!base.CanExecute()) return false;
			return true;
		}

		public override bool CanExecute(WorldModel worldModel)
		{
			if (!base.CanExecute(worldModel)) return false;

			return true;
		}

		public override void Execute()
		{
			base.Execute();
			this.Character.GameManager.GetHealthPotion (this.Target);
		}


		public override void ApplyActionEffects(WorldModel worldModel)
		{
			base.ApplyActionEffects(worldModel);
			worldModel.SetProperty(Properties.HP, (int)worldModel.GetProperty(Properties.MAXHP));
			//disables the target object so that it can't be reused again
			worldModel.SetProperty(this.Target.name, false);
		}
    }
}
