using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ArduinoManager : Singleton<ArduinoManager> {

    [SerializeField]
    [Range(0.1f, 10f)]
    private float frequency = 1f;
    private SerialPort serialPort = new SerialPort("COM4", 9600);

    private AudioClip clip;
    private AudioSource source;
    private float[] data;

    private void OnEnable() {
        serialPort.Open();
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
        float loudness = 0;

        int sampleCount = samplesPerSecond / 60;

        for (int i = 0; i < sampleCount; i++) {
            loudness += Mathf.Abs(data[i]);
        }

        loudness = loudness / sampleCount * 255f;
        return loudness;
    }

    private void Update() {
        if (serialPort.IsOpen) {
            // Send an oscillating byte value from 0 to 255
            byte volume = (byte)Mathf.RoundToInt(SampleLoudness());
            byte[] buffer = new byte[] {volume};
            serialPort.Write(buffer, 0, 1);
        }
    }

    private void OnDisable() {
        serialPort.Close();
    }
}
