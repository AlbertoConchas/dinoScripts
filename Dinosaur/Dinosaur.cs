
﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.My_Assets.dinoScripts.search;
using Assets.My_Assets.dinoScripts.Dinosaur;

public class Dinosaur : MonoBehaviour{
    //public Transform m_Prey;
    public float hp = 100f;			//Salud de la entidad
    public int np = 10;			//Nutricion aportada a quien se alimente de la entidad
    public int speed = 2;			//Velocidad de la entidad
    public int comRange = 10;			//Rango de comunicacion
    public double stamina = 100f;			//Resistencia (nesesaria para correr etc....)
    public float lifetime = 10000f;		//Tiempo de vida
    public float attack = 10f;			//Daño que realiza la entidad
    public float flesh = 200f;
    public float leadership;
	public bool female;
    public States state;
	public Priorities priority;
	
    protected NodesController nodes;//A* pathfinding
	public PathNode actualNode;
	
    //Memory
    protected Dictionary<Vector3, Remembrance> memory;//The memorized nodes
    protected DateTime last_update;
    private static int tw = 5;//Time lapse in seconds that have to be present since last update in order to store information in memory

    protected float stoppingDistance;
    protected NavMeshAgent nav;

    public List<GameObject> herd = new List<GameObject>();
    public GameObject leader;
    private bool requestResponded;
    private GameObject tempLeader;

    public enum States { ChoosingLeader, Searching, Following, Moving, Hunting, Eating, Hiding, Reproduce, Repose, Reagruping, Die };
    public enum Priorities {Eat, Obey, Reproduce, Run};

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------ Lider Chosing --------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    protected void start()
    {
        if (memory == null)
        {
            memory = new Dictionary<Vector3, Remembrance>();
        }

        if (nodes == null)
        {
            setNodesController();
        }
    }

    /**
 *	Fijar el objeto lider
 */
    public void setLeader(GameObject l)
    {
        leader = l;
        nav.avoidancePriority = 1;
        state = States.Repose;
    }

