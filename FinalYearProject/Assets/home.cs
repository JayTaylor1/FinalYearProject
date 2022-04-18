using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class home : MonoBehaviour
{

    List<GameObject> contents = new List<GameObject>();
    public List<GameObject> occupants = new List<GameObject>();
    //GameObjecy[]
    public int animalsInside;

    // Start is called before the first frame update
    void Start()
    {
        int animalsInside = 0;

    }
    


    public void updateContentsAge()
    {
        if (contents != null || contents.Count > 0)
        {
            for (int i = 0; i < contents.Count - 1; i++)
            {
                contents[i].gameObject.GetComponent<RabbitBehaviour>().incrementAge();
            }
        }
    }
/*
    public void enterRabbit(GameObject r)
    {
        r.SetActive(false);
        contents.Add(r);
        animalsInside++;
    }
*/
    public void enterAnimal(GameObject a)
    {
        a.SetActive(false);
        contents.Add(a);
        animalsInside++;
    }
/*
    public void releaseRabbits()
    {
        if (contents != null)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].SetActive(true);
                contents[i].GetComponent<RabbitBehaviour>().setEnergy(100f);
            }
        }
        animalsInside = contents.Count;
        contents.Clear();
    }
*/
    public void releaseAnimals()
    {
        if (contents != null)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].SetActive(true);
                if (contents[i].tag == "rabbit")
                {
                    contents[i].GetComponent<RabbitBehaviour>().setEnergy(100f);
                }
            }
        }
        animalsInside = contents.Count;
        contents.Clear();
    }

    public void addOccupant(GameObject a)
    {
        occupants.Add(a);
    }

    public void removeOccupant(GameObject a)
    {
        occupants.Remove(a);
    }

    public int getOccupantCount()
    {
        return occupants.Count;
    }

    public int getContentsCount()
    {
        return contents.Count;
    }

    public List<GameObject> getOccupants()
    {
        return occupants;
    }

}

