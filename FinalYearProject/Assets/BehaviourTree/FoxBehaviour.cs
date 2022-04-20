using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FoxBehaviour : MonoBehaviour
{
    public float Speed = 0;
    public float Sense = 0;


    BehaviourTree tree;
    NavMeshAgent agent;
    GameObject target;
    public GameObject home = null;
    public string Action;
    public string Gender = null;
    public bool isDead = false;
    public int Hunger = 100;

    public bool CanReproduce = true;
    public string Maturity;
    public int Age;
    public int reproductionCoolDown;
    public GameObject mate;
    public GameObject foxPrefab;

    GameObject SceneManager;
    //public float timeofday;

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
                                                                                    //          
        Sequence reprodcution = new Sequence("Reproduction");                       //      Reproduce (Sequence)
        Leaf CheckCanReproduce = new Leaf("Check if can reproduce", canReproduce);  //          Check if can reproduce (condition)

        Selector ProduceChild = new Selector("Produce Child");                      //          Produce Child (Selector)
        Sequence MateInRange = new Sequence("MateInRange");                         //              Mate In Range (Sequence)
        Leaf IsMateInRange = new Leaf("Is mate close enough", isMateInRange);       //                  Is mate Close enough to reproduce (condition)
        Leaf reproduce = new Leaf("Produce Child", Reproduce);                      //                  reproduce(Action)
        Leaf GoToMate = new Leaf("Go To Mate", goToMate);                           //              Go To Mate (Action)


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

        MateInRange.AddChild(IsMateInRange);
        MateInRange.AddChild(reproduce);

        ProduceChild.AddChild(MateInRange);
        ProduceChild.AddChild(GoToMate);

        reprodcution.AddChild(CheckCanReproduce);
        reprodcution.AddChild(ProduceChild);


        Wonder.AddChild(roam);

        FoxTree.AddChild(Chase);
        FoxTree.AddChild(sleep);
        FoxTree.AddChild(reprodcution);
        FoxTree.AddChild(Wonder);

        tree.AddChild(FoxTree);
        

        //tree.PrintTree();

        if (Age <= 1)
        {
            Maturity = "Child";
            CanReproduce = false;
        }
        else if (Age <= 12)
        {
            Maturity = "Adult";
        }
        else
        {
            Maturity = "Elder";
            CanReproduce = false;
        }

        if (home == null)
        {   
            findNewHome();
        }


        if (Gender == null || Gender == "")
        {
            if (Random.value < 0.5)
            {
                Gender = "Male";
            }
            else
            {
                Gender = "Female";
            }
        }

        if (Speed <= 0)
        {
            Speed = Random.Range(3.0f, 3.5f);
        }

        if (Sense <= 0)
        {
            Sense = Random.Range(8.0f, 12.0f);
        }

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
        if (Hunger >= 100)
        {
            return Node.Status.FAILED;
        }
        target = GetClosestTarget();
        if (target == null )
        {
            return Node.Status.FAILED;
        }
        if (Vector3.Distance(this.transform.position, target.transform.position) < Sense)
        {
            return Node.Status.SUCCESS;
        }
        /*
        else if (Action == "Chasing")
        {
            Action = "Idle";
            agent.isStopped = true;
            return Node.Status.FAILED;
        }
        */
        return Node.Status.FAILED;
    }

    public Node.Status isNightTime()
    {
        if (getTimeofDay() >= 22f || getTimeofDay() <= 4f || Hunger > 100)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }


    public Node.Status isPreyEatable()
    {
        //target = GetClosestTarget();
        if (target == null)
        {
            return Node.Status.FAILED;
        }


        if (Vector3.Distance(this.transform.position, target.transform.position) <= 2)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }




    public Node.Status isPreyNotinSight()
    {
        //target = GetClosestTarget();
        if (target == null)
        {
            return Node.Status.SUCCESS;
        }
        if (Vector3.Distance(this.transform.position, target.transform.position) > Sense)
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


    public Node.Status canReproduce()
    {
        //mate;
        //mate = null;
        if (!CanReproduce && Hunger >= 50f)
        {
            mate = null;
            return Node.Status.FAILED;
        }
        GameObject[] foxs;
        if (Gender == "Female")
        {
            mate = null;
            foxs = GameObject.FindGameObjectsWithTag("fox");

            float dist = Mathf.Infinity;
            GameObject chosenFox = null;

            for (int i = 0; i < foxs.Length; i++)
            {
                if (foxs[i].GetComponent<FoxBehaviour>().getGender() == "Male" && foxs[i].GetComponent<FoxBehaviour>().getCanReproduce())
                {
                    if (Vector3.Distance(this.transform.position, foxs[i].transform.position) < dist)
                    {
                        chosenFox = foxs[i];
                        dist = Vector3.Distance(this.transform.position, foxs[i].transform.position);
                    }
                }

            }

            if (dist > Sense)
            {
                return Node.Status.FAILED;
            }


            if (chosenFox == null)
            {
                return Node.Status.FAILED;
            }
            mate = chosenFox;
            mate.GetComponent<FoxBehaviour>().setMate(this.gameObject);
            return Node.Status.SUCCESS;
        }


        if (Gender == "Male" && mate != null)
        {
            float distanceToMate = Vector3.Distance(mate.transform.position, this.transform.position);
            if (distanceToMate > Sense)
            {
                mate = null;
                return Node.Status.FAILED;
            }
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;

    }


    public Node.Status isMateInRange()
    {
        float distanceToMate = Vector3.Distance(mate.transform.position, this.transform.position);
        if (distanceToMate < 2)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }


    public Node.Status Reproduce()
    {
        if (Gender == "Female")
        {
            GameObject Child = (GameObject)Instantiate(foxPrefab, this.transform.position, Quaternion.identity);
            Child.GetComponent<FoxBehaviour>().setAge(0);
            if (Random.value < .5)
            {
                Child.GetComponent<FoxBehaviour>().setGender("Male");
            }
            else
            {
                Child.GetComponent<FoxBehaviour>().setGender("Female");
            }

            Child.GetComponent<FoxBehaviour>().setHome(home);
            Child.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            CanReproduce = false;
            reproductionCoolDown = 4;
            mate.GetComponent<FoxBehaviour>().setCanReproduce(false);
            mate.GetComponent<FoxBehaviour>().setReproductionCooldown(4);
            Hunger -= 20;
            SceneManager.GetComponent<LightingManager>().addAnimal(Child);
        }
        return Node.Status.SUCCESS;
    }

    public Node.Status goToMate()
    {
        if (mate != null)
        {
            Action = "Going To Mate";
            return GoToLocation(mate.transform.position);
        }
        return Node.Status.FAILED;
    }

    Vector3 lastSeen = Vector3.zero;
    public Node.Status gotoLastSeen()
    {
        Action = "Alert";
        return GoToLocation(lastSeen);
    }

    public Node.Status GoToTarget()
    {
        return GoToLocation(target.transform.position);
    }

    public Node.Status eatPrey()
    {
        //Action = "Eating";
        //target = GetClosestTarget();
        target.GetComponent<RabbitBehaviour>().Die();
        target = null;
        //print("Rabbit Eaten!");
        Hunger += 40;
        return Node.Status.SUCCESS;
    }


    public Node.Status ChasePrey()
    {
        if (target == null || Vector3.Distance(target.transform.position, this.transform.position) > Sense)
        {
            return Node.Status.FAILED;
        }
        Action = "In Persuit";
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



    Vector3 previousTargetPosition = Vector3.zero;
    float timeTaken = 0;
    Vector3 wanderTarget = Vector3.zero;
    public Node.Status Roam()
    {
        Action = "Roaming";
        float range = 20f;

        if (previousTargetPosition == Vector3.zero || Vector3.Distance(this.transform.position, previousTargetPosition) > range || Vector3.Distance(this.transform.position, previousTargetPosition) <= 2 || timeTaken > 5f)
        {
            timeTaken = 0;
            Vector2 circle = Random.insideUnitCircle;
            Vector3 Circle3D = new Vector3(circle.x, 0, circle.y);
            previousTargetPosition = this.transform.position + Circle3D * range;
            NavMeshPath path = new NavMeshPath();
            while (!agent.CalculatePath(previousTargetPosition, path))
            {
                //print("Cant Reach");
                circle = Random.insideUnitCircle;
                Circle3D = new Vector3(circle.x, 0, circle.y);
                previousTargetPosition = this.transform.position + Circle3D * range;
            }
        }




        if (Vector3.Distance(this.transform.position, previousTargetPosition) <= range)
        {
            timeTaken += Time.deltaTime;
            if (timeTaken > 5f)
            {
                previousTargetPosition = Vector3.zero;
            }
            return GoToLocation(previousTargetPosition);
        }
        return Node.Status.FAILED;
        /*
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
        */
    }

    public Node.Status returnHome()
    {
        Action = "Going Home";

        GoToLocation(home.transform.position);
        float distanceToTarget = Vector3.Distance(home.transform.position, this.transform.position);
        if (distanceToTarget < 2)
        {
            Action = "Resting";
            home.GetComponent<home>().enterAnimal(this.gameObject);
        }
        return Node.Status.SUCCESS;
    }


    Node.Status GoToLocation(Vector3 destination)
    {
        /*
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
        */
        float currentSpeed = Speed;
        if (Maturity == "Child" || Maturity == "Elder")
        {
            currentSpeed *= 0.9f;
        }
        if (Hunger <= 30)
        {
            currentSpeed *= 0.8f;
        }

        

        agent.speed = currentSpeed;
        agent.SetDestination(destination);
        return Node.Status.SUCCESS;
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

    public void decrementHunger()
    {
        Hunger--;
        if (Hunger <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        SceneManager.GetComponent<LightingManager>().removeAnimal(this.gameObject);
        home.GetComponent<home>().removeOccupant(this.gameObject);
        isDead = true;
        this.gameObject.SetActive(false);
    }

    public string getAction()
    {
        return Action;
    }



    public void incrementAge()
    {
        Age++;

        if (reproductionCoolDown > 0)
        {
            reproductionCoolDown--;
            //this.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        if (Age <= 1)
        {
            Maturity = "Child";
            CanReproduce = false;
            this.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        }
        else if (Age <= 12)
        {
            Maturity = "Adult";
            if (reproductionCoolDown == 0)
            {
                CanReproduce = true;
            }
            this.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            Maturity = "Elder";
            CanReproduce = false;
        }

        if (Age == 4)
        {
            findNewHome();
        }

        if (Age > 14)
        {
            float randValue = Random.value;

            if (randValue < (((1 / (Age - 12)) * 0.5)))
            {
                return;
            }
            else
            {
                Die();
            }
        }
    }









    public GameObject findNewHome()
    {

        //print("Rehoming");
        List<GameObject> foxhomes = new List<GameObject>();
        
        if (home == null)
        { 
            foxhomes.AddRange(GameObject.FindGameObjectsWithTag("foxhome"));
            int randomNum = Random.Range(0, foxhomes.Count - 1);
            home = foxhomes[randomNum];
            home.GetComponent<home>().addOccupant(this.gameObject);
            //print(home);
            return home;
        }

        foxhomes = SceneManager.GetComponent<LightingManager>().getFoxHomes();

        for (int i = 0; i < foxhomes.Count; i++)
        {
            //print(foxhomes[i].GetComponent<home>().getOccupantCount());
            if (foxhomes[i].GetComponent<home>().getOccupantCount() == 0)
            {
                home.GetComponent<home>().removeOccupant(this.gameObject);
                home = foxhomes[i];
                home.GetComponent<home>().addOccupant(this.gameObject);                
                return home;

            }

            if (foxhomes[i].GetComponent<home>().getOccupantCount() == 1)
            {
                if (Gender == "Male" && foxhomes[i].GetComponent<home>().getOccupants()[0].GetComponent<FoxBehaviour>().getGender() == "Female")
                {
                    home.GetComponent<home>().removeOccupant(this.gameObject);
                    home = foxhomes[i];
                    home.GetComponent<home>().addOccupant(this.gameObject);
                    return home;
                }

                if (Gender == "Female" && foxhomes[i].GetComponent<home>().getOccupants()[0].GetComponent<FoxBehaviour>().getGender() == "Male")
                {
                    home.GetComponent<home>().removeOccupant(this.gameObject);
                    home = foxhomes[i];
                    home.GetComponent<home>().addOccupant(this.gameObject);
                    return home;
                }
            }

            if (home.GetComponent<home>().getOccupantCount() > 5)
            {
                //Create New Home
            }
        }
        return null;
    }

    public string getGender()
    {
        return Gender;
    }

    public void setMate(GameObject m)
    {
        mate = m;
    }

    public bool getCanReproduce()
    {
        return CanReproduce;
    }

    public void setCanReproduce(bool cr)
    {
        CanReproduce = cr;
    }

    void setAge(int a)
    {
        Age = a;
    }

    void setGender(string g)
    {
        Gender = g;
    }

    void setHome(GameObject h)
    {
        home = h;
    }

    public void setReproductionCooldown(int cd)
    {
        reproductionCoolDown = cd;
    }

    public float getSpeed()
    {
        return Speed;
    }

    public float getSense()
    {
        return Sense;
    }

    public int getHunger()
    {
        return Hunger;
    }

    public string getMaturity()
    {
        return Maturity;
    }

    public int getAge()
    {
        return Age;
    }

    public void setHunger(int h) 
    { 
            Hunger = h;
    }


}
