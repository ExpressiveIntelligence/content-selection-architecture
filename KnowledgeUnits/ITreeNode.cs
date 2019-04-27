using System.Collections.Generic;
using CSA.Core;
using static CSA.KnowledgeUnits.CUSlots;

namespace CSA.KnowledgeUnits
{
    /*
     * "Tag" interface for ITreeNode. Extension methods for ITreeNode defined below. 
     */
    public interface ITreeNode : IUnit
    {
    }

    public static class ITreeNode_Extensions
    {
        public static ITreeNode GetParent(this ITreeNode node)
        {
            try
            {
                return (ITreeNode)node.Slots[ITreeNode_Parent];
            }
            catch (KeyNotFoundException)
            {
                // If an ITreeNode_Parent slot has not been created yet, return null. 
                return null;
            }
        }

        public static void SetParent(this ITreeNode node, ITreeNode parent)
        {
            node.Slots[ITreeNode_Parent] = parent;
        }

        public static IList<ITreeNode> GetChildren(this ITreeNode node)
        {
            try
            {
                return (IList<ITreeNode>)node.Slots[ITreeNode_Children];
            }
            catch (KeyNotFoundException)
            {
                // If an ITreeNode_Children slot has not been created yet, return null. 
                return null;
            }
        }

        public static void AddChild(this ITreeNode node, ITreeNode child)
        {
            try
            {
                IList<ITreeNode> children = (IList<ITreeNode>)node.Slots[ITreeNode_Children];
                children.Add(child);
            }
            catch (KeyNotFoundException)
            {
                // If the ITreeNode_Children slot has not been created yet, create it and add the child. 
                IList<ITreeNode> children = new List<ITreeNode>
                {
                    child
                };
                node.Slots[ITreeNode_Children] = children;
            }
        }

        public static void RemoveChild(this ITreeNode node, ITreeNode child)
        {
            // If the ITreeNode_Children slot has not been created yet, RemoveChild will throw an exception. 
            IList<ITreeNode> children = (IList<ITreeNode>)node.Slots[ITreeNode_Children];
            children.Remove(child);
        }

        public static bool IsLeaf(this ITreeNode node)
        {
            try
            {
                IList<ITreeNode> children = (IList<ITreeNode>)node.Slots[ITreeNode_Children];
                return children.Count > 0;

            }
            catch (KeyNotFoundException)
            {
                // If the ITreeNode_Children slot has not been created yet, then the node is a leaf. 
                return true;
            }
        }

        /*
         * Initialization method (similar to a constructor) for ITreeNode.
         * fixme: not sure how I'll use init methods yet, but defining them for now.         
         */
        public static void InitITreeNode(this ITreeNode node)
        {
            node.Slots[ITreeNode_Parent] = null; 
            node.Slots[ITreeNode_Children] = new List<ITreeNode>();
        }
    }
}
