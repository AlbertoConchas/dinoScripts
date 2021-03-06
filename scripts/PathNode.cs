﻿using UnityEngine;
using System.Collections;


public class PathNode : MonoBehaviour {

	public GameObject[] nodeNeighbors;
    private string[] plants = { "Bush5", "Shrub_01", "Shrub_02", "Shrub_03", "Shrub_04", "Weeds_01", "Weeds_02", "Weeds_04","Weeds_05","tree" };

	public Font font;
    public GameObject forest;

	public float ratius;

	private float fertility;
	private float linkVisibleDuration = 40f;
	private TextMesh textMesh;
	private Light nodeLight;
	private int maxTrees;

    public float getFertility()
    {
        return fertility;
    }

	// Use this for initialization
	void Start () {
		//El radio se usa para delimitar que objeto le pertenece a que PATHNODE
		if ( ratius == 0 ) 
			ratius = transform.localScale.x;

		//Dibuja lineas hacia los otrs path Nodes
		for (int i =0; i < this.nodeNeighbors.Length; i++) {
			Debug.DrawLine(transform.position, nodeNeighbors[i].transform.position,Color.green, linkVisibleDuration);
		}
 
		//inicializa que tan rapido nacen los arboles y cuantos puede haber en cada PATHNODE
		fertility = Random.Range (0, 30);
		maxTrees = Random.Range (5, 15);


		//Inicializa el objeto que despliega el texto
		textMesh = (TextMesh) transform.Find("Capsule").gameObject.AddComponent("TextMesh");
		textMesh.font = font;
		textMesh.fontSize = 0;

		//Obtiene el numero de comida en las plantas actuales en el PATHNODE
		int c = getPlants ();
		textMesh.text = "Comida = " + c;

		//Inicializa el objeto luz y le da color
		nodeLight = ((Light)transform.Find ("Capsule").gameObject.GetComponent ("Light"));
		nodeLight.color = getNodeColor( c );
		nodeLight.intensity = 3;
	}
	
	// Update is called once per frame
	void Update () {
		int c = getPlants ();
		//Cambia el color de la luz
		nodeLight.color = getNodeColor( c );

		//Cambia el texto y lo gira hacia la camara
		textMesh.text = "Comida = " + c;
		transform.LookAt( 2 * transform.position - Camera.main.transform.position );

		//Aleatoriamente decide si planta o no un arbol
		if (Random.Range (0f, 4000f) < fertility) {
			if ( getNumberOfTrees() < maxTrees ) {
				plantTree();
			}
		}
	}


	/*
	 * Funcion que Retorna el color que debe tener la luz, de acuerdo a la cantidad de comida
	 */
	private Color getNodeColor(int plants){
		if( plants <= 33 ) 
			return Color.red;
		if( plants <= 66 ) 
			return Color.yellow;
		return Color.green;
	}

	/*
	 * Regresa los nodos aledaños a este nodo
	 */
	public GameObject[] getNeighbors(){
		return nodeNeighbors;
	}


	/*
	 * Regresa la cantidad de comida en los arboles que del nodo
	 */
	public int getPlants(){
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.ratius);

		//Valor MINIMO, Agregado para evitar errores de matriz vacia en el algoritmo de FUZZY LOGIC
		float plants = 10;

		//Por cada objeto encontrado revisa si es un arbol y añada su cantidad de comida
		for (int i = 0; i < hitColliders.Length; i++) {
			if(hitColliders[i].gameObject.tag == "Tree"){
				plants += hitColliders[i].GetComponent<Plant>().flesh;
			}
		}
		return (int)plants/10;
	}

	public int getNumberOfTrees(){
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.ratius);

			//Valor MINIMO, Agregado para evitar errores de matriz vacia en el algoritmo de FUZZY LOGIC
			int plants = 1;
			for (int i = 0; i < hitColliders.Length; i++) {
				if(hitColliders[i].gameObject.tag == "Tree"){
					plants++;
				}
			}
			return plants;
	}

	public int getPrays(){
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.ratius);

		//Valor MINIMO, Agregado para evitar errores de matriz vacia en el algoritmo de FUZZY LOGIC
		int prays = 0;
		for (int i = 0; i < hitColliders.Length; i++) {
			
			//Si es un velocirraptor
			if(hitColliders[i].GetComponent<Prey>() != null){
				prays++;
			}
		}
		return prays;
	}


	public int getPredators(){
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, this.ratius);
		
		//Por cada objeto encontrado
		int predators = 0;
		for (int i = 0; i < hitColliders.Length; i++) {
			
			//Si es un velocirraptor
			if(hitColliders[i].GetComponent<Predator>() != null){
				predators++;
			}
		}
		return predators;
	}


	private void plantTree(){
		Vector3 randPos = randomPosition();

        int num = Random.Range(0, plants.Length); ;
        string plant = plants[num];


        GameObject g = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/My Assets/Prefab/"+plant+".prefab", typeof(GameObject)), randPos , Quaternion.identity);
      //  GameObject g = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/My Assets/Prefab/tree.prefab", typeof(GameObject)), randPos, Quaternion.identity);
      	g.name = "tree";
        g.transform.parent = forest.transform;


        if (Random.Range(0, 1000) < 20) {
            GameObject x = (GameObject)Instantiate(Resources.LoadAssetAtPath("Assets/My Assets/Prefab/VolumeSteam.prefab", typeof(GameObject)), randPos, Quaternion.identity);
            x.name = "niebla";
            x.transform.parent = forest.transform;
        }

	}

	/*
	 * Regresa una psicion aleatoria dento del nodo, NO REVISA SI SE ENCUENTRA ALGO EN ESA POCICION ACTUALMENTE
	 */
	private Vector3 randomPosition(){
		Vector2 r = (Random.insideUnitCircle * ratius)/2;
		return transform.position + new Vector3 (r.x, 0, r.y);
	}
}
