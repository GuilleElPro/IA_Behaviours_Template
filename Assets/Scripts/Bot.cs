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

    //Wander parameters
    public float wanderRadius = 2f;
    public float wanderDistance = 3f;
    public float wanderJitter = 1f;

    //Hide parameters
    public GameObject[] obstacles;
    public float hideDistance = 2f;

    //Vision parameters
    public float visionAngle = 60f;
    public float visionDistance = 20f;
    public float detectionRange = 30f;

    //Complex parameters
    public float hideCooldown = 3f;

    enum ComplexState
    {
        Wander,
        Pursue,
        Hide
    }

    ComplexState currentState = ComplexState.Wander;
    float stateTimer = 0f;

    enum ManualBehavior
    {
        None,
        Hide,
        Pursue,
        Evade,
        Wander
    }

    ManualBehavior currentBehavior = ManualBehavior.None;
    bool complexActive = false;



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
        // Calculamos la distancia al target
        float distance = Vector3.Distance(
         target.transform.position, transform.position);

        // Calculamos el tiempo de prediccion
        float futureTime = distance / agent.speed;

        // Predecimos la posicion futura del target
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
        // Modificar aleatoriamente el punto
        wanderTarget += new Vector3(
            Random.Range(-1f, 1f) * wanderJitter,
            0,
            Random.Range(-1f, 1f) * wanderJitter
        );

        // Mantener el punto dentro del circulo
        wanderTarget = wanderTarget.normalized * wanderRadius;

        // Calcular el punto delante del agente
        Vector3 targetPos =transform.position + agent.velocity.normalized * wanderDistance + wanderTarget;

        // Ir hacia ese punto
        Seek(targetPos);
    }

    void Hide()
    {
        //TODO
        float distance = Mathf.Infinity;
        Vector3 bestHidingSpot = Vector3.zero;

        foreach (GameObject obstacle in obstacles)
        {
            // Calculamos la dirección desde el policía hacia el obstáculo
            Vector3 direction = (obstacle.transform.position - target.transform.position).normalized;

            // Calculamos un punto detrás del obstáculo
            Vector3 hidingSpot = obstacle.transform.position + direction * hideDistance;

            // Comprobamos cuál es el punto de escondite más cercano al bot
            float currentDistance = Vector3.Distance(transform.position, hidingSpot);

            if (currentDistance < distance)
            {
                distance = currentDistance;
                bestHidingSpot = hidingSpot;
            }
        }

        Seek(bestHidingSpot);
    }

    void CleverHide()
    {
        //TODO
        float distance = Mathf.Infinity;
        Vector3 bestHidingSpot = Vector3.zero;
        bool foundHidingSpot = false;

        foreach (GameObject obstacle in obstacles)
        {
            Vector3 direction = (obstacle.transform.position - target.transform.position).normalized;

            Vector3 hidingSpot = obstacle.transform.position + direction * hideDistance;

            // Comprobamos si el obstáculo tapa al ladrón desde el policía
            Vector3 directionToBot = transform.position - target.transform.position;

            RaycastHit hit;

            if (Physics.Raycast(target.transform.position, directionToBot.normalized, out hit, directionToBot.magnitude))
            {
                if (hit.transform.gameObject == obstacle)
                {
                    float currentDistance = Vector3.Distance(transform.position, hidingSpot);

                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                        bestHidingSpot = hidingSpot;
                        foundHidingSpot = true;
                    }
                }
            }
        }

        if (foundHidingSpot)
        {
            Seek(bestHidingSpot);
        }
        else
        {
            Hide();
        }
    }


    bool CanSeeTarget()
    {
        //TODO
        // Comprobar si el ladron puede ver al policia

        Vector3 direction = target.transform.position - transform.position;

        if (direction.magnitude > visionDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > visionAngle / 2f)
            return false;

        RaycastHit hit;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            direction.normalized,
            out hit, visionDistance))
        {
            return hit.transform == target.transform ||
                   hit.transform.IsChildOf(target.transform);
        }

        return false;
    }

    //En el siguiente comportamiento el robber se acerca al cop mientras este no le mira pero, cuando esto sucede, el robber se esconde.
    //Tenemos que decidir cuando el cop ve al rubber, digamos que esto sucede si esta en su angulo de vision (p.e. 60�)
    bool TargetCanSeeMe()
    {
        //TODO
        // Comprobar si el policia puede ver al ladron

        Vector3 direction = transform.position - target.transform.position;

        if (direction.magnitude > visionDistance)
            return false;

        float angle = Vector3.Angle(target.transform.forward, direction);

        if (angle > visionAngle / 2f)
            return false;

        RaycastHit hit;

        if (Physics.Raycast(
            target.transform.position + Vector3.up,
            direction.normalized,
            out hit, visionDistance))
        {
            return hit.transform == transform ||
                   hit.transform.IsChildOf(transform);
        }

        return false;
    }

    bool IsTargetInRange()
    {
        // Comprobar si el policia esta cerca del ladron

        float distance = Vector3.Distance(
            transform.position, target.transform.position);

        return distance < detectionRange;
    }


    // Update is called once per frame
    void Update()
    {

        if (target == null)
            return;

        // Calcular velocidad del policia
        targetVelocity = (target.transform.position - lastTargetPosition) / Time.deltaTime;

        lastTargetPosition = target.transform.position;

       
        // Seleccionar comportamiento

        if (Input.GetKeyDown(KeyCode.H))
        {
            StopComplex();
            currentBehavior = ManualBehavior.Hide;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StopComplex();
            currentBehavior = ManualBehavior.Pursue;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            StopComplex();
            currentBehavior = ManualBehavior.Evade;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StopComplex();
            currentBehavior = ManualBehavior.Wander;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            complexActive = !complexActive;
            currentState = ComplexState.Wander;
            stateTimer = 0f;
            currentBehavior = ManualBehavior.None;
        }

        // Ejecutar Complex

        if (complexActive)
        {
            stateTimer -= Time.deltaTime;

            switch (currentState)
            {
                case ComplexState.Wander:

                    if (IsTargetInRange())
                        currentState = ComplexState.Pursue;
                    else
                        Wander();

                    break;

                case ComplexState.Pursue:

                    if (!IsTargetInRange())
                    {
                        currentState = ComplexState.Wander;
                    }
                    else if (TargetCanSeeMe())
                    {
                        currentState = ComplexState.Hide;
                        stateTimer = hideCooldown;
                        Evade();
                    }
                    else
                    {
                        Pursue();
                    }

                    break;

                case ComplexState.Hide:

                    CleverHide();

                    if (stateTimer <= 0f && !TargetCanSeeMe())
                    {
                        if (IsTargetInRange())
                            currentState = ComplexState.Pursue;
                        else
                            currentState = ComplexState.Wander;
                    }

                    break;
            }
        }
        else
        {
            // Ejecutar comportamiento seleccionado

            switch (currentBehavior)
            {
                case ManualBehavior.Hide:
                    Hide();
                    break;

                case ManualBehavior.Pursue:
                    Pursue();
                    break;

                case ManualBehavior.Evade:
                    Evade();
                    break;

                case ManualBehavior.Wander:
                    Wander();
                    break;
            }
        }
       
    }
    void StopComplex()
    {
        complexActive = false;
    }

}