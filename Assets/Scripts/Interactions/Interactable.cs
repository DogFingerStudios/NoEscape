using UnityEngine;

public interface Interactable
{
    string HelpText { get; }
    void OnFocus();
    void OnDefocus();

    void Interact();
}
