using System;
using System.Net.Sockets;
using UnityEngine;

public class CSharpClient : MonoBehaviour {
    TcpClient client;
    NetworkStream stream;

    void Start() {
        ConnectToServer();
    }

    void ConnectToServer() {
        try {
            client = new TcpClient("127.0.0.1", 8080);
            stream = client.GetStream();
            Debug.Log("Connected to server.");
        }
        catch (Exception e) {
            Debug.Log("Error connecting to server: " + e.Message);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("Space key was pressed!");
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent input to server.");
        }
    }
}
