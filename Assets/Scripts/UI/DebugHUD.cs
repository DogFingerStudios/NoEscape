#nullable enable
using TMPro;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    // UI Elements
    public GameObject hudPanel;
    public TextMeshProUGUI groundedText;
    public TextMeshProUGUI sonNPCStateText;
    public TextMeshProUGUI sonNPCDistanceText;
    public TextMeshProUGUI enemyHealthText;

    // Objects of interest
    public CharacterController controller;
    public GameObject enemyNPC;
    private GameEntity enemyNPCHealth;
    public SonNPC sonNPC;

    private bool isVisible = true;

    void Start()
    {
        if (enemyNPC)
        {
            enemyNPCHealth = enemyNPC.GetComponent<GameEntity>();
        }
    }

    void Update()
    {
        // Toggle visibility with "."
        if (Input.GetKeyDown(KeyCode.Period))
        {
            isVisible = !isVisible;
            hudPanel.SetActive(isVisible);
        }

        if (isVisible)
        {
            groundedText.text = "Grounded: " + controller.isGrounded.ToString();
            sonNPCStateText.text = "SonNPC State: " + sonNPC.StateMachine.CurrentState?.GetType().Name;
            float distance = Vector3.Distance(controller.transform.position, sonNPC.transform.position);
            sonNPCDistanceText.text = "SonNPC Dist: " + distance.ToString("F2");

            // As we now have multiple enemies, we should either extend the DebugHUD to support multiple enemies
            // Or simply remove the enemy health display if not needed
            // This object check prevents null reference exceptions when we don't have an enemy assigned
            if (enemyNPC != null && enemyNPCHealth != null)
            {
                enemyHealthText.text = "Enemy Health: " + enemyNPCHealth.Health.ToString("F2");
            }
        }
    }
}