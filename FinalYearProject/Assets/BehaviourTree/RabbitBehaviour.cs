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
        Selector Hide = new Selector("Hide");
        Leaf goToTarget = new Leaf("Go to Target", GoToTarget);
        Leaf goToHide = new Leaf("Go to Hide", GoToHide);


        Hide.AddChild(goToHide);
        Hide.AddChild(goToTarget);

        tree.AddChild(Hide);

        tree.PrintTree();


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
