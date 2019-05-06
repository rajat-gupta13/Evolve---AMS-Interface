using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net.Sockets;
using System;

public class PuppetManager : MonoBehaviour
{
    OscOut _oscOut;
    OscIn _oscIn;
    private string[] data;
    public RawImage rawimage;  //Image for rendering what the camera sees.
    Texture2D loadTexture = null;
    byte[] fileData;
    private string savePath = "C:\\Users\\demo\\Documents\\WebcamSnapshots\\";
    public string address = "/scan";
    public string capturedImage = "capturedScan";

    bool socketReady = false;                // global variables are setup here
    TcpClient mySocket;
    public NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;
    public String Host = "192.168.0.203";
    public Int32 Port = 5000;

    // Start is called before the first frame update
    void Start()
    {
        _oscOut = gameObject.AddComponent<OscOut>();
        _oscIn = gameObject.AddComponent<OscIn>();

        _oscIn.Open(8000);
        _oscOut.Open(8000, "255.255.255.255");

        _oscIn.MapInt("/puppet", LoadImage);
        SetupSocket();
    }

    public String ReadSocket()
    {                        // function to read data in
        if (!socketReady)
            return "";
        if (theStream.DataAvailable)
            return theReader.ReadLine();
        return "NoData";
    }

    public void CloseSocket()
    {                            // function to close the socket
        if (!socketReady)
            return;
        theWriter.Close();
        theReader.Close();
        mySocket.Close();
        socketReady = false;
    }

    public void MaintainConnection()
    {                    // function to maintain the connection (not sure why! but Im sure it will become a solution to a problem at somestage)
        if (!theStream.CanRead)
        {
            SetupSocket();
        }
    }

    // Update is called once per frame
    void SetupSocket()
    {

        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            theReader = new StreamReader(theStream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error:" + e);
        }
    }

    public void RobotBatteryPowered()
    {
        theWriter.Write("eyes blue");
        theWriter.Flush();
    }

    public void Start45MinVideo()
    {
        _oscOut.Send("/video", 1);
    }

    public void PostShowHappy()
    {
        _oscOut.Send("/video", 2);
    }

    public void PostShowSad()
    {
        _oscOut.Send("/video", 3);
    }

    public void OpenCrate1()
    {
        _oscOut.Send("/crate", 1);
    }

    public void OpenCrate2()
    {
        _oscOut.Send("/crate", 2);
    }

    public void OpenCrate3()
    {
        _oscOut.Send("/crate", 3);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            LoadImage(1);
        }
    }

    public void ScanPelvis()
    {
        _oscOut.Send(address, "pelvis");
    }

    public void ScanHand()
    {
        _oscOut.Send(address, "hand");
    }

    public void ScanIncomplete()
    {
        _oscOut.Send(address, "incomplete");
    }

    public void ResetBCD()
    {
        _oscOut.Send("/resetbcd", 1);
    }

    public void ResetSEM()
    {
        _oscOut.Send("/resetsem", 1);
    }

    public void ResetAMS()
    {
        _oscOut.Send("/resetams", 1);
    }

    public void ResetPostShow()
    {
        _oscOut.Send("/resetpost", 1);
    }

    public void ResetDOS()
    {
        _oscOut.Send("/resetdos", 1);
    }

    public void AllowReport()
    {
        _oscOut.Send("/dosreport", true);
    }

    void LoadImage(int captureCounter)
    {
        string filePath = savePath + capturedImage + ".png";
        loadTexture = new Texture2D(2,2);
        if (File.Exists(filePath))
        {
            Debug.Log("File exists");
            fileData = File.ReadAllBytes(filePath);
            loadTexture.LoadImage(fileData);
        }
        rawimage.texture = loadTexture;
        rawimage.material.mainTexture = loadTexture;
    }
}
