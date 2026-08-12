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

    /// <summary>
    /// How long the currently-playing animation is, in seconds.
    /// </summary>
    public float GetCurrentStateLength()
    {
        if (animator == null) return 0f;
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    /// <summary>
    /// True while the animator is mid-transition between states.
    /// </summary>
    public bool IsInTransition()
    {
        if (animator == null) return false;
        return animator.IsInTransition(0);
    }
}