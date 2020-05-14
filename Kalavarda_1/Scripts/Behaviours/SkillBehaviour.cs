using System;
using System.Linq;
using Assets.Scripts.Behaviours;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Skills;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject Tooltip;
    public KeyCode SkillKey;

    public static SkillBehaviour Instance { get; private set; }

    public float SkillProgressNormalized { get; private set; }

    private Text _tooltipText_Name;
    private Text _tooltipText_Description;
    private Text _tooltipText_Interval;
    private Text _buttonText;
    private RectTransform _cooldownPanel;
    private RectTransform _backPanel;

    private readonly Player _player = Player.Instance;

    void Start()
    {
        try
        {
            Instance = this;

            Tooltip.SetActive(false);
            _tooltipText_Name = Tooltip.GetComponentsInChildren<Text>().First(t => t.name.Contains("Name"));
            _tooltipText_Description = Tooltip.GetComponentsInChildren<Text>().First(t => t.name.Contains("Description"));
            _tooltipText_Interval = Tooltip.GetComponentsInChildren<Text>().First(t => t.name.Contains("Interval"));

            _buttonText = GetComponentInChildren<Text>();

            var transforms = gameObject.GetComponentsInChildren<Transform>();
            _backPanel = transforms.First(obj => obj.name.Contains("Back")).GetComponent<RectTransform>();
            _cooldownPanel = transforms.First(obj => obj.name.Contains("Cooldown")).GetComponent<RectTransform>();

            foreach (var skill in _player.Skills)
                skill.OnSuccessUsed += Skill_OnSuccessUsed;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void Skill_OnSuccessUsed(ISkill skill, SkillContext context)
    {
        var animState = AnimationAttribute.GetAnimationState(skill);
        if (animState != null)
            PlayerMoveBehaviour.Instance.AnimationManager.SetState(animState.Value);

        var audioSource = GetAudioSource(skill);
        if (audioSource != null)
            audioSource.Play();
    }

    void Update()
    {
        try
        {
            var skill = GetSkill(_player, SkillKey);
            if (skill == null)
            {
                _buttonText.text = string.Empty;
                _cooldownPanel.gameObject.SetActive(false);
                return;
            }

            ShowCooldown(skill);

            if (skill.Cooldown > TimeSpan.Zero)
                return;

            if (PlayerMoveBehaviour.Instance == null)
                return;

            SkillProgressNormalized = 0;

            if (Input.GetKeyDown(SkillKey))
                UseSkill(skill, HUDBehaviour.Instance, PlayerBehaviour.Instance);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static void UseSkill(ISkill skill, HUDBehaviour hud, PlayerBehaviour playerBehaviour)
    {
        if (hud == null)
            return;

        var distance = hud.TargetGameObject != null
            ? Utils.Distance(playerBehaviour.PlayerGameObject, hud.TargetGameObject)
            : 0;

        var angle = hud.TargetGameObject != null // TODO: не всегда правильно считается
            ? Vector3.Angle(playerBehaviour.PlayerGameObject.transform.forward, hud.TargetGameObject.transform.forward) / 180
            : 0;

        var skillContext = new SkillContext(hud.Target, distance, angle);
        playerBehaviour.Player.Use(skill, skillContext);
    }

    private void ShowCooldown(ISkill skill)
    {
        if (skill.Cooldown > TimeSpan.Zero)
        {
            _cooldownPanel.gameObject.SetActive(true);

            _cooldownPanel.offsetMin = new Vector2(_backPanel.offsetMin.x, _backPanel.offsetMin.y);
            _cooldownPanel.sizeDelta =
                new Vector2(_backPanel.sizeDelta.x, _backPanel.sizeDelta.y * skill.CooldownNormalized);

            _buttonText.text = Math.Round(skill.Cooldown.TotalSeconds, 1) + " s";
        }
        else
        {
            _cooldownPanel.sizeDelta = new Vector2();
            _buttonText.text = SkillKey.ToString().Replace("Alpha", string.Empty);
        }
    }

    internal static AudioSource GetAudioSource(ISkill skill)
    {
        // TODO: закэшировать

        var asName = AudioSourceAttribute.GetAudioSourceName(skill);
        if (asName == null)
            return null;

        return FindObjectsOfType<AudioSource>().FirstOrDefault(aS => aS.name == asName);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var skill = GetSkill(_player, SkillKey);
        if (skill != null)
        {
            SetTooltipValues(skill);
            Tooltip.SetActive(true);
        }
    }

    private void SetTooltipValues(ISkill skill)
    {
        _tooltipText_Name.text = skill.Name;
        _tooltipText_Description.text = skill.Description;
        _tooltipText_Interval.text = Math.Round(skill.Interval.TotalSeconds, 1) + " sec.";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.SetActive(false);
    }

    private static ISkill GetSkill(ISkilled skilled, KeyCode key)
    {
        var skills = skilled.Skills.Where(sk => !(sk is UseThing));
        switch (key)
        {
            case KeyCode.Alpha1:
                return skills.Skip(0).FirstOrDefault();
            case KeyCode.Alpha2:
                return skills.Skip(1).FirstOrDefault();
            default:
                return null;
        }
    }
}
