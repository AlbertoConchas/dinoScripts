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
    //Probability of crossover between the parents
    private float a = 0.5f;

    //Probability of mutation 
    private float p = 0.4f;

    private System.Random random = new System.Random();

    public void findPartner()
    {
        StartCoroutine(startElection());
    }

    public void selectPartner()
    {

        //Find the other in state of Reproduce
        foreach (GameObject dino in gameObject.GetComponent<Dinosaur>().getHerd())
        {
            if (dino.GetComponent<Dinosaur>().state == Dinosaur.States.Reproduce)
            {
                //If is female
                if (GetComponent<Dinosaur>().female && !dino.GetComponent<Dinosaur>().female)
                {
                    partner = dino;
                }
                else
                {
                    //If is a male
                    if (!GetComponent<Dinosaur>().female && dino.GetComponent<Dinosaur>().female)
                    {
                        partner = dino;
                    }
                }
            }
        }

        if (partner != null)
        {
            startReproduction();
        }
    }


    /**
     * Consegui ser pareja, crea la luz encima de el
     **/
    private void startReproduction()
    {
        if (GetComponent<Dinosaur>().female)
        {
            crossover();
            mutation();
        }
    }

    private void mutation()
    { 
        //Genotipe
       

       //Salud de la entidad
        if (generateRandom() < p) {
            child.GetComponent<Dinosaur>().hp = random.Next(50,700); 
        }

        //Nutricion aportada a quien se alimente
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().np = random.Next(10, 600);
        }


        //Velocidad de la entidad
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().speed = random.Next(2, 500);
        }


        //Rango de comunicacion
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().comRange = random.Next(500, 3000);
        }

        //Resistencia (nesesaria para correr etc....)
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().stamina = random.Next(10, 5000);
        }

        //Tiempo de vida
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().lifetime = random.Next(1000, 100000);
        }

        //Daño que realiza la entidad
        if (generateRandom() < p)
        {
            child.GetComponent<Dinosaur>().attack = random.Next(5, 7000);
        }
    }


    private void crossover()
    {
        String path = "";
        if (GetComponent<Dinosaur>().name == "stegosaurus")
        {
            path = "Assets/My Assets/stegosaurus.prefab";
        }


        child = (GameObject)Instantiate(Resources.LoadAssetAtPath(path, typeof(GameObject)), partner.GetComponent<Rigidbody>().position, Quaternion.identity);
        child.name = GetComponent<Dinosaur>().name;
        child.transform.parent = transform.parent;
        child.GetComponent<Dinosaur>().transform.localScale = new Vector3((float)(0.5 * child.GetComponent<Dinosaur>().transform.localScale.x),
                                                                           (float)(0.5 * child.GetComponent<Dinosaur>().transform.localScale.y),
                                                                            (float)(0.5 * child.GetComponent<Dinosaur>().transform.localScale.z));

        //Recombination

        //Salud de la entidad
        child.GetComponent<Dinosaur>().hp = (a * GetComponent<Dinosaur>().hp) + ((1 - a) * partner.GetComponent<Dinosaur>().hp);

        //Nutricion aportada a quien se alimente de la entidad
        child.GetComponent<Dinosaur>().np = (int)((a * GetComponent<Dinosaur>().np) + ((1 - a) * partner.GetComponent<Dinosaur>().np));

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

        //Female
        if ((random.Next(0, 100)/100) < 0.5)
        {
            child.GetComponent<Dinosaur>().female = true;
        }
        else
        {
            child.GetComponent<Dinosaur>().female = false;
        }
    
    
    }

    private float generateRandom(){
        return (random.Next(0,100)/100);
    
    }
    private IEnumerator startElection()
    {
        selectPartner();
        yield return new WaitForSeconds(5);
    }

}