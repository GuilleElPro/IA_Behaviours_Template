using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IncompletoBot : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target; //target sera el cop que lo pondremos en Unity

    Vector3 targetVelocity = Vector3.zero;
    Vector3 lastTargetPosition;

    private Vector3 steering;
    private Vector3 desired;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        lastTargetPosition = target.transform.position;
    }

    void Seek(Vector3 location)
    {
        //TODO
        
        agent.SetDestination(location);
    }
    //En seek ponemos como destino la posicion del target, en Flee (huir) ponemos como
    //destino la posicion del target en negativo para que vaya huyendo de esa posicion
    void Flee(Vector3 location)
    {
        //TODO

        Vector3 direction = (transform.position - location).normalized;
        Vector3 destination = transform.position + direction * 10f;
        agent.SetDestination(destination);
    }

    void Pursue()
    {
        //TODO
        float distance = Vector3.Distance(
         target.transform.position, transform.position);

        float futureTime = distance / agent.speed;

        Vector3 futurePosition =
            target.transform.position + targetVelocity * futureTime;

        Seek(futurePosition);
    }

    //Evade es practicamente igual a Pursue solo que se debe ir en la direccion contraria
    void Evade()
    {
        //TODO
        float distance = Vector3.Distance(target.transform.position, transform.position);

        float futureTime = distance / agent.speed;

        Vector3 futurePosition = target.transform.position + targetVelocity * futureTime;

        Flee(futurePosition);
    }


	//Para Wander necesitamos una var. que recordemos en cada llamada y por lo
	//tanto no puede ser local al metodo.
    Vector3 wanderTarget = Vector3.zero; //Se actualiza cada vez que el agente
										 //tiene un nuevo valor en su posicion
    void Wander()
    {
        //TODO
    }

    void Hide()
    {
        //TODO
    }

    void CleverHide()
    {
        //TODO
    }


    bool CanSeeTarget()
    {
        //TODO
        return false;
    }

    //En el siguiente comportamiento el robber se acerca al cop mientras este no le mira pero, cuando esto sucede, el robber se esconde.
    //Tenemos que decidir cuando el cop ve al rubber, digamos que esto sucede si esta en su angulo de vision (p.e. 60�)
    bool TargetCanSeeMe()
    {
        //TODO
        return false;
    }


    // Update is called once per frame
    void Update()
    {
        targetVelocity = (target.transform.position - lastTargetPosition) / Time.deltaTime;

        lastTargetPosition = target.transform.position;
        //Seek(target.transform.position);
        //Flee(target.transform.position);
        Pursue();
        //Evade();
    }
}