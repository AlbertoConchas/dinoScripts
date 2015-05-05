using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.My_Assets.dinoScripts.Dinosaur;

namespace Assets.My_Assets.dinoScripts.search.bayesiannetwork
{
    class BayesianFactory
    {
        private bool isPrey;
        
        public BayesianFactory(DinoObject dinosaur)
        {
            isPrey = (dinosaur is Prey) ? true : false;
        }

        public BayesianNode[] createBayNet()
        {
            return isPrey ? dag_factory_prey() : dag_factory_predator();
        }

        private static BayesianNode[] dag_factory_prey()
        {
            BayesianNode[] nodes = new BayesianNode[5];

            //Create Node Y (ROOT)
            Dictionary<string, Value> h_values = new Dictionary<string, Value>();
            Dictionary<string, float> h1_probs = new Dictionary<string, float>();
            h1_probs.Add("h1", 0.4f);
            Dictionary<string, float> h2_probs = new Dictionary<string, float>();
            h2_probs.Add("h2", 0.6f);
            h_values.Add("h1", new Value("h1", h1_probs));
            h_values.Add("h2", new Value("h2", h2_probs));

            //Create Node D
            Dictionary<string, Value> d_values = new Dictionary<string, Value>();
            Dictionary<string, float> d1_probs = new Dictionary<string, float>();
            d1_probs.Add("h1", 0.2f);
            d1_probs.Add("h2", 0.3f);
            d_values.Add("d1", new Value("d1", d1_probs));

            Dictionary<string, float> d2_probs = new Dictionary<string, float>();
            d2_probs.Add("h1", 0.8f);
            d2_probs.Add("h2", 0.7f);
            d_values.Add("d2", new Value("d2", d2_probs));

            //Create Node S1
            Dictionary<string, Value> s1_values = new Dictionary<string, Value>();
            Dictionary<string, float> s1_1_probs = new Dictionary<string, float>();
            s1_1_probs.Add("d1", 0.1f);
            s1_1_probs.Add("d2", 0.4f);
            s1_values.Add("s1_1", new Value("s1_1", s1_1_probs));

            Dictionary<string, float> s1_2_probs = new Dictionary<string, float>();
            s1_2_probs.Add("d1", 0.9f);
            s1_2_probs.Add("d2", 0.6f);
            s1_values.Add("s1_2", new Value("s1_2", s1_2_probs));

            //Create Node C
            Dictionary<string, Value> c_values = new Dictionary<string, Value>();
            Dictionary<string, float> c1_probs = new Dictionary<string, float>();
            c1_probs.Add("h1", 0.2f);
            c1_probs.Add("h2", 0.7f);
            c_values.Add("c1", new Value("c1", c1_probs));

            Dictionary<string, float> c2_probs = new Dictionary<string, float>();
            c2_probs.Add("h1", 0.8f);
            c2_probs.Add("h2", 0.3f);
            c_values.Add("c2", new Value("c2", c2_probs));

            //Create Node S2
            Dictionary<string, Value> s2_values = new Dictionary<string, Value>();
            Dictionary<string, float> s2_1_probs = new Dictionary<string, float>();
            s2_1_probs.Add("c1", 0.7f);
            s2_1_probs.Add("c2", 0.6f);
            s2_values.Add("s2_1", new Value("s2_1", s2_1_probs));

            Dictionary<string, float> s2_2_probs = new Dictionary<string, float>();
            s2_2_probs.Add("c1", 0.3f);
            s2_2_probs.Add("c2", 0.4f);
            s2_values.Add("s2_2", new Value("s2_2", s2_2_probs));


            //Form the dag
            BayesianNode H = new BayesianNode("H", h_values);
            BayesianNode D = new BayesianNode("D", d_values);
            BayesianNode S1 = new BayesianNode("S1", s1_values);
            BayesianNode S2 = new BayesianNode("S2", s2_values);
            BayesianNode C = new BayesianNode("C", c_values);

            H.Children.Add(D.Name, D);
            H.Children.Add(C.Name, C);
            D.Children.Add(S1.Name, S1);
            C.Children.Add(S2.Name, S2);

            D.Parent = H;
            C.Parent = H;
            S1.Parent = D;
            S2.Parent = C;

            //Add Nodes to list
            nodes[0] = H;
            nodes[1] = D;
            nodes[2] = S1;
            nodes[3] = S2;
            nodes[4] = C;
            return nodes;
        }

