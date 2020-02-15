using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing tree node properties. Trees of units are created by by connecting KC_TreeNode
     * components *within* Units rather than by directly connecting the Units themselves. From within a KC_TreeNode
     * component you get at the enclosing unit via the ContainingUnit property on KnowledgeComponent. 
     */
    public class KC_TreeNode : KnowledgeComponent
    {
        /*
         * fixme: need to figure out how to serialize this class. Don't currently have examples of authors directly authoring trees, but totally
         * seems reasonable that they will. 
         */

        public KC_TreeNode Parent { get; protected set; }
        public IList<KC_TreeNode> Children { get; protected set; }

        public bool IsLeaf => Children.Count == 0;

        public bool IsRoot => Parent == null;

        public void AddChild(KC_TreeNode child)
        {
            Children.Add(child);
            if (child.Parent == null)
            {
                // Child didn't have a previous parent - set the parent's child
                child.Parent = this;
            }
            else
            {
                // Child had previous parent - remove the child from the Children of previous parent and set new parent
                bool removed = child.Parent.Children.Remove(child);
                Debug.Assert(removed);
                child.Parent = this;
            }
        }

        public override object Clone() => new KC_TreeNode();

        public void RemoveChild(KC_TreeNode child)
        {
            if (Children.Remove(child))
            {
                // Child removed - set the parent link of child to null
                Debug.Assert(child.Parent == this);
                child.Parent = null;
            }
        }

        public void RemoveChildren()
        {
            // Set the parent of each child to null
            foreach (KC_TreeNode child in Children)
            {
                child.Parent = null;
            }

            // Clear the child list.
            Children.Clear();
        }

        public override string ToString()
        {
            return "(IsLeaf: " + IsLeaf + ", IsRoot: " + IsRoot + ", ChildCount: " + Children.Count + ")";
        }

        /* 
         * This is the copy constructor used by clone. Since copies of KC_TreeNode don't copy the parent and children, and there are no base copy constructors, 
         * the "copy" constructor doesn't need an argument. For readability it would still be nice to include one, but then there's a signature collision with
         * KC_TreeNode(parent). 
         */
        protected KC_TreeNode() : base()
        {
            /* 
             * When copying a KC_TreeNode, leave parent null and children empty. If we're copying a KC_TreeNode, it's because we're copying a unit, and it wouldn't 
             * make sense to have the new KC_TreeNode still pointing to the old units. 
             */
            Parent = null;
            Children = new List<KC_TreeNode>();
        }

        public KC_TreeNode(KC_TreeNode parent)
        {
            //  Set the parent and add this node to the children of the parent.
            // fixme: remove
            // Parent = parent;
            parent?.AddChild(this);

            Children = new List<KC_TreeNode>();
        }

        public KC_TreeNode(KC_TreeNode parent, IList<KC_TreeNode> children)
        {
            // Set the Parent and add this node to the children of the parent
            // fixme: remove
            // Parent = parent;
            parent?.AddChild(this);

            // Set the Children and set the parent of each of the children to this node
            Children = children;
            foreach (KC_TreeNode child in children)
            {
                child.Parent = this;
            }
        }
    }

    public static class KC_TreeNode_Extensions
    {
        public static KC_TreeNode GetTreeParent(this Unit unit)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                return unit.GetComponent<KC_TreeNode>().Parent;
            }
            throw new InvalidOperationException("GetTreeParent() called on Unit without a KC_TreeNode componenent.");
        }

        public static void SetTreeParent(this Unit unit, KC_TreeNode parent)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                parent.AddChild(unit.GetComponent<KC_TreeNode>());
            }
            else
            {
                throw new InvalidOperationException("SetTreeParent() called on Unit without a KC_TreeNode componenent.");
            }
        }

        public static IList<KC_TreeNode> GetTreeChildren(this Unit unit)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                return unit.GetComponent<KC_TreeNode>().Children;
            }
            throw new InvalidOperationException("GetTreeChildren() called on Unit without a KC_TreeNode componenent.");
        }

        public static void SetTreeChildren(this Unit unit, IList<KC_TreeNode> children)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                KC_TreeNode thisNode = unit.GetComponent<KC_TreeNode>();

                // First remove any existing children. 
                thisNode.RemoveChildren();

                // Now add each new child to the node
                foreach (KC_TreeNode child in children)
                {
                    thisNode.AddChild(child);
                }
            }
            else
            {
                throw new InvalidOperationException("SetTreeChildren() called on Unit without a KC_TreeNode componenent.");
            }
        }

        public static void AddTreeChild(this Unit unit, KC_TreeNode child)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                unit.GetComponent<KC_TreeNode>().AddChild(child);
            }
            else
            {
                throw new InvalidOperationException("AddTreeChild() called on Unit without a KC_TreeNode componenent.");
            }
        }

        public static void RemoveTreeChild(this Unit unit, KC_TreeNode child)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                unit.GetComponent<KC_TreeNode>().RemoveChild(child);
            }
            else
            {
                throw new InvalidOperationException("RemoveTreeChild() called on Unit without a KC_TreeNode componenent.");
            }
        }

        public static bool IsTreeLeaf(this Unit unit)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                return unit.GetComponent<KC_TreeNode>().IsLeaf;
            }
            throw new InvalidOperationException("IsTreeLeaf() called on Unit without a KC_TreeNode componenent.");
        }

        public static bool IsTreeRoot(this Unit unit)
        {
            if (unit.HasComponent<KC_TreeNode>())
            {
                return unit.GetComponent<KC_TreeNode>().IsRoot;
            }
            throw new InvalidOperationException("IsTreeRoot() called on Unit without a KC_TreeNode componenent.");
        }
    }
}
