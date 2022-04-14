using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [SerializeField, Range(0, 24)] private float TimeOfDay;
    public int Day;
    private bool isnewDawn = true;


    private int previousDay = 0;

    private void Update()
    {
        if (Preset == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime;
            Day = (int)(TimeOfDay / 24);

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
        GameObject[] rabbits = GameObject.FindGameObjectsWithTag("rabbit");
        for (int i = 0; i < rabbits.Length; i++)
        {
            rabbits[i].GetComponent<RabbitBehaviour>().incrementAge();
        }
        GameObject[] homes = GameObject.FindGameObjectsWithTag("home");
        for (int i = 0; i < homes.Length; i++)
        {
            homes[i].GetComponent<home>().updateContentsAge();
        }
    }

    public void newDawn()
    {

        GameObject[] homes = GameObject.FindGameObjectsWithTag("home");
        for (int i = 0; i < homes.Length; i++)
        {
            homes[i].GetComponent<home>().releaseRabbits();
        }
    }

}
