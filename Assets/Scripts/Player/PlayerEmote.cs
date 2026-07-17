using UnityEngine;

public class PlayerEmote : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool dancing=false;

    private void Update()
    {
        if(dancing && Input.anyKeyDown)
        {
            dancing = false;
            animator.SetBool("Emoting", dancing);
        }
    }

    public void TriggerDance()
    {
        dancing = true;
        animator.SetBool("Emoting", dancing);
    }
}
