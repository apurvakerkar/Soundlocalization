using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System.Collections.Generic;

public class CalculateVector3DDirectionFromTwoViveTrackers : MonoBehaviour
{
    [SerializeField]
    private Transform tracker_1;

    [SerializeField]
    private Transform tracker_2;

    [SerializeField]
    private string text_file_path;

    // IP address and port of the Python server
    public string serverIP = "127.0.0.1";
    public int port = 12346;

    public bool trigger = false;

    private TcpClient client;


    void Start()
    {
        StreamWriter writer = new StreamWriter(text_file_path, true);
        string string_to_write = "\t\t time \t\t\t sound_file_path \t\t\t diff \t\t direction \t\t direction_normalized \t\t position_original \t\t position_target";
        writer.WriteLine(string_to_write);
        writer.Close();

        Debug.Log(ChaperoneInfo.instance.playAreaSizeX.ToString());
        Debug.Log(ChaperoneInfo.instance.playAreaSizeZ.ToString());

        // Start coroutine to connect to Python server
        StartCoroutine(ConnectToServer());
    }

    void Update()
    {
        Vector3 direction = tracker_2.position - tracker_1.position;
        Debug.DrawRay(tracker_1.position, direction.normalized, Color.red);
        Debug.Log("direction:" + direction);
        Debug.Log("direction normalized:" + (direction.normalized * 4 * Time.deltaTime));

        // StreamWriter writer = new StreamWriter(text_file_path, true);
        string string_to_write = System.DateTime.Now.ToString() + "\t\t\t" + "some\\path\\to\\sound\\file.wav" + "\t\t\t" + "?????" + "\t\t" +
                                 direction.ToString() + "\t\t" + (direction.normalized * 4 * Time.deltaTime).ToString() + "\t\t" +
                                 tracker_1.position.ToString() + "\t\t" + tracker_2.position.ToString();
        // writer.WriteLine(string_to_write);
        // writer.Close();

        if (trigger == true)
        {
            StartCoroutine(SendData(string_to_write));
        }


        Debug.Log(ChaperoneInfo.instance.playAreaSizeX.ToString());
        Debug.Log(ChaperoneInfo.instance.playAreaSizeZ.ToString());

    }

    // Coroutine to connect to Python server
    IEnumerator ConnectToServer()
    {
        client = new TcpClient();
        yield return StartCoroutine(ConnectAsync());
        Debug.Log("Connected to Python server.");

        // Send a message to Python to indicate that recording should start
        string message = "StartRecording";
        yield return StartCoroutine(SendData(message));

        trigger = true;
    }

    // Asynchronous connection to the server
    IEnumerator ConnectAsync()
    {
        yield return new WaitForEndOfFrame(); // Wait until the end of the frame to avoid blocking the main thread
        yield return client.ConnectAsync(serverIP, port);
    }

    // Asynchronous data sending
    IEnumerator SendData(string data)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        yield return client.GetStream().WriteAsync(byteData, 0, byteData.Length);
    }
}