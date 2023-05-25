using System;
using TMPro;
using UnityEngine;

public class FpsUi : MonoBehaviour
{
    [System.Serializable]
    private struct UiSettings
    {
        public TMP_Text FpsText;
        public TMP_Text MinFrameTimeText;
        public TMP_Text MaxFrameTimeText;
        public TMP_Text MinMaxFrameTimeDeltaText;
    }


    [SerializeField] private int _bufferSize = 32;
    [SerializeField] private UiSettings _uiSettings;

    private float[] _buffer;
    private int _index;


    private void Awake()
    {
        _buffer = new float[_bufferSize];

        if (_bufferSize < 1)
            enabled = false;
    }

    private void Update()
    {
        _buffer[_index++] = Time.deltaTime;

        if (_index == _bufferSize)
        {
            _index = 0;

            CalculateValues(out float fps, out float min,
                out float max, out float minMaxDelta);

            var ui = _uiSettings;
            ui.FpsText.text = $"FPS: {fps:N0}";
            ui.MinFrameTimeText.text = $"MIN: {min:N4}";
            ui.MaxFrameTimeText.text = $"MAX: {max:N4}";
            ui.MinMaxFrameTimeDeltaText.text = $"DELTA: {minMaxDelta:N4}";
        }
    }

    private void CalculateValues(out float fps, out float min, out float max, out float minMaxDelta)
    {
        var len = _bufferSize;

        if (len == 0)
        {
            fps = min = max = Time.deltaTime;
            minMaxDelta = 0;
            return;
        }

        var span = _buffer.AsSpan(0, len);

        fps = min = max = span[0];

        for (int i = 1; i < len; i++)
        {
            var frameTime = span[i];

            if (frameTime > max)
                max = frameTime;

            if (frameTime < min)
                min = frameTime;

            fps += frameTime;
        }

        fps /= len;
        fps = 1f / fps;
        minMaxDelta = max - min;
    }
}
