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

    LightingManager LM;

    // Start is called before the first frame update
    void Start()
    {
        LM = GameObject.Find("SceneManagement").GetComponent<LightingManager>();

        foxText.text = "Fox's: " + LM.getFoxCount().ToString();
        rabbitText.text = "Rabbits's: " + LM.getRabbitCount().ToString();

        Day.text = LM.getDay().ToString();
        TimeofDay.text = ((int)LM.getTime()).ToString() + ":00";

    }

    // Update is called once per frame
    void Update()
    {
        foxText.text = "Fox's: " + LM.getFoxCount().ToString();
        rabbitText.text = "Rabbits's: " + LM.getRabbitCount().ToString();

        Day.text = "Day: " + LM.getDay().ToString();
        TimeofDay.text = ((int)LM.getTime()).ToString() + ":00";
    }
}
