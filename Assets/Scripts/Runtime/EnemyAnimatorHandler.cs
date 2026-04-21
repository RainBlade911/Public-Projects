using UnityEngine;

public class EnemyAnimatorHandler : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAttack()
    {
        if (animator != null)
        {
            Debug.Log("Skeleton attack trigger fired");
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogWarning("EnemyAnimatorHandler: Animator is missing.");
        }
    }
}