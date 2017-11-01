using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UIManager : MonoBehaviour
{
    public GameManager GM;
    public WindManager WM;
    public DroneManager DM;

    private Slider _windSlider;
    private InputField _windSliderInput;

    private Slider _droneMassSlider;
    private InputField _droneMassSliderInput;


    private Slider _droneDragSlider;
    private InputField _droneDragSliderInput;

    private InputField _droneMotorsMaxThrust;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Mat: " + A);
        //WM = new WindManager();
        GetComponentInChildren<Canvas>().enabled = false;
        _windSlider = GameObject.Find("Wind Slider").GetComponent<Slider>();
        _windSliderInput = GameObject.Find("Wind Slider Input").GetComponent<InputField>();
        _windSliderInput.text = _windSlider.value.ToString();

        _droneMassSlider = GameObject.Find("Mass Slider").GetComponent<Slider>();
        _droneMassSliderInput = GameObject.Find("Mass Slider Input").GetComponent<InputField>();
        _droneMassSliderInput.text = _droneMassSlider.value.ToString();
        _droneMassSlider.value = 1.0f;

        _droneDragSlider = GameObject.Find("Drag Slider").GetComponent<Slider>();
        _droneDragSliderInput = GameObject.Find("Drag Slider Input").GetComponent<InputField>();
        _droneDragSliderInput.text = _droneDragSlider.value.ToString();
        _droneDragSlider.value = 1.0f;

        _droneMotorsMaxThrust = GameObject.Find("MaxThrust Input").GetComponent<InputField>();
        _droneMotorsMaxThrust.text = "0.5";
    }

    // Update is called once per frame
    void Update()
    {
        ScanForKeyStroke();
        ScanForKeyRestart();
    }

    void ScanForKeyStroke()
    {
        /*for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick 1 button " + i))
            {
                print("joystick 1 button " + i);
            }
        }*/
        if (Input.GetKeyDown("escape") || Input.GetKeyDown("joystick 1 button 7"))
        {
            GM.TogglePauseMenu();
        }
    }


    void ScanForKeyRestart()
    {
        if (Input.GetKeyDown("r") || Input.GetKeyDown("joystick 1 button 6"))
        {
            DM.restartPositionRotation();
        }
    }

    //-----------------------------------------------------------
    // Game Options Function Definitions
    public void OptionSliderUpdate(float val) {  }
    void SetCustomSettings(bool val) { }
    //void WriteSettingsToInputText(GameSettings settings) {  }

    //-----------------------------------------------------------
    // Music Settings Function Definitions

    //Debug.Log("sdfrvgs " + typeof(mySlider.value));
    //Debug.Log("float value changed: " + value);
    float tmp;
    public void UpdateInputFieldValueFromFloat(Slider mySlider, InputField myInputField)
    {
        /*if (mySlider.isActiveAndEnabled)
        {
            Debug.Log("sdfrvgs " + mySlider.value);
        }*/
        if (string.IsNullOrEmpty(myInputField.text))
        {
            tmp = 0.0f;
        }
        else
        {
            tmp = Single.Parse(myInputField.text);
        }
        if (mySlider.isActiveAndEnabled)
        {
            mySlider.value = tmp;
        }
    }

    public void UpdateSliderValueFromFloat(Slider mySlider, InputField myInputField)
    {
        if (myInputField.isActiveAndEnabled)
        {
            Debug.Log("InputField Active");
            myInputField.text = mySlider.value.ToString();
        }      
    }

    public void UpdateValueFromString(Slider mySlider, InputField myInputField, string value)
    {
        //Debug.Log("string value changed: " + value);
        if (mySlider) { mySlider.value = float.Parse(value); }
        if (myInputField) { myInputField.text = value; }
    }

    public void WindSliderUpdate()
    {
        //_windSliderText.text = val.ToString("0.0");
        UpdateSliderValueFromFloat(_windSlider, _windSliderInput);
        WM.SetWind(_windSlider.value);
    }

    public void WindSliderInputUpdate()
    {
        UpdateInputFieldValueFromFloat(_windSlider, _windSliderInput);
        WM.SetWind(_windSlider.value);
    }

    public void WindToggle(bool val)
    {
        _windSlider.interactable = val;
        WM.SetWind(val ? _windSlider.value : 0f);
    }



    public void DroneMassSliderUpdate()
    {
        //_windSliderText.text = val.ToString("0.0");
        UpdateSliderValueFromFloat(_droneMassSlider, _droneMassSliderInput);
        DM.SetMass(_droneMassSlider.value);
    }

    public void DroneMassSliderInputUpdate()
    {
        UpdateInputFieldValueFromFloat(_droneMassSlider, _droneMassSliderInput);
        DM.SetMass(_droneMassSlider.value);
    }

    public void DroneMassToggle(bool val)
    {
        _droneMassSlider.interactable = val;
        DM.SetMass(val ? _droneMassSlider.value : 0f);
    }



    public void DroneDragSliderUpdate()
    {
        //_windSliderText.text = val.ToString("0.0");
        UpdateSliderValueFromFloat(_droneDragSlider, _droneDragSliderInput);
        DM.SetDrag(_droneDragSlider.value);
    }

    public void DroneDragSliderInputUpdate()
    {
        UpdateInputFieldValueFromFloat(_droneDragSlider, _droneDragSliderInput);
        DM.SetDrag(_droneDragSlider.value);
    }

    public void DroneDragToggle(bool val)
    {
        _droneDragSlider.interactable = val;
        DM.SetDrag(val ? _droneDragSlider.value : 0f);
    }

    public void DroneMotorsMaxThrust()
    {
        DM.setMaxThrust(Single.Parse(_droneMotorsMaxThrust.text));
       
    }

    public void DroneRestart()
    {
        DM.restartPositionRotation();
    }

}
