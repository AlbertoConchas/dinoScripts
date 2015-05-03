using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DinosaurReproduce : MonoBehaviour
{

    private bool requestResponded;
    private GameObject partner;
    private GameObject child;

    private List<GameObject> posiblePartner = new List<GameObject>();

    private System.Random random = new System.Random();

    public void findPartner()
    {
       startElection();
    }

    public void selectPartner()
    {
		posiblePartner.Clear ();
        //Find the other in state of Reproduce
        foreach (GameObject dino in gameObject.GetComponent<Dinosaur>().getHerd())
        {
			if (dino.GetComponent<Dinosaur>().state != Dinosaur.States.Die && dino.GetComponent<Dinosaur>().priority == Dinosaur.Priorities.Reproduce ) 
            {
                //If is female
                if (GetComponent<Dinosaur>().female && !dino.GetComponent<Dinosaur>().female)
                {
                    posiblePartner.Add(dino);
                }
                else
                {
                    //If is a male
                    if (!GetComponent<Dinosaur>().female && dino.GetComponent<Dinosaur>().female)
                    {
                        posiblePartner.Add(dino);
                    }
                }
            }
        }
		
		//Debug.Log(posiblePartner.Count);
        if (posiblePartner.Count >= 1)
        {
            int num = random.Next(0, posiblePartner.Count);
            partner= posiblePartner[num];

        }
        else {
            partner = null;
        }


        if (partner != null)
        {
            startReproduction();
            unbecomeReproduce();
			gameObject.GetComponent<Dinosaur>().state = Dinosaur.States.Waiting;
        }
        else {
            unbecomeReproduce();
			//Debug.Log(" No se reprodujo");
			gameObject.GetComponent<Dinosaur>().state = Dinosaur.States.Waiting;
        }

            return;
    }


    /**
     * Consegui ser pareja, crea la luz encima de el
     **/
    private void startReproduction()
    {
     
        if (GetComponent<Dinosaur>().female)
        {
            crossover(GetComponent<Dinosaur>().crossover);
            mutation(GetComponent<Dinosaur>().mutation);
        }
    }

    private void mutation(float p)
    { 
        //Genotipe Uniform
       

        //Velocidad de la entidad
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().speed = random.Next(6, 10);
        }

        //Rango de comunicacion
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().comRange = random.Next(8,12);
        }

        //Resistencia (nesesaria para correr etc....)
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().stamina = random.Next(100, 110);
        }

        //Tiempo de vida
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().lifetime = random.Next(540, 720);
        }

        //Daño que realiza la entidad
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().attack = random.Next(6,16);
        }
    }


    private void crossover(float a)
    {
        String path = "";
        //Depredator
        if (GetComponent<Dinosaur>().name == "carnotaurus")
        {
            path = "Assets/My Assets/Prefab/Depredator/carnotaurus.prefab";
        }else
            if (GetComponent<Dinosaur>().name == "spinosaurus")
            {
                path = "Assets/My Assets/Prefab/Depredator/spinosaurus.prefab";
            }
            else
                if (GetComponent<Dinosaur>().name == "tiranosaurus")
                {
                    path = "Assets/My Assets/Prefab/Depredator/tiranosaurus.prefab";
                }
                else
                    if (GetComponent<Dinosaur>().name == "velociraptor")
                    {
                        path = "Assets/My Assets/Prefab/Depredator/velociraptor.prefab";
                    }
                    else //Prey
                        if (GetComponent<Dinosaur>().name == "ankylosaurus")
                        {
                            path = "Assets/My Assets/Prefab/Prey/ankylosaurus.prefab";
                        }
                        else
                            if (GetComponent<Dinosaur>().name == "parasaurolophus")
                            {
                                path = "Assets/My Assets/Prefab/Prey/parasaurolophus.prefab";
                            }
                            else
                                if (GetComponent<Dinosaur>().name == "stegosaurus")
                                {
                                    path = "Assets/My Assets/Prefab/Prey/stegosaurus.prefab";
                                }
                                else
                                    if (GetComponent<Dinosaur>().name == "triceratops")
                                    {
                                        path = "Assets/My Assets/Prefab/Prey/triceratops.prefab";
                                    }
                   


        child = (GameObject)Instantiate(Resources.LoadAssetAtPath(path, typeof(GameObject)), partner.GetComponent<Rigidbody>().position, Quaternion.identity);
        child.name = GetComponent<Dinosaur>().name;
        child.transform.parent = transform.parent;
        child.GetComponent<Dinosaur>().transform.localScale = new Vector3((float)(0.5 * child.GetComponent<Dinosaur>().transform.localScale.x),
                                                                           (float)(0.5 * child.GetComponent<Dinosaur>().transform.localScale.y),
                                                                            (float)(0.5 * child.GetComponent<Dinosaur>().transform.localScale.z));

        //Recombination Floating-Point Arithmetic Recombination

        //Salud de la entidad
        child.GetComponent<Dinosaur>().hp = (a * GetComponent<Dinosaur>().hp) + ((1 - a) * partner.GetComponent<Dinosaur>().hp);

        //Velocidad de la entidad
        child.GetComponent<Dinosaur>().speed = (int)((a * GetComponent<Dinosaur>().speed) + ((1 - a) * partner.GetComponent<Dinosaur>().speed));

        //Rango de comunicacion
        child.GetComponent<Dinosaur>().comRange = (int)((a * GetComponent<Dinosaur>().comRange) + ((1 - a) * partner.GetComponent<Dinosaur>().comRange));

        //Resistencia (nesesaria para correr etc....)
        child.GetComponent<Dinosaur>().stamina = (a * GetComponent<Dinosaur>().stamina) + ((1 - a) * partner.GetComponent<Dinosaur>().stamina);

        //Tiempo de vida
        child.GetComponent<Dinosaur>().lifetime = (a * GetComponent<Dinosaur>().lifetime) + ((1 - a) * partner.GetComponent<Dinosaur>().lifetime);

        //Daño que realiza la entidad
        child.GetComponent<Dinosaur>().attack = (a * GetComponent<Dinosaur>().attack) + ((1 - a) * partner.GetComponent<Dinosaur>().attack);

		//Defense
		child.GetComponent<Dinosaur>().defense = (a * GetComponent<Dinosaur>().defense) + ((1 - a) * partner.GetComponent<Dinosaur>().defense);

		//Female
        if (random.Next(0, 100) < 50)
        {
            child.GetComponent<Dinosaur>().female = true;
        }
        else
        {
            child.GetComponent<Dinosaur>().female = false;
        }

        //La energia de la mama decremente despues de dar a luz

        GetComponent<Dinosaur>().stamina = GetComponent<Dinosaur>().stamina - 5;

        List<GameObject> momHerd = GetComponent<Dinosaur>().herd;
        List<GameObject> childHerd = new List<GameObject>(momHerd);

        //Agregar al hijo la manada de la madre
        child.GetComponent<Dinosaur>().herd = childHerd;
        child.GetComponent<Dinosaur>().herd.Add(gameObject);


        // Agrega al hijo a la cada miembro de la manada
        foreach (GameObject o in momHerd)
        {
            o.GetComponent<Dinosaur>().herd.Add(child);
        }

        //Agregar a la manada de la mama el hijo
        momHerd.Add(child);

        //Si el hijo es mejor que el lider actual
        if (child.GetComponent<Dinosaur>().getLeadershipStat() > GetComponent<Dinosaur>().getLeader().GetComponent<Dinosaur>().getLeadershipStat())
        {
            //Soy mi propio lider y destrono al anterior
            child.GetComponent<Dinosaur>().setLeader(child);
            GetComponent<Dinosaur>().getLeader().GetComponent<LeaderChoosing>().unbecomeLeader();
            child.GetComponent<LeaderChoosing>().becomeLeader();
                        
            //Informarlo a la manada
            foreach (GameObject o in child.GetComponent<Dinosaur>().herd)
            {
                o.GetComponent<Dinosaur>().setLeader(child);
            }


        }
        
    }

    private float generateRandom(){
        return (random.Next(0,100)/100);
    
    }
    private void startElection()
    {
        becomeReproduce();
        selectPartner();
    }





    /**
    * Consegui ser lider, crea la luz encima de el
    **/
    public void becomeReproduce()
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
            brigth.light.color = Color.magenta;
            brigth.light.range = 15.0f;
            brigth.light.spotAngle = 20.0f;
            brigth.light.intensity = 1.20f;
        }
        else
        {
            brigth = t.gameObject;
        }
    }


    /*
    * Otro dino me quito el liderazgo
    */
    public void unbecomeReproduce()
    {
        //encuentra el objeto al que se le agregara la luz
        Transform t = gameObject.transform.Find("shine");
        if (t == null) return;
        else
        {
            Destroy(t.gameObject);
        }
    }




}