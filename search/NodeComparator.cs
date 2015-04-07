using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.dinoScripts.search
{
    /// <summary>
    /// @Author: Héctor Guillermo Rodríguez Fuentes.
    /// Implementation of IComparar for Node comparison and BinaryHeap priority queue.
    /// </summary>
    public class NodeComparator : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            if(x.getF() == y.getF())
                return 0;
            if(x.getF() < y.getF())
                return 1;
            return -1;
        }
    }

    /// <summary>
    /// @Author: Héctor Guillermo Rodríguez Fuentes.
    /// Implementation of IEqualityComparer for Node comparison in HashSet
    /// </summary>
    public class NodeEqualityComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            return x.getPosition() == y.getPosition();
        }

        public int GetHashCode(Node item)
        {
            return item.getPosition().GetHashCode();
        }
    }
}
