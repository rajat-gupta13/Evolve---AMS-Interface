using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Phidget22;
using Phidget22.Events;
using Uduino;

public class AMSManager : MonoBehaviour
{
    private DigitalInput downButton = new DigitalInput();        //used to register when the player presses the scan button
    private DigitalInput upButton = new DigitalInput();
    private VoltageInput selectionSlider = new VoltageInput();

    private RFID RFIDMicroscope = new RFID();           //used for the RFID Scanner

    OscOut _oscOut;
    OscIn _oscIn;

    public string earthIceRFIDTagSting;
    public string marsIceRFIDTagString;
    public string teethRFIDTagString;
    public string ringRFIDTagString;

    public Image chartImage;
    public GameObject imageHolder;
    public Text dataNumber;
    public GameObject introduction;
    public GameObject noSampleScanned;
    public GameObject incorrectSample;
    
    public Sprite earthIceChart;
    public Sprite marsIceChart;
    public Sprite ringChart;
    public Sprite teethChart;
    public Sprite leadUraniumNoScanChart;
    public Sprite carbonNoScanChart;

    private bool tagPresent = false;
    private bool leadUraniumDating = false;
    private bool carbonDating = false;
    //private List<MicroscopeSamples> microscopeSamples;



    //public AudioClip MicroscopeClick;
    //public AudioClip MicroscopeBackground;
    //public AudioSource MicroscopeClickSource;
    //public AudioSource MicroscopeBackgroundSource;
    private int scanCounter = 0;
    string address1 = "/amsie";
    string address2 = "/amsim";
    string address3 = "/amst";
    string address4 = "/amsr";
    string address5 = "/amscn";
    string address6 = "/amslun";

    public AudioSource amsSFX;
    public AudioClip amsClip;

    public GameObject scanning;

    private bool clipPlaying = false;

    public float effectTimer = 16.5f;
    //private bool timerOn = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Displays connected: " + Display.displays.Length);

        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();

        imageHolder.SetActive(false);
        dataNumber.gameObject.SetActive(false);
        introduction.SetActive(true);
        noSampleScanned.SetActive(false);

        _oscOut = gameObject.AddComponent<OscOut>();
        _oscIn = gameObject.AddComponent<OscIn>();

        _oscIn.Open(8000);
        _oscOut.Open(8000, "255.255.255.255");


        _oscIn.MapInt("/resetasm", ResetMachine);
        //MicroscopeBackgroundSource.Play();

        downButton.DeviceSerialNumber = 523979;
        downButton.Channel = 0;
        downButton.IsLocal = true;
        downButton.Attach += digitalInput_Attach;
        downButton.StateChange += downButton_StateChange;

        upButton.DeviceSerialNumber = 523979;
        upButton.Channel = 1;
        upButton.IsLocal = true;
        upButton.Attach += digitalInput_Attach;
        upButton.StateChange += upButton_StateChange;

        selectionSlider.DeviceSerialNumber = 523979;
        selectionSlider.Channel = 0;
        selectionSlider.IsLocal = true;
        selectionSlider.Attach += voltageInput_Attach;
        selectionSlider.VoltageChange += selectionSlider_StateChange;

        RFIDMicroscope.DeviceSerialNumber = 452966;
        RFIDMicroscope.Channel = 0;
        RFIDMicroscope.IsLocal = true;
        RFIDMicroscope.Attach += rfid_Attach;
        RFIDMicroscope.Tag += rfid_Tag;
        RFIDMicroscope.TagLost += rfid_TagLost;



        try
        {
            downButton.Open(5000);
            upButton.Open(5000);
            selectionSlider.Open(5000);
            RFIDMicroscope.Open(5000);
        }
        catch (PhidgetException e)
        {
            Debug.Log("Failed: " + e.Message);
        }

