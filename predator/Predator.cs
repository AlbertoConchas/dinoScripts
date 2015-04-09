using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Predator : Dinosaur {

	private bool isNeededRun = false;
	public GameObject actualFood;
	
	
	// Use this for initialization
	void Start () {
        base.start();//Init Dinosaur

		state = States.ChoosingLeader;
        updateHerd<Predator>();

		//Fija los parametros iniciales en torno a la escala
		comRange = (int) ( comRange * ((float)transform.localScale.x/0.3));

		this.stoppingDistance = travelStopDistance ();
		
		//Inicializa el NavMeshAgent
		nav = GetComponent<NavMeshAgent> ();
		
		nav.speed = (float)speed/3;
		if(isNeededRun)
			nav.speed = (float)speed*3;


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
	void Update () {

		if (!metabolism()) 
			return;

        actualNode = getActualPathNode();

        memorize();


        updateHerd<Predator>();
        
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



			//LEADER BEHAVIOR 
			if ( isMyLeader(gameObject) ) {
				
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
		if( distanceFromDestination() <= distanceToBite() ){
			nav.destination = transform.position;
			transform.LookAt (actualFood.transform);
			if (actualFood.GetComponent<Prey> ().hp < 0) {
				state = States.Eating;
				this.GetComponent<DinasorsAnimationCorrector> ().eating();
			}else {
				biteEnemy();
			}
		}
	}
	
	
	void behavior_leader_Eating(){
		if ( actualFood == null ){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = States.Searching;
			return;
		}
		
		eatEnemy();
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
		if (distanceFromDestination () <= distanceToBite ()) {
			
			nav.destination = transform.position;
			if (actualFood.GetComponent<Prey> ().hp < 0) {
				state = States.Eating;
				this.GetComponent<DinasorsAnimationCorrector> ().eating ();
			} else {
				biteEnemy ();
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
		
		eatEnemy();
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
	
	
	
	
	///////////////////////////////////////////////////////////////
	///////////////// Reacciones a ordenes del lider //////////////
	///////////////////////////////////////////////////////////////
	void LeaderSaysFollowMe( GameObject l ){
		if (state != States.Following && 0 < hp ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = States.Following;
					order_followMe(l);	//Reply the message to others
				}
			}
		}
	}
	
	
	void LeaderSaysStop( GameObject l ){
		if (state != States.Waiting  && 0 < hp  ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = States.Waiting;
					order_stop(l);	//Reply the message to others
				}
			}
		}
	}
	
	void LeaderSaysReagrupate( GameObject l ){
		if (state != States.Reagruping && 0 < hp ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = States.Reagruping;
					nav.destination = Dispersal( l.transform.position );
					order_reagrupate(l);	//Reply the message to others
				}
			}
		}
	}
	
	void LeaderSaysHunt( GameObject l ){
		if (state != States.Hunting && 0 < hp ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = States.Hunting;
					nav.destination = l.GetComponent<NavMeshAgent>().destination;
					order_hunt(l);	//Reply the message to others
				}
			}
		}
	}

	
	/**
	 *	Funciones Biologicas de consumir energia
	 */
	private bool metabolism(){
		if ( state == States.Die ){
			if ( this.flesh <= 0 )
				Destroy( gameObject );
			return false;
		}
		if (0 < this.stamina) {	
			this.stamina -= 0.000001;			
		}
		if (stamina <= 0) {
			if ( 0 < this.hp ) {	
				this.hp -= 0.001f;
			}
		}
		if( this.hp <= 0){
			die ();
			return false;
		}
		return true;
	}
	
	
	//Mueve las estadisticas del enemigo y del agente
	void eatEnemy(){
		actualFood.GetComponent<Prey> ().flesh -= ((float)this.attack / (1f / Time.deltaTime))*0.6f;
        //actualFood.GetComponent<Prey>().isNeededRun = true;
		if ( this.stamina < 100f )
			this.stamina += ((float)this.attack / (1f / Time.deltaTime));
		else 
			this.hp += ((float)this.attack / (1f / Time.deltaTime));
	}
	

	
	/**
	 * Distancia Optima para atacar al enemigo actual
	 */
	float distanceToBite(){
		return ((nav.radius) * transform.localScale.x * 1.3f) +
			((actualFood.GetComponent<NavMeshAgent>().radius) * actualFood.transform.localScale.x * 1.3f);
	}
	
	
	/**
	 * Funcion que inflige daño al enemigo
	 */
	void biteEnemy(){
		actualFood.GetComponent<Prey> ().hp -= (this.attack / (1f / Time.deltaTime));
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
			if( !isMe( hitColliders[i].gameObject ) ){ //No me lo envio a mi
				if (hitColliders [i].GetComponent<Prey> () != null )
                {
                    preys.Add(hitColliders[i].gameObject);
					foodCounter++;
				}
			}
		}
        return preys.ToArray();
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



	/*
	*	Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
	*/
	private Vector3 searchForFood(){
		return GetComponent<PredatorSearchFood> ().searchForFood (transform.position);
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
}