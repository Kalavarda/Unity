using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Skills;
using Assets.Scripts.Utils;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    private readonly UseThing _useThingSkill;
    private IThing _usableThing;

    public PlayerBehaviour()
    {
        _useThingSkill = Player.Skills.OfType<UseThing>().First();
    }

    public static PlayerBehaviour Instance { get; private set; }

    public GameObject PlayerGameObject => gameObject;

    public Player Player => Player.Instance;

    void Start()
    {
        Instance = this;
        Player.Died += PlayerDied;

        _useThingSkill.OnEndCast += OnEndSkillCast;
    }

    private void OnEndSkillCast(ICastableSkill skill)
    {
        if (skill is UseThing useThingSkill)
            if (useThingSkill.Thing == _usableThing)
            {
                var gameObj = ThingsBehaviour.Instance.Things[_usableThing];
                gameObj.SetActive(false);
            }
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
                var skillContext = new SkillContext(Player, distance, 0);

                if (!_useThingSkill.ReadyToUse(skillContext))
                    return;

                _usableThing = thing;
                _useThingSkill.Thing = thing;

                Player.Use(_useThingSkill, skillContext);
            }
        }
    }
}
