using UnityEngine;

public class ConsumableRestore : MonoBehaviour
{
    public static ConsumableRestore instance;
    [SerializeField] private PlayerStatusBasic playerStatus;

    private void Awake()
    {
        instance = this;
    }

    public bool ConsumeItem(ConsumableItems item)
    {
        if (playerStatus.GetCurrentHealth() == playerStatus.GetMaxHealth()) return false;

        playerStatus.SetCurrentHealth(playerStatus.GetCurrentHealth() + item.hpRestored);
        playerStatus.SetCurrentHunger(playerStatus.GetCurrentHunger() + item.hungerRestored);

        return true;
    }



}
