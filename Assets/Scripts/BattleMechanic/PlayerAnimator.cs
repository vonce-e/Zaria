// This script handles the player's battle animation triggering.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Fires the player model's attack animation from combat events. Idle plays
/// by default from the Animator. Blocks re-triggering while an attack is still
/// playing so rapid card plays don't stack or restart the swing.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Tooltip("The player's Animator. If empty, searches this object's children.")]
    public Animator animator;

    [Tooltip("How long the attack animation lasts (seconds). Blocks re-triggering during this time.")]
    public float attackDuration = 1f;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    private float _attackEndTime = 0f; // time when the current attack finishes

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Play the attack animation, unless one is still playing.
    /// </summary>
    public void PlayAttack()
    {
        if (animator == null) return;

        // Still within the current attack's duration? Don't retrigger.
        if (Time.time < _attackEndTime) return;

        animator.SetTrigger(AttackTrigger);
        _attackEndTime = Time.time + attackDuration;
    }
}