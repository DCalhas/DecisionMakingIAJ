using RAIN.Navigation.Graph;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
	public class EuclideanHeuristic : IHeuristic
	{
		public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
		{
			var distance = (goalNode.Position - node.Position).sqrMagnitude;
			// 1 + 1/distance should solve the ties problem
			return (1 + 1/distance) * distance;
		}
	}
}
