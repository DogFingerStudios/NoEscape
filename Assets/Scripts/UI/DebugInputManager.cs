using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugInputManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerObject;
    private GameEntity playerEntity;
    
    void Awake()
    {
        playerEntity = _playerObject.GetComponent<GameEntity>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            playerEntity.Heal(5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            playerEntity.TakeDamage(5f);
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            SceneManager.LoadScene("Game"); 
        }
    }
}