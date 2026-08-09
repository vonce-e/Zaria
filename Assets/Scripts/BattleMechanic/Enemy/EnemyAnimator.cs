// This script handles the enemy animation triggering.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Fires an enemy model's animation triggers from combat events.
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [Tooltip("The enemy's Animator. If empty, searches this object's children.")]
    public Animator animator;

    // Trigger names
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int DieTrigger    = Animator.StringToHash("Die");

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Play the attack animation
    /// </summary>
    public void PlayAttack()
    {
        if (animator != null)
            animator.SetTrigger(AttackTrigger);
    }

    /// <summary>
    /// Play the death animation
    /// </summary>
    public void PlayDeath()
    {
        if (animator != null)
            animator.SetTrigger(DieTrigger);
    }
}