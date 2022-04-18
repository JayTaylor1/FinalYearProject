using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [SerializeField, Range(0, 24)] private float TimeOfDay;



    List<GameObject> rabbits = new List<GameObject>();
    List<GameObject> foxs = new List<GameObject>();
    List<GameObject> foxhomes = new List<GameObject>();
    List<GameObject> rabbithomes = new List<GameObject>();

    public int RabbitCount;
    public int FoxCount;

    public int Day;
    private bool isnewDawn = true;

    public float secondsPerHour;




    int previousDay = 0;
    private int previousHour = 0;

    public int currentHour = 0;

    private void Start()
    {
        rabbits.AddRange(GameObject.FindGameObjectsWithTag("rabbit"));
        foxs.AddRange(GameObject.FindGameObjectsWithTag("fox"));
        foxhomes.AddRange(GameObject.FindGameObjectsWithTag("foxhome"));
        rabbithomes.AddRange(GameObject.FindGameObjectsWithTag("rabbithome"));

    }

    private void Update()
    {
        RabbitCount = rabbits.Count;
        FoxCount = foxs.Count;

        if (Preset == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime / secondsPerHour;
            Day = (int)(TimeOfDay / 24);

            if (((int)TimeOfDay % 24 == 0) && (Day != previousDay))
            {
                newDay();

                //print("New Day");
                previousDay = Day;
                isnewDawn = true;
            }

            //int previousHour = 0;

            currentHour = (int)(TimeOfDay % 24);
            if (currentHour != previousHour)
            {
                newHour();
                previousHour = currentHour;
            }

            if (((int)TimeOfDay % 24 == 0) && (Day != previousDay))
            {
                newDay();

                //print("New Day");
                previousDay = Day;
                isnewDawn = true;
            }

            if (((int)TimeOfDay % 24 == 4) && (isnewDawn == true))
            {
                newDawn();
                //print("New Day");
                isnewDawn = false;
            }



            //TimeOfDay %= 24;
            UpdateLighting((TimeOfDay % 24) / 24f);
            
        }
        
    }
    
    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColour.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColour.Evaluate(timePercent);
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColour.Evaluate(timePercent);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, -170f, 0));
        }
    }
    
    
    
    
    
    
    
    
    
    private void OnValidate()
    {
        if (DirectionalLight != null)
        {
            return;
        }
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light light in lights)
            {
                if(light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }

    public float getTime()
    {
        return TimeOfDay % 24;
    }

    public int getDay()
    {
        return Day;
    }

    public void newDay()
    {
        //GameObject[] rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        for (int i = 0; i < rabbits.Count; i++)
        {
            rabbits[i].GetComponent<RabbitBehaviour>().incrementAge();
        }

        //GameObject[] rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        for (int i = 0; i < foxs.Count; i++)
        {
            foxs[i].GetComponent<FoxBehaviour>().incrementAge();
        }

        //GameObject[] homes = GameObject.FindGameObjectsWithTag("home");
        for (int i = 0; i < rabbithomes.Count; i++)
        {
            if (rabbithomes[i].GetComponent<home>().getContentsCount() > 0)
            {
                rabbithomes[i].GetComponent<home>().updateContentsAge();
            }
        }
        for (int i = 0; i < foxhomes.Count; i++)
        {
            if (foxhomes[i].GetComponent<home>().getContentsCount() > 0)
            {
                foxhomes[i].GetComponent<home>().updateContentsAge();
            }
        }
    }

    public void newHour()
    {
        //GameObject[] rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        for (int i = 0; i < rabbits.Count; i++)
        {
            if (rabbits[i].GetComponent<RabbitBehaviour>().getAction() != "Resting")
            {
                rabbits[i].GetComponent<RabbitBehaviour>().decrementEnergy();
            }
        }
        for (int i = 0; i < foxs.Count; i++)
        {
            if (foxs[i].GetComponent<FoxBehaviour>().getAction() != "Resting")
            {
                foxs[i].GetComponent<FoxBehaviour>().decrementHunger();
            }
        }

    }

    public void newDawn()
    {

        //GameObject[] homes = GameObject.FindGameObjectsWithTag("home");
        for (int i = 0; i < foxhomes.Count; i++)
        {
            foxhomes[i].GetComponent<home>().releaseAnimals();
        }
        for (int i = 0; i < rabbithomes.Count; i++)
        {
            rabbithomes[i].GetComponent<home>().releaseAnimals();
        }
    }

    public void addAnimal(GameObject a)
    {
        if (a.tag == "rabbit")
        {
            rabbits.Add(a);
            return;
        }
        if (a.tag == "fox")
        {
            foxs.Add(a);
            return;
        }
    }
    public void removeAnimal(GameObject a)
    {
        if (a.tag == "rabbit")
        {
            rabbits.Remove(a);
            return;
        }
        if (a.tag == "fox")
        {
            foxs.Remove(a);
            return;
        }
    }

    public int getFoxCount()
    {
        return FoxCount;
    }

    public int getRabbitCount()
    {
        return RabbitCount;
    }

    public List<GameObject> getRabbitHomes()
    {
        return rabbithomes;
    }

    public List<GameObject> getFoxHomes()
    {
        return foxhomes;
    }





}
