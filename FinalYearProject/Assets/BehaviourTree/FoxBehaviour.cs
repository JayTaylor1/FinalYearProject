using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FoxBehaviour : MonoBehaviour
{
    BehaviourTree tree;
    NavMeshAgent agent;
    GameObject target;
    public GameObject home;
    public string Action = "Idle";

    GameObject SceneManager;
    public float timeofday;

    public enum ActionState {IDLE, WORKING};
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;


    // Start is called before the first frame update
    void Start()
    {
        SceneManager = GameObject.Find("SceneManagement");
        agent = this.GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();                                                 //Root Node

        Selector FoxTree = new Selector("Fox");                                  //  Fox (Selector)

        Sequence Chase = new Sequence("Chase");                                     //      Prey in Range (Sequence)
        Leaf inRangeTarget = new Leaf("Target in Range?", TargetInRange);           //          Sensed Prey (Condition)
        Selector sensedPrey = new Selector("Sensed Prey");                          //          Prey Sensed (Selector)
        Sequence CanSeePrey = new Sequence("Can See Prey");                         //              Can See Prey (Sequenmce)
        Leaf preyInSight = new Leaf("Prey in sight?", isPreyNotinSight);            //                  Is prey (not) in sight? (Condition)
        Leaf goLastSeen = new Leaf("Go to last seen", gotoLastSeen);                //                  Go to last seen location (Action)

        Selector preyClose = new Selector("Prey Close Enough");                     //              Prey Close enough to eat (Selector)
        Sequence canEat = new Sequence("Can Eat Prey");                             //                  Prey Close enough to be eaten (Sequence)
        Leaf PreyEatable = new Leaf("Is Prey Close enough to eat", isPreyEatable);  //                      Is prey close enough to eat (Condition)
        Leaf EatPrey = new Leaf("Eat Prey", eatPrey);                               //                      Eat the Prey (Action)

   
        Leaf chasePrey = new Leaf("Chase Prey", ChasePrey);                         //                  Chase Prey (Action)

        Sequence sleep = new Sequence("if Night Time");                             //      If night time (Sequence)
        Leaf IsNightTime = new Leaf("Check if Night", isNightTime);                 //          Check if Night time (condition)    
        Leaf ReturnHome = new Leaf("Return Home", returnHome);                      //          Go Home(Action) 


        Sequence Wonder = new Sequence("Wonder");                                   //      Wonder (Sequence)
        Leaf roam = new Leaf("Roam Freely", Roam);                                  //          Roam (Action)

        


        CanSeePrey.AddChild(preyInSight);
        CanSeePrey.AddChild(goLastSeen);

        canEat.AddChild(PreyEatable);
        canEat.AddChild(EatPrey);

        preyClose.AddChild(canEat);
        preyClose.AddChild(chasePrey);

        sensedPrey.AddChild(CanSeePrey);
        sensedPrey.AddChild(preyClose);

        Chase.AddChild(inRangeTarget);
        Chase.AddChild(sensedPrey);


        sleep.AddChild(IsNightTime);
        sleep.AddChild(ReturnHome);




        Wonder.AddChild(roam);

        FoxTree.AddChild(Chase);
        FoxTree.AddChild(sleep);
        FoxTree.AddChild(Wonder);

        tree.AddChild(FoxTree);
        

        tree.PrintTree();


    }

    public GameObject GetClosestTarget()
    {
        GameObject[] Preys = GameObject.FindGameObjectsWithTag("rabbit");
        if (Preys.Length == 0)
        {
            return null;
        }
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
        return closestPrey;
    }

    public Node.Status TargetInRange()
    {
        target = GetClosestTarget();
        if (target == null)
        {
            return Node.Status.FAILED;
        }
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

    public Node.Status isNightTime()
    {
        if (getTimeofDay() >= 22f || getTimeofDay() <= 4f)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }


    public Node.Status isPreyEatable()
    {
        target = GetClosestTarget();

        if (Vector3.Distance(this.transform.position, target.transform.position) < 2)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }




    public Node.Status isPreyNotinSight()
    {
        target = GetClosestTarget();
        if (target == null)
        {
            return Node.Status.SUCCESS;
        }
        if (Vector3.Distance(this.transform.position, target.transform.position) > 10)
        {

            Vector3 targetDir = target.transform.position - this.transform.position;
            var ray = new Ray(this.transform.position, targetDir.normalized);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.gameObject.tag == "rabbit")
                {
                    return Node.Status.FAILED;
                }
            }
            return Node.Status.SUCCESS;
        }       
        return Node.Status.FAILED;

    }

    Vector3 lastSeen = Vector3.zero;
    public Node.Status gotoLastSeen()
    {
        return GoToLocation(lastSeen);
    }

    public Node.Status GoToTarget()
    {
        return GoToLocation(target.transform.position);
    }

    public Node.Status eatPrey()
    {
        target = GetClosestTarget();
        target.SetActive(false);
        print("Rabbit Eaten!");
        return Node.Status.SUCCESS;
    }


    public Node.Status ChasePrey()
    {
        Action = "In Pursuit";
        Vector3 targetDir = target.transform.position - this.transform.position;
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(target.transform.forward));
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDir));

        //If target is not moving
        if ((toTarget > 90 && relativeHeading < 20) || target.GetComponent<NavMeshAgent>().velocity.magnitude < 0.01f)
        {
            return GoToLocation(target.transform.position);

        }
        float lookAhead = targetDir.magnitude / (agent.speed + target.GetComponent<NavMeshAgent>().speed);
        lastSeen = target.transform.position;
        return GoToLocation(target.transform.position + target.transform.forward * lookAhead * 5.0f);
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

    public Node.Status returnHome()
    {
        Action = "Going Home";
        /*
        if (GoToLocation(home.transform.position) == Node.Status.SUCCESS)
        {
            this.gameObject.SetActive(false);
            return Node.Status.SUCCESS;
        }
        return Node.Status.SUCCESS;
        */
        GoToLocation(home.transform.position);
        float distanceToTarget = Vector3.Distance(home.transform.position, this.transform.position);
        if (distanceToTarget < 2)
        {
            this.gameObject.SetActive(false);
        }
        return Node.Status.SUCCESS;
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

    float getTimeofDay()
    {
        return SceneManager.GetComponent<LightingManager>().getTime();
    }
}
