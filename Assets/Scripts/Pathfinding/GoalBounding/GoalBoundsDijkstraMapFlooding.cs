﻿using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    //The Dijkstra algorithm is similar to the A* but with a couple of differences
    //1) no heuristic function
    //2) it will not stop until the open list is empty
    //3) we dont need to execute the algorithm in multiple steps (because it will be executed offline)
    //4) we don't need to return any path (partial or complete)
    //5) we don't need to do anything when a node is already in closed
    public class GoalBoundsDijkstraMapFlooding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public NodeGoalBounds NodeGoalBounds { get; protected set; }
        protected NodeRecordArray NodeRecordArray { get; set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        
        public GoalBoundsDijkstraMapFlooding(NavMeshPathGraph graph)
        {
            this.NavMeshGraph = graph;
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
        }

        public void Search(NavigationGraphNode startNode, NodeGoalBounds nodeGoalBounds)
        {
			NodeRecord startNodeRecord = this.NodeRecordArray.GetNodeRecord (startNode);
			startNodeRecord.StartNodeOutConnectionIndex = -1;
			startNodeRecord.fValue = 0;
			startNodeRecord.parent = null;

			for (int i = 0; i < startNode.OutEdgeCount; i++) {
				NodeRecord nodeChildRecord = this.NodeRecordArray.GetNodeRecord (startNode.EdgeOut (i).ToNode);
				nodeChildRecord.parent = startNodeRecord;
				nodeChildRecord.StartNodeOutConnectionIndex = i;
				nodeChildRecord.fValue = (startNodeRecord.node.Position - nodeChildRecord.node.Position).magnitude;

				this.Open.AddToOpen(nodeChildRecord);

			}

			this.Closed.AddToClosed (startNodeRecord);
			

			while (this.Open.CountOpen () > 0) {
				var bestRecord = this.Open.GetBestAndRemove ();
				for (int i = 0; i < bestRecord.node.OutEdgeCount; i++) {
					this.ProcessChildNode (bestRecord, bestRecord.node.EdgeOut (i), i);
				}
				nodeGoalBounds.connectionBounds [bestRecord.StartNodeOutConnectionIndex].UpdateBounds (bestRecord.node.Position);
				this.Closed.AddToClosed (bestRecord);
			}
        }

       
        protected void ProcessChildNode(NodeRecord parent, NavigationGraphEdge connectionEdge, int connectionIndex)
        {
			NodeRecord nodeChildRecord = this.NodeRecordArray.GetNodeRecord (connectionEdge.ToNode);
			NodeRecord nodeChildRecordOpen = this.Open.SearchInOpen (nodeChildRecord);

			float f = parent.fValue;
			f += (parent.node.Position - nodeChildRecord.node.Position).magnitude;
			if(nodeChildRecord.status == NodeStatus.Unvisited || nodeChildRecord.fValue > f) {
				if (nodeChildRecord.status == NodeStatus.Unvisited) {
					nodeChildRecord.parent = parent;
					nodeChildRecord.StartNodeOutConnectionIndex = parent.StartNodeOutConnectionIndex;
					nodeChildRecord.fValue = (parent.node.Position - nodeChildRecord.node.Position).magnitude;
					nodeChildRecord.fValue += parent.fValue;
					this.Open.AddToOpen (nodeChildRecord);
				} else if (nodeChildRecord.status == NodeStatus.Open) {
					nodeChildRecord.parent = parent;
					nodeChildRecord.StartNodeOutConnectionIndex = parent.StartNodeOutConnectionIndex;
					nodeChildRecord.fValue = (parent.node.Position - nodeChildRecord.node.Position).magnitude;
					nodeChildRecord.fValue += parent.fValue;
					this.Open.Replace (nodeChildRecordOpen, nodeChildRecord);
				}
			}
        }

        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }

		public void Clear() {
			this.Open.Initialize ();
		}
    }
}
