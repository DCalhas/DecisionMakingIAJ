using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
	public class WorldModel
	{

		private object[] Properties;
		private object[] Resource;
		private List<Action> Actions { get; set; }
		protected IEnumerator<Action> ActionEnumerator { get; set; }

		private Dictionary<string, float> GoalValues { get; set; }

		protected WorldModel Parent { get; set; }

		public WorldModel(List<Action> actions)
		{
			this.Properties = new object[8];
			this.Resource = new object[14];
			this.GoalValues = new Dictionary<string, float>();
			this.Actions = actions;
			this.ActionEnumerator = actions.GetEnumerator();
		}

		public WorldModel(WorldModel parent)
		{
			this.Properties = new object[8];
			this.Resource = new object[14];
			this.GoalValues = new Dictionary<string, float> ();
			this.Actions = parent.Actions;
			this.Parent = parent;
			this.ActionEnumerator = this.Actions.GetEnumerator();
		}

		public virtual object GetProperty(string propertyName)
		{
			//recursive implementation of WorldModel
			int aux = GetNumberFromName(propertyName);
			int aux2 = GetNumberFromNameResources(propertyName);
			if (aux != -1 && this.Properties[aux] != null)
			{
				return this.Properties[aux];
			}
			else if (aux2 != -1 && this.Resource[aux2] != null)
			{
				return this.Resource[aux2];
			}
			else if (this.Parent != null)
			{
				return this.Parent.GetProperty(propertyName);
			}
			else
			{
				return null;
			}
		}

		public virtual void SetProperty(string propertyName, object value)
		{
			int aux = GetNumberFromName(propertyName);
			if (aux != -1)
			{
				this.Properties[aux] = value;
			}
			else
			{
				int aux2 = GetNumberFromNameResources(propertyName);
				if(aux2 != -1)
					this.Resource[aux2] = value;
			}
		}

		public virtual float GetGoalValue(string goalName)
		{
			//recursive implementation of WorldModel
			if (this.GoalValues.ContainsKey(goalName))
			{
				return this.GoalValues[goalName];
			}
			else if (this.Parent != null)
			{
				return this.Parent.GetGoalValue(goalName);
			}
			else
			{
				return 0;
			}
		}

		public virtual void SetGoalValue(string goalName, float value)
		{
			var limitedValue = value;
			if (value > 10.0f)
			{
				limitedValue = 10.0f;
			}

			else if (value < 0.0f)
			{
				limitedValue = 0.0f;
			}

			this.GoalValues[goalName] = limitedValue;
		}

		public virtual WorldModel GenerateChildWorldModel()
		{
			return new WorldModel(this);
		}

		public float CalculateDiscontentment(List<Goal> goals)
		{
			var discontentment = 0.0f;

			foreach (var goal in goals)
			{
				var newValue = this.GetGoalValue(goal.Name);

				discontentment += goal.GetDiscontentment(newValue);
			}

			return discontentment;
		}

		public virtual Action GetNextAction()
		{
			Action action = null;
			//returns the next action that can be executed or null if no more executable actions exist
			if (this.ActionEnumerator.MoveNext())
			{
				action = this.ActionEnumerator.Current;
			}

			while (action != null && !action.CanExecute(this))
			{
				if (this.ActionEnumerator.MoveNext())
				{
					action = this.ActionEnumerator.Current;
				}
				else
				{
					action = null;
				}
			}

			return action;
		}

		public virtual Action[] GetExecutableActions()
		{
			return this.Actions.Where(a => a.CanExecute(this)).ToArray();
		}

		public virtual bool IsTerminal()
		{
			return true;
		}


		public virtual float GetScore()
		{
			return 0.0f;
		}

		public virtual int GetNextPlayer()
		{
			return 0;
		}

		public virtual void CalculateNextPlayer()
		{
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int GetNumberFromName(string n)
		{
			if (n.Equals("Mana"))
				return 0;
			else if (n.Equals("HP"))
				return 1;
			else if (n.Equals("MAXHP"))
				return 2;
			else if (n.Equals("XP"))
				return 3;
			else if (n.Equals("Time"))
				return 4;
			else if (n.Equals("Money"))
				return 5;
			else if (n.Equals("Level"))
				return 6;
			else if (n.Equals("Position"))
				return 7;

			return -1;

		}

		public int GetNumberFromNameResources(string n)
		{
			if (n.Equals("Skeleton1"))
				return 0;
			else if (n.Equals("Skeleton2"))
				return 1;
			else if (n.Equals("Orc1"))
				return 2;
			else if (n.Equals("Orc2"))
				return 3;
			else if (n.Equals("Dragon"))
				return 4;
			else if (n.Equals("ManaPotion1"))
				return 5;
			else if (n.Equals("ManaPotion2"))
				return 6;
			else if (n.Equals("HealthPotion1"))
				return 7;
			else if (n.Equals("HealthPotion2"))
				return 8;
			else if (n.Equals("Chest1"))
				return 9;
			else if (n.Equals("Chest2"))
				return 10;
			else if (n.Equals("Chest3"))
				return 11;
			else if (n.Equals("Chest4"))
				return 12;
			else if (n.Equals("Chest5"))
				return 13;

			return -1;

		}
	}

}