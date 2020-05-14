using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffsBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject PlayerPosBuffs;
    public GameObject PlayerNegBuffs;
    public GameObject BuffTooltip;
    public GameObject TargetPosBuffs;
    public GameObject TargetNegBuffs;

    private readonly Player _player = Player.Instance;
    private readonly Dictionary<int, BuffControls> _negControls = new Dictionary<int, BuffControls>();
    private readonly Dictionary<int, BuffControls> _posControls = new Dictionary<int, BuffControls>();
    private readonly Dictionary<int, BuffControls> _targetNegControls = new Dictionary<int, BuffControls>();
    private readonly Dictionary<int, BuffControls> _targetPosControls = new Dictionary<int, BuffControls>();

    void Start()
    {
        try
        {
            PrepareControls(PlayerNegBuffs, _negControls);
            PrepareControls(PlayerPosBuffs, _posControls);
            PrepareControls(TargetPosBuffs, _targetPosControls);
            PrepareControls(TargetNegBuffs, _targetNegControls);

            BuffTooltip.SetActive(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void PrepareControls(GameObject buffsPanel, IDictionary<int, BuffControls> controls)
    {
        var objects = buffsPanel.GetComponentsInChildren<Transform>()
            .Where(t => t != buffsPanel.transform);
        foreach (var backPanel in objects.Where(t => t.name.Contains("Panel")))
        {
            var i = backPanel.name.Split('_').Last();
            var texts = backPanel.GetComponentsInChildren<Text>();
            controls.Add(int.Parse(i), new BuffControls
            {
                BackPanel = backPanel,
                RemainText = texts.First(t => t.name == "Text_Remain")
            });
        }
    }

    void Update()
    {
        UpdateBuffs(GetDebuffs(), _negControls);
        UpdateBuffs(GetBuffs(), _posControls);
        UpdateBuffs(GetTargetDebuffs(), _targetNegControls);
        UpdateBuffs(GetTargetBuffs(), _targetPosControls);
    }

    private void UpdateBuffs(IEnumerable<IBuff> buffs, IDictionary<int, BuffControls> controls)
    {
        var i = 0;
        foreach (var buff in buffs)
        {
            controls[i].RemainText.text = Assets.Scripts.Utils.Utils.ToString(buff.EndTime - DateTime.Now);
            controls[i].BackPanel.gameObject.SetActive(true);
            i++;
        }

        foreach (var pair in controls.Skip(i))
            pair.Value.BackPanel.gameObject.SetActive(false);
    }

    private IEnumerable<IBuff> GetBuffs()
    {
        return _player.Buffs.Where(b => b.IsNegitive == false).OrderByDescending(b => b.EndTime);
    }

    private IEnumerable<IBuff> GetDebuffs()
    {
        return _player.Buffs.Where(b => b.IsNegitive).OrderByDescending(b => b.EndTime);
    }

    private IEnumerable<IBuff> GetTargetBuffs()
    {
        if (HUDBehaviour.Instance.Target is IEnemy enemy)
            return enemy.Buffs.Where(b => b.IsNegitive == false).OrderByDescending(b => b.EndTime);
        
        return new IBuff[0];
    }

    private IEnumerable<IBuff> GetTargetDebuffs()
    {
        if (HUDBehaviour.Instance.Target is IEnemy enemy)
            return enemy.Buffs.Where(b => b.IsNegitive == true).OrderByDescending(b => b.EndTime);

        return new IBuff[0];
    }

    public class BuffControls
    {
        public Transform BackPanel { get; set; }

        public Text RemainText { get; set; }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip(eventData, _negControls, GetDebuffs());
        ShowTooltip(eventData, _posControls, GetBuffs());
        ShowTooltip(eventData, _targetPosControls, GetTargetBuffs());
        ShowTooltip(eventData, _targetNegControls, GetTargetDebuffs());
    }

    private void ShowTooltip(PointerEventData eventData, IReadOnlyDictionary<int, BuffControls> controlDict, IEnumerable<IBuff> buffs)
    {
        var controls = controlDict.Values.FirstOrDefault(p => p.BackPanel.gameObject == eventData.pointerEnter || p.RemainText.gameObject == eventData.pointerEnter);
        if (controls != null)
            ShowTooltip(controls, buffs, controlDict);
    }

    private void ShowTooltip(BuffControls selectedControls, IEnumerable<IBuff> buffs, IReadOnlyDictionary<int, BuffControls> controls)
    {
        var pair = controls.First(p => p.Value == selectedControls);
        var buff = buffs.Skip(pair.Key).FirstOrDefault();
        if (buff != null)
            ShowTooltip(buff);
    }

    private void ShowTooltip(IBuff buff)
    {
        var texts = BuffTooltip.gameObject.GetComponentsInChildren<Text>();
        texts.First(t => t.name == "Text_Name").text = buff.Name;
        texts.First(t => t.name == "Text_Description").text = buff.Description;
        BuffTooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BuffTooltip.SetActive(false);
    }
}
