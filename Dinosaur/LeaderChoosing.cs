using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LeaderChoosing : MonoBehaviour {

	public float leadership;
	private bool requestResponded;
    private bool hasToBroadcast;
	private GameObject tempLeader;


    // Llamada a la votacion
    public void choose()
    {
        GetComponent<Dinosaur>().state = Dinosaur.States.ChoosingLeader;
        tempLeader = null;
        requestResponded = false;
        hasToBroadcast = false;
        leadership = gameObject.GetComponent<Dinosaur>().getLeadershipStat();

        StartCoroutine(startElection());
        StartCoroutine(endElection());
    }

    /**
     * Esperar un tiempo antes de empezar eleccion
     **/
    private IEnumerator startElection()
    {
        yield return new WaitForSeconds(.5f);
        sendElectionMessage();
        //StartCoroutine ("waitDeadTime");
    }


    /**
     * Les solicita a los que tienen mejor capacidad de liderazgo que si pueden ser lideres
     **/
    private void sendElectionMessage()
    {
        //por cada integrante en la manada (distinto de mi)
        foreach (GameObject dino in gameObject.GetComponent<Dinosaur>().getHerd())
        {
            //Si es mejor lider que yo
            if (leadership < dino.GetComponent<Dinosaur>().leadership && dino.GetComponent<Dinosaur>().state != Dinosaur.States.Die)
            {
                //Pidele que sea lider
                dino.SendMessage("leadershipRequest", gameObject);
            }
        }
    }

    /**
     * Me solicitan ser lider
     **/
    private void leadershipRequest(GameObject sender)
    {
        sender.SendMessage("leadershioRequestResponse");
        hasToBroadcast = true;
    }



    /**
     * Respuesta a la solicitud de lideresgo
     **/
    private void leadershioRequestResponse()
    {
        //Alguien acepto el cargo, no puedo ser yo
        requestResponded = true;
    }





    /**
     * Informar quien sera el lider
     **/
    private void BroadcastLeadership(GameObject leader)
    {
        if (tempLeader == null || tempLeader.GetComponent<Dinosaur>().leadership < leader.GetComponent<Dinosaur>().leadership)
        {
            tempLeader = leader;
            gameObject.GetComponent<Dinosaur>().BroadCast("BroadcastLeadership", tempLeader);
        }
    }


    


    /**
     * La eleccion termino, enviar quien sera el lider
     **/
    private IEnumerator endElection()
    {
        if (hasToBroadcast) BroadcastLeadership(gameObject);
        yield return new WaitForSeconds(2);
        if (requestResponded == false)
        {
            BroadcastLeadership(gameObject);

            //Espera 2 segundos por si alguien tambien quiere ser lider y tiene mejores capacidades que yo
            yield return new WaitForSeconds(3);
            GetComponent<Dinosaur>().setLeader(tempLeader);
        }
        else
        {
            yield return new WaitForSeconds(3);
            GetComponent<Dinosaur>().setLeader(tempLeader);
        }

        if (tempLeader != null) 
        { 
            // quita o pone la luz del lider
            if (tempLeader.GetInstanceID() == gameObject.GetInstanceID())
            {
                becomeLeader();
            }
            else
            {
                unbecomeLeader();
            }
        }
        GetComponent<Dinosaur>().state = Dinosaur.States.Waiting;
    }

    /**
    * Consegui ser lider, crea la luz encima de el
    **/
    public void becomeLeader()
    {

        //Crea el objeto al que se le agregara la luz
        Transform t = gameObject.transform.Find("leaderLigth");
        GameObject brigth = null;
        if (t == null)
        {
            brigth = new GameObject("leaderLigth");
            brigth.AddComponent(typeof(Light));							//se le agrega la luz

            brigth.transform.parent = transform;							//Se fija a la entidad


            brigth.light.type = LightType.Spot;								//Se elije el tipo de luz SPOT

            //Se pone la mira hacia abajo
            brigth.transform.position = brigth.transform.parent.position + new Vector3(0, 3.5f, 0);
            brigth.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));

            //Color, Alcance, Dispercion
            brigth.light.color = Color.white;
            brigth.light.range = 15.0f;
            brigth.light.spotAngle = 20.0f;
            brigth.light.intensity = 1.20f;
        }
        else
        {
            brigth = t.gameObject;
        }
        gameObject.GetComponent<Dinosaur>().isLeader = true;
    }

    /*
     * Otro dino me quito el liderazgo
     */
    public void unbecomeLeader()
    {
        //encuentra el objeto al que se le agregara la luz
        Transform t = gameObject.transform.Find("leaderLigth");
        if (t == null) return;
        else
        {
            Destroy(t.gameObject);
        }
        gameObject.GetComponent<Dinosaur>().isLeader = false;
    }

    internal void mergeHerd(List<GameObject> dinosDetected)
    {
        Dinosaur me = GetComponent<Dinosaur>();
        List<GameObject> myHerd = me.herd;

        // solo mezclar manadas cuando yo soy lider
        if (me.getLeader() != null && me.getLeader().GetInstanceID() == gameObject.GetInstanceID()) 
        {
            // filtrar quienes son los nuevos dinos
            foreach (GameObject friend in myHerd) 
            {
                if (dinosDetected.Contains(friend)) dinosDetected.Remove(friend);
            }

            //  si hay mas de algun dino nuevo
            if (dinosDetected.Count > 0) 
            {
                foreach (GameObject newDinoObject in dinosDetected) 
                { 
                    Dinosaur newDino = newDinoObject.GetComponent<Dinosaur>();

                    // el dino nuevo es lider de su manada y podemos unir manadas!
                    if (newDino.getLeader() != null && newDino.getLeader().GetInstanceID() == newDinoObject.GetInstanceID()) 
                    {
                        if (newDino.herd != null) 
                        {
                            newDino.herd.RemoveAll(item => item == null);
                            // agregar dinos externos y lider externo a mi manada
                            foreach (GameObject dinoObject in me.herd) 
                            {
                                if (dinoObject != null) 
                                { 
                                    Dinosaur dino = dinoObject.GetComponent<Dinosaur>();
                                    dino.herd.AddRange(newDino.herd);
                                    dino.herd.Add(newDinoObject);
                                }
                            }
                        }

                        if (me.herd != null) 
                        {
                            me.herd.RemoveAll(item => item == null);
                            // agregar a mi y mis dinos a manada externa
                            foreach (GameObject dinoObject in newDino.herd)
                            {
                                if (dinoObject != null)
                                {
                                    Dinosaur dino = dinoObject.GetComponent<Dinosaur>();
                                    dino.herd.AddRange(me.herd);
                                    dino.herd.Add(gameObject);
                                    dino.setLeader(gameObject);
                                }
                            }
                        }

                        List<GameObject> tempHerd = new List<GameObject>(me.herd);
                        
                        // agregar dinos externos a mi
                        me.herd.AddRange(newDino.herd);
                        me.herd.Add(newDinoObject);

                        // agregar mis dinos a lider externo
                        newDino.herd.AddRange(tempHerd);
                        newDino.herd.Add(gameObject);
                        newDino.setLeader(gameObject);
                        newDinoObject.GetComponent<LeaderChoosing>().unbecomeLeader();

                        return;
                    }
                }
            }
        }
    }
}
