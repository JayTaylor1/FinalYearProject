using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class UIHandler : MonoBehaviour
{
    public Text foxText;
    public Text rabbitText;
    public Text Day;
    public Text TimeofDay;

    // Start is called before the first frame update
    void Start()
    {
        foxText.text = GameObject.FindGameObjectsWithTag("fox").Length.ToString() ;
        rabbitText.text = GameObject.FindGameObjectsWithTag("rabbit").Length.ToString();

        Day.text = GameObject.Find("SceneManagement").GetComponent<LightingManager>().getDay().ToString();
        TimeofDay.text = ((int)GameObject.Find("SceneManagement").GetComponent<LightingManager>().getTime()).ToString() + ":00";

    }

    // Update is called once per frame
    void Update()
    {
        foxText.text = "Fox's: " + GameObject.FindGameObjectsWithTag("fox").Length.ToString();
        rabbitText.text = "Rabbits's: " + GameObject.FindGameObjectsWithTag("rabbit").Length.ToString();

        Day.text = "Day: " + GameObject.Find("SceneManagement").GetComponent<LightingManager>().getDay().ToString();
        TimeofDay.text = ((int)GameObject.Find("SceneManagement").GetComponent<LightingManager>().getTime()).ToString() + ":00";
    }
}
