using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Slider _healthSlider;

    private float _maxHealth;

    private readonly Color _fullColor = Color.green;
    private readonly Color _zeroColor = Color.red;

    private void Awake()
    {
        if (!_targetObject.TryGetComponent<IHasHealth>(out var iHasHealth))
        {
            throw new System.Exception("Target object does not implement IHasHealth interface.");
        }

        _maxHealth = iHasHealth.MaxHealth;
        iHasHealth.OnHealthChanged += UpdateBar;

        _healthSlider.minValue = 0f;
        _healthSlider.maxValue = _maxHealth;
        _healthSlider.value = _maxHealth;

        UpdateBar(iHasHealth.Health);
    }

    private void UpdateBar(float health)
    {
        float t = health / _maxHealth;

        _healthSlider.SetValueWithoutNotify(health);
        _fillImage.color = Color.Lerp(_zeroColor, _fullColor, t);
    }
}