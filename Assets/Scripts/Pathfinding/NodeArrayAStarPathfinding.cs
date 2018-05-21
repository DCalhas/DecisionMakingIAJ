using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathFinding : AStarPathfinding
    {
        protected NodeRecordArray NodeRecordArray { get; set; }
        public NodeArrayAStarPathFinding(NavMeshPathGraph graph, IHeuristic heuristic) : base(graph,null,null,heuristic)
        {
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
			this.InProgress = false;
        }

		public override void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
		{
			this.StartPosition = startPosition;
			this.GoalPosition = goalPosition;
			this.StartNode = this.Quantize(this.StartPosition);
			this.GoalNode = this.Quantize(this.GoalPosition);

			//if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
			if (this.StartNode == null || this.GoalNode == null) return;

			//I need to do this because in Recast NavMesh graph, the edges of polygons are considered to be nodes and not the connections.
			//Theoretically the Quantize method should then return the appropriate edge, but instead it returns a polygon
			//Therefore, we need to create one explicit connection between the polygon and each edge of the corresponding polygon for the search algorithm to work
			((NavMeshPoly)this.StartNode).AddConnectedPoly(this.StartPosition);
			((NavMeshPoly)this.GoalNode).AddConnectedPoly(this.GoalPosition);

			this.InProgress = true;
			this.TotalExploredNodes = 0;
			this.TotalProcessingTime = 0.0f;
			this.MaxOpenNodes = 0;

			var initialNode = new NodeRecord
			{
				gValue = 0,
				hValue = this.Heuristic.H(this.StartNode, this.GoalNode),
				node = this.StartNode
			};

			initialNode.fValue = AStarPathfinding.F(initialNode);

			this.Open.Initialize(); 
			this.Open.AddToOpen(initialNode);
			this.Closed.Initialize();
		}

		protected override void ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge, int edgeIndex)
        {
            float f;
            float g;
            float h;

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

			#region do not look into this
            if (childNodeRecord == null)
            {
                //this piece of code is used just because of the special start nodes and goal nodes added to the RAIN Navigation graph when a new search is performed.
                //Since these special goals were not in the original navigation graph, they will not be stored in the NodeRecordArray and we will have to add them
                //to a special structure
                //it's ok if you don't understand this, this is a hack and not part of the NodeArrayA* algorithm, just do NOT CHANGE THIS, or your algorithm will not work
                childNodeRecord = new NodeRecord
                {
                    node = childNode,
                    parent = bestNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childNodeRecord);
            }
			#endregion

			h = this.Heuristic.H (childNode, this.GoalNode);
			g = bestNode.gValue + connectionEdge.Cost;
			f = g + h;

			NodeRecord nodeOpen = this.Open.SearchInOpen (childNodeRecord);
			NodeRecord closedNode = this.Closed.SearchInClosed (childNodeRecord);

			if (nodeOpen != null) {
				if (nodeOpen.fValue >= f) {
					Open.RemoveFromOpen (nodeOpen);
					childNodeRecord.parent = bestNode;
					childNodeRecord.gValue = g;
					childNodeRecord.hValue = h;
					childNodeRecord.fValue = f;
					Open.AddToOpen(childNodeRecord);
				}
				return;
			}else if (closedNode != null) {
				if(closedNode.fValue > f){
					Closed.RemoveFromClosed (closedNode);
					childNodeRecord.parent = bestNode;
					childNodeRecord.gValue = g;
					childNodeRecord.hValue = h;
					childNodeRecord.fValue = f;
					Open.AddToOpen (childNodeRecord);
				}
				return;
			}
			childNodeRecord.parent = bestNode;
			childNodeRecord.gValue = g;
			childNodeRecord.hValue = h;
			childNodeRecord.fValue = f;
			Open.AddToOpen (childNodeRecord);
        }
            
        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>) Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }
    }
}