        private static BayesianNode[] dag_factory_predator()
        {
            BayesianNode[] nodes = new BayesianNode[5];

            //Create Node Y (ROOT)
            Dictionary<string, Value> h_values = new Dictionary<string, Value>();
            Dictionary<string, float> h1_probs = new Dictionary<string, float>();
            h1_probs.Add("h1", 0.4f);
            Dictionary<string, float> h2_probs = new Dictionary<string, float>();
            h2_probs.Add("h2", 0.6f);
            h_values.Add("h1", new Value("h1", h1_probs));
            h_values.Add("h2", new Value("h2", h2_probs));

            //Create Node D
            Dictionary<string, Value> d_values = new Dictionary<string, Value>();
            Dictionary<string, float> d1_probs = new Dictionary<string, float>();
            d1_probs.Add("h1", 0.8f);
            d1_probs.Add("h2", 0.3f);
            d_values.Add("d1", new Value("d1", d1_probs));

            Dictionary<string, float> d2_probs = new Dictionary<string, float>();
            d2_probs.Add("h1", 0.2f);
            d2_probs.Add("h2", 0.7f);
            d_values.Add("d2", new Value("d2", d2_probs));

            //Create Node S1
            Dictionary<string, Value> s1_values = new Dictionary<string, Value>();
            Dictionary<string, float> s1_1_probs = new Dictionary<string, float>();
            s1_1_probs.Add("d1", 0.9f);
            s1_1_probs.Add("d2", 0.4f);
            s1_values.Add("s1_1", new Value("s1_1", s1_1_probs));

            Dictionary<string, float> s1_2_probs = new Dictionary<string, float>();
            s1_2_probs.Add("d1", 0.1f);
            s1_2_probs.Add("d2", 0.6f);
            s1_values.Add("s1_2", new Value("s1_2", s1_2_probs));

            //Create Node C
            Dictionary<string, Value> c_values = new Dictionary<string, Value>();
            Dictionary<string, float> c1_probs = new Dictionary<string, float>();
            c1_probs.Add("h1", 0.8f);
            c1_probs.Add("h2", 0.7f);
            c_values.Add("c1", new Value("c1", c1_probs));

            Dictionary<string, float> c2_probs = new Dictionary<string, float>();
            c2_probs.Add("h1", 0.2f);
            c2_probs.Add("h2", 0.3f);
            c_values.Add("c2", new Value("c2", c2_probs));

            //Create Node S2
            Dictionary<string, Value> s2_values = new Dictionary<string, Value>();
            Dictionary<string, float> s2_1_probs = new Dictionary<string, float>();
            s2_1_probs.Add("c1", 0.7f);
            s2_1_probs.Add("c2", 0.6f);
            s2_values.Add("s2_1", new Value("s2_1", s2_1_probs));

            Dictionary<string, float> s2_2_probs = new Dictionary<string, float>();
            s2_2_probs.Add("c1", 0.3f);
            s2_2_probs.Add("c2", 0.4f);
            s2_values.Add("s2_2", new Value("s2_2", s2_2_probs));


            //Form the dag
            BayesianNode H = new BayesianNode("H", h_values);
            BayesianNode D = new BayesianNode("D", d_values);
            BayesianNode S1 = new BayesianNode("S1", s1_values);
            BayesianNode S2 = new BayesianNode("S2", s2_values);
            BayesianNode C = new BayesianNode("C", c_values);

            H.Children.Add(D.Name, D);
            H.Children.Add(C.Name, C);
            D.Children.Add(S1.Name, S1);
            C.Children.Add(S2.Name, S2);

            D.Parent = H;
            C.Parent = H;
            S1.Parent = D;
            S2.Parent = C;

            //Add Nodes to list
            nodes[0] = H;
            nodes[1] = D;
            nodes[2] = S1;
            nodes[3] = S2;
            nodes[4] = C;
            return nodes;
        }
    }
}
