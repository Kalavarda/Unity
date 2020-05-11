using Assets.Scripts.Model;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public static PlayerBehaviour Instance { get; private set; }

    public GameObject PlayerGameObject => gameObject;

    public Player Player => Player.Instance;

    void Start()
    {
        Instance = this;
        Player.Died += PlayerDied;
    }

    private void PlayerDied(IHealth player)
    {
        Application.Quit();
    }

    void Update()
    {
        Player.Update();
    }
}
