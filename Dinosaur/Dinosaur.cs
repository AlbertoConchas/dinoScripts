using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.My_Assets.dinoScripts.search;

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
    public States state;
    public Priorities priority;
    public PathNode actualNode;
    protected float stoppingDistance;
    protected NavMeshAgent nav;

    private List<GameObject> herd = new List<GameObject>();
    protected GameObject leader;
    private bool requestResponded;
    private GameObject tempLeader;

    public enum States { ChoosingLeader, Searching, Following, Moving, Hunting, Eating, Hiding, Reproduce, Waiting, Reagruping, Die };
    public enum Priorities {Eat, Obey, Reproduce, Run};

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------ Lider Chosing --------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

  
    /**
 *	Fijar el objeto lider
 */
    public void setLeader(GameObject l)
    {
        leader = l;
        nav.avoidancePriority = 1;
        state = States.Searching;
    }

    /*
 *	Funcion que detiene al nav Agent
 */
    public float getLeadershipStat()
    {
        return
            (this.hp / 100) +
                (this.speed / 3) +
                ((float)this.stamina / 100) +
                ((this.lifetime * 2) / 10000);
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
       if(herd.Count>0)
        foreach (GameObject dino in herd)
        {
            //Enviale la eleccion de lider
            if (dino != null || dino.GetComponent<Dinosaur>().state != States.Die)
            {
                dino.SendMessage(message, (GameObject)obj);
            }
            else
            {
                herd.Remove(dino);
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
       if (g.GetInstanceID() == gameObject.GetInstanceID())
           return true;
       return false;
   }

   //Retorna si el gameobject enviado es igual al lider de la unidad actual
  protected bool isMyLeader(GameObject l)
   {
       if (l.GetInstanceID() == leader.GetInstanceID())
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
      pos.x = pos.x + (((float)Random.Range(-50, 50) / 100) * this.comRange);
      pos.z = pos.z + (((float)Random.Range(-50, 50) / 100) * this.comRange);
      return pos;
  }


  protected  float travelStopDistance()
  {
      return comRange * ((float)Random.Range(30, 50) / 100);
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
}