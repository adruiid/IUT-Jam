using UnityEngine;

public class DemoEnemy : MonoBehaviour
{
    public Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    [ContextMenu("Speed=10f")]
    public void SetSpeedAnim()
    {
        anim.SetFloat("Speed", 10f);
    }

    [ContextMenu("Attack")]
    public void SetAttack()
    {
        anim.SetTrigger("Attack");
    }

    [ContextMenu("Death")]
    public void SetDeath()
    {
        anim.SetTrigger("Death");
    }

}
