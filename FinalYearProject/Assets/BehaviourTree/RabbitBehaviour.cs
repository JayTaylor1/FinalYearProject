using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RabbitBehaviour : MonoBehaviour
{
    public float Speed = 0;
    public float Sense = 0;

    BehaviourTree tree;
    NavMeshAgent agent;
    GameObject target;
    float timeofday;
    public GameObject home = null;
    public GameObject mate;
    public string Action;
    public string Status;
    public string Maturity;
    public string Gender;
    public bool CanReproduce = true;
    public float Energy = 100;
    public bool isDead = false;
    int reproductionCoolDown = 0;

    public GameObject rabbitPrefab;
    public GameObject rabbitHomePrefab;

    GameObject SceneManager;


    private int DayofCreation = 0;
    public int Age = 0;


    public enum ActionState {IDLE, WORKING};
    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;

    //print((int)TimeOfDay + ":00");

    // Start is called before the first frame update
    void Start()
    {
        
        SceneManager = GameObject.Find("SceneManagement");

        agent = this.GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();                                                 //Root Node


        Selector RabbitTree = new Selector("Rabbit");                               //  Rabbit (Selector)
        
        Sequence Hide = new Sequence("Hide");                                       //      Preditor in Radius (Sequence)
        Leaf inRangeTarget = new Leaf("Target in Range?", TargetInRange);           //          Sensed Enemy (Condition)
        Selector Flee = new Selector("Flee");                                       //          Flee (Selector)
        Sequence GoHome = new Sequence("Go Home");                                  //              GoHome (Sequence)
        Leaf canGoHome = new Leaf("Can go Home?", CanGoHome);                       //                  Can Go Home (Condition)
        Leaf goToHome = new Leaf("Go To Home", FleeHome);                           //                  Flee Home (Action)
        Leaf goToHide = new Leaf("Go to Hide", GoToHide);                           //              Evade (Action)

        Sequence sleep = new Sequence("if Night Time");                             //      If night time (Sequence)
        Leaf IsNightTime = new Leaf("Check if Night", isNightTime);                 //          Check if Night time (condition)    
        Leaf ReturnHome = new Leaf("Return Home", returnHome);                      //          Go Home(Action)

        Sequence reprodcution = new Sequence("Reproduction");                       //      Reproduce (Sequence)
        Leaf CheckCanReproduce = new Leaf("Check if can reproduce", canReproduce);  //          Check if can reproduce (condition)

        Selector ProduceChild = new Selector("Produce Child");                      //          Produce Child (Selector)
        Sequence MateInRange = new Sequence("MateInRange");                         //              Mate In Range (Sequence)
        Leaf IsMateInRange = new Leaf("Is mate close enough", isMateInRange);       //                  Is mate Close enough to reproduce (condition)
        Leaf reproduce = new Leaf("Produce Child", Reproduce);                      //                  reproduce(Action)
        Leaf GoToMate = new Leaf("Go To Mate", goToMate);                           //              Go To Mate (Action)

        Sequence Wonder = new Sequence("Wonder");                                   //      Wonder (Sequence)
        Leaf roam = new Leaf("Roam Freely", Roam);                                  //          Roam (Action)



        GoHome.AddChild(canGoHome);
        GoHome.AddChild(goToHome);

        Flee.AddChild(GoHome);
        Flee.AddChild(goToHide);

        Hide.AddChild(inRangeTarget);
        Hide.AddChild(Flee);

        sleep.AddChild(IsNightTime);
        sleep.AddChild(ReturnHome);

        MateInRange.AddChild(IsMateInRange);
        MateInRange.AddChild(reproduce);

        ProduceChild.AddChild(MateInRange);
        ProduceChild.AddChild(GoToMate);

        reprodcution.AddChild(CheckCanReproduce);
        reprodcution.AddChild(ProduceChild);

        Wonder.AddChild(roam);

        RabbitTree.AddChild(Hide);
        RabbitTree.AddChild(sleep);
        RabbitTree.AddChild(reprodcution);
        RabbitTree.AddChild(Wonder);

        tree.AddChild(RabbitTree);

        //tree.PrintTree();

        if (Age <= 1)
        {
            Maturity = "Child";
            CanReproduce = false;
        }
        else if (Age <= 10)
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
        agent.speed = Speed;

        if (Sense <= 0)
        {
            Sense = Random.Range(8.0f, 12.0f);
        }

    }

    public GameObject GetClosestTarget()
    {
        GameObject[] Preditors = GameObject.FindGameObjectsWithTag("fox");
        //print(Preditors.Length);
        if (Preditors.Length == 0)
        {
            return null;
        }
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
        return closestPreditors;
    }


    public Node.Status TargetInRange()
    {
        target = GetClosestTarget();
        if (target == null)
        {
            return Node.Status.FAILED;
        }
        if (Vector3.Distance(this.transform.position, target.transform.position) < Sense)
        {
            return Node.Status.SUCCESS;
        }
        /*
        else if (Action == "Fleeing")
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
        if (getTimeofDay() >= 22f || getTimeofDay()<= 4f || Energy <= 30f)
        {
            return Node.Status.SUCCESS;
        }
        return Node.Status.FAILED;
    }




    public Node.Status canReproduce()
    {
        //mate;
        //mate = null;
        if (!CanReproduce && Energy >= 50f)
        {
            mate = null;
            return Node.Status.FAILED;
        }
        GameObject[] rabbits;
        if (Gender == "Female")
        {
            mate = null;
            rabbits = GameObject.FindGameObjectsWithTag("rabbit");

            float dist = Mathf.Infinity;
            GameObject chosenRabbit = null;

            for (int i = 0; i < rabbits.Length; i++)
            {
                if (rabbits[i].GetComponent<RabbitBehaviour>().getGender() == "Male" && rabbits[i].GetComponent<RabbitBehaviour>().getCanReproduce())
                {
                    if (Vector3.Distance(this.transform.position, rabbits[i].transform.position) < dist)
                    {
                        chosenRabbit = rabbits[i];
                        dist = Vector3.Distance(this.transform.position, rabbits[i].transform.position);
                    }
                }

            }

            if (dist > Sense)
            {
                return Node.Status.FAILED;
            }


            if (chosenRabbit == null)
            {
                return Node.Status.FAILED;
            }
            mate = chosenRabbit;
            mate.GetComponent<RabbitBehaviour>().setMate(this.gameObject);
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



    public Node.Status goToMate()
    {
        if (mate != null)
        {
            Action = "Going to Mate";
            return GoToLocation(mate.transform.position);
        }
        return Node.Status.FAILED;
    }

    public Node.Status Reproduce()
    {
        if (Gender == "Female")
        {
            GameObject Child = (GameObject)Instantiate(rabbitPrefab, this.transform.position, Quaternion.identity);

            float ChildSpeed;
            float ChildSense;

            if (Speed > mate.GetComponent<RabbitBehaviour>().getSpeed())
            {
                int PosorNeg = Random.Range(0, 2) * 2 - 1;
                ChildSpeed = Speed + (PosorNeg * Speed * 0.1f);
            }
            else
            {
                int PosorNeg = Random.Range(0, 2) * 2 - 1;
                ChildSpeed = mate.GetComponent<RabbitBehaviour>().getSpeed() + (PosorNeg * mate.GetComponent<RabbitBehaviour>().getSpeed() * 0.1f);
            }

            if (Sense > mate.GetComponent<RabbitBehaviour>().getSense())
            {
                int PosorNeg = Random.Range(0, 2) * 2 - 1;
                ChildSense = Sense + (PosorNeg * Sense * 0.1f);
            }
            else
            {
                int PosorNeg = Random.Range(0, 2) * 2 - 1;
                ChildSense = mate.GetComponent<RabbitBehaviour>().getSense() + (PosorNeg * mate.GetComponent<RabbitBehaviour>().getSense() * 0.1f);
            }
            Child.GetComponent<RabbitBehaviour>().setSpeed(ChildSpeed);
            Child.GetComponent<RabbitBehaviour>().setSense(ChildSense);

            Child.GetComponent<RabbitBehaviour>().setAge(0);
            if (Random.value < 0.5)
            {
                Child.GetComponent<RabbitBehaviour>().setGender("Male");
            }
            else
            {
                Child.GetComponent<RabbitBehaviour>().setGender("Female");
            }
                
            Child.GetComponent<RabbitBehaviour>().setHome(home);
            Child.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            CanReproduce = false;
            reproductionCoolDown = 2;
            mate.GetComponent<RabbitBehaviour>().setCanReproduce(false);
            mate.GetComponent<RabbitBehaviour>().setReproductionCooldown(2);
            Energy -= 20;
            SceneManager.GetComponent<LightingManager>().addAnimal(Child);
        }
        return Node.Status.SUCCESS;
    }


    //Can go home if facing towards home and there is no preditors infront
    //NOTE IN ORDER FOR THE RABBIT TO GO HOME THIS CONDITION MUST SUCCEED
    public Node.Status CanGoHome()
    {
        Vector3 toHome = home.transform.position - this.transform.position;
        //If home is in front
        float lookingAngle = Vector3.Angle(this.transform.forward, toHome);
        if (lookingAngle < 45)
        {
            GameObject[] Preditors = GameObject.FindGameObjectsWithTag("fox");
            //GameObject closestPreditors = Preditors[0];
            //If there is any preditors in front
            foreach (GameObject preditor in Preditors)
            {
                Vector3 toEnemy = preditor.transform.position - this.transform.position;
                float angleToEnemy = Vector3.Angle(this.transform.forward, toEnemy);
                //if in sense radius and enemy is BEHIND  
                //if ((Vector3.Distance(this.transform.position, preditor.transform.position) < Sense) && angleToEnemy > 45.0f)
                //FAIL IF ENEMY IN RADIUS IS IN FRONT
                if ((Vector3.Distance(this.transform.position, preditor.transform.position) < Sense) && angleToEnemy < 60.0f)
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




    public Node.Status FleeHome()
    {
        Action = "Fleeing Home";
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


    public Node.Status GoToHide()
    {
        
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject[] HidingSpots = World.Instance.GetHidingSpots();
        GameObject chosenGO = HidingSpots[0];

        //for (int i = 0; i < World.Instance.GetHidingSpots().Length; i++)
        foreach (GameObject HidingSpot in HidingSpots)
        {
            Vector3 hideDir = HidingSpot.transform.position - target.transform.position;
            Vector3 hidePos = HidingSpot.transform.position + hideDir.normalized * 5;
            if (Vector3.Distance(this.transform.position, hidePos) < dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenGO = HidingSpot;
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }

        //if theres no fox ibetween this and the chosen hide


        //Vector3 toHide = chosenGO.transform.position - this.transform.position;
        //Vector3 toTarget = target.transform.position - this.transform.position;

        //float lookingAngle = Vector3.Angle(toTarget, toHide);

        Vector3 VtoHide = chosenGO.transform.position - this.transform.position;
        Vector3 VtoTarget = target.transform.position - this.transform.position;
        float AngleToTarget = Vector3.Angle(this.transform.forward, VtoTarget);
        float AngleToHide = Vector3.Angle(this.transform.forward, VtoHide);

        float AngleBetwenFoxRabbitHide = Vector3.Angle(VtoHide, VtoTarget);

        //Looking Angle is the angle betwen the 
        bool targetinfront = false;


        //if (lookingAngle > 60 || AngleToTarget > 135)

        //if fox is behind you and hide is infront
        //if (AngleToTarget > 135 && AngleToHide < 90)
        if ((AngleToTarget > 135 && AngleToHide < 90) || (AngleToTarget > 135 && dist < 10))
        {
            Action = "Hiding";
            Collider hideCol = chosenGO.GetComponent<Collider>();
            Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
            RaycastHit info;
            float distance = 100.0f;
            hideCol.Raycast(backRay, out info, distance);
            return GoToLocation(info.point + chosenDir.normalized * 2);
        }
        else
        {
            Action = "Fleeing";
            Vector3 fleeVector = target.transform.position - this.transform.position;
            return GoToLocation(this.transform.position - fleeVector);

            //print("help");
        }

        return Node.Status.SUCCESS;
    }




    //Vector3 wanderTarget = Vector3.zero;
    public Vector3 previousTargetPosition = Vector3.zero;
    public float timeTaken = 0;
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
        float wanderRadius = 5;
        float wanderDistance = 5;
        float wanderJitter = 1;

        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter, 0, Random.Range(-1.0f, 1.0f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);
        */

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
        if (Energy <= 30)
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

        if (isDead)
        {
            Status = "Dead";
        }
        else if (Energy < 30)
        {
            Status = "Tired";
        }
        else
        {
            Status = "Normal";
        }

    }


    float getTimeofDay()
    {
        return SceneManager.GetComponent<LightingManager>().getTime();
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

    public void setSpeed(float s)
    {
        Speed = s;
        if (agent != null) { 
            agent.speed = Speed;
        }
    }

    public void setSense(float s)
    {
        Sense = s;
    }

    public void incrementAge()
    {
        Age++;

        if (reproductionCoolDown > 0)
        {
            reproductionCoolDown--;
            //this.transform.localScale = new Vector3(0.65f, 0.75f, 0.75f);
        }

        if (Age <= 1)
        {
            Maturity = "Child";
            CanReproduce = false;
            this.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        }
        else if (Age <= 10)
        {
            Maturity = "Adult";
            if (reproductionCoolDown == 0)
            {
                CanReproduce=true;
            }
            this.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            Maturity = "Elder";
            CanReproduce = false;
        }
        if (Age > 12)
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



    public string getGender()
    {
        return Gender;
    }

    public bool getCanReproduce()
    {
        return CanReproduce;
    }

    public void setCanReproduce(bool cr)
    {
        CanReproduce = cr;
    }

    public void setReproductionCooldown(int cd)
    {
        reproductionCoolDown = cd;
    }
    
    public void setMate(GameObject m)
    {
        mate = m;
    }

    public void setEnergy(float e)
    {
        Energy = e;
    }


    public void decrementEnergy()
    {
        Energy--;
        if (Energy <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        SceneManager.GetComponent<LightingManager>().removeAnimal(this.gameObject);
        home.GetComponent<home>().removeOccupant(this.gameObject);
        isDead = true;
        Status = "Dead";
        this.gameObject.SetActive(false);
    }

    public string getAction()
    {
        return Action;
    }

    public float getSpeed()
    {
        return Speed;
    }

    public float getSense()
    {
        return Sense;
    }

    public float getEnergy()
    {
        return Energy;
    }
    public string getMaturity()
    {
        return Maturity;
    }
    public string getStatus()
    {
        return Status;
    }
    public int getAge()
    {
        return Age;
    }


    public GameObject findNewHome()
    {
        
        List<GameObject> rabbithomes = new List<GameObject>();
        

        if (home == null)
        {
            //Incase Manager hasnt been  instantiated yet.
            rabbithomes.AddRange(GameObject.FindGameObjectsWithTag("rabbithome"));
            float dist = Mathf.Infinity;
            GameObject closestHome = rabbithomes[0];

            foreach (GameObject rabbithome in rabbithomes)
            {
                if (Vector3.Distance(this.transform.position, rabbithome.transform.position) < dist)
                {
                    closestHome = rabbithome;
                    dist = Vector3.Distance(this.transform.position, rabbithome.transform.position);
                }
            }
            home = closestHome;
            home.GetComponent<home>().addOccupant(this.gameObject);
            return home;
        }
        rabbithomes = SceneManager.GetComponent<LightingManager>().getRabbitHomes();
        //for (int i = 0; i < rabbithomes.Count; i++)
        foreach (GameObject rabbithome in rabbithomes)
        {
            //print(rabbithomes[i].GetComponent<home>().getOccupantCount());
            if(rabbithome.GetComponent<home>().getOccupantCount() == 0)
            {
                home.GetComponent<home>().removeOccupant(this.gameObject);
                home = rabbithome;
                home.GetComponent<home>().addOccupant(this.gameObject);
                //print("rehomed");
                return home;

            }

            if (rabbithome.GetComponent<home>().getOccupantCount() == 1)
            {
                if (Gender == "Male" && rabbithome.GetComponent<home>().getOccupants()[0].GetComponent<RabbitBehaviour>().getGender() == "Female")
                {
                    home.GetComponent<home>().removeOccupant(this.gameObject);
                    home = rabbithome;
                    home.GetComponent<home>().addOccupant(this.gameObject);
                    return home;
                }

                if (Gender == "Female" && rabbithome.GetComponent<home>().getOccupants()[0].GetComponent<RabbitBehaviour>().getGender() == "Male")
                {
                    home.GetComponent<home>().removeOccupant(this.gameObject);
                    home = rabbithome;
                    home.GetComponent<home>().addOccupant(this.gameObject);
                    return home;
                }
            }
        }
        if (home.GetComponent<home>().getOccupantCount() > 3)
        {
            float circleRadius = 5f;
            Vector2 circle = Random.insideUnitCircle;
            Vector3 Circle3D = new Vector3(circle.x, 0, circle.y);
            Vector3 HomePostion = this.transform.position + new Vector3(0, home.transform.position.y, 0) + (Circle3D * circleRadius);

            //NavMeshPath path = new NavMeshPath();
            /*
            while (!agent.CalculatePath(HomePostion, path))
            {
                circle = Random.insideUnitCircle;
                Circle3D = new Vector3(circle.x, 0, circle.y);
                HomePostion = this.transform.position + new Vector3(0, home.transform.position.y, 0) + (Circle3D * circleRadius);
            }
            */


            home.GetComponent<home>().removeOccupant(this.gameObject);
            GameObject newHome = (GameObject)Instantiate(rabbitHomePrefab, HomePostion, Quaternion.identity);
            home = newHome;
            home.GetComponent<home>().addOccupant(this.gameObject);

            var ray = new Ray(home.transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                print(hit.transform.gameObject);
                if (hit.transform.gameObject.tag == "floor")
                {
                    home.transform.position = new Vector3(home.transform.position.x, hit.point.y, home.transform.position.z);
                }
            }



            
            return home;
        }
        return null;
    }
}