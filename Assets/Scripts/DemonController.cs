using UnityEngine;

public class DemonController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void HasRisenTrue()
    {
        animator.SetBool("HasRisen", true);
    }

    public void HasRisenFalse()
    {
        animator.SetBool("HasRisen", false);
    }
}