    /*
 *	Funcion que detiene al nav Agent
 */
   public float getLeadershipStat()
    {

        this.leadership =
            (this.hp / 100) +
                (this.speed / 3) +
                ((float)this.stamina / 100) +
                ((this.lifetime * 2) / 10000);
        return this.leadership;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------



    /*
 * Funcion para enviar a todos los objetos cercanos
 * string Messaage: Funcion que sera ejecutada en los objetos encontrados
 * object obj: Parametros para enviar a esa funcion
 */
   public void BroadCast(string message, object obj)
    {
        herd.Remove(null);
       	if(herd.Count>0)
        foreach (GameObject dino in herd)
        {
            if (dino != null || dino.GetComponent<Dinosaur>().state != States.Die)
            {
				dino.SendMessage(message, (GameObject)obj);
            }
            else
            {
                //herd.Remove(dino);
            }
            
        }
    }
   public List<GameObject> getHerd() {
       return herd;
   }

   //actualiza la manada cuando alguien muere (en especial el lider) o cuando aun no se de que manada soy
  protected void updateHerd<T>() where T:Dinosaur
   {
       if (herd.Count == 0)
       {
           Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
           for (int i = 0; i < hitColliders.Length; i++)
           {

               //Si es un velocirraptor
               if (hitColliders[i].GetComponent<T>() != null)
               {
                   //Que no soy yo
                   if (hitColliders[i].gameObject.GetInstanceID() != gameObject.GetInstanceID())
                   {
                       herd.Add(hitColliders[i].gameObject);
                   }
               }
           }
       }
       else {
           List<GameObject> newHerd = new List<GameObject>();

           foreach (GameObject dino in herd)
           {
               // si el dino no ha muerto y el obj no ha sido destrudio.
               if (dino != null && dino.GetComponent<T>().state != States.Die)
               {
                   newHerd.Add(dino);
               }
           }
           herd = newHerd;
       }
   }
   //Retorna si el gameobject enviado es igual a la entidad actual
  protected bool isMe(GameObject g)
   {
       if (g!=null && gameObject!=null && g.GetInstanceID() == gameObject.GetInstanceID())
           return true;
       return false;
   }

   //Retorna si el gameobject enviado es igual al lider de la unidad actual
  protected bool isMyLeader(GameObject l)
   {
       if (l!=null && leader!=null && l.GetInstanceID() == leader.GetInstanceID())
           return true;
       return false;
   }

   /**
*	Regresa la distancia desde la posicion actual a el destino deseado
*/
  protected float distanceFromDestination()
   {
       return Vector3.Distance(transform.position, nav.destination);
   }

  /*
   *	Regresa una pocicion aleatoria alrededor de la pocicion dada
   */
  protected Vector3 Dispersal(Vector3 pos)
  {
      pos.x = pos.x + (((float)UnityEngine.Random.Range(-50, 50) / 100) * this.comRange);
      pos.z = pos.z + (((float)UnityEngine.Random.Range(-50, 50) / 100) * this.comRange);
      return pos;
  }


  protected  float travelStopDistance()
  {
      return comRange * ((float)UnityEngine.Random.Range(30, 50) / 100);
  }


  protected  bool isOnRangeToStop()
  {
      return isOnRangeToStop(1f);
  }

  protected bool isOnRangeToStop(float factor)
  {
      return (distanceFromDestination() < this.stoppingDistance * factor);
  }

 protected  void stop()
  {
      nav.destination = transform.position;
  }	
	protected void die(){
		state = States.Die;
		this.GetComponent<DinasorsAnimationCorrector>().die();
		//gameObject.GetComponent<PredatorLeaderChoosing> ().enabled = false;
		if (isMyLeader (gameObject)) 
        {
			//LeaderSaysUnsetLeader (gameObject);
            Transform t = gameObject.transform.Find("leaderLigth");
            if(t!=null)
            {
                Destroy(t.gameObject);
            }
		}
	}

    protected bool hungry()
    {
        if (stamina < 85f || hp < 100)
            return true;
        return false;
    }

    protected bool satisfied()
    {
        if (stamina < 100 || hp < 100)
            return false;
        return true;
    }	
    //=============Navigation functions=================

    /// <summary>
    /// Transform PathNode to Node.
    /// </summary>
    /// <param name="n">PathNode to convert</param>
    /// <returns>Node converted</returns>
    protected Node toNode(PathNode n)
    {
        Node nn = new Node(n.transform.position, n.getFertility(), n.getPlants(), n.getPredators(), 0, 0);
        nn.setF(nn.getFertility());
        return nn;
    }

    /// <summary>
    /// Gets the actual node
    /// </summary>
    /// <returns>Actual node</returns>
    protected PathNode getActualPathNode()
    {
        GameObject an = nodes.getNeartestNode(transform.position);	//Obtiene el nodo actual
        return an.GetComponent<PathNode>();
    }

    /// <summary>
    /// Gets the actual node as GameObject instance
    /// </summary>
    /// <returns>Actual node</returns>
    protected GameObject getActualNode()
    {
        return nodes.getNeartestNode(transform.position);	//Obtiene el nodo actual
    }

    /// <summary>
    /// Get the neighbors of the actual node
    /// </summary>
    /// <returns>Neighbors</returns>
    protected PathNode[] getNeighbors()
    {
        GameObject an = nodes.getNeartestNode(transform.position);	//Obtiene el nodo actual
        GameObject[] n = nodes.getNeighbors(an);
        PathNode[] neighbors = new PathNode[n.Length];
        for (int i = 0; i < n.Length; i++)
        {
            neighbors[i] = n[i].GetComponent<PathNode>();

        }
        return neighbors;
    }

    protected void setNodesController()
    {
        nodes = GameObject.Find("Global").GetComponent<NodesController>();
    }

    /// <summary>
    /// Expand Neighbourhood of actual PathNode
    /// </summary>
    protected Node[] expand()
    {
        GameObject actualNode = getActualNode();
        GameObject[] nbh = nodes.getNeighbors(actualNode);
        Node[] neighbourhood = new Node[nbh.Length];
        PathNode p = null;
        for (int i = 0; i < nbh.Length; i++)
        {
            p = nbh[i].GetComponent<PathNode>();
            //(Vector3 position, float fertility, int plants, int predators, int f, int g)
            neighbourhood[i] = new Node(p.transform.position, p.getFertility(), p.getPlants(), p.getPredators(), 0, 1);
            neighbourhood[i].setF(getH(neighbourhood[i]));
        }
        return neighbourhood;
    }

    /// <summary>
    /// Gets the h.
    /// </summary>
    /// <returns>The h.</returns>
    /// <param name="n">N.</param>
    private float getH(Node n)
    {
        float h = n.getFertility();
        return h;
    }

    //=============Memory functions=================
    protected void memorize()
    {
        if ((DateTime.Now - last_update).TotalSeconds > tw)
        {
            try
            {
                Node node = toNode(actualNode);
                if (memory.ContainsKey(node.getPosition()))
                {
                    memory.Remove(node.getPosition());
                }
                memory.Add(node.getPosition(), new Remembrance(node));
                last_update = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}