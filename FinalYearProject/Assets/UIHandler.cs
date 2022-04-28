using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class UIHandler : MonoBehaviour
{
    public Image Panel;

    //public Text foxText;
    public Text FoxCountTxt;
    //public Text rabbitText;
    public Text RabbitCountTxt;
    //public Text Day;
    public Text DayTxt;
    //public Text TimeofDay;
    public Text TimeTxt;
    //public Text SelectedGO;

    public Text AnimalTypeTxt;
    public Text AnimalStatusTxt;
    public Text AnimalSpeedTxt;
    public Text AnimalSenseTxt;
    public Text AnimalGenderTxt;
    public Text AnimalActionTxt;
    public Text AnimalEnergyTxt;
    public Text AnimalMaturityTxt;
    public Text AnimalAgeTxt;

    public GameObject cloudTog;
    public InputField SecondsPHTxt;

    public GameObject SelectedAnimal = null;




    public bool CloudsEnabled;


    LightingManager LM;

    // Start is called before the first frame update
    void Start()
    {
        LM = GameObject.Find("SceneManagement").GetComponent<LightingManager>();



        //foxText.text = "Fox's: " + LM.getFoxCount().ToString();
        //rabbitText.text = "Rabbits's: " + LM.getRabbitCount().ToString();

        //Day.text = LM.getDay().ToString();
        //TimeofDay.text = ((int)LM.getTime()).ToString() + ":00";


        FoxCountTxt = GameObject.Find("FoxCountTxt").GetComponent<Text>();
        RabbitCountTxt = GameObject.Find("RabbitCountTxt").GetComponent<Text>();
        DayTxt = GameObject.Find("DayTxt").GetComponent<Text>();
        TimeTxt = GameObject.Find("TimeTxt").GetComponent<Text>();


        AnimalTypeTxt = GameObject.Find("AnimalTypeTxt").GetComponent<Text>();
        AnimalStatusTxt = GameObject.Find("AnimalStatusTxt").GetComponent<Text>();
        AnimalSpeedTxt = GameObject.Find("AnimalSpeedTxt").GetComponent<Text>();
        AnimalSenseTxt = GameObject.Find("AnimalSenseTxt").GetComponent<Text>();
        AnimalGenderTxt = GameObject.Find("AnimalGenderTxt").GetComponent<Text>();
        AnimalActionTxt = GameObject.Find("AnimalActionTxt").GetComponent<Text>();
        AnimalEnergyTxt = GameObject.Find("AnimalEnergyTxt").GetComponent<Text>();
        AnimalMaturityTxt = GameObject.Find("AnimalMaturityTxt").GetComponent<Text>();
        AnimalAgeTxt = GameObject.Find("AnimalAgeTxt").GetComponent<Text>();

        cloudTog = GameObject.Find("CloudsTog");
        cloudTog.GetComponent<Toggle>().isOn = true;

        SecondsPHTxt = GameObject.Find("SecondsPHTxt").GetComponent<InputField>();
        SecondsPHTxt.text = LM.getSecondsPerHour().ToString();

        Panel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        FoxCountTxt.text = "Fox's: " + LM.getFoxCount().ToString();
        RabbitCountTxt.text = "Rabbits's: " + LM.getRabbitCount().ToString();

        DayTxt.text = "Day: " + LM.getDay().ToString();
        TimeTxt.text = ((int)LM.getTime()).ToString() + ":00";

        if (SelectedAnimal != null)
        {
            updateDisplayAnimal();
        }
    }

    public void displayAnimal(GameObject Animal)
    {
        SelectedAnimal = Animal;
    }

    public void updateDisplayAnimal()
    {
        /*
         * Name/Type of Animal
         * Status
         * Speed
         * Sense
         * Gender
         * Action
         * Energy
         * Maturity + Age
        */
        if (SelectedAnimal.tag == "rabbit")
        {
            RabbitBehaviour rb = SelectedAnimal.GetComponent<RabbitBehaviour>();

            AnimalTypeTxt.text = "Rabbit";
            if (rb.getStatus() == "Normal")
            {
                AnimalStatusTxt.text = rb.getStatus();
            }
            else
            {
                AnimalStatusTxt.text = rb.getStatus() + "!";
            }
            AnimalSpeedTxt.text = "Speed: " + rb.getSpeed().ToString("F1");
            AnimalSenseTxt.text = "Sense: " + rb.getSense().ToString("F1");

            AnimalGenderTxt.text = "Gender: " + rb.getGender();
            AnimalActionTxt.text = rb.getAction() + "...";
            AnimalEnergyTxt.text = "Energy: " + rb.getEnergy();
            AnimalMaturityTxt.text = "Maturity: " + rb.getMaturity();
            AnimalAgeTxt.text = "Age: " + rb.getAge();
        }
        if (SelectedAnimal.tag == "fox")
        {
            FoxBehaviour fb = SelectedAnimal.GetComponent<FoxBehaviour>();

            AnimalTypeTxt.text = "Fox";
            if (fb.getStatus() == "Normal")
            {
                AnimalStatusTxt.text = fb.getStatus();
            }
            else
            {
                AnimalStatusTxt.text = fb.getStatus() + "!";
            }
            AnimalSpeedTxt.text = "Speed: " + fb.getSpeed().ToString("F1");
            AnimalSenseTxt.text = "Sense: " + fb.getSense().ToString("F1");

            AnimalGenderTxt.text = "Gender: " + fb.getGender();
            AnimalActionTxt.text = fb.getAction() + "...";

            if (fb.getHunger() > 100)
            {
                AnimalEnergyTxt.text = "Hunger: 100";
            }
            else
            {
                AnimalEnergyTxt.text = "Hunger: " + fb.getHunger();
            }
            AnimalMaturityTxt.text = "Maturity: " + fb.getMaturity();
            AnimalAgeTxt.text = "Age: " + fb.getAge();
        }
    }

    public void DisplayPanel()
    {
        Panel.gameObject.SetActive(true);
    }
    public void HidePanel()
    {
        Panel.gameObject.SetActive(false);
        SelectedAnimal = null;
    }

    public void ChangeClouds()
    {
        if (cloudTog.GetComponent<Toggle>().isOn)
        {
            CloudsEnabled = true;
        }
        else if(!cloudTog.GetComponent<Toggle>().isOn)
        {
            CloudsEnabled = false;
        }
        LM.enableClouds(CloudsEnabled);
    }

    public void ChangeSecondsPerHour()
    {

        if (int.TryParse(SecondsPHTxt.text, out int result))
        {
            LM.setSecondsPerHour(result);

            SecondsPHTxt.transform.Find("Text").GetComponent<Text>().color = Color.black;
            //print("Change");
        }
        else
        {
            SecondsPHTxt.transform.Find("Text").GetComponent<Text>().color = Color.red;
            //print("invalid");

        }
        
        
        //SecondsPHTxt.text = "APPLE";
    }

    public void clearAnimal()
    {
        SelectedAnimal = null;
    }

}
