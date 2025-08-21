using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[Serializable]
public class DetectionPayload
{
    public bool person_detected;
}

public class PythonReceiver : MonoBehaviour
{
    public int port = 50007;
    private TcpListener listener;
    private Thread listenThread;
    private bool lastDetected = false;
    private string status = "Waiting...";

    void Start()
    {
        listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listenThread = new Thread(ListenLoop);
        listenThread.IsBackground = true;
        listenThread.Start();
        Debug.Log("Listening on port " + port);
    }

    private void ListenLoop()
    {
        while (true)
        {
            try
            {
                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var payload = JsonUtility.FromJson<DetectionPayload>(line);
                        lastDetected = payload.person_detected;
                        status = "Received: " + line;
                    }
                }
            }
            catch { }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 600, 20), "Status: " + status);
        GUI.Label(new Rect(10, 40, 600, 20),
            lastDetected ? "Person detected!" : "No person detected.");
    }
}