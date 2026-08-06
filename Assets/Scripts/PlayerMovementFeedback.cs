// Adds first-person movement feedback without changing the movement controller.

using StarterAssets;
using UnityEngine;

/// <summary>
/// Plays footsteps and applies subtle head bob while the grounded player moves.
/// </summary>
[DisallowMultipleComponent]
public class PlayerMovementFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController movementController;
    [SerializeField] private Transform cameraBobTarget;
    [SerializeField] private AudioSource footstepSource;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] [Range(0f, 1f)] private float footstepVolume = 0.35f;
    [SerializeField] [Min(0.1f)] private float walkStepDistance = 2f;
    [SerializeField] [Min(0.1f)] private float sprintStepDistance = 1.4f;
    [SerializeField] [Min(1f)] private float sprintVolumeMultiplier = 1.1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Head Bob")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private Vector2 walkBobAmount = new Vector2(0.015f, 0.035f);
    [SerializeField] private Vector2 sprintBobAmount = new Vector2(0.025f, 0.05f);
    [SerializeField] [Min(0f)] private float walkBobFrequency = 8f;
    [SerializeField] [Min(0f)] private float sprintBobFrequency = 11f;
    [SerializeField] [Min(0f)] private float bobSmoothing = 12f;
    [SerializeField] [Min(0f)] private float minimumMovementSpeed = 0.15f;

    private StarterAssetsInputs _input;
    private Vector3 _cameraRestPosition;
    private float _distanceSinceLastStep;
    private float _bobTime;
    private int _lastFootstepIndex = -1;

    private void Awake()
    {
        FindRequiredReferences();

        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
        }

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
        }

        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
        footstepSource.spatialBlend = 0f;

        if (cameraBobTarget != null)
        {
            _cameraRestPosition = cameraBobTarget.localPosition;
        }
    }

    private void LateUpdate()
    {
        if (characterController == null || cameraBobTarget == null)
        {
            return;
        }

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;

        float horizontalSpeed = horizontalVelocity.magnitude;
        bool isGrounded = movementController != null
            ? movementController.Grounded
            : characterController.isGrounded;

        bool isMoving = isGrounded &&
                        horizontalSpeed > minimumMovementSpeed &&
                        !UIState.IsPanelOpen;

        if (!isMoving)
        {
            _distanceSinceLastStep = 0f;
            ReturnCameraToRestPosition();
            return;
        }

        bool isSprinting = _input != null && _input.sprint;

        UpdateFootsteps(horizontalSpeed, isSprinting);
        UpdateHeadBob(isSprinting);
    }

    private void FindRequiredReferences()
    {
        if (characterController == null)
        {
            characterController = GetComponentInChildren<CharacterController>(true);
        }

        if (movementController == null)
        {
            movementController = GetComponentInChildren<FirstPersonController>(true);
        }

        if (movementController != null)
        {
            _input = movementController.GetComponent<StarterAssetsInputs>();

            if (cameraBobTarget == null && movementController.CinemachineCameraTarget != null)
            {
                cameraBobTarget = movementController.CinemachineCameraTarget.transform;
            }
        }
    }

    private void UpdateFootsteps(float horizontalSpeed, bool isSprinting)
    {
        if (footstepClips == null || footstepClips.Length == 0)
        {
            return;
        }

        _distanceSinceLastStep += horizontalSpeed * Time.deltaTime;

        float requiredDistance = isSprinting
            ? sprintStepDistance
            : walkStepDistance;

        if (_distanceSinceLastStep < requiredDistance)
        {
            return;
        }

        _distanceSinceLastStep -= requiredDistance;
        PlayFootstep(isSprinting);
    }

    private void PlayFootstep(bool isSprinting)
    {
        int clipIndex = Random.Range(0, footstepClips.Length);

        if (footstepClips.Length > 1 && clipIndex == _lastFootstepIndex)
        {
            clipIndex = (clipIndex + 1) % footstepClips.Length;
        }

        AudioClip clip = footstepClips[clipIndex];
        if (clip == null)
        {
            return;
        }

        float minimumPitch = Mathf.Min(pitchRange.x, pitchRange.y);
        float maximumPitch = Mathf.Max(pitchRange.x, pitchRange.y);
        float volume = isSprinting
            ? footstepVolume * sprintVolumeMultiplier
            : footstepVolume;

        footstepSource.pitch = Random.Range(minimumPitch, maximumPitch);
        footstepSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        _lastFootstepIndex = clipIndex;
    }

    private void UpdateHeadBob(bool isSprinting)
    {
        if (!enableHeadBob)
        {
            ReturnCameraToRestPosition();
            return;
        }

        Vector2 bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;
        float bobFrequency = isSprinting ? sprintBobFrequency : walkBobFrequency;

        _bobTime += Time.deltaTime * bobFrequency;

        float horizontalOffset = Mathf.Sin(_bobTime * 0.5f) * bobAmount.x;
        float verticalOffset = Mathf.Sin(_bobTime) * bobAmount.y;

        Vector3 targetPosition = _cameraRestPosition +
                                 new Vector3(horizontalOffset, verticalOffset, 0f);

        cameraBobTarget.localPosition = Vector3.Lerp(
            cameraBobTarget.localPosition,
            targetPosition,
            Time.deltaTime * bobSmoothing);
    }

    private void ReturnCameraToRestPosition()
    {
        if (cameraBobTarget == null)
        {
            return;
        }

        cameraBobTarget.localPosition = Vector3.Lerp(
            cameraBobTarget.localPosition,
            _cameraRestPosition,
            Time.deltaTime * bobSmoothing);
    }

    private void OnDisable()
    {
        if (cameraBobTarget != null)
        {
            cameraBobTarget.localPosition = _cameraRestPosition;
        }

        if (footstepSource != null)
        {
            footstepSource.pitch = 1f;
        }
    }
}
