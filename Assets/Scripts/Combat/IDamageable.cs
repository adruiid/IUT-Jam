/// <summary>
/// Implement this on anything bullets should damage (enemies, destructibles, the player).
/// Add it to your existing health component: `public class Health : MonoBehaviour, IDamageable`
/// and implement TakeDamage. If your method is named/shaped differently, tell me and I'll
/// adapt the Bullet call instead.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
