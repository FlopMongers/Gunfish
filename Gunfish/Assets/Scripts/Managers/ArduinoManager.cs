using UnityEngine;
using System.IO.Ports;

public class ArduinoManager : Singleton<ArduinoManager> {

    private SerialPort serialPort = new SerialPort("COM4", 9600);

    private AudioClip clip;
    private AudioSource source;
    private float[] data;

    private void OnEnable() {
        try {
            serialPort?.Open();
        } catch {
            Debug.LogWarning("Could not open serial port. Is the Arduino connected?");
        }
    }

    private void Start() {
        serialPort.ReadTimeout = 100;
        
        source = GetComponent<AudioSource>();
        clip = source.clip;

        data = new float[clip.samples * clip.channels];
    }

    private float SampleLoudness() {
        var time = source.timeSamples;
        clip.GetData(data, time);

        // Calculate loudness for the first second of the audio
        // Replace these numbers to adjust for desired interval
        int samplesPerSecond = clip.frequency * clip.channels;
        float amplitude = 0;
        float sumDerivatives = 0;

        int sampleCount = samplesPerSecond / 8;

        for (int i = 1; i < sampleCount; i++) {
            amplitude += Mathf.Abs(data[i]);
            sumDerivatives += Mathf.Abs(data[i]-data[i-1]);
        }

        amplitude /= sampleCount;
        sumDerivatives /= sampleCount - 1;

        // f(t) = a * s(t) + b * (s(t) - s(t-1))

        float loudness = amplitude * 255;
        // loudness = Mathf.sin(sumDerivatives * 255*2f,255);

        return loudness;
    }

    private void Update() {
        if (serialPort.IsOpen) {
            // Send an oscillating byte value from 0 to 255
            byte volume = (byte)Mathf.RoundToInt(SampleLoudness());
            byte[] buffer = new byte[] {volume};
            serialPort.Write(buffer, 0, 1);
        }

        if (!GameManager.debug) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            source?.Play();
        }
    }

    private void OnDisable() {
        serialPort?.Close();
    }
}
