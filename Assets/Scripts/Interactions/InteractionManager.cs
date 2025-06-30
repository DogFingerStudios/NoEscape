using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct InteractionData
{
    public string key;
    public string description;

    // a lambda that returns a bool to determine if the interaction is available, defaulting
    // to always available if not provided
    public Func<bool> IsAvailable;

    public InteractionData(string key, string description, Func<bool> isAvailable = null)
    {
        this.key = key;
        this.description = description;
        this.IsAvailable = isAvailable ?? (() => true); // Default to always available
    }
}

// This script is attached to the `InteractMenus` canvas in the Hierarchy. `InteractMenus` is the parent
// of all the interact menus in the game. This class is responsible for opening and closing the interact
// menu, acreating the buttons, and handling button clicks
public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private GameObject _buttonPanel;
    [SerializeField] private GameObject crossHair;
    [SerializeField] private GameObject helpText;

    // define a callback that callers can use to execute the action
    public event Action<string> OnInteractionAction;
    public event Action OnLateInteractionAction;
    public event Action OnMenuClosed;

    private static InteractionManager _instance;
    public static InteractionManager Instance
    {
        get => _instance;
        private set => _instance = value;
    }

    public void OpenMenu(List<InteractionData> interactions)
    {
        // Check if the menu is already open to prevent repeated button spawning
        if (_buttonPanel.activeInHierarchy)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _buttonPanel.SetActive(true);
        crossHair.SetActive(false);
        helpText.SetActive(false);

        foreach (var interaction in interactions)
        {
            GameObject buttonObj = Instantiate(_buttonPrefab, _buttonPanel.transform);
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = interaction.description;
            }

            if (buttonObj.TryGetComponent<Button>(out var button))
            {
                // Add listener to the button, call OnInteractionClicked and lock the mouse cursor
                button.onClick.AddListener(() =>
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    OnInteractionClicked(interaction.key);
                });
            }

            if (interaction.IsAvailable != null && !interaction.IsAvailable())
            {
                buttonObj.GetComponent<Button>().interactable = false;
                buttonText.color = Color.gray;
            }
        }
    }

    private void CloseMenu()
    {
        foreach (Transform child in _buttonPanel.transform)
        {
            // Only destroy the button objects, otherwise the panel frame and title are also deleted
            if (child.TryGetComponent<Button>(out var _))
            {
                Destroy(child.gameObject);
            }
        }

        helpText.SetActive(true);
        crossHair.SetActive(true);
        _buttonPanel.SetActive(false);

        OnMenuClosed?.Invoke();
        OnMenuClosed = null;
        OnInteractionAction = null;
    }

    private void OnInteractionClicked(string action)
    {
        OnInteractionAction?.Invoke(action);
        CloseMenu();
        OnLateInteractionAction?.Invoke();
    }

    void Awake()
    {
        _instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // nothing to do
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            CloseMenu();
        }
    }
}
