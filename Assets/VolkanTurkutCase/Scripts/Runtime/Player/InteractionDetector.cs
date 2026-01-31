using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Player
{
    /// <summary>
    /// Detects and manages interactions with IInteractable objects using raycasting.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        #region Fields

        private const float k_DefaultInteractionRange = 3f;
        private const float k_HoldProgressUpdateInterval = 0.016f;

        [Header("Detection Settings")]
        [SerializeField] private float m_InteractionRange = k_DefaultInteractionRange;
        [SerializeField] private LayerMask m_InteractableLayer = -1;
        [SerializeField] private Transform m_RaycastOrigin;

        [Header("Input Settings")]
        [SerializeField] private InputActionReference m_InteractAction;

        private IInteractable m_CurrentTarget;
        private bool m_IsHolding;
        private float m_HoldTimer;
        private Camera m_Camera;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a new interactable is detected.
        /// </summary>
        public event Action<IInteractable> OnInteractableDetected;

        /// <summary>
        /// Invoked when the current interactable is no longer in range.
        /// </summary>
        public event Action OnInteractableLost;

        /// <summary>
        /// Invoked during hold interaction with current progress (0-1).
        /// </summary>
        public event Action<float> OnHoldProgressChanged;

        /// <summary>
        /// Invoked when hold interaction is cancelled.
        /// </summary>
        public event Action OnHoldCancelled;

        /// <summary>
        /// Invoked when hold interaction completes successfully.
        /// </summary>
        public event Action OnHoldCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the currently targeted interactable, if any.
        /// </summary>
        public IInteractable CurrentTarget => m_CurrentTarget;

        /// <summary>
        /// Gets whether a hold interaction is in progress.
        /// </summary>
        public bool IsHolding => m_IsHolding;

        /// <summary>
        /// Gets the current hold progress (0-1).
        /// </summary>
        public float HoldProgress => m_CurrentTarget != null && m_CurrentTarget.HoldDuration > 0
            ? m_HoldTimer / m_CurrentTarget.HoldDuration
            : 0f;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            m_Camera = Camera.main;
            if (m_Camera == null)
            {
                Debug.LogError("[InteractionDetector] Main camera not found!");
            }

            if (m_RaycastOrigin == null)
            {
                m_RaycastOrigin = m_Camera?.transform;
            }
        }

        private void OnEnable()
        {
            if (m_InteractAction != null && m_InteractAction.action != null)
            {
                m_InteractAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (m_InteractAction != null && m_InteractAction.action != null)
            {
                m_InteractAction.action.Disable();
            }

            CancelHold();
        }

        private void Update()
        {
            DetectInteractable();
            HandleInput();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs raycast to detect interactable objects.
        /// </summary>
        private void DetectInteractable()
        {
            if (m_RaycastOrigin == null)
            {
                return;
            }

            Ray ray = new Ray(m_RaycastOrigin.position, m_RaycastOrigin.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, m_InteractionRange, m_InteractableLayer))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable == null)
                {
                    interactable = hit.collider.GetComponentInParent<IInteractable>();
                }

                if (interactable != null)
                {
                    if (m_CurrentTarget != interactable)
                    {
                        SetNewTarget(interactable);
                    }
                    return;
                }
            }

            if (m_CurrentTarget != null)
            {
                ClearTarget();
            }
        }

        /// <summary>
        /// Handles interaction input based on interaction type.
        /// </summary>
        private void HandleInput()
        {
            if (m_CurrentTarget == null || m_InteractAction == null)
            {
                return;
            }

            bool isPressed = m_InteractAction.action.IsPressed();
            bool wasPressed = m_InteractAction.action.WasPressedThisFrame();

            switch (m_CurrentTarget.InteractionType)
            {
                case InteractionType.Instant:
                case InteractionType.Toggle:
                    if (wasPressed)
                    {
                        TryInteract();
                    }
                    break;

                case InteractionType.Hold:
                    HandleHoldInteraction(isPressed);
                    break;
            }
        }

        /// <summary>
        /// Handles hold-type interactions.
        /// </summary>
        private void HandleHoldInteraction(bool isPressed)
        {
            if (!m_CurrentTarget.CanInteract())
            {
                if (m_IsHolding)
                {
                    CancelHold();
                }
                return;
            }

            if (isPressed)
            {
                m_IsHolding = true;
                m_HoldTimer += Time.deltaTime;

                float progress = m_HoldTimer / m_CurrentTarget.HoldDuration;
                m_CurrentTarget.OnHoldProgress(progress);
                OnHoldProgressChanged?.Invoke(progress);

                if (m_HoldTimer >= m_CurrentTarget.HoldDuration)
                {
                    m_CurrentTarget.Interact();
                    OnHoldCompleted?.Invoke();
                    ResetHold();
                }
            }
            else if (m_IsHolding)
            {
                CancelHold();
            }
        }

        /// <summary>
        /// Attempts to interact with the current target.
        /// </summary>
        public void TryInteract()
        {
            if (m_CurrentTarget == null)
            {
                Debug.LogWarning("[InteractionDetector] No target to interact with.");
                return;
            }

            if (!m_CurrentTarget.CanInteract())
            {
                Debug.Log($"[InteractionDetector] Cannot interact: {m_CurrentTarget.GetPromptMessage()}");
                return;
            }

            m_CurrentTarget.Interact();
        }

        private void SetNewTarget(IInteractable target)
        {
            if (m_CurrentTarget != null)
            {
                CancelHold();
            }

            m_CurrentTarget = target;
            OnInteractableDetected?.Invoke(target);
        }

        private void ClearTarget()
        {
            CancelHold();
            m_CurrentTarget = null;
            OnInteractableLost?.Invoke();
        }

        private void CancelHold()
        {
            if (m_IsHolding)
            {
                m_CurrentTarget?.OnHoldCancelled();
                OnHoldCancelled?.Invoke();
            }
            ResetHold();
        }

        private void ResetHold()
        {
            m_IsHolding = false;
            m_HoldTimer = 0f;
        }

        #endregion
    }
}
