using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.My_Assets.dinoScripts.search;
using Assets.My_Assets.dinoScripts.search.bayesiannetwork;
using Assets.My_Assets.dinoScripts.Dinosaur;

public class Prey : Dinosaur
{
    public int runningTime = 200;
	private FuzzyLogic fLogic;
	public bool debug = false;

    //Heuristic objects
    private BayesianFactory byfactory;
    private PearlsMessagePassing pearls;

    //Enum Para los estados del seguidor



    // Use this for initialization
    void Start()
    {
        becomePrey();
		if( fLogic == null )
			setFuzzyLogic();

        base.start();//Init Dinosaur

        //flesh = 500f;
        updateHerd<Prey>();

        if (leader == null)
        {
            state = States.ChoosingLeader;
            GetComponent<LeaderChoosing>().choose();
        }
        else 
        { 
            lifetime = 0;
        }

        //Fija los parametros iniciales en torno a la escala
        comRange = (int)(comRange * ((float)transform.localScale.x / 0.3));
        this.stoppingDistance = travelStopDistance();

        //Inicializa el NavMeshAgent
        nav = GetComponent<NavMeshAgent>();

		nav.speed =Velocidad(false);        
        StartCoroutine("preyGrow");

    }


    // Update is called once per frame	
    void Update()
	{
		if (!Metabolism())
			return;
		if (state == States.Die) return;



        
		if (runningTime > 0 && priority == Priorities.Run)
        {
            runningTime--;
            return;
        }

        actualNode = getActualPathNode();
       // priority = priorities();
		priority = fLogic.calPriority (actualNode, 100, 720, stamina, lifetime,hp,maxHp);
        memorize();

		//Debug.Log (fLogic.calPriority(actualNode,100,maxLifeTime,stamina,lifetime));

        if (priority == Priorities.Run)
        {
			nav.speed =Velocidad(true);
        }
        else
			nav.speed = Velocidad(false);

        updateHerd<Prey>();

        if (state == States.Hiding || priority == Priorities.Run)
        {
			if(IsMyLeader(gameObject))order_panic(gameObject);

            // PreyNeuronalChoose.NeuralReturn r = GetComponent<PreyNeuronalChoose>().migrate();
            nav.destination = actualNode.getNeighbors()[0].transform.position;
            for (int i = 0; i < actualNode.getNeighbors().Length; i++)
            {
                if (lastNode == null)
                {
                    lastNode = actualNode;
                    nav.destination = actualNode.getNeighbors()[i].transform.position;// hacia donde correr!
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
            state = States.Searching;

        }


        // si el lider ya no existe o esta muerto y ademas no se esta seleccionando lider
        else if ((leader == null || leader.GetComponent<Prey>().state == Prey.States.Die) && state != States.ChoosingLeader)
        {
			//updateHerd<Prey>();
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
            
			if(priority==Priorities.Reproduce && state==States.Waiting && repLapse <=0){

				state=States.Reproduce;
				//repLapse = 60;

			}else if((state==States.Waiting || state==States.Reproduce)&&priority==Priorities.Eat ){
				if(actualFood==null){//si no tiene comida caza
					state=States.Hunting;
				}else{
					state=States.Eating;
				}
			}

            if (state == States.Reproduce && female)
            {
				if(debug)
					Debug.Log("Aqui");
                behavior_reproduce();
				state = States.Waiting;
            }

            //LEADER BEHAVIOR 
            if (IsMyLeader(gameObject))
            {
                if (priority == Priorities.Run)
                {
                    order_panic(gameObject);
                    state = States.Hiding;
                }
                //senseForSomething();
                else if (state == States.Searching)
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
                else if (state == States.Waiting && priority == Priorities.Eat)
                {
                    state = States.Searching;

                }



                //FOLLOWER BEHAVIOR 
            }
            else
            {
                if (state == States.Following)
                {			//Seguir al lider
                    behavior_follower_following();

                }
               /* else if (state==States.Repose || leader.GetComponent<Prey>().actualNode.transform.position == actualNode.transform.position)
                {		//Esperar a que el lider tome una decicion
                    if (state != States.Repose)
                        state = States.Repose;
                    behavior_follower_waiting();

                }*/
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


  
    /// Reproduce
  
    void behavior_reproduce()
    {
        GetComponent<DinosaurReproduce>().findPartner();
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////// Comportamiento del lider ///////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    

    void behavior_leader_searching()
    {
        //Calcula nueva posicion de la comida
        Vector3 foodPosition = searchForFood();
        state = States.Following;
        order_followMe(gameObject);
        if (!(foodPosition == actualNode.transform.position))
        {
            nav.destination = foodPosition;
        }
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
                return;
                //order_stop(gameObject);
            }
        }

        nav.destination = actualFood.transform.position;
        if (DistanceFromDestination() <= DistanceToBite(true))
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
				BiteEnemy(true);
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

        EatEnemy(true);
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Searching;
        }

        if (satisfied())
        {
            state = States.Waiting;
            order_stop(gameObject);
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

        if (leader.GetComponent<Prey>().state == States.Waiting && leader.GetComponent<Prey>().actualNode.transform.position == actualNode.transform.position)
        {
            if( isOnRangeToStop() ){
                stop();
                state = States.Waiting;
            }
        }
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
        if (DistanceFromDestination() <= DistanceToBite(true))
        {

            nav.destination = transform.position;
            if (actualFood.GetComponent<Plant>().hp < 0)
            {
                state = States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                BiteEnemy(true);
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

        EatEnemy(true);
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Hunting;
        }

        if (satisfied())
        {
            state = States.Waiting;
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


	//pedimiento de reproduccion.. de la hembra hacia el macho

	void letsMakeAChild(GameObject g)
	{
		if (state == States.Reproduce && !female && repLapse<=0)
		{
			state=States.Waiting;
			repLapse=60;
			g.GetComponent<DinosaurReproduce>().Reproduce();
		}
	}
	
	
	///////////////////////////////////////////////////////////////
	///////////////// Reacciones a ordenes del lider //////////////
    ///////////////////////////////////////////////////////////////
    void LeaderSaysFollowMe(GameObject l)
    {
        if (state != States.Following && 0 < hp)
        {
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
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
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
                {
                    if (leader.GetComponent<Prey>().actualNode.transform.position == actualNode.transform.position)
                    {
                        state = States.Waiting;
                    }
                    else state = States.Following;
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
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
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
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
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
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
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
            if (!IsMe(hitColliders[i].gameObject))
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
            if (!IsMe(hitColliders[i].gameObject))
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

    /// <summary>
    /// Method that Dinosaur uses in order to know if node is Goal in the searching for this type.
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>true if is node is goal for this type of Dinosaur. false otherwise.</returns>
    override protected bool isGoal(Node node)
    {
        return (node.getPlants() > 0);
    }

    /// <summary>
    /// The heuristic for an especific. Actual implementation is bayesian network.
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>h value for node</returns>
    override protected float getH(Node node)
    {
        //Initial h
        float h = node.getFertility();

        //Initialize if needed
        if (byfactory == null)
        {
            byfactory = new BayesianFactory(this);
        }
        if (pearls == null)
        {
            pearls = new PearlsMessagePassing();
        }

        //Get memory events in order to update bayesian network
        Remembrance remembrance = null;
        try
        {
            remembrance = memory[node.getPosition()];
        }
        catch (KeyNotFoundException) { }
        if (remembrance != null)
        {
            BayesianNode[] nodes = byfactory.createBayNet();
            pearls.initial_tree(ref nodes, ref nodes[0]);

            List<string> a = new List<string>();

            //Do something with memory

            //There was Predators
            if (remembrance.getNode().getPredators() > 0)
            {
                pearls.update_tree(ref nodes, ref nodes[0], "D", "d1");
                a.Add("d1");
            }
            else
            {
                pearls.update_tree(ref nodes, ref nodes[0], "D", "d2");
                a.Add("d2");
            }

            //There was Food
            if (remembrance.getNode().getPlants() > 0)
            {
                pearls.update_tree(ref nodes, ref nodes[0], "C", "c1");
                a.Add("c1");
            }
            else
            {
                pearls.update_tree(ref nodes, ref nodes[0], "C", "c2");
                a.Add("c2");
            }

            foreach (string aval in a)
            {
                h = 0f;
                try
                {
                    h += pearls.getNode(ref nodes, "S1").Values["s1"].Probs[aval] * 10;
                    h += pearls.getNode(ref nodes, "S2").Values["s2"].Probs[aval] * 10;
                }
                catch (KeyNotFoundException) { }
                
            }
        }
        return h;
    }

    IEnumerator preyGrow()
    {
        while (gameObject.transform.localScale.x < 1 && state!=States.Die)
        {
            gameObject.transform.localScale = new Vector3((float)(gameObject.transform.localScale.x + 0.005),
                                                          (float)(gameObject.transform.localScale.y + 0.005),
                                                          (float)(gameObject.transform.localScale.z + 0.005));
            yield return new WaitForSeconds(1);
        }
    }
	private void setFuzzyLogic(){
		fLogic = GameObject.Find ("Global").GetComponent<FuzzyLogic> ();
	}
	/*
	 * FormatData
	 * Le da formato a la informacion de los nodos para procesarla
	 */
	private double[,] formatData(PathNode actualNode, GameObject[] neighbors){
		double[,] nodesData = new double[ 3 , neighbors.Length + 1 ];
		
		//Agrega los vecinos para ser procesados
		for (int i = 0; i < neighbors.Length; i++) {
			nodesData[0,i] = neighbors[i].GetComponent<PathNode>().getPlants();
			nodesData[1,i] =  actualNode.GetComponent<PathNode>().getPredators();
			nodesData[2, i] = actualNode.GetComponent<PathNode>().getPrays();
		}
		//Agrega el nodo actual para ser procesado tambien
		nodesData[0, neighbors.Length ] = actualNode.getPlants();
		nodesData[1, neighbors.Length ] = actualNode.getPredators();
		nodesData[2, neighbors.Length] = actualNode.getPrays();
		
		return nodesData;
	}
   
    public void becomePrey()
    {

        //Crea el objeto al que se le agregara la luz
        Transform t = gameObject.transform.Find("shine");
        GameObject brigth = null;
        if (t == null)
        {
            brigth = new GameObject("shine");
            brigth.AddComponent(typeof(Light));							//se le agrega la luz

            brigth.transform.parent = transform;							//Se fija a la entidad
            brigth.light.type = LightType.Spot;								//Se elije el tipo de luz SPOT

            //Se pone la mira hacia abajo
            brigth.transform.position = brigth.transform.parent.position + new Vector3(0, 3, 0);
            brigth.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Color, Alcance, Dispercion
            brigth.light.color = Color.yellow;
            brigth.light.range = 15.0f;
            brigth.light.spotAngle = 20.0f;
            brigth.light.intensity = 2.20f;
        }
        else
        {
            brigth = t.gameObject;
        }
    }
}