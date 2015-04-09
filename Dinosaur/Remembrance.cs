using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.My_Assets.dinoScripts.search;

namespace Assets.My_Assets.dinoScripts.Dinosaur
{
    /// <summary>
    /// /// @Author: Héctor Guillermo Rodríguez Fuentes.
    /// Class that represents a Node at a given time
    /// </summary>
    public class Remembrance
    {
        private DateTime timestamp;
        private Node node;

        public Remembrance(Node node)
        {
            this.timestamp = DateTime.Now;
            this.node = node;
        }

        public DateTime getTimestamp()
        {
            return timestamp;
        }

        public Node getNode()
        {
            return node;
        }
    }
}
