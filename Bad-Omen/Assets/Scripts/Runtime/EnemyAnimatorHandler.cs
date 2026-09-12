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

    public void PlayDamage()
    {
        if (animator != null)
        {
            Debug.Log("Skeleton damage trigger fired");
            animator.SetTrigger("GetHit");
        }
        else
        {
            Debug.LogWarning("EnemyAnimatorHandler: Animator is missing.");
        }
    }

    public void ResetAttackTrigger()
    {
        animator.ResetTrigger("Attack");
    }

    public void ResetDamageTrigger()
    {
        animator.ResetTrigger("GetHit");
    }
}