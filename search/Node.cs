using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.My_Assets.dinoScripts.search
{
    public class Node
    {
        /// <summary>
        /// @Author: Héctor Guillermo Rodríguez Fuentes.
        /// Node class that represents a node of the map for use in pathfinding algorithm.
        /// </summary>
        private Vector3 position;
        private Node parent;
        private float fertility;
        private int plants;
        private int preys;
        private int predators;
        private float f;//f(x) in A* algorithm
        private int g;//g(x) in A* algorithm

        public Node(Vector3 position, float fertility, int plants, int preys, int predators, float f, int g)
        {
            this.position = position;
            this.parent = null;
            this.fertility = fertility;
            this.plants = plants;
            this.preys = preys;
            this.predators = predators;
            this.f = f;
            this.g = g;
        }

        public Node(Vector3 position, Node parent, float fertility, int plants, int preys, int predators, float f, int g)
        {
            this.position = position;
            this.parent = parent;
            this.fertility = fertility;
            this.plants = plants;
            this.preys = preys;
            this.predators = predators; ;
            this.f = f;
            this.g = g;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        public void setPosition(Vector3 position)
        {
            this.position = position;
        }

        public Node getParent()
        {
            return parent;
        }

        public void setParent(Node parent)
        {
            this.parent = parent;
        }

        public float getFertility()
        {
            return fertility;
        }

        public void setFertility(float fertility)
        {
            this.fertility = fertility;
        }

        public int getPlants()
        {
            return plants;
        }

        public void setPlants(int plants)
        {
            this.plants = plants;
        }

        public int getPreys()
        {
            return preys;
        }
        
        public void setPreys(int preys)
        {
            this.preys = preys;
        }

        public int getPredators()
        {
            return predators;
        }

        public void setPredators(int predators)
        {
            this.predators = predators;
        }

        public float getF()
        {
            return f;
        }

        public void setF(float f)
        {
            this.f = f;
        }

        public int getG()
        {
            return g;
        }

        public void setG(int g)
        {
            this.g = g;
        }

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
