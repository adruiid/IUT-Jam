using StarterAssets;
using UnityEngine;

public class PlayerEmote : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ThirdPersonController _controller;
    private bool dancing=false;

    private void Start()
    {
        _controller = GetComponent<ThirdPersonController>();
    }

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
        if (_controller.playerIsMoving) return;
        dancing = true;
        animator.SetBool("Emoting", dancing);
    }
}
