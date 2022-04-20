using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    float Speed = 5f;
    public UIHandler ui = null;
    public GameObject selectedObject = null;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.Find("SceneManagement").GetComponent<UIHandler>();
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
                if ((Vector3.Distance(hit.point, ClosestAnimal.transform.position) < 10)){
                    ui.DisplayPanel();
                    selectedObject = ClosestAnimal;
                    ui.displayAnimal(ClosestAnimal);
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


        GameObject[] rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        GameObject[] foxs = GameObject.FindGameObjectsWithTag("fox");

        if (rabbits.Length == 0 && foxs.Length == 0)
        {
            return null;
        }

        float dist = Mathf.Infinity;
        
        ClosestGO = rabbits[0];

        for (int i = 0; i < rabbits.Length; i++)
        {
            if (Vector3.Distance(h.point, rabbits[i].transform.position) < dist)
            {
                ClosestGO = rabbits[i];
                dist = Vector3.Distance(h.point, rabbits[i].transform.position);
            }
        }
        //print(foxs.Length);
        for (int i = 0; i < foxs.Length; i++)
        {
            //print(Vector3.Distance(h.point, foxs[i].transform.position));
            if (Vector3.Distance(h.point, foxs[i].transform.position) < dist)
            {
                ClosestGO = foxs[i];
                dist = Vector3.Distance(h.point, foxs[i].transform.position);
            }
        }
        return ClosestGO;
    }
}
