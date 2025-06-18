using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class FriendlyNPC : BaseNPC
{
    // TODO: these three fields should be refactored out of here
    [SerializeField] private GameObject _mapLabelDialogPrefab;
    [SerializeField] private Canvas _uiCanvas;
    private PlayerStats _playerStats;

    public List<InteractionData> Interactions = new List<InteractionData>();

    protected override void Start()
    {
        base.Start();

        _playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        if (_playerStats == null)
        {
            // TODO: error handling is shyte!
            Debug.LogError("PlayerStats component not found on the player character.");
        }

        Interactions.Add(new InteractionData { key = "stay", description = "Stay" });
        Interactions.Add(new InteractionData { key = "follow", description = "Follow" });
        Interactions.Add(new InteractionData { key = "goto", description = "Go To", IsAvailable = () => _playerStats.LabeledPoints.Count > 0 });
    }

#region Interactable Interface Implementation

    public override string HelpText 
    {
        get
        {
            if (!this.IsAlive) return $"{this.name} is dead";
            return $"Press [E] to interact with {this.name}";
        }
    }

    public override void OnFocus() { }
    public override void OnDefocus() { }

    public override void Interact(GameEntity? interactor = null)
    {
        if (!this.IsAlive) return;
        InteractionManager.Instance.OnInteractionAction += this.DoInteraction;
        InteractionManager.Instance.OpenMenu(Interactions);
    }
#endregion

    // TODO: this is copied from ItemInteraction.cs, should be refactored to a common base class
    private void DoInteraction(string actionName)
    {
        Type type = this.GetType();
        while (type != null && type != typeof(MonoBehaviour))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                ItemAction attr = method.GetCustomAttribute<ItemAction>();
                if (attr != null && attr.ActionName == actionName)
                {
                    method.Invoke(this, null);
                    return;
                }
            }

            type = type.BaseType;
        }

        Debug.LogWarning($"No action found for '{actionName}' in {this.GetType().Name}");
    }

    [ItemAction("stay")]
    protected virtual void onStay()
    {
        this.setState(new NPCIdleState(this));
    }

    [ItemAction("follow")]
    protected virtual void onFollow()
    {
        this.Panic();

        var entityTraits = this.Profile.Personality;
        var relationshipTraits = this.Profile.Relationships[GameObject.FindGameObjectWithTag("Player")];
        if (DecisionProfile.Evaluate(1, 1, entityTraits, relationshipTraits) == DecisionResult.Obey)
        {
            this.setState(new NPCFollowState(this));
        }
        else
        {
            this.setState(new NPCIdleState(this));
        }
    }

    [ItemAction("goto")]
    protected virtual void onGoTo()
    {
        InteractionManager.Instance.OnLateInteractionAction += ResetCursor;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var dlgInstance = Instantiate(_mapLabelDialogPrefab, _uiCanvas.transform, worldPositionStays: false);
        Button confirmBtn = dlgInstance.transform.Find("ButtonContainer/ConfirmButton").GetComponent<Button>();
        confirmBtn.onClick.AddListener(() =>
        {
            var labelDropdown = dlgInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
            if (labelDropdown.value == 0) return;

            string labelName = labelDropdown.options[labelDropdown.value].text;
            var destination = _playerStats.LabeledPoints[labelName];
            this.setState(new NPCGoToState(this, destination));

            Destroy(dlgInstance);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });

        Button cancelBtn = dlgInstance.transform.Find("ButtonContainer/CancelButton").GetComponent<Button>();
        cancelBtn.onClick.AddListener(() =>
        {
            Destroy(dlgInstance);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });

        var labelDropdown = dlgInstance.transform.Find("LabelDropdown").GetComponent<TMP_Dropdown>();
        labelDropdown.ClearOptions();
        var options = new List<string>(_playerStats.LabeledPoints.Keys);
        options.Insert(0, "Select a label");
        labelDropdown.AddOptions(options);
    }

    private void ResetCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        InteractionManager.Instance.OnLateInteractionAction -= ResetCursor;
    }
}

