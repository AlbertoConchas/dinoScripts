using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.My_Assets.dinoScripts.search;

public class Prey : Dinosaur
{
    //public bool isNeededRun = false;
    public GameObject actualFood;

    private BinaryHeap<Node> open;//A* pathfinding
    private HashSet<Node> closed;//A* pathfinding
    private PathNode lastNode;
    private int runningTime = 200;
    //Enum Para los estados del seguidor



    // Use this for initialization
    void Start()
    {
        base.start();//Init Dinosaur

        flesh = 500f;
        updateHerd<Prey>();

        //  state = States.Reproduce;

        state = States.ChoosingLeader;
        //Fija los parametros iniciales en torno a la escala
        comRange = (int)(comRange * ((float)transform.localScale.x / 0.3));
        this.stoppingDistance = travelStopDistance();

        //Inicializa el NavMeshAgent
        nav = GetComponent<NavMeshAgent>();

        nav.speed = (float)((stamina / 100f) * speed) / 3;
        /*if(isNeededRun)
            nav.speed = (float)((stamina/100f)*speed)*3;
        */
        //Si no cuenta con eleccion de lider, el es el lider
        if (GetComponent<LeaderChoosing>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<LeaderChoosing>().choose();
        }
        StartCoroutine("preyGrow");
    }


    // Update is called once per frame	
    void Update()
    {
        if (!metabolism())
            return;
        if (runningTime > 0 && priority == Priorities.Run)
        {
            runningTime--;
            return;
        }

        actualNode = getActualPathNode();
        priority = priorities();
        memorize();

        if (priority == Priorities.Run)
        {
            nav.speed = (float)((stamina / 100f) * speed) * 3;
        }
        else
            nav.speed = (float)((stamina / 100f) * speed) / 3;

        updateHerd<Prey>();

        if (state == States.Hiding || priority == Priorities.Run)
        {

            // PreyNeuronalChoose.NeuralReturn r = GetComponent<PreyNeuronalChoose>().migrate();
            nav.destination = actualNode.getNeighbors()[0].transform.position;
            for (int i = 0; i < actualNode.getNeighbors().Length; i++)
            {
                if (lastNode == null)
                {
                    lastNode = actualNode;
                    nav.destination = actualNode.getNeighbors()[i].transform.position;
                    break;
                }
                else if (lastNode.transform.position != actualNode.getNeighbors()[i].transform.position)
                {
                    nav.destination = actualNode.getNeighbors()[i].transform.position;
                    break;
                }
            }
            /*  if (actualNode.transform.position != r.node.transform.position)
             {
                 nav.destination = r.node.transform.position;
             }else {
                 if ( isOnRangeToStop() ){
                     stop ();
                     state = States.Searching;
                 }
             }*/

        }
        // si el lider ya no existe o esta muerto y ademas no se esta seleccionando lider
        else if ((leader == null || leader.GetComponent<Prey>().state == Prey.States.Die) && state != States.ChoosingLeader)
        {
            
            if (GetComponent<LeaderChoosing>() == null)
                setLeader(gameObject);
            else
            {
                GetComponent<LeaderChoosing>().choose();
            }

        }
        else if (state != States.ChoosingLeader)
        {

            /////////////////////////////////////////////////////////REPRODUCE
            if (state == States.Reproduce)
            {
                ////Debug.Log("Estado de reproduccion");
                behavior_reproduce();
                //Debug.Log("LEader eating");
            }

            //LEADER BEHAVIOR 
            if (isMyLeader(gameObject))
            {

                //senseForSomething();
                if (state == States.Searching)
                {			//Entra en estado para buscar comida
                    ////Debug.Log("Buscando por lugar con comida");
                    behavior_leader_searching();
                    //Debug.Log("LEader searching");

                }
                else if (state == States.Following)
                {	//Entra en estado de viaje en grupo
                    ////Debug.Log("Viajando lugar con comida");
                    behavior_leader_following();
                    //Debug.Log("LEader Follow");

                }
                else if (state == States.Hunting)
                {
                    ////Debug.Log("Cazando comida");
                    behavior_leader_Hunting();
                    //Debug.Log("LEader Hunting");

                }
                else if (state == States.Eating)
                {
                    ////Debug.Log("Comiendo...");
                    behavior_leader_Eating();
                    //Debug.Log("LEader eating");
                }



                //FOLLOWER BEHAVIOR 
            }
            else
            {
                if (state == States.Following)
                {			//Seguir al lider
                    behavior_follower_following();

                }
                else if (state == States.Waiting)
                {		//Esperar a que el lider tome una decicion
                    behavior_follower_waiting();

                }
                else if (state == States.Reagruping)
                {

                }
                else if (state == States.Hunting)
                {
                    ////Debug.Log("Cazando comida");
                    behavior_follower_Hunting();

                }
                else if (state == States.Eating)
                {
                    ////Debug.Log("Comiendo...");
                    behavior_follower_Eating();
                }
            }
        }
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////// Comportamiento del lider ///////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void behavior_reproduce()
    {
        GetComponent<DinosaurReproduce>().findPartner();
        state = States.Searching;
    }


    void behavior_leader_searching()
    {
        //Calcula nueva posicion de la comida
        Vector3 foodPosition = searchForFood();
        state = States.Following;
        order_followMe(gameObject);
        nav.destination = foodPosition;
    }

    void behavior_leader_following()
    {
        if (isOnRangeToStop())
        {
            if (hungry())
            {
                state = States.Hunting;
                order_hunt(gameObject);
                stop();
                actualFood = getBestFood();
                if (actualFood == null)
                {
                    state = States.Searching;
                    return;
                }
                nav.destination = actualFood.transform.position;
            }
            else
            {
                state = States.Searching;
                //order_stop(gameObject);
            }
        }
    }


    void behavior_leader_Hunting()
    {
        if (actualFood == null)
        {
            actualFood = getBestFood();
            if (actualFood == null)
            {
                state = States.Searching;
                //order_stop(gameObject);
            }
        }

        nav.destination = actualFood.transform.position;
        if (distanceFromDestination() <= distanceToBite())
        {
            nav.destination = transform.position;
            transform.LookAt(actualFood.transform);
            if (actualFood.GetComponent<Plant>().hp < 0)
            {
                state = States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                biteEnemy();
            }
        }
    }


    void behavior_leader_Eating()
    {
        if (actualFood == null)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Searching;
            return;
        }

        eatEnemy();
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Searching;
        }

