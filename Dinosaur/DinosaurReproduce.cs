﻿using UnityEngine;
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
        if (GetComponent<Dinosaur>().female)
            startElection();
    }


    public void selectPartner()
    {
        posiblePartner.Clear();
        //Find the other in state of Reproduce
        foreach (GameObject dino in gameObject.GetComponent<Dinosaur>().getHerd())
        {
            if (dino.GetComponent<Dinosaur>().state != Dinosaur.States.Die && dino.GetComponent<Dinosaur>().priority == Dinosaur.Priorities.Reproduce)
            {
                //If is female
                if (dino != null && !dino.GetComponent<Dinosaur>().female && dino.GetComponent<Dinosaur>().repLapse <= 0)
                {
                    posiblePartner.Add(dino);
                }
                /*else
                {
                    //If is a male
                    if (!GetComponent<Dinosaur>().female && dino.GetComponent<Dinosaur>().female && dino.GetComponent<DinosaurReproduce>().partner == null)
                    {
                        posiblePartner.Add(dino);
                    }
                }*/
            }
        }

        //Debug.Log(posiblePartner.Count);
        if (posiblePartner.Count >= 1)
        {
            int num = random.Next(0, posiblePartner.Count);
            partner = posiblePartner[num];

        }
        else
        {
            partner = null;
        }


        if (partner != null)
        {
            //say
            partner.SendMessage("letsMakeAChild", gameObject);
            gameObject.GetComponent<Dinosaur>().repLapse = 60;
        }
    }

    public void Reproduce()
    {
        startReproduction();
        unbecomeReproduce();
        partner = null;
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
            child.GetComponent<Dinosaur>().comRange = random.Next(8, 12);
        }

        //Resistencia (nesesaria para correr etc....)
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().stamina = random.Next(100, 110);
        }


        //Daño que realiza la entidad
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().attack = random.Next(6, 16);
        }
    }


    private void crossover(float a)
    {
        String path = "";
        //Depredator
        if (GetComponent<Dinosaur>().name == "carnotaurus")
        {
            path = "Assets/My Assets/Prefab/Depredator/carnotaurus.prefab";
        }
        else
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


        Vector3 pos = new Vector3(GetComponent<Rigidbody>().position.x + 5f, GetComponent<Rigidbody>().position.y+5f, GetComponent<Rigidbody>().position.z);
        child = (GameObject)Instantiate(Resources.LoadAssetAtPath(path, typeof(GameObject)), pos, Quaternion.identity);
        var dinoChild = child.GetComponent<Dinosaur>();
        var mama = GetComponent<Dinosaur>();


        child.name = mama.name;
        child.transform.parent = transform.parent;
        dinoChild.transform.localScale = new Vector3((float)(0.5 * dinoChild.transform.localScale.x),
                                                                           (float)(0.5 * dinoChild.transform.localScale.y),
                                                                            (float)(0.5 * dinoChild.transform.localScale.z));

        //Recombination Floating-Point Arithmetic Recombination

        //Salud de la entidad
        dinoChild.hp = (a * GetComponent<Dinosaur>().hp) + ((1 - a) * partner.GetComponent<Dinosaur>().hp);

        //Velocidad de la entidad
        dinoChild.speed = (int)((a * GetComponent<Dinosaur>().speed) + ((1 - a) * partner.GetComponent<Dinosaur>().speed));

        //Rango de comunicacion
        dinoChild.comRange = (int)((a * GetComponent<Dinosaur>().comRange) + ((1 - a) * partner.GetComponent<Dinosaur>().comRange));

        //Resistencia (nesesaria para correr etc....)
        dinoChild.stamina = (a * GetComponent<Dinosaur>().stamina) + ((1 - a) * partner.GetComponent<Dinosaur>().stamina);

        //Tiempo de vida
        dinoChild.lifetime = 0;

        //Daño que realiza la entidad
        dinoChild.attack = (a * GetComponent<Dinosaur>().attack) + ((1 - a) * partner.GetComponent<Dinosaur>().attack);

        //Defense
        dinoChild.defense = (a * GetComponent<Dinosaur>().defense) + ((1 - a) * partner.GetComponent<Dinosaur>().defense);

        //child.GetComponent<Dinosaur>().lifetime =  

        //Female
        if (random.Next(0, 100) < 50)
        {
            dinoChild.female = true;
        }
        else
        {
            dinoChild.female = false;
        }

        //La energia de la mama decremente despues de dar a luz

        GetComponent<Dinosaur>().stamina = GetComponent<Dinosaur>().stamina - 5;

        List<GameObject> momHerd = GetComponent<Dinosaur>().herd;
        List<GameObject> childHerd = new List<GameObject>(momHerd);

        //Agregar al hijo la manada de la madre
        dinoChild.herd = childHerd;
        dinoChild.herd.Add(gameObject);


        // Agrega al hijo a  cada miembro de la manada
        foreach (GameObject o in momHerd)
        {
            if (o != null && o.GetComponent<Dinosaur>().herd!=null) 
            { 
                o.GetComponent<Dinosaur>().herd.Add(child);
            }
        }

        //Agregar a la manada de la mama el hijo
        momHerd.Add(child);


        //Si el hijo es mejor que el lider actual
        if (dinoChild.getLeadershipStat() > GetComponent<Dinosaur>().getLeader().GetComponent<Dinosaur>().getLeadershipStat())
        {
            //Soy mi propio lider y destrono al anterior
            dinoChild.setLeader(child);
            GetComponent<Dinosaur>().getLeader().GetComponent<LeaderChoosing>().unbecomeLeader();
            child.GetComponent<LeaderChoosing>().becomeLeader();

            //Informarlo a la manada
            foreach (GameObject o in child.GetComponent<Dinosaur>().herd)
            {
                if (o != null)
                {
                    o.GetComponent<Dinosaur>().setLeader(child);
                }
            }


        }
        else
        {
            dinoChild.setLeader(GetComponent<Dinosaur>().getLeader());
        }


        //Classify the herd 

        GameObject leader = child.GetComponent<Dinosaur>().getLeader();
        leader.GetComponent<Dinosaur>().rank = "A";
        float pa;
        float pb;

        foreach (GameObject o in leader.GetComponent<Dinosaur>().herd)
        {

            float ml = ((o.GetComponent<Dinosaur>().getLeadershipStat() * 100) / leader.GetComponent<Dinosaur>().getLeadershipStat());

            float px = (ml * 0.2f * 0.4f) + (ml * 0.35f * 0.6f);

            pa = (ml * 0.2f * 0.4f * 0.4f) / px;

            pb = (ml * 0.35f * 0.6f * 0.6f) / px;


            if (pa > pb)
            {
                o.GetComponent<Dinosaur>().rank = "A";
            }
            else
            {
                o.GetComponent<Dinosaur>().rank = "B";
            }

        }

    }

    private float generateRandom()
    {
        return (random.Next(0, 100) / 100);

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
        Transform t = gameObject.transform.Find("shiner");
        GameObject brigth = null;
        if (t == null)
        {
            brigth = new GameObject("shiner");
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
        Transform t = gameObject.transform.Find("shiner");
        if (t == null) return;
        else
        {
            Destroy(t.gameObject);
        }
    }




}