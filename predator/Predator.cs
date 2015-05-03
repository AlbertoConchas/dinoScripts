using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.My_Assets.dinoScripts.search;

public class Predator : Dinosaur {
	public bool debug = false;
	
	// Use this for initialization
	void Start () {
        becomeDepredator();

        base.start();//Init Dinosaur

		state = States.ChoosingLeader;
        updateHerd<Predator>();

		//Fija los parametros iniciales en torno a la escala
		comRange = (int) ( comRange * ((float)transform.localScale.x/0.3));

		this.stoppingDistance = travelStopDistance ();
		
		//Inicializa el NavMeshAgent
		nav = GetComponent<NavMeshAgent> ();
		
		nav.speed = Velocidad (isNeededRun);


        //Si no cuenta con eleccion de lider, el es el lider
        if (GetComponent<LeaderChoosing>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<LeaderChoosing>().choose();
        }
        
        //Inicia corrutina de crecimiento
        StartCoroutine("predatorGrow");
	}
	
	
	// Update is called once per frame	
	void Update () 
    {
		if (!Metabolism()) 
			return;

        if (state == States.Die) return;


        actualNode = getActualPathNode();


        priority = priorities();
        memorize();


        updateHerd<Predator>();
		if (state == States.Hunting) {
			isNeededRun = true;
		} else {
			isNeededRun = false;				
	    }

		nav.speed = Velocidad (isNeededRun);
        
        if ((leader == null || leader.GetComponent<Predator>().state == Predator.States.Die) && state != States.ChoosingLeader)
        {

            //Si no cuenta con eleccion de lider, el es el lider
            if (GetComponent<LeaderChoosing>() == null)
                setLeader(gameObject);
            else
            {
                GetComponent<LeaderChoosing>().choose();
            }
		
			
	
		} else if (state != States.ChoosingLeader) {

            /////////////////////////////////////////////////////////REPRODUCE
			/// 
			if(priority==Priorities.Reproduce && state==States.Waiting && repLapse <=0){
				
				state=States.Reproduce;
				
			}else if((state==States.Waiting || state==States.Reproduce)&&priority==Priorities.Eat){
				
				state=States.Eating;
			}

			if (state == States.Reproduce&& female)
			{
				if(debug)
					Debug.Log("Aqui");
				behavior_reproduce();
				state = States.Waiting;

            }

			//LEADER BEHAVIOR 
			if ( IsMyLeader(gameObject) ) {
				
				//senseForSomething();
				if (state == States.Searching) {			//Entra en estado para buscar comida
					////Debug.Log("Buscando por lugar con comida");
					behavior_leader_searching();
					//Debug.Log("LEader searching");
					
				} else if ( state == States.Following) {	//Entra en estado de viaje en grupo
					////Debug.Log("Viajando lugar con comida");
					behavior_leader_following();
					//Debug.Log("LEader Follow");
					
				} else if ( state == States.Hunting ) {
					////Debug.Log("Cazando comida");
					behavior_leader_Hunting();
					//Debug.Log("LEader Hunting");
					
				} else if ( state == States.Eating ) {
					////Debug.Log("Comiendo...");
					behavior_leader_Eating();
					//Debug.Log("LEader eating");
                }
                else if (state == States.Waiting && priority == Priorities.Eat)
                {
                    state = States.Searching;
                }
				
				
				
				//FOLLOWER BEHAVIOR 
			}else{
				if ( state == States.Following ){			//Seguir al lider
					behavior_follower_following();
					
				}else if ( state == States.Waiting ){		//Esperar a que el lider tome una decicion
					behavior_follower_waiting();
					
				}else if ( state == States.Reagruping ){	
					
				}else if ( state == States.Hunting ) {
					////Debug.Log("Cazando comida");
					behavior_follower_Hunting();
					
				} else if ( state == States.Eating ) {
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
		//repLapse = 60;
     
    }

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////// Comportamiento del lider ///////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void behavior_leader_searching(){
		//Calcula nueva posicion de la comida
		Vector3 foodPosition = 	searchForFood();
		if (foodPosition != Vector3.zero) {
			state = States.Following;
			nav.destination = foodPosition;
			order_followMe (gameObject);
		}
	}
	
	void behavior_leader_following(){
		if( isOnRangeToStop() ){
			if ( hungry() ){
				state = States.Hunting;
				order_hunt(gameObject);
				stop();
				actualFood = getBestFood();
				if ( actualFood == null ){
					state = States.Searching;
					return;
				}
				nav.destination = actualFood.transform.position;
			}else{
				//Debug.Log("Descanzar");
				state = States.Searching;
			}
		}
	}
	
	
	void behavior_leader_Hunting(){
		if ( actualFood == null ){
			actualFood = getBestFood();
			if ( actualFood == null ){
				state = States.Searching;
				//order_stop(gameObject);
			}
		}
		
        // no se encontro comida
        if (actualFood == null)
        {
            return;
        }

		nav.destination = actualFood.transform.position;
		if( DistanceFromDestination() <= DistanceToBite(false) ){
			nav.destination = transform.position;
			transform.LookAt (actualFood.transform);
			if (actualFood.GetComponent<Prey> ().hp < 0) {
				state = States.Eating;
				this.GetComponent<DinasorsAnimationCorrector> ().eating();
			}else {
				BiteEnemy(false);
			}
		}
	}
	
	
	void behavior_leader_Eating(){
		if ( actualFood == null ){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = States.Searching;
			return;
		}
		
		EatEnemy(false);
		if ( actualFood.GetComponent<Prey>().flesh < 0){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = States.Searching;
		}
		
		if ( satisfied() ) {
			state = States.Searching;
			this.GetComponent<DinasorsAnimationCorrector>().idle();
		}
	}	
	
	
	
	
	
	
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////// Comportamiento del Seguidor ////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void behavior_follower_following(){
		nav.stoppingDistance = travelStopDistance ();
		nav.destination = leader.transform.position;
		/*if( leader.GetComponent<Predator>().state != States.Following ){
			if( isOnRangeToStop(1.5f) ){
				stop();
				state = States.Waiting;
			}
		}*/
	}
	
	void behavior_follower_waiting(){
		if (nav.velocity != Vector3.zero ){
			stop();
		}
	}
	
	void behavior_follower_reagruping(){
		if( isOnRangeToStop(3f) ){
			/*stop();
			state = States.Hunting;
			GameObject[] food = getNearbyFood();
			actualFood = getNeardest(food);
			nav.destination = actualFood.transform.position;*/
		}
	}
	
	
	void behavior_follower_Hunting(){
		if (actualFood == null) {
			actualFood = getBestFood ();
			if (actualFood == null) {
				state = States.Following;
				nav.stoppingDistance = travelStopDistance();
				////Debug.Log ("No Food, nearby");
				return;
			}
		}
		
		nav.stoppingDistance = 0;
		//nav.stoppingDistance = distanceToBite();
		nav.destination = actualFood.transform.position;
		if (DistanceFromDestination () <= DistanceToBite (false)) {
			
			nav.destination = transform.position;
			if (actualFood.GetComponent<Prey> ().hp < 0) {
				state = States.Eating;
				this.GetComponent<DinasorsAnimationCorrector> ().eating ();
			} else {
				BiteEnemy (false);
			}
		}
	}
	
	void behavior_follower_Eating(){
		if ( actualFood == null ){
			state = States.Following;
			nav.stoppingDistance = travelStopDistance();
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			return;
		}
		
		EatEnemy(false);
		if ( actualFood.GetComponent<Prey>().flesh < 0){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = States.Hunting;
		}
		
		if ( satisfied() ) {
			state = States.Following;
			nav.stoppingDistance = travelStopDistance();
			this.GetComponent<DinasorsAnimationCorrector>().idle();
		}
	}
	
	
	
	
	
	
	
	///////////////////////////////////////////////////////////////
	///////////////// Ordenes del lider ///////////////////////////
	///////////////////////////////////////////////////////////////
	void order_followMe( GameObject l ){
		BroadCast ("LeaderSaysFollowMe", l);
	}
	
	void order_stop( GameObject l ){
		BroadCast ("LeaderSaysStop", l);
	}
	
	void order_reagrupate( GameObject l ){
		BroadCast ("LeaderSaysReagrupate", l);
	}
	
	
	void order_hunt( GameObject l ){
		BroadCast ("LeaderSaysHunt", l);
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
	void LeaderSaysFollowMe( GameObject l ){
		if (state != States.Following && 0 < hp ) {
			if ( IsMyLeader(l) ) {
				if( !IsMe(leader) ){
					state = States.Following;
					order_followMe(l);	//Reply the message to others
				}
			}
		}
	}
	
	
	void LeaderSaysStop( GameObject l ){
        if (state != States.Waiting && 0 < hp)
        {
			if ( IsMyLeader(l) ) {
				if( !IsMe(leader) ){
                    state = States.Waiting;
					order_stop(l);	//Reply the message to others
				}
			}
		}
	}
	
	void LeaderSaysReagrupate( GameObject l ){
		if (state != States.Reagruping && 0 < hp ) {
			if ( IsMyLeader(l) ) {
				if( !IsMe(leader) ){
					state = States.Reagruping;
					nav.destination = Dispersal( l.transform.position );
					order_reagrupate(l);	//Reply the message to others
				}
			}
		}
	}
	
	void LeaderSaysHunt( GameObject l ){
		if (state != States.Hunting && 0 < hp ) {
			if ( IsMyLeader(l) ) {
				if( !IsMe(leader) ){
					state = States.Hunting;
					nav.destination = l.GetComponent<NavMeshAgent>().destination;
					order_hunt(l);	//Reply the message to others
				}
			}
		}
	}

	
	/**
	 **Recive un arreglo de GameObject y regresa el mas cercano a la posicion actual
	 */
	GameObject getNeardest( GameObject[] objects ) {
		if (objects == null)
			return null;
		if (objects.Length == 0) {
			////Debug.Log("GetNeardes: Lista vacia");
			return null;
		}
		GameObject ret = objects [0];
		float distMin, distTemp;
		distMin = Vector3.Distance (transform.position, ret.transform.position );
		for (int i = 1; i < objects.Length; i++) {
			distTemp = Vector3.Distance (transform.position, objects[i].transform.position);
			if ( distTemp  < distMin ){
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
        List<GameObject> preys = new List<GameObject>();

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange*2.5f);
		for (int i = 0; i < hitColliders.Length; i++) 
        {
			if( !IsMe( hitColliders[i].gameObject ) ){ //No me lo envio a mi
				if (hitColliders [i].GetComponent<Prey> () != null )
                {
                    preys.Add(hitColliders[i].gameObject);
					foodCounter++;
				}
			}
		}
        return preys.ToArray();
	}
    private Priorities priorities()
    {

        if (hungry())
        {
            return Priorities.Eat;
        }
        return Priorities.Obey;
    }

	
	/*
	 * Retorna la mejor presa posible
	 */
	GameObject getBestFood(){
		GameObject[] g = getNearbyFood();
		if (g.Length == 0)
			return null;
		for (int i = 0; i < g.Length; i++) {
			if( g[i].GetComponent<Prey>().hp <= 0 )
				return g[i];
		}
		//return g [Random.Range (0, g.Length - 1)];
		return getNeardest (g);
	}

    override protected bool isGoal(Node node)
    {
        return (node.getPreys() > 0);
    }

	/*
	private bool hungry(){
		if (stamina < 120f || hp < 100)
			return true;
		return false;
	}
	
	private bool satisfied(){
		if (stamina < 150 || hp < 100)
			return false;
		return true;
	}*/


    IEnumerator predatorGrow()
    {
        while (gameObject.transform.localScale.x < 1)
        {
            gameObject.transform.localScale = new Vector3((float)(gameObject.transform.localScale.x + 0.005),
                                                          (float)(gameObject.transform.localScale.y + 0.005),
                                                          (float)(gameObject.transform.localScale.z + 0.005));
            yield return new WaitForSeconds(1);
        }
    }

    public void becomeDepredator()
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
            brigth.light.color = Color.red;
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