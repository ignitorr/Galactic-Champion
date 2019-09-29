using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

public class PlatformController : MonoBehaviour
{
    float nextSend = 0;

    public static PlatformController instance;
    public enum PlatformModes { Mode_8Bit, Mode_Float32 };
    public PlatformModes mode = PlatformModes.Mode_Float32;

    private SerialPort serialPort;
    public string comPort;
    public int baudRate = 9600;

    public byte[] byteValues; // the six byte values to be send to the platform (8Bit Mode)
    public float[] floatValues; // the sizx float values to be sent to the platform (Float32 Mode)

    private string startFrame = "!"; // '!' startFrame character (33) (to indicate the start of a message)
    private string endFrame = "#"; // '#' endFrame character (35) (to indicate the end of a message)

    void Awake()
    {
        instance = this; // static reference to the most recent instance of this class (wannabe singleton)

        // Define and set some default values
        byteValues = new byte[] { 0, 0, 0, 0, 0, 0 };
        floatValues = new float[] { 0, 0, 0, 0, 0, 0 };
    }

    void Start()
    {
        Connect();
    }

    public void Connect()
    {

        if (serialPort == null)
        {
            serialPort = new SerialPort(@"\\.\" + comPort); // special port formating to force Unity to recognize ports beyond COM9            
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.ReadTimeout = 20; // miliseconds
        }

        // Attempt to open the SerialPort and log any errors
        try
        {
            serialPort.Open();
            Debug.Log("Initialize Serial Port: " + comPort);
        }
        catch (System.IO.IOException ex)
        {
            Debug.Log("Error opening " + comPort + "\n" + ex.Message);
        }
    }

    private void Update()
    {
        //SendSerial();
    }

    public float MapRange(float val, float min, float max, float newMin, float newMax)
    {
        return ((val - min) / (max - min) * (newMax - newMin) + newMin);
        // or Y = (X-A)/(B-A) * (D-C) + C
    }

    // SendSerial overload that uses 8bit (byte) values for each index
    public void SendSerial(byte[] _values)
    {
        System.Array.Copy(_values, byteValues, _values.Length);
        SendSerial();
    }

    // SendSerial overload that uses 32bit (float) values for each index
    public void SendSerial(float[] _values)
    {
        System.Array.Copy(_values, floatValues, _values.Length);
        SendSerial();
    }

    // The main function to send the values to our platform
    // There are two formats, one for 8bit values, and one for 32bit values
    public void SendSerial()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            if (mode == PlatformModes.Mode_8Bit)
            {
                serialPort.Write(startFrame);
                serialPort.Write(byteValues, 0, byteValues.Length);
                serialPort.Write(endFrame);
            }
            else if (mode == PlatformModes.Mode_Float32)
            {
                serialPort.Write(startFrame);
                for (int i = 0; i < floatValues.Length; i++)
                {
                    byte[] myBytes = System.BitConverter.GetBytes(floatValues[i]);
                    serialPort.Write(myBytes, 0, myBytes.Length);
                }
                serialPort.Write(endFrame);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
        {
            // Send platform to home values
            if (mode == PlatformModes.Mode_8Bit) SendSerial(new float[] { 0, 0, 0, 0, 0, 0 });
            else if (mode == PlatformModes.Mode_Float32) SendSerial(new byte[] { 128, 128, 128, 128, 128, 128 });
            serialPort.Close();
        }
    }


    public void ShakeIt(float duration, float intensity = 5)
    {
        StartCoroutine(_ShakeIt(duration, intensity));
    }

    IEnumerator _ShakeIt(float duration, float intensity)
    {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime; // increment time
            for (int i = 0; i < floatValues.Length; i++)
            {
                floatValues[i] = Random.Range(-intensity, intensity); // randomize dof
            }
            SendSerial();
            yield return 0; // allows the program stack to leave this scope and come back to the same spot in next update
        }
    }
}
