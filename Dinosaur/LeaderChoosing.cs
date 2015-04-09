using UnityEngine;
using System.Collections;

public class LeaderChoosing : MonoBehaviour {

	public float leadership;
	private bool requestResponded;
	private GameObject tempLeader;


    // Llamada a la votacion
    public void choose()
    {

        leadership = gameObject.GetComponent<Dinosaur>().getLeadershipStat();

        StartCoroutine(startElection());
        StartCoroutine(endElection());
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
            if (leadership < dino.GetComponent<Dinosaur>().leadership)
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
     * Consegui ser lider, crea la luz encima de el
     **/
    private void becomeLeader()
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
            brigth.transform.position = brigth.transform.parent.position + new Vector3(0, 3, 0);
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
    }

    /**
     * Informar quien sera el lider
     **/
    private void BroadcastLeadership(GameObject leader)
    {
        if (tempLeader != null && tempLeader.GetInstanceID() == leader.GetInstanceID())
        {
            return;
        }
        if (tempLeader == null || tempLeader.GetComponent<Dinosaur>().leadership < leader.GetComponent<Dinosaur>().leadership)
        {
            tempLeader = leader;
            gameObject.GetComponent<Dinosaur>().BroadCast("BroadcastLeadership", tempLeader);
        }
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
     * La eleccion termino, enviar quien sera el lider
     **/
    private IEnumerator endElection()
    {
        yield return new WaitForSeconds(3);
        if (requestResponded == false)
        {
            BroadcastLeadership(gameObject);

            //Espera 2 segundos por si alguien tambien quiere ser lider y tiene mejores capacidades que yo
            yield return new WaitForSeconds(2);
            if (tempLeader == null)
                tempLeader = gameObject;
            GetComponent<Dinosaur>().setLeader(tempLeader);
            if (tempLeader.GetInstanceID() == gameObject.GetInstanceID() && !requestResponded)
                becomeLeader();
        }
        else
        {
            yield return new WaitForSeconds(2);
            GetComponent<Dinosaur>().setLeader(tempLeader);
        }

    }

	
}
