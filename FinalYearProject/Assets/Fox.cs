using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Fox : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;


    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    //Persiot Predicts the preys future location
    void Persue()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        //If target is not moving
        if ((toTarget > 90 && relativeHeading <20 ) || target.GetComponent<NavMeshAgent>().velocity.magnitude < 0.01f)
        {
            Seek(target.transform.position);
            return;
        }


        float lookAhead = targetDir.magnitude / (agent.speed + target.GetComponent<NavMeshAgent>().speed);
        Seek(target.transform.position + target.transform.forward * lookAhead * 2.0f);
    }






    // Update is called once per frame
    void Update()
    {
        Persue();
    }
    
    
}
