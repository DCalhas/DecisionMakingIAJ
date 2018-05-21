using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.NavMesh;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    public class GoalBoundingPathfinding : NodeArrayAStarPathFinding
    {
        public GoalBoundingTable GoalBoundingTable { get; protected set;}
        public int DiscardedEdges { get; protected set; }
		public int TotalEdges { get; protected set; }

        public GoalBoundingPathfinding(NavMeshPathGraph graph, IHeuristic heuristic, GoalBoundingTable goalBoundsTable) : base(graph, heuristic)
        {
            this.GoalBoundingTable = goalBoundsTable;
        }

        public override void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
        {
            this.DiscardedEdges = 0;
			this.TotalEdges = 0;
            base.InitializePathfindingSearch(startPosition, goalPosition);
        }

		protected override void ProcessChildNode(NodeRecord parentNode, NavigationGraphEdge connectionEdge, int edgeIndex)
        {
			NodeGoalBounds nodeBounds = this.GoalBoundingTable.table [parentNode.node.NodeIndex];

			this.TotalEdges++;

			if (nodeBounds == null) {
				base.ProcessChildNode (parentNode, connectionEdge, edgeIndex);
			} else {
				if (nodeBounds.connectionBounds.Length - 1 < edgeIndex) {
					base.ProcessChildNode (parentNode, connectionEdge, edgeIndex);
					return;
				}
				else if (nodeBounds.connectionBounds [edgeIndex].PositionInsideBounds (this.GoalPosition)) {
					base.ProcessChildNode (parentNode, connectionEdge, edgeIndex);
					return;
				}
				this.DiscardedEdges++;
			}
        }
    }
}