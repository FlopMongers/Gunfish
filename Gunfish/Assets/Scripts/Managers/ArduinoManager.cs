using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class ArduinoManager : Singleton<ArduinoManager> {

    public List<AudioClip> attractorLines;
    public AudioClip HARK;

    public float secondsBetweenAttractors = 60f;
    private float secondsSinceLastAttractor;

    private SerialPort serialPort;

    private AudioClip clip;
    private AudioSource source;
    private float[] data;

    public bool playAttractors;

    public float loudness;

    private bool initialized = false;

    private void Attractor() {
        if (!playAttractors) {
            secondsSinceLastAttractor = 0f;
            return;
        }

        if (secondsSinceLastAttractor > secondsBetweenAttractors) {
            secondsSinceLastAttractor = 0f;

            if (Random.Range(0, 1000) == 0) {
                clip = HARK;
            } else {
                clip = attractorLines[Random.Range(0, attractorLines.Count)];
            }
            secondsSinceLastAttractor -= PlayClip(clip);
        } else {
            secondsSinceLastAttractor += Time.deltaTime;
        }
    }

    public float PlayClip(AudioClip clip, float decibels = 0f) {
        this.clip = clip;
        source.clip = clip;
        data = new float[source.clip.samples * source.clip.channels];
        source.clip.GetData(data, 0);
        source?.Play();
        return clip.length;
    }

    public override void Initialize() {
        if (GameManager.Instance.debug) {
            secondsBetweenAttractors = 5f;
        }
        secondsSinceLastAttractor = 0f;
        source = GetComponent<AudioSource>();
        clip = source.clip;
        ConnectArduino();

        initialized = true;
        base.Initialize();
    }
    private void HandleArduino() {
        if (serialPort.IsOpen) {
            float loudness = SampleLoudness();
            byte volume = (byte)Mathf.RoundToInt(loudness);
            byte[] buffer = new byte[] { volume };
            serialPort.Write(buffer, 0, 1);
        }
    }

    private void ConnectArduino() {
        foreach (var port in new string[] { "COM3", "COM4", "COM5" }) {
            serialPort = new SerialPort(port, 9600) {
                ReadTimeout = 100
            };
            try {
                serialPort?.Open();
                Debug.Log($"Connected Arduino on port {port}");
                break;
            } catch {
                Debug.Log($"Failed to connect Arduino on port {port}");
            }
        }
    }

    private void DisconnectArduino() {
        if (serialPort != null && serialPort.IsOpen) {
            serialPort?.Close();
            Debug.Log("Disconnected Arduino!");
        }
    }

    private float SampleLoudness() {
        if (data == null || source.timeSamples > data.Length) {
            return 0f;
        }

        var index = source.timeSamples;
        if (index >= data.Length) {
            return 0f;
        }
        var amplitude = data[index];
        loudness = Mathf.Abs(amplitude * 255);
        return loudness;
    }

    private void Update() {
        if (!initialized) return;
        Attractor();
        HandleArduino();
    }

    protected override void OnDestroy() {
        DisconnectArduino();
        base.OnDestroy();
    }
}