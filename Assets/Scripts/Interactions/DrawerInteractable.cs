using UnityEngine;

public class DrawerInteractable : MonoBehaviour, Interactable
{
    public string HelpText
    {
        get { return "Press [E] to open/close drawer"; }
    }

    public void Interact(GameEntity? interactor = null)
    {
        // print the name of the object being interacted with
        // Debug.Log("Drawer " + gameObject.name + " opened!");

        DrawerMech drawer = GetComponent<DrawerMech>();
        if (drawer == null) return;

        // toggle the drawer's state
        drawer.drawerBool = !drawer.drawerBool;
    }

    public void OnFocus()
    {
        // print the name of the object being focused on
        // Debug.Log("Looking at drawer " + gameObject.name);
    }

    public void OnDefocus()
    {
        // print the name of the object being defocused from
        // Debug.Log("Defocused from drawer " + gameObject.name);
    }
}
