using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    float Speed = 5f;
    public UIHandler ui = null;
    public GameObject selectedObject = null;
    LightingManager lm;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("SceneManagement").GetComponent<UIHandler>();
        lm = GameObject.Find("SceneManagement").GetComponent<LightingManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        float zoomInput = Input.mouseScrollDelta.y;

        if (Input.GetKey(KeyCode.Q))
        {
            //yValue = -Speed;

            this.transform.Rotate(Vector3.right * Speed * 2 * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            this.transform.Rotate(-Vector3.right * Speed * 2 * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            ui.HidePanel();
            selectedObject = null;
        }

        if (Input.GetMouseButtonDown(0))
        {
            


            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            GameObject ClosestAnimal = null;
            if (Physics.Raycast(ray, out hit, 1000))
            {
                ClosestAnimal = getNearestGameObject(hit);
                //Debug.Log(ClosestAnimal + "  Distance:  " + Vector3.Distance(hit.point, ClosestAnimal.transform.position));
                if (ClosestAnimal != null && Vector3.Distance(hit.point, ClosestAnimal.transform.position) < 10){
                    ui.DisplayPanel();
                    if (selectedObject != null)
                    {
                        selectedObject.transform.Find("SelectedIndicator").gameObject.SetActive(false);
                    }
                    selectedObject = ClosestAnimal;
                    ui.displayAnimal(ClosestAnimal);
                    ClosestAnimal.transform.Find("SelectedIndicator").gameObject.SetActive(true);
                }
                else
                {
                    if (ClosestAnimal != null)
                    {
                        ClosestAnimal.transform.Find("SelectedIndicator").gameObject.SetActive(false);
                    }
                    selectedObject = null;
                    ui.HidePanel();
                    ui.clearAnimal();
                }
            }
        }

        //update the position
        this.transform.position = transform.position + new Vector3(horizontalInput * Speed * Time.deltaTime, -zoomInput, verticalInput * Speed * Time.deltaTime);
    }


    public GameObject getNearestGameObject(RaycastHit h)
    {
        //print(h.point);
        GameObject ClosestGO = null;

        List<GameObject> rabbits = lm.getRabbits();
        List<GameObject> foxs = lm.getFoxs();


        //GameObject[] rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        //GameObject[] foxs = GameObject.FindGameObjectsWithTag("fox");

        if (rabbits.Count == 0 && foxs.Count == 0)
        {
            return null;
        }

        float dist = Mathf.Infinity;
        
        //ClosestGO = rabbits[0];

        //for (int i = 0; i < rabbits.Length; i++)
        foreach (GameObject rabbit in rabbits)
        {
            if (Vector3.Distance(h.point, rabbit.transform.position) < dist && rabbit.active)
            {
                ClosestGO = rabbit;
                dist = Vector3.Distance(h.point, rabbit.transform.position);
            }
        }
        //print(foxs.Length);
        //for (int i = 0; i < foxs.Length; i++)
        foreach (GameObject fox in foxs)
        {
            //print(Vector3.Distance(h.point, foxs[i].transform.position));
            if (Vector3.Distance(h.point, fox.transform.position) < dist && fox.active)
            {
                ClosestGO = fox;
                dist = Vector3.Distance(h.point, fox.transform.position);
            }
        }
        return ClosestGO;
    }
}
