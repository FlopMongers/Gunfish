using System.IO.Ports;
using UnityEngine;

public class ArduinoManager : Singleton<ArduinoManager> {

    private SerialPort serialPort = new SerialPort("COM3", 9600);

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

    private void OnEnable() {
        try {
            serialPort?.Open();
            Debug.Log("Connected Arduino!");
        }
        catch {
            Debug.LogWarning("Could not open serial port. Is the Arduino connected?");
        }
    }

    private void Start() {
        serialPort.ReadTimeout = 100;

        source = GetComponent<AudioSource>();
        clip = source.clip;
    }

    private float SampleLoudness() {
        if (data == null || source.timeSamples > data.Length) {
            return 0f;
        }

        var index = source.timeSamples;
        var amplitude = data[index];
        float loudness = amplitude * 255;
        var debug = $"Amplitude at {index}/{data.Length}: {amplitude} ({loudness})";
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

    private void OnDisable() {
        serialPort?.Close();
    }
}