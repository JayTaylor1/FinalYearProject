using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class home : MonoBehaviour
{

    public List<GameObject> contents = new List<GameObject>();
    public List<GameObject> occupants = new List<GameObject>();
    //GameObjecy[]
    public int animalsInside;

    // Start is called before the first frame update
    void Start()
    {
        int animalsInside = 0;

    }
    

    /*
    public void updateContentsAge()
    {
        if (contents != null || contents.Count > 0)
        {
            foreach (GameObject animal in contents)
            //for (int i = 0; i < contents.Count - 1; i++)
            {
                animal.GetComponent<RabbitBehaviour>().incrementAge();

            }
        }
    }
    */
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
            //for (int i = 0; i < contents.Count; i++)
            foreach(GameObject animal in contents)
            {
                animal.SetActive(true);
                if (animal.tag == "rabbit")
                {
                    animal.GetComponent<RabbitBehaviour>().setEnergy(100f);
                }
                if (animal.tag == "fox")
                {
                    if (animal.GetComponent<FoxBehaviour>().getHunger() >= 100)
                    {
                        animal.GetComponent<FoxBehaviour>().setHunger(99);
                    }
                }
            }
        }
        animalsInside = 0;
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

