using System;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehaviour : MonoBehaviour
{
    public static HUDBehaviour Instance { get; private set; }

    public Image HPBack;
    public Image HPTop;
    public Text HPText;

    public Image TargetBack;
    public Image TargetTop;
    public Text TargetText;
    public Text TargetNameText;

    public GameObject DpsPanel;
    public GameObject FightIndicator;

    public IHealth Target { get; private set; }
    public GameObject TargetGameObject { get; private set; }

    private const float MaxTargetDistance = 50f;

    private Player _player;
    private RectTransform _hpBackRect, _hpTopRect;
    private Image _hpTopImage;

    private RectTransform _targetBackRect, _targetTopRect;
    private readonly TimeIntervalLimiter _targetLimiter = new TimeIntervalLimiter(TimeSpan.FromSeconds(1));

    public readonly DpsMeter DpsMeter = new DpsMeter();

    void Start()
    {
        Instance = this;

        _player = Player.Instance;
        _hpBackRect = HPBack.GetComponent<RectTransform>();
        _hpTopRect = HPTop.GetComponent<RectTransform>();
        _hpTopImage = HPTop.GetComponent<Image>();

        _targetBackRect = TargetBack.GetComponent<RectTransform>();
        _targetTopRect = TargetTop.GetComponent<RectTransform>();

        _player.FightBegin += obj => DpsMeter.StartFight();
        _player.DagameReceived += damage => DpsMeter.CurrentFight.AddDamage(damage);
    }

    void Update()
    {
        try
        {
            Target = null;
            TargetGameObject = null;

            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out var hit))
            {
                var obj = hit.collider.gameObject;
                if (PlayerBehaviour.Instance != null && Utils.Distance(obj, PlayerBehaviour.Instance.PlayerGameObject) < MaxTargetDistance)
                    if (SpawnerBehaviour.SpawnedHealth.TryGetValue(obj, out var health))
                    {
                        TargetGameObject = obj;
                        Target = health;
                    }
            }

            ShowHP(_player);

            // Target
            ShowTarget();

            if (Input.GetKeyDown(KeyCode.P))
                DpsPanel.SetActive(!DpsPanel.activeSelf);
            if (DpsPanel.activeSelf)
                ShowDps(DpsMeter);

            FightIndicator.SetActive(_player.InFight);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void ShowTarget()
    {
        if (Target != null && !Target.IsDied)
        {
            _targetTopRect.sizeDelta =
                new Vector2(_targetBackRect.rect.width * Target.HPNormalized, _targetBackRect.rect.height);
            _targetTopRect.offsetMin = _targetBackRect.offsetMin;
            TargetText.text = Math.Round(Target.HP) + " / " + Math.Round(Target.MaxHP);
            if (Target is IEnemy enemy)
                TargetNameText.text = enemy.Name;
            TargetBack.enabled = true;
            TargetTop.enabled = true;
            TargetText.enabled = true;
            TargetNameText.enabled = true;
        }
        else
            _targetLimiter.Do(() =>
            {
                TargetBack.enabled = false;
                TargetTop.enabled = false;
                TargetText.enabled = false;
                TargetNameText.enabled = false;
            });
    }

    private void ShowHP(IHealth health)
    {
        _hpTopRect.sizeDelta = new Vector2(_hpBackRect.rect.width * health.HPNormalized, _hpBackRect.rect.height);
        _hpTopRect.offsetMin = _hpBackRect.offsetMin;
        var r = 1.5f - _player.HPNormalized;
        var g = 2 * _player.HPNormalized;
        _hpTopImage.color = new Color(r, g, 0, _hpTopImage.color.a);
        HPText.text = Math.Round(health.HP) + " / " + Math.Round(health.MaxHP);
    }

    private void ShowDps(DpsMeter dpsMeter)
    {
        var textDuration = DpsPanel.GetComponentsInChildren<Text>().First(t => t.name == "Dps_Duration");
        var textTotal = DpsPanel.GetComponentsInChildren<Text>().First(t => t.name == "Dps_Total");
        var textDetails = DpsPanel.GetComponentsInChildren<Text>().First(t => t.name == "Dps_Details");

        if (dpsMeter.CurrentFight == null)
        {
            textDuration.text = "-";
            textTotal.text = "-";
            textDetails.text = "-";
            return;
        }

        textDuration.text = "Duration: " + Math.Round(dpsMeter.CurrentFight.Duration.TotalSeconds) + " sec";
        textTotal.text = "Total DPS: " + Math.Round(dpsMeter.CurrentFight.GetDpsOf(Player.Instance), 1);

        var detailedDps = dpsMeter.CurrentFight.GetDetailedDpsOf(Player.Instance);
        textDetails.text = string.Join(Environment.NewLine, detailedDps
            .OrderByDescending(p => p.Value)
            .Select(p => p.Key.Name + " : " + Math.Round(p.Value, 1)));
    }
}
