using System.Collections;
using UnityEngine;

public class LampInteractable : MonoBehaviour, Interactable
{
    public Light Light;

    public string HelpText
    {
        get { return "Press [E] to toggle lamp"; }
    }

    public AudioClip lampSwitchSound;

    [Header("Flickering Settings")]
    public float minTurnOnDelay = 0f;
    public float maxTurnOnDelay = 2f;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float lightChangeSpeed = 0.5f;
    private float targetIntensity;
    public float minFlickerTime = 0.1f;
    public float maxFlickerTime = 1f;


    public void Interact()
    {
        if (Light == null)
        {
            Debug.Log("No Light component found on LampInteractable, should probably have one.");
            return;
        }

        Light.intensity = Light.intensity == 0 ? 1 : 0;

        if (Light.intensity == 0)
        {
            DisableLightFlicker();
        }
        else
        {
            EnableLightFlicker();
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (lampSwitchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lampSwitchSound);
        }
    }

    public void OnFocus()
    {
        // Debug.Log("Looking at lamp " + gameObject.name);
    }

    public void OnDefocus()
    {
        // Debug.Log("Defocused from lamp " + gameObject.name);
    }

    #region Flickering

    private void EnableLightFlicker()
    {
        StartCoroutine(FlickerLight());
    }

    private void DisableLightFlicker()
    {
        StopAllCoroutines();
    }

    private IEnumerator FlickerLight()
    {
        targetIntensity = Random.Range(minIntensity, maxIntensity);
        yield return new WaitForSeconds(Random.Range(minFlickerTime, maxFlickerTime));
        StartCoroutine(FlickerLight());
    }

    private void Update()
    {
        if (Light.enabled)
        {
            Light.intensity = Mathf.Lerp(Light.intensity, targetIntensity, lightChangeSpeed * Time.deltaTime);
        }
    }

    #endregion

}