using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

public class CraftBehaviour : MonoBehaviour
{
    public Button CraftButton;
    public Dropdown Dropdown;
    public AudioSource SuccessSound;
    public AudioSource FailSound;
    public Text Message;

    public static CraftBehaviour Instance { get; private set; }

    private readonly ICraftMachine _craftMachine = new CraftMachine(Player.Instance.Bag, new Recipes1());
    private readonly IDictionary<int, Recipe> _options = new Dictionary<int, Recipe>();

    void Start()
    {
        Instance = this;

        gameObject.SetActive(false);

        var i = 0;
        _options.Clear();
        Dropdown.options.Clear();
        foreach (var recipe in _craftMachine.Recipes.OrderBy(r => r.Result.Prototype.Name))
        {
            Dropdown.options.Add(new Dropdown.OptionData(recipe.Result.Prototype.Name));
            _options.Add(i, recipe);
            i++;
        }

        // hack
        Dropdown.value = -1;
        Dropdown.value = 0;

        CraftButton.onClick.AddListener(Craft);

        Message.text = string.Empty;
    }

    private void Craft()
    {
        try
        {
            var selectedRecipe = _options[Dropdown.value];
            if (selectedRecipe != null)
            {
                var result = _craftMachine.Craft(selectedRecipe, 1f);
                if (result.Result.Any())
                {
                    Message.text = "Предмет '" + selectedRecipe.Result.Prototype.Name + "' успешно создан!";
                    SuccessSound.Play();
                }
                else
                {
                    Message.text = "Неудача";
                    FailSound.Play();
                }
            }
        }
        catch (Exception error)
        {
            Message.text = error.GetBaseException().Message;
            FailSound.Play();
        }
    }

    public void StartCraft()
    {
        gameObject.SetActive(true);
    }
}
