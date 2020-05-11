using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Behaviours;
using Assets.Scripts.Behaviours.Skills;
using Assets.Scripts.Model;
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

    private DateTime _startTime = DateTime.MinValue;
    private bool _progressStarted;

    void Start()
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
    }

    void Update()
    {
        var skill = GetSkill(Player.Instance, SkillKey);
        if (skill == null)
        {
            _buttonText.text = string.Empty;
            return;
        }

        ShowCooldown(skill);

        if (!skill.ReadyToUse)
            return;

        if (PlayerMoveBehaviour.Instance == null)
            return;

        var animManager = PlayerMoveBehaviour.Instance.AnimationManager;
        SkillProgressNormalized = 0;

        if (skill is ICastableSkill castable)
            UseAfterProgress(castable, animManager);
        else
            if (Input.GetKeyDown(SkillKey))
                UseSkill(skill, HUDBehaviour.Instance, animManager, PlayerBehaviour.Instance);
    }

    private void UseAfterProgress(ICastableSkill castableSkill, IAnimationManager animManager)
    {
        if (Input.GetKeyDown(SkillKey))
        {
            _progressStarted = true;
            _startTime = DateTime.Now;
            castableSkill.BeginCast();
        }

        if (_progressStarted && Input.GetKey(SkillKey))
        {
            var _pressElapsed = DateTime.Now - _startTime;
            SkillProgressNormalized = (float)_pressElapsed.TotalSeconds / (float)castableSkill.MaxCastDuration.TotalSeconds;
            if (_pressElapsed >= castableSkill.MaxCastDuration)
            {
                castableSkill.EndCast();
                UseSkill(castableSkill as ISkill, HUDBehaviour.Instance, animManager, PlayerBehaviour.Instance);
                _progressStarted = false;
                SkillProgressNormalized = 0;
            }
        }

        if (_progressStarted && Input.GetKeyUp(SkillKey))
        {
            _progressStarted = false;
            var _pressElapsed = DateTime.Now - _startTime;
            if (_pressElapsed >= castableSkill.MinCastDuration)
            {
                castableSkill.EndCast();
                UseSkill(castableSkill as ISkill, HUDBehaviour.Instance, animManager, PlayerBehaviour.Instance);
                SkillProgressNormalized = 0;
            }
        }
    }

    private void UseSkill(ISkill skill, HUDBehaviour hud, IAnimationManager animManager, PlayerBehaviour playerBehaviour)
    {
        if (hud == null || hud.TargetGameObject == null)
            return;

        var distance = Utils.Distance(playerBehaviour.PlayerGameObject, hud.TargetGameObject);

        playerBehaviour.Player.Use(skill, hud.Target, distance, () =>
        {
            var animState = AnimationAttribute.GetAnimationState(skill);
            if (animState != null)
                animManager.SetState(animState.Value);

            if (skill is IThrowingSkill throwing)
            {
                var castableSkill = skill as ICastableSkill;
                ThrowingBehaviour.Throw(throwing, castableSkill.CastDurationNormalized.Value);
            }

            GetAudioSource(skill)?.Play();
        });
    }

    private void ShowCooldown(ISkill skill)
    {
        if (skill.Cooldown > TimeSpan.Zero)
        {
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
        var skill = GetSkill(Player.Instance, SkillKey);
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
        switch (key)
        {
            case KeyCode.Alpha1:
                return skilled.Skills.FirstOrDefault();
            case KeyCode.Alpha2:
                return skilled.Skills.Skip(1).FirstOrDefault();
            default:
                return null;
        }
    }
}
