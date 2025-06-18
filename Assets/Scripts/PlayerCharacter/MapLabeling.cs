using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// this script is attached to the `Player` character and contains the 
// functionality to marking and naming points on the map
public class MapLabeler : MonoBehaviour
{
    private enum State
    {
        Idle,
        MarkingMap,
        Labeling
    }

    [SerializeField] private GameObject _circlePrefab;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private GameObject _labelDialogPrefab;
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private GameObject _crossHair;

    private GameObject _circleInstance;
    private GameObject _dialogInstance;

    private Camera _mainCamera;
    private Vector3 _lastHitPoint;
    private Vector3 _savedPosition;
    private State _currentState = State.Idle;
    private PlayerStats _playerStats;

    private void Start()
    {
        _mainCamera = Camera.main;
        _playerStats = GetComponent<PlayerStats>();
        if (_playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on the player character.");
            return;
        }
    }

    private void Update()
    {
        if (_currentState == State.Idle && Input.GetKeyDown(KeyCode.Comma))
        {
            _currentState = State.MarkingMap;
            _crossHair.SetActive(false);

            _circleInstance = Instantiate(_circlePrefab);
        }

        if (_currentState == State.MarkingMap)
        {
            HandleMarkingMap();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == State.MarkingMap)
            {
                Destroy(_circleInstance);
                _circleInstance = null;
            }
            else if (_currentState == State.Labeling)
            {
                CleanupInstances();
            }

            _currentState = State.Idle;
            _crossHair.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleMarkingMap()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _groundLayerMask))
        {
            if (Vector3.Distance(hitInfo.point, _lastHitPoint) > 0.02f)
            {
                _circleInstance.transform.localScale = new Vector3(2f, 1.5f, 2f);
                _circleInstance.transform.position = hitInfo.point;
                _circleInstance.transform.position += Vector3.up * 0.01f;
                _lastHitPoint = hitInfo.point;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _savedPosition = hitInfo.point;
                _currentState = State.Labeling;
                ShowLabelNameDialog();
            }
        }
    }

    private void ShowLabelNameDialog()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _dialogInstance = Instantiate(_labelDialogPrefab, _uiCanvas.transform, worldPositionStays: false);

        Button confirmBtn = _dialogInstance.transform.Find("ButtonContainer/ConfirmButton").GetComponent<Button>();
        confirmBtn.onClick.AddListener(() => ConfirmButtonClicked());

        Button cancelBtn = _dialogInstance.transform.Find("ButtonContainer/CancelButton").GetComponent<Button>();
        cancelBtn.onClick.AddListener(() => CancelButtonClicked());
    }

    private void ConfirmButtonClicked()
    {
        var inputField = _dialogInstance.transform.Find("LabelInputField").GetComponent<TMP_InputField>();
        string labelName = inputField.text.Trim();
        if (string.IsNullOrEmpty(labelName)) return;

        _currentState = State.Idle;
        RestoreUI();
        CleanupInstances();

        // create a permanent merker at the saved position
        var marker = Instantiate(_circlePrefab);
        marker.transform.position = _savedPosition;
        marker.transform.localScale = new Vector3(2f, 1.5f, 2f);

        _playerStats.LabeledPoints[labelName] = _savedPosition;
    }

    private void CancelButtonClicked()
    {
        _currentState = State.Idle;
        RestoreUI();
        CleanupInstances();
    }

    private void CleanupInstances()
    {
        if (_dialogInstance != null)
        {
            Destroy(_dialogInstance);
            _dialogInstance = null;
        }

        if (_circleInstance != null)
        {
            Destroy(_circleInstance);
            _circleInstance = null;
        }
    }

    private void RestoreUI()
    {
        _crossHair.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
