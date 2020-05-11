using System;
using Assets.Scripts.Behaviours;
using Assets.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    public static UIBehaviour Instance { get; private set; }

    public GameObject MenuPanel;
    public Button MenuCheatsButton;
    public Button MenuGoToButton;
    public Button MenuQuitButton;

    public GameObject GoToPanel;
    public InputField GoToInputX;
    public InputField GoToInputZ;
    public Button GoToGoButton;
    public Button GoToCancelButton;

    public GameObject CheatsPanel;
    public InputField CheatsPowerInput;
    public Button CheatsOkButton;
    public Button CheatsCancelButton;

    public GameObject BagPanel;
    public GameObject CraftPanel;
    public GameObject CharacterPanel;

    private readonly Player _player = Player.Instance;

    void Start()
    {
        Instance = this;

        // Cheats
        MenuCheatsButton.onClick.AddListener(() =>
        {
            MenuPanel.SetActive(false);

            CheatsPowerInput.text = Math.Round(_player.CheatModifier.Boost, 2).ToString();
            CheatsPanel.SetActive(true);
        });
        CheatsOkButton.onClick.AddListener(() =>
        {
            if (float.TryParse(CheatsPowerInput.text.Replace(".", ","), out var p))
            {
                CheatsPanel.SetActive(false);
                _player.CheatModifier.Boost = p;
            }
        });
        CheatsCancelButton.onClick.AddListener(() => CheatsPanel.SetActive(false));

        // GoTo
        MenuGoToButton.onClick.AddListener(() =>
        {
            MenuPanel.SetActive(false);

            GoToInputX.text = Math.Round(PlayerMoveBehaviour.Instance.Player.position.x, 1).ToString();
            GoToInputZ.text = Math.Round(PlayerMoveBehaviour.Instance.Player.position.z, 1).ToString();
            GoToPanel.SetActive(true);
        });
        GoToGoButton.onClick.AddListener(() =>
        {
            if (float.TryParse(GoToInputX.text, out var x) && float.TryParse(GoToInputZ.text, out var z))
            {
                GoToPanel.SetActive(false);
                PlayerMoveBehaviour.Instance.GoPlayerTo(x, z);
            }
        });
        GoToCancelButton.onClick.AddListener(() => GoToPanel.SetActive(false));

        // Quit
        MenuQuitButton.onClick.AddListener(Application.Quit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            MenuPanel.SetActive(!MenuPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.B))
            BagPanel.SetActive(!BagPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.T))
            CraftPanel.SetActive(!CraftPanel.activeSelf);

        if (Input.GetKeyDown(KeyCode.C))
            CharacterPanel.SetActive(!CharacterPanel.activeSelf);
    }
}
