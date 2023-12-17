using System.IO.Ports;
using UnityEngine;

public class ArduinoManager : Singleton<ArduinoManager> {

    private SerialPort serialPort;

    private AudioClip clip;
    private AudioSource source;
    private float[] data;

    public void PlayClip(AudioClip clip) {
        this.clip = clip;
        source.clip = clip;
        data = new float[source.clip.samples * source.clip.channels];
        source.clip.GetData(data, 0);
        source?.Play();
    }

    public override void Initialize() {
        serialPort = new SerialPort("COM3", 9600) {
            ReadTimeout = 100
        };

        source = GetComponent<AudioSource>();
        clip = source.clip;
        ConnectArduino();

        base.Initialize();
    }

    private void ConnectArduino() {
        try {
            serialPort?.Open();
            Debug.Log("Connected Arduino!");
        }
        catch {
            Debug.LogWarning("Could not open serial port. Is the Arduino connected?");
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
        var amplitude = data[index];
        float loudness = amplitude * 255;
        Debug.Log(loudness);
        return loudness;
    }

    private void Update() {
        if (serialPort.IsOpen) {
            float loudness = SampleLoudness();
            byte volume = (byte)Mathf.RoundToInt(loudness);
            byte[] buffer = new byte[] { volume };
            serialPort.Write(buffer, 0, 1);
        }

        if (!GameManager.debug) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            PlayClip(clip);
        }
    }

    protected override void OnDestroy() {
        DisconnectArduino();
        base.OnDestroy();
    }
}