using UnityEngine;

public class DemoStatusScript : MonoBehaviour
{
    public PlayerStatusBasic playerStatusBasic;

    [ContextMenu("Minus Health")]
    public void MinusHealth()
    {
        playerStatusBasic.SetCurrentHealth(playerStatusBasic.GetCurrentHealth() - 10);
    }

    [ContextMenu("Minus Hunger")]
    public void MinusHungeer()
    {
        playerStatusBasic.SetCurrentHunger(playerStatusBasic.GetCurrentHunger() - 10);
    }
}
