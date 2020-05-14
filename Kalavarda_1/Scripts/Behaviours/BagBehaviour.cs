using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Skills;
using UnityEngine;
using UnityEngine.UI;

public class BagBehaviour : MonoBehaviour
{
    private readonly IDictionary<int, Button> _buttonPositions = new Dictionary<int, Button>();
    private readonly Player _player = Player.Instance;
    private readonly IBag _bag = Player.Instance.Bag;
    private UseThing _useThingSkill;
    private IBagCell _lastClickedCell;

    void Start()
    {
        _useThingSkill = _player.Skills.OfType<UseThing>().First();

        Init();

        _bag.OnCellAdded += Bag_OnCellAddOrRemove;
        _bag.OnCellRemoved += Bag_OnCellAddOrRemove;
    }

    private void OnUseThingEndCast(ICastableSkill castableSkill)
    {
        _useThingSkill.OnEndCast -= OnUseThingEndCast;

        if (_useThingSkill.Thing is IStack st)
            _bag.Pull(st);
        else
            _bag.Pull(_lastClickedCell.Item);
    }

    private void Bag_OnCellAddOrRemove(IBag bag, IBagCell cell)
    {
        Init();
    }

    private void Init()
    {
        var buttons = GetComponentsInChildren<Button>();

        var list = new List<Tuple<Button, int, int>>();

        foreach (var button in buttons)
        {
            var parts = button.name.Split(new[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
            var row = int.Parse(parts[1]);
            var col = int.Parse(parts[2]);
            list.Add(new Tuple<Button, int, int>(button, row, col));
        }

        var cellsInRow = list.GroupBy(t => t.Item2).Max(gr => gr.Count());

        _buttonPositions.Clear();
        foreach (var tuple in list)
        {
            var button = tuple.Item1;

            var pos = tuple.Item2 * cellsInRow + tuple.Item3;
            _buttonPositions.Add(pos, button);

            var bagCell = _bag.Cells.Skip(pos).FirstOrDefault();
            TuneButton(button, bagCell);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { OnButtonClick(pos); });
        }
    }

    private void TuneButton(Button button, IBagCell bagCell)
    {
        var text = button.GetComponentInChildren<Text>();
        text.text = bagCell != null ? bagCell.Name : string.Empty;
    }

    private void OnButtonClick(int cellPos)
    {
        var skillContext = new SkillContext(_player, 0, null);

        if (!_useThingSkill.ReadyToUse(skillContext))
            return;

        var bagCell = _bag.Cells.Skip(cellPos).FirstOrDefault();
        if (bagCell != null)
        {
            var thing = bagCell.Item;
            if (thing is IStack stack)
                thing = stack.Prototype.CreateInstance();

            _lastClickedCell = bagCell;
            _useThingSkill.OnEndCast += OnUseThingEndCast;

            _useThingSkill.Thing = thing;
            _player.Use(_useThingSkill, skillContext);
        }
    }

    void Update()
    {
        try
        {
            foreach (var pair in _buttonPositions)
            {
                var bagCell = _bag.Cells.Skip(pair.Key).FirstOrDefault();
                TuneButton(pair.Value, bagCell);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
