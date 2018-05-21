using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class RightPriorityList : IOpenSet, IComparer<NodeRecord>
    {
        private List<NodeRecord> Open { get; set; }

        public RightPriorityList()
        {
            this.Open = new List<NodeRecord>();    
        }
		public void Initialize()
		{
			this.Open.Clear(); 
		}

		public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
		{
			this.RemoveFromOpen(nodeToBeReplaced);
			this.AddToOpen(nodeToReplace);
		}

		public NodeRecord GetBestAndRemove()
		{
			NodeRecord best = this.PeekBest ();

			this.Open.RemoveAt (this.Open.Count-1);
			return best;
		}

		public NodeRecord PeekBest()
		{
			if (this.Open.Count == 0) {
				return null;
			}
			return this.Open [this.Open.Count-1];
		}

		public void AddToOpen(NodeRecord nodeRecord)
		{
			NodeRecord current = this.PeekBest();

			if(current != null) 
				for(int i = 0; i < this.Open.Count; i++) {
					current = this.PeekBest();
					if (current.fValue < nodeRecord.fValue) {
						this.Open.Insert (i, nodeRecord);
						return;
					}
				}
			this.Open.Add (nodeRecord);
		}

		public void RemoveFromOpen(NodeRecord nodeRecord)
		{
			this.Open.Remove (nodeRecord);
		}

		public NodeRecord SearchInOpen(NodeRecord nodeRecord)
		{
			int i = this.Open.BinarySearch (nodeRecord);
			if (i < 0) {
				return null;
			}
			return this.Open[i];
		}

		public ICollection<NodeRecord> All()
		{
			return this.Open;
		}

		public int CountOpen()
		{
			return this.Open.Count;
		}

		public int Compare(NodeRecord first, NodeRecord second) {
			if (first.fValue > second.fValue) {
				return -1;
			} else if (first.fValue < second.fValue) {
				return 1;
			} else {
				return 0;
			}
		}
    }
}