        // StartCoroutine(TurnRFIDAntennaOff());
    }

    private void ResetMachine(int args)
    {
        imageHolder.SetActive(false);
        introduction.SetActive(true);
        noSampleScanned.SetActive(false);
    }
   

    void OnDestroy()
    {
        downButton.Close();
        upButton.Close();
        selectionSlider.Close();
        RFIDMicroscope.Close();
    }

    private IEnumerator StartArduinoLights()
    {
        yield return null;
        Debug.Log("Start Lights");
        UduinoManager.Instance.sendCommand("turnOn");
        //timerOn = true;
        yield return new WaitForSeconds(16.5f);
        Debug.Log("End Lights");
        UduinoManager.Instance.sendCommand("turnOff");
        //timerOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            downButton.Attach -= digitalInput_Attach;
            downButton.StateChange -= downButton_StateChange;
            downButton.Close();
            downButton = null;

            upButton.Attach -= digitalInput_Attach;
            upButton.StateChange -= upButton_StateChange;
            upButton.Close();
            upButton = null;

            selectionSlider.Attach -= digitalInput_Attach;
            selectionSlider.VoltageChange -= selectionSlider_StateChange;
            selectionSlider.Close();
            selectionSlider = null;

            RFIDMicroscope.Attach -= rfid_Attach;
            RFIDMicroscope.Tag -= rfid_Tag;
            RFIDMicroscope.TagLost -= rfid_TagLost;
            RFIDMicroscope.Close();
            RFIDMicroscope = null;

            Application.Quit();
        }

    }



    void OnApplicationQuit()
    {
        if (Application.isEditor)
            Phidget.FinalizeLibrary(0);
        else
            Phidget.FinalizeLibrary(0);
    }

    void rfid_Tag(object sender, RFIDTagEventArgs e)
    {
        tagPresent = true;
        Debug.Log("Tag Present: " + tagPresent);
    }

    void rfid_TagLost(object sender, RFIDTagLostEventArgs e)
    {
        tagPresent = false;
        Debug.Log("Tag Present: " + tagPresent);
    }

    private IEnumerator TurnRFIDAntennaOff()
    {
        yield return new WaitForSeconds(0.5f);
        RFIDMicroscope.AntennaEnabled = false;
    }

    void digitalInput_Attach(object sender, Phidget22.Events.AttachEventArgs e)
    {
        DigitalInput attachedDevice = ((DigitalInput)sender);
        int deviceSerial = attachedDevice.DeviceSerialNumber;
        Debug.Log("Attached device " + attachedDevice.DeviceSerialNumber);
    }

    void voltageInput_Attach(object sender, Phidget22.Events.AttachEventArgs e)
    {
        VoltageInput attachedDevice = ((VoltageInput)sender);
        int deviceSerial = attachedDevice.DeviceSerialNumber;
        Debug.Log("Attached device " + attachedDevice.DeviceSerialNumber);
    }

    void rfid_Attach(object sender, Phidget22.Events.AttachEventArgs e)
    {
        RFID attachedDevice = ((RFID)sender);
        int deviceSerial = attachedDevice.DeviceSerialNumber;
        Debug.Log("Attached device " + attachedDevice.DeviceSerialNumber);
    }

    void downButton_StateChange(object sender, Phidget22.Events.DigitalInputStateChangeEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(DownButtonPressed(downButton.State));
    }

    void upButton_StateChange(object sender, Phidget22.Events.DigitalInputStateChangeEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(UpButtonPressed(downButton.State));
    }

    void selectionSlider_StateChange(object sender, Phidget22.Events.VoltageInputVoltageChangeEventArgs e)
    {
        VoltageInput sliderVoltage = (VoltageInput)sender;
        UnityMainThreadDispatcher.Instance().Enqueue(SliderVoltageChange(sliderVoltage.Voltage));
    }



    public IEnumerator SliderVoltageChange(double voltage)
    {
        Debug.Log("This is executed from the main thread");
        if (voltage <= 2.5d)
        {
            leadUraniumDating = true;
            carbonDating = false;
        }
        else if (voltage > 2.5d)
        {
            carbonDating = true;
            leadUraniumDating = false;
        }
        yield return null;
    }


    private IEnumerator WaitTillClipEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        clipPlaying = false;
    }

    public IEnumerator UpButtonPressed(bool state)
    {
        Debug.Log("This is executed from the main thread");
        switch (state)
        {
            case true:

                break;
            case false:

                break;
        }
        yield return null;
    }

    public IEnumerator DownButtonPressed(bool state)
    {
        Debug.Log("This is executed from the main thread");

        switch (state)
        {

            case true:
                //MicroscopeClickSource.PlayOneShot(MicroscopeClick);
                if (!clipPlaying)
                {
                    clipPlaying = true;
                    amsSFX.clip = amsClip;
                    amsSFX.Play();
                    StartCoroutine(WaitTillClipEnd(amsClip.length));
                    introduction.SetActive(false);
                    imageHolder.SetActive(false);
                    dataNumber.gameObject.SetActive(false);
                    scanning.SetActive(true);
                    noSampleScanned.SetActive(false);
                    StartCoroutine(StartArduinoLights());
                    yield return new WaitForSeconds(16.5f);
                    RFIDMicroscope.AntennaEnabled = true;
                    if (tagPresent)
                    {
                        Debug.Log("Tag scanned: " + RFIDMicroscope.GetLastTag().TagString);
                        scanning.SetActive(false);
                        if (RFIDMicroscope.GetLastTag().TagString == earthIceRFIDTagSting && carbonDating)
                        {
                            scanning.SetActive(false);
                            imageHolder.SetActive(true);
                            dataNumber.gameObject.SetActive(true);
                            scanCounter++;
                            dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                            chartImage.sprite = earthIceChart;
                            _oscOut.Send(address1, scanCounter);
                        }
                        else if (RFIDMicroscope.GetLastTag().TagString == marsIceRFIDTagString && carbonDating)
                        {
                            imageHolder.SetActive(true);
                            dataNumber.gameObject.SetActive(true);
                            scanCounter++;
                            dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                            chartImage.sprite = marsIceChart;
                            _oscOut.Send(address2, scanCounter);
                            scanning.SetActive(false);
                        }
                        else if (RFIDMicroscope.GetLastTag().TagString == teethRFIDTagString && carbonDating)
                        {
                            imageHolder.SetActive(true);
                            dataNumber.gameObject.SetActive(true);
                            scanCounter++;
                            dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                            chartImage.sprite = teethChart;
                            _oscOut.Send(address3, scanCounter);
                            scanning.SetActive(false);
                        }
                        else if (RFIDMicroscope.GetLastTag().TagString == ringRFIDTagString && carbonDating)
                        {
                            imageHolder.SetActive(true);
                            dataNumber.gameObject.SetActive(true);
                            scanCounter++;
                            dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                            chartImage.sprite = carbonNoScanChart;
                            _oscOut.Send(address5, scanCounter);
                            scanning.SetActive(false);
                        }
                        else if (RFIDMicroscope.GetLastTag().TagString == ringRFIDTagString && leadUraniumDating)
                        {
                            imageHolder.SetActive(true);
                            dataNumber.gameObject.SetActive(true);
                            scanCounter++;
                            dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                            chartImage.sprite = ringChart;
                            _oscOut.Send(address4, scanCounter);
                            scanning.SetActive(false);
                        }
                        else if ((RFIDMicroscope.GetLastTag().TagString == earthIceRFIDTagSting || RFIDMicroscope.GetLastTag().TagString == marsIceRFIDTagString || RFIDMicroscope.GetLastTag().TagString == teethRFIDTagString) && leadUraniumDating)
                        {
                            imageHolder.SetActive(true);
                            dataNumber.gameObject.SetActive(true);
                            scanCounter++;
                            dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                            chartImage.sprite = leadUraniumNoScanChart;
                            _oscOut.Send(address6, scanCounter);
                            scanning.SetActive(false);
                        }
                        else
                        {
                            if (leadUraniumDating)
                            {
                                imageHolder.SetActive(true);
                                dataNumber.gameObject.SetActive(true);
                                scanCounter++;
                                dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                                chartImage.sprite = leadUraniumNoScanChart;
                                _oscOut.Send(address6, scanCounter);
                                scanning.SetActive(false);
                            }
                            else if (carbonDating)
                            {
                                imageHolder.SetActive(true);
                                dataNumber.gameObject.SetActive(true);
                                scanCounter++;
                                dataNumber.text = "Data #: CMNHAMS0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + scanCounter.ToString("00");
                                chartImage.sprite = carbonNoScanChart;
                                _oscOut.Send(address5, scanCounter);
                                scanning.SetActive(false);
                            }
                        }

                    }
                    else
                    {
                        imageHolder.SetActive(false);
                        introduction.SetActive(false);
                        noSampleScanned.SetActive(true);
                        scanning.SetActive(false);
                    }
                }
                break;
            case false:
                //RFIDMicroscope.AntennaEnabled = false;
                break;
        }
        yield return null;
    }
}
