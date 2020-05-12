using Assets.Scripts.Model;
using Assets.Scripts.Model.Skills;
using Assets.Scripts.Utils;
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

        if (Input.GetKeyDown(KeyCode.F))
        {
            var thing = ThingsBehaviour.Instance.SearchNearly(PlayerGameObject);
            if (thing != null)
            {
                var gameObj = ThingsBehaviour.Instance.Things[thing];
                var distance = Utils.Distance(gameObj, PlayerGameObject);
                Player.Use(new UseThing(Player, thing), Player, distance, null);
                gameObj.SetActive(false);
            }
        }
    }
}
