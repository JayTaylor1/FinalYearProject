using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RabbitBehaviour : MonoBehaviour
{
    BehaviourTree tree;
    NavMeshAgent agent;
    public GameObject target;
    public GameObject home;
    public string Action = "Idle";


    public enum ActionState {IDLE, WORKING};
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;


    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();                                         //Root Node


        Selector RabbitTree = new Selector("Rabbit");                       //  Rabbit (Selector)
        
        Sequence Hide = new Sequence("Hide");                               //      Preditor in Radius (Selector)
        Leaf inRangeTarget = new Leaf("Target in Range?", TargetInRange);   //          Sensed Enemy (Condition)
        Selector Flee = new Selector("Flee");                               //          Flee (Selector)
        Sequence GoHome = new Sequence("Go Home");                          //              GoHome (Sequence)
        Leaf canGoHome = new Leaf("Can go Home?", CanGoHome);               //                  Can Go Home (Condition)
        Leaf goToHome = new Leaf("Go To Home", FleeHome);                   //                  Flee Home (Action)
        Leaf goToHide = new Leaf("Go to Hide", GoToHide);                   //              Evade (Action)
        Sequence Wonder = new Sequence("Wonder");                           //      Wonder (Sequence)
        Leaf roam = new Leaf("Roam Freely", Roam);                          //          Roam (Action)


        GoHome.AddChild(canGoHome);
        GoHome.AddChild(goToHome);

        Flee.AddChild(GoHome);
        Flee.AddChild(goToHide);

        Hide.AddChild(inRangeTarget);
        Hide.AddChild(Flee);
        
        Wonder.AddChild(roam);

        RabbitTree.AddChild(Hide);
        RabbitTree.AddChild(Wonder);

        tree.AddChild(RabbitTree);

        tree.PrintTree();


    }

    public GameObject GetClosestTarget()
    {
        GameObject[] Preditors = GameObject.FindGameObjectsWithTag("fox");
        float dist = Mathf.Infinity;
        GameObject closestPreditors = Preditors[0];

        for (int i = 0; i < Preditors.Length; i++)
        {
            if (Vector3.Distance(this.transform.position, Preditors[i].transform.position) < dist)
            {
                closestPreditors = Preditors[i];
                dist = Vector3.Distance(this.transform.position, Preditors[i].transform.position);
            }
        }
        return target;
    }


    public Node.Status TargetInRange()
    {
        target = GetClosestTarget();
        if (Vector3.Distance(this.transform.position, target.transform.position) < 10)
        {
            Action = "Fleeing";
            print(Action);
            return Node.Status.SUCCESS;
        }
        else if (Action == "Fleeing")
        {
            Action = "Idle";
            agent.isStopped = true;
            return Node.Status.FAILED;
        }
        return Node.Status.FAILED;
    }

    //Can go home if facing towards home and there is no preditors infront
    public Node.Status CanGoHome()
    {
        Vector3 toHome = this.transform.position - home.transform.position;
        float lookingAngle = Vector3.Angle(this.transform.forward, toHome);
        if (lookingAngle < 15)
        {
            GameObject[] Preditors = GameObject.FindGameObjectsWithTag("fox");

            GameObject closestPreditors = Preditors[0];

            for (int i = 0; i < Preditors.Length; i++)
            {
                Vector3 toEnemy = this.transform.position - Preditors[i].transform.position;
                float angleToEnemy = Vector3.Angle(this.transform.forward, toEnemy);

                if ((Vector3.Distance(this.transform.position, Preditors[i].transform.position) < 10) && angleToEnemy > 45.0f)
                {
                    return Node.Status.FAILED;
                }
            }
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }

    public Node.Status GoToTarget()
    {
        return GoToLocation(target.transform.position);
    }

    public Node.Status FleeHome()
    {
        Action = "Fleeing Home";
        if (GoToLocation(home.transform.position) == Node.Status.SUCCESS)
        {
            this.gameObject.SetActive(false);
            return Node.Status.SUCCESS;
        }
        return Node.Status.RUNNING;
    }


    public Node.Status GoToHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;
            if (Vector3.Distance(this.transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance);



        return GoToLocation(info.point + chosenDir.normalized * 2);
    }




    Vector3 wanderTarget = Vector3.zero;
    public Node.Status Roam()
    {
        //Action = "Roaming";
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
