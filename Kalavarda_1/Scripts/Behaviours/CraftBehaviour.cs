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

    private ICraftMachine _craftMachine;
    private readonly IDictionary<int, Recipe> _options = new Dictionary<int, Recipe>();
    private readonly Player _player = Player.Instance;

    void Start()
    {
        Instance = this;

        gameObject.SetActive(false);

        _player.Recipes.Changed += InitReipes;
        InitReipes(_player.Recipes);

        CraftButton.onClick.AddListener(Craft);

        Message.text = string.Empty;

        _craftMachine = new CraftMachine(_player.Bag, _player.Recipes);
    }

    private void InitReipes(IRecipeCollection recipes)
    {
        var i = 0;
        _options.Clear();
        Dropdown.options.Clear();

        if (recipes.GetRecipes().Any())
            foreach (var recipe in recipes.GetRecipes().OrderBy(r => r.Result.Prototype.Name))
            {
                Dropdown.options.Add(new Dropdown.OptionData(recipe.Result.Prototype.Name));
                _options.Add(i, recipe);
                i++;
            }
        else
        {
            Dropdown.options.Add(new Dropdown.OptionData("---------------"));
        }

        // hack
        Dropdown.value = -1;
        Dropdown.value = 0;
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
                    Message.text = "Успешно создано: " + selectedRecipe.Result.Prototype.Name;
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

    void OnDisable()
    {
        Message.text = string.Empty;
    }

    void OnEnable()
    {
    }
}
