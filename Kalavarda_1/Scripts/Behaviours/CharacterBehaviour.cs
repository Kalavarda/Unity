using System;
using System.Linq;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

public class CharacterBehaviour : MonoBehaviour
{
    private Text _text1;
    private readonly Player _player = Player.Instance;

    void Start()
    {
        _text1 = GetComponentsInChildren<Text>().First();
    }

    void Update()
    {
        _text1.text = string.Join(Environment.NewLine, _player.Characteristics.AllModifiers
            .OrderBy(m => m.Name)
            .Select(m => m.Name + ": " + Math.Round(m.Value, 1)));
    }
}
