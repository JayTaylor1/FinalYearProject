using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RabbitBehaviour : MonoBehaviour
{
    BehaviourTree tree;
    NavMeshAgent agent;
    public GameObject target;

    public enum ActionState {IDLE, WORKING};
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;


    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();

        Selector RabbitTree = new Selector("Rabbit"); 
        
        Sequence Hide = new Sequence("Hide");


        Leaf goToTarget = new Leaf("Go to Target", GoToTarget);
        Leaf goToHide = new Leaf("Go to Hide", GoToHide);
        //Condition
        Leaf inRangeTarget = new Leaf("Target in Range?", TargetInRange);


        Hide.AddChild(inRangeTarget);
        Hide.AddChild(goToHide);
        //Hide.AddChild(goToTarget);

        RabbitTree.AddChild(Hide);

        tree.AddChild(RabbitTree);

        tree.PrintTree();


    }

    public Node.Status TargetInRange()
    {
        if (Vector3.Distance(this.transform.position, target.transform.position) < 10)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }

    public Node.Status GoToTarget()
    {
        return GoToLocation(target.transform.position);
    }


    public Node.Status GoToHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;
            if (Vector3.Distance(this.transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }
        return GoToLocation(chosenSpot);
    }

    public Node.Status CleverGoToHide()
    {
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        float chosenAngle = Mathf.Infinity;
        GameObject chosenGO = World.Instance.GetHidingSpots()[0];

        //check if hiding spot is infront of rabbit
        for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        {
            Vector3 toHide = World.Instance.GetHidingSpots()[i].transform.position - this.transform.position;
            float lookingAngle = Vector3.Angle(this.transform.forward, toHide);

            if (lookingAngle < 60 || lookingAngle < chosenAngle)
            {
                Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - target.transform.position;
                Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 5;
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenAngle = lookingAngle;
                chosenGO = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(this.transform.position, hidePos);
            }

        }

        if (chosenSpot == Vector3.zero)
        {
            for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
            {


                //##########################################################################



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
        }

        Collider hideCol = chosenGO.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float distance = 100.0f;
        hideCol.Raycast(backRay, out info, distance);

        return GoToLocation(info.point + chosenDir.normalized * 2);
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
        /*
        if (treeStatus != Node.Status.SUCCESS)
        {
            treeStatus = tree.Process();
        }
        */
    }
}
