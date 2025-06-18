#nullable enable
using NUnit.Framework.Internal.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is attached to the Main Camera which is a child of the Player object.
/// 
/// When a player looks at an interactable object, aka and Item, within a certain 
/// range, this script is responsible for detecting if the player is looking at an 
/// interactable object, displaying the help text, and invoking the Item's
/// interaction menu.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{

#region Interaction Interface Settings
    public float interactRange = 3f;
    public Image? crosshairImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.green;
    public TextMeshProUGUI? helpTextUI;
#endregion

    public GameObject? FocusedObject = null;    
    private Interactable? _currentFocus;

    private EquippedItemController? _equippedItemCtrl;
    private ItemEntity? _itemEntity = null;

    private void Start()
    {
        helpTextUI?.gameObject.SetActive(false);
        _equippedItemCtrl = this.GetComponentInParent<EquippedItemController>();
        if (_equippedItemCtrl == null)
        {
            throw new System.Exception("PlayerInteractor: EquippedItem script not found on Player object.");
        }
    }

    private void checkInteractions()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, interactRange, ~0))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable != _currentFocus)
                {
                    ClearFocus();
                    _currentFocus = interactable;
                    _currentFocus!.OnFocus();
                    
                    if (crosshairImage != null)
                    {
                        crosshairImage.color = defaultColor;
                    }

                    if (helpTextUI != null)
                    {
                        helpTextUI.gameObject.SetActive(true);
                        helpTextUI.text = interactable.HelpText;
                    }

                    FocusedObject = hit.collider.gameObject;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    _currentFocus!.Interact();

                }
            }
            else
            {
                ClearFocus();
            }
        }
        else
        {
            ClearFocus();
        }
    }

    void ClearFocus()
    {
        if (_currentFocus != null)
        {
            _currentFocus!.OnDefocus();
            _currentFocus = null;
        }

        if (helpTextUI != null)
        {
            helpTextUI.text = "";
            helpTextUI.gameObject.SetActive(false);
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = defaultColor;
        }
        FocusedObject = null;
    }

    void Update()
    {
        checkInteractions();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (FocusedObject != null)
            {
                _itemEntity = _equippedItemCtrl.SetEquippedItem(FocusedObject);
            }
            else if (_equippedItemCtrl.EquippedItemObject != null)
            {
                _equippedItemCtrl.DropEquippedItem();
                _itemEntity = null;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (_itemEntity != null)
            {
                _itemEntity.Attack();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _equippedItemCtrl.ThrowEquippedItem();
            _itemEntity = null; 
        }
    }
}
