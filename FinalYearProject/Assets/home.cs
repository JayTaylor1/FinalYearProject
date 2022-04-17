using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class home : MonoBehaviour
{

    List<GameObject> contents = new List<GameObject>();
    //GameObjecy[]
    public int rabbitsInside;

    // Start is called before the first frame update
    void Start()
    {
        int rabbitsInside = 0;

    }
    


    public void updateContentsAge()
    {
        if (contents != null)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].gameObject.GetComponent<RabbitBehaviour>().incrementAge();
            }
        }
    }

    public void enterRabbit(GameObject r)
    {
        r.SetActive(false);
        contents.Add(r);
        rabbitsInside++;
    }

    public void releaseRabbits()
    {
        if (contents != null)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].SetActive(true);
            }
        }
        rabbitsInside = contents.Count;
        contents.Clear();
    }

}

