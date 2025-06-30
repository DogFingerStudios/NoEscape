using UnityEngine;

// This script is attached to the `SonNPC` object in the Hierarchy (there is no
// prefab for this NPC).
public class SonNPC : FriendlyNPC
{
    protected override void Start()
    {
        base.Start();

        var playerObject = GameObject.FindGameObjectWithTag("Player");
        Profile.Relationships.Add(playerObject, new RelationshipTraits
        {
            TrustToward = 0.99f,    // nearly full trust
            Love = 0.87f,           // nearly full love
            FearOf = 0.25f          // some fear
        });

        stateMachine.SetState(new NPCIdleState(this));
    }

    private void onGunFired()
    {
        this.Profile.MentalState.Comfort -= (this.Profile.MentalState.Comfort * 0.1f);
        this.Profile.MentalState.Comfort = Mathf.Clamp(this.Profile.MentalState.Comfort, -1f, 1f);
    }
}
