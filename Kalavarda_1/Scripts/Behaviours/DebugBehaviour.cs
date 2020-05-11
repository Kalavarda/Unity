using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugBehaviour : MonoBehaviour
{
    private readonly IDictionary<string, string> _values = new Dictionary<string, string>();

    public static DebugBehaviour Instance { get; private set; }

    public DebugBehaviour()
    {
        Instance = this;
    }

    void OnGUI()
    {
        if (_values.Any())
        {
            const int width = 300;
            const int height = 200;
            var rect = new Rect(5, 100, width, height);
            GUI.Box(rect, string.Join(Environment.NewLine, _values.OrderBy(p => p.Key).Select(p => p.Key + ": " + p.Value)));
        }
    }

    public void Show(string key, string value)
    {
        if (_values.ContainsKey(key))
            _values[key] = value;
        else
            _values.Add(key, value);
    }
}
