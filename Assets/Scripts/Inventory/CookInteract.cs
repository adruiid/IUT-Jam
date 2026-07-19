using UnityEngine;

public class CookInteract : Interactable
{
    private void Reset()
    {
        type = InteractionType.Cook;
    }

    public override void Interacted(PlayerInteractor player)
    {
        player.BeginCooking(this);
    }
}