        if (satisfied())
        {
            state = States.Searching;
            this.GetComponent<DinasorsAnimationCorrector>().idle();
        }
    }







    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////// Comportamiento del Seguidor ////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void behavior_follower_following()
    {
        nav.stoppingDistance = travelStopDistance();
        nav.destination = leader.transform.position;

        /*if( leader.GetComponent<Prey>().state != States.Following && leader.GetComponent<Prey>().state != States.Searching){
            if( isOnRangeToStop() ){
                stop();
                state = States.Waiting;
            }
        }*/
    }

    void behavior_follower_waiting()
    {
        if (nav.velocity != Vector3.zero)
        {
            stop();
        }
    }

    void behavior_follower_reagruping()
    {
        if (isOnRangeToStop(3f))
        {
            /*stop();
            state = States.Hunting;
            GameObject[] food = getNearbyFood();
            actualFood = getNeardest(food);
            nav.destination = actualFood.transform.position;*/
        }
    }


    void behavior_follower_Hunting()
    {
        if (actualFood == null)
        {
            actualFood = getBestFood();
            if (actualFood == null)
            {
                state = States.Following;
                nav.stoppingDistance = travelStopDistance();

                //Debug.Log ("No Food, nearby");
                return;
            }
        }

        nav.stoppingDistance = 0;
        //nav.stoppingDistance = distanceToBite();
        nav.destination = actualFood.transform.position;
        if (distanceFromDestination() <= distanceToBite())
        {

            nav.destination = transform.position;
            if (actualFood.GetComponent<Plant>().hp < 0)
            {
                state = States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                biteEnemy();
            }
        }

    }

    void behavior_follower_Eating()
    {
        if (actualFood == null)
        {
            state = States.Following;
            nav.stoppingDistance = travelStopDistance();
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            return;
        }

        eatEnemy();
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Hunting;
        }

        if (satisfied())
        {
            state = States.Following;
            nav.stoppingDistance = travelStopDistance();
            this.GetComponent<DinasorsAnimationCorrector>().idle();
        }
    }







    ///////////////////////////////////////////////////////////////
    ///////////////// Ordenes del lider ///////////////////////////
    ///////////////////////////////////////////////////////////////
    void order_followMe(GameObject l)
    {
        BroadCast("LeaderSaysFollowMe", l);
    }

    void order_stop(GameObject l)
    {
        BroadCast("LeaderSaysStop", l);
    }

    void order_reagrupate(GameObject l)
    {
        BroadCast("LeaderSaysReagrupate", l);
    }


    void order_hunt(GameObject l)
    {
        BroadCast("LeaderSaysHunt", l);
    }

    void order_unsetLeader(GameObject l)
    {
        BroadCast("LeaderSaysUnsetLeader", l);
    }

    void order_panic(GameObject l)
    {
        BroadCast("SaysPanic", l);
    }

    ///////////////////////////////////////////////////////////////
    ///////////////// Reacciones a ordenes del lider //////////////
    ///////////////////////////////////////////////////////////////
    void LeaderSaysFollowMe(GameObject l)
    {
        if (state != States.Following && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Following;
                    order_followMe(l);	//Reply the message to others
                }
            }
        }
    }


    void LeaderSaysStop(GameObject l)
    {

        if (priority == Priorities.Run)
            return;

        if (state != States.Waiting && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Waiting;
                    order_stop(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysReagrupate(GameObject l)
    {
        if (priority == Priorities.Run)
            return;

        if (state != States.Reagruping && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Reagruping;
                    nav.destination = Dispersal(l.transform.position);
                    order_reagrupate(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysHunt(GameObject l)
    {
        if (priority == Priorities.Run)
            return;

        if (state != States.Hunting && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Hunting;
                    nav.destination = l.GetComponent<NavMeshAgent>().destination;
                    order_hunt(l);	//Reply the message to others
                }
            }
        }
    }


    void LeaderSaysUnsetLeader(GameObject l)
    {
        if (leader != null && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Hiding;
                    leader = null;
                    order_unsetLeader(l);	//Reply the message to others
                }
            }
        }
    }



    void SaysPanic(GameObject l)
    {
        if (0 < hp)
            state = States.Hiding;

    }


    private Priorities priorities()
    {
        if (nodes == null)
        {
            setNodesController();
        }
        if (actualNode.getPredators() > 0 || state == States.Hiding)
        {
            return Priorities.Run;
        }
        else if (hungry())
        {
            return Priorities.Eat;
        }
        else if (mature())
        {
            return Priorities.Reproduce;
        }
        return Priorities.Obey;
    }


    private bool mature()
    {
        if (lifetime <= 5000f && lifetime > 1000f)
            return true;
        return false;
    }
    /**
     *	Funciones Biologicas de consumir energia
     */
    private bool metabolism()
    {
        float factor = 1f;
        this.lifetime -= 0.007f * factor;

        if (priority == Priorities.Run)
            factor *= 2f;


        if (state == States.Die)
        {
            if (this.flesh <= 0)
                Destroy(gameObject);
            return false;
        }
        if (0 < this.stamina)
        {
            this.stamina -= 0.001 * factor;
        }
        if (stamina <= 0)
        {
            if (0 < this.hp)
            {
                this.hp -= 0.001f * factor;
            }
        }
        if (this.hp <= 0)
        {
            die();
            return false;
        }
        return true;
    }


    //Mueve las estadisticas del enemigo y del agente
    void eatEnemy()
    {
        actualFood.GetComponent<Plant>().flesh -= (float)this.attack / (1f / Time.deltaTime);
        if (this.stamina < 100f)
            this.stamina += ((float)this.attack / (1f / Time.deltaTime));
        else
            this.hp += ((float)this.attack / (1f / Time.deltaTime)) * 0.5f;
    }

    /**
     * Distancia Optima para atacar al enemigo actual
     */
    float distanceToBite()
    {
        return ((nav.radius) * transform.localScale.x * 1.3f) +
            ((actualFood.GetComponent<MeshRenderer>().bounds.size.x) * 1.3f);
    }


    /**
     * Funcion que inflige daño al enemigo
     */
    void biteEnemy()
    {
        actualFood.GetComponent<Plant>().hp -= this.attack / (1f / Time.deltaTime);
    }




    /**
     **Recive un arreglo de GameObject y regresa el mas cercano a la posicion actual
     */
    GameObject getNeardest(GameObject[] objects)
    {
        if (objects == null)
            return null;
        if (objects.Length == 0)
        {
            ////Debug.Log("GetNeardes: Lista vacia");
            return null;
        }
        GameObject ret = objects[0];
        float distMin, distTemp;
        distMin = Vector3.Distance(transform.position, ret.transform.position);
        for (int i = 1; i < objects.Length; i++)
        {
            distTemp = Vector3.Distance(transform.position, objects[i].transform.position);
            if (distTemp < distMin)
            {
                distMin = distTemp;
                ret = objects[i];
            }
        }
        return ret;
    }


    /**
     *	Obtiene los objetos "COMIDA", cercanos a la posicion del objeto
     */
    GameObject[] getNearbyFood()
    {
        int foodCounter = 0;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange * 2.5f);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (!isMe(hitColliders[i].gameObject))
            { //No me lo envio a mi
                if (hitColliders[i].tag == "Tree")
                {
                    foodCounter++;
                }
            }
        }
        GameObject[] ret = new GameObject[foodCounter];
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (!isMe(hitColliders[i].gameObject))
            { //No me lo envio a mi
                if (hitColliders[i].tag == "Tree")
                {
                    ret[--foodCounter] = hitColliders[i].gameObject;
                }
            }
        }
        return ret;
    }

    /*
     * Retorna la mejor presa posible
     */
    GameObject getBestFood()
    {
        GameObject[] g = getNearbyFood();
        if (g.Length == 0)
            return null;
        return g[Random.Range(0, g.Length - 1)];
    }


    /*
    *	Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
    */
    private Vector3 searchForFood()
    {
        //init data structures if needed
        if (open == null)
        {
            open = new BinaryHeap<Node>(new NodeComparator());
            closed = new HashSet<Node>(new NodeEqualityComparer());
        }


        closed.Add(toNode(actualNode));
        if (actualNode.getPlants() > 0)
        {
            open = null;
            closed = null;
            return toNode(actualNode).getPosition();
        }
        Node[] neighbors = expand();//Equivalent to expand step on A* algorithm
        foreach (Node n in neighbors)
        {
            if (n.getPlants() > 0)
            {
                return n.getPosition();
            }
            if (!closed.Contains(n))
            {
                open.Insert(n);
            }
        }
        return open.RemoveRoot().getPosition();
    }



    IEnumerator preyGrow()
    {
        while (gameObject.transform.localScale.x < 1)
        {
            gameObject.transform.localScale = new Vector3((float)(gameObject.transform.localScale.x + 0.005),
                                                          (float)(gameObject.transform.localScale.y + 0.005),
                                                          (float)(gameObject.transform.localScale.z + 0.005));
            yield return new WaitForSeconds(1);
        }
    }
}