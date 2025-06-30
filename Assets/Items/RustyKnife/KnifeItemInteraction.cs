#nullable enable
using UnityEngine;
using System.Collections;

public class KnifeItemInteraction : ItemEntity
{
    private Vector3 _defaultLocalPosition2; 
    private Coroutine? _attackCoroutine;

    public override void Attack()
    {
    
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }

        _attackCoroutine = StartCoroutine(AnimateAttack());
    }

    // this is called AFTER the item is equipped
    public override void onEquipped()
    {
        _defaultLocalPosition2 = this.gameObject.transform.localPosition;
        if (_hitCollider) _hitCollider.enabled = false;
    }

    private IEnumerator AnimateAttack()
    {
        OnStartAttack();

        float duration = 0.1f; // time to thrust
        float returnDuration = 0.15f;
        float elapsed = 0f;

        Vector3 start = _defaultLocalPosition2;
        Vector3 target = _defaultLocalPosition2 + new Vector3(-0.3f, 0f, 0.0f); // forward thrust

        // AI: Thrust forward
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            this.gameObject.transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        elapsed = 0f;

        // AI: Return to default
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            this.gameObject.transform.localPosition = Vector3.Lerp(target, start, t);
            yield return null;
        }

        OnEndAttack();

        this.gameObject.transform.localPosition = _defaultLocalPosition2;
        _attackCoroutine = null;
    }

}
