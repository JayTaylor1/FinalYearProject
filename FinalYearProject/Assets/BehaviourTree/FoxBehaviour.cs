using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FoxBehaviour : MonoBehaviour
{
    BehaviourTree tree;
    NavMeshAgent agent;
    public GameObject target;
    public string Action = "Idle";

    public enum ActionState {IDLE, WORKING};
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;


    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();

        Selector FoxTree = new Selector("Fox"); 
        
        Sequence Chase = new Sequence("Chase");
        Sequence Wonder = new Sequence("Wonder");



        Leaf chasePrey = new Leaf("Chase Prey", ChasePrey);
        //Condition
        Leaf inRangeTarget = new Leaf("Target in Range?", TargetInRange);


        Leaf roam = new Leaf("Roam Freely", Roam);


        Chase.AddChild(inRangeTarget);
        Chase.AddChild(chasePrey);

        Wonder.AddChild(roam);


        FoxTree.AddChild(Chase);
        FoxTree.AddChild(Wonder);

        tree.AddChild(FoxTree);

        tree.PrintTree();


    }

    public GameObject GetClosestTarget()
    {
        GameObject[] Preys = GameObject.FindGameObjectsWithTag("rabbit");
        float dist = Mathf.Infinity;
        GameObject closestPrey = Preys[0];

        for (int i = 0; i < Preys.Length; i++)
        {
            if (Vector3.Distance(this.transform.position, Preys[i].transform.position) < dist)
            {
                closestPrey = Preys[i];
                dist = Vector3.Distance(this.transform.position, Preys[i].transform.position);
            }
        }
        return target;
    }

    public Node.Status TargetInRange()
    {
        target = GetClosestTarget();
        if (Vector3.Distance(this.transform.position, target.transform.position) < 10)
        {
            Action = "Chasing";
            return Node.Status.SUCCESS;
        }
        else if (Action == "Chasing")
        {
            Action = "Idle";
            agent.isStopped = true;
            return Node.Status.FAILED;
        }
        return Node.Status.FAILED;
    }

    public Node.Status GoToTarget()
    {
        return GoToLocation(target.transform.position);
    }


    public Node.Status ChasePrey()
    {
        Vector3 targetDir = target.transform.position - this.transform.position;
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        //If target is not moving
        if ((toTarget > 90 && relativeHeading < 20) || target.GetComponent<NavMeshAgent>().velocity.magnitude < 0.01f)
        {
            return GoToLocation(target.transform.position);

        }
        float lookAhead = targetDir.magnitude / (agent.speed + target.GetComponent<NavMeshAgent>().speed);
        return GoToLocation(target.transform.position + target.transform.forward * lookAhead * 2.0f);
    }


    Vector3 wanderTarget = Vector3.zero;
    public Node.Status Roam()
    {
        Action = "Roaming";
        float wanderRadius = 10;
        float wanderDistance = 10;
        float wanderJitter = 1;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter, 0, Random.Range(-1.0f, 1.0f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        return GoToLocation(targetWorld);
    }


    Node.Status GoToLocation(Vector3 destination)
    {
        float distanceToTarget = Vector3.Distance(destination, this.transform.position);
        if(state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILED;
        }
        else if (distanceToTarget < 2)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        return Node.Status.RUNNING;
    }


    // Update is called once per frame
    void Update()
    {

        treeStatus = tree.Process();

    }
}
