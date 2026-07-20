using UnityEngine;

public class ShowTutorial : MonoBehaviour
{
    private TutorialText tutorialText;

    private void Awake()
    {
        tutorialText = GetComponent<TutorialText>();
        tutorialText.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tutorialText.enabled)
        {
            GetComponentInParent<TutorialText>().enabled = false;
            tutorialText.enabled = true;
        }
    }
}
