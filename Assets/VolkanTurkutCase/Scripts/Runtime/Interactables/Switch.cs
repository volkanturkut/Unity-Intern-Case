using UnityEngine;
using UnityEngine.Events;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Interactables
{
    /// <summary>
    /// Switch/Lever that can be toggled to trigger other objects.
    /// Can be connected to doors, lights, or any other object via UnityEvents.
    /// </summary>
    public class Switch : InteractableBase
    {
        #region Fields

        [Header("Switch Settings")]
        [SerializeField] private bool m_IsOn;
        [SerializeField] private bool m_CanToggleOff = true;
        [SerializeField] private bool m_RequireKey;
        [SerializeField] private KeyData m_RequiredKey;

        [Header("Visual Settings")]
        [SerializeField] private Transform m_LeverTransform;
        [SerializeField] private Vector3 m_OffRotation = new Vector3(0f, 0f, -30f);
        [SerializeField] private Vector3 m_OnRotation = new Vector3(0f, 0f, 30f);
        [SerializeField] private float m_RotationSpeed = 5f;

        [Header("Audio")]
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip m_SwitchOnSound;
        [SerializeField] private AudioClip m_SwitchOffSound;

        [Header("Messages")]
        [SerializeField] private string m_TurnOnMessage = "Press E to Activate";
        [SerializeField] private string m_TurnOffMessage = "Press E to Deactivate";
        [SerializeField] private string m_LockedMessage = "Requires Key";

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnSwitchOn;
        [SerializeField] private UnityEvent m_OnSwitchOff;
        [SerializeField] private UnityEvent<bool> m_OnSwitchToggled;

        private Quaternion m_TargetRotation;
        private bool m_ShowingLockedMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the switch is currently on.
        /// </summary>
        public bool IsOn => m_IsOn;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_AudioSource == null)
            {
                m_AudioSource = GetComponent<AudioSource>();
            }

            // Set initial rotation
            if (m_LeverTransform != null)
            {
                m_TargetRotation = Quaternion.Euler(m_IsOn ? m_OnRotation : m_OffRotation);
                m_LeverTransform.localRotation = m_TargetRotation;
            }
        }

        private void Update()
        {
            // Animate lever
            if (m_LeverTransform != null && m_LeverTransform.localRotation != m_TargetRotation)
            {
                m_LeverTransform.localRotation = Quaternion.Slerp(
                    m_LeverTransform.localRotation,
                    m_TargetRotation,
                    Time.deltaTime * m_RotationSpeed
                );
            }
        }

        #endregion

        #region InteractableBase Implementation

        /// <inheritdoc/>
        public override bool CanInteract()
        {
            return true;
        }

        /// <inheritdoc/>
        protected override void ExecuteInteraction()
        {
            // Check for key requirement
            if (m_RequireKey && m_RequiredKey != null)
            {
                var inventory = Player.PlayerInventory.Instance;
                if (inventory == null)
                {
                    ShowLockedFeedback();
                    return;
                }

                var selectedKey = inventory.SelectedKey;
                if (selectedKey == null || selectedKey.KeyId != m_RequiredKey.KeyId)
                {
                    ShowLockedFeedback();
                    return;
                }
            }

            // Check if can toggle off
            if (m_IsOn && !m_CanToggleOff)
            {
                return;
            }

            Toggle();
        }

        /// <inheritdoc/>
        public override string GetPromptMessage()
        {
            if (m_ShowingLockedMessage)
            {
                return m_LockedMessage;
            }

            if (!m_IsOn)
            {
                return m_TurnOnMessage;
            }

            return m_CanToggleOff ? m_TurnOffMessage : "Already Activated";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Toggles the switch state.
        /// </summary>
        public void Toggle()
        {
            m_IsOn = !m_IsOn;
            UpdateState();
        }

        /// <summary>
        /// Sets the switch to on.
        /// </summary>
        public void TurnOn()
        {
            if (!m_IsOn)
            {
                m_IsOn = true;
                UpdateState();
            }
        }

        /// <summary>
        /// Sets the switch to off.
        /// </summary>
        public void TurnOff()
        {
            if (m_IsOn)
            {
                m_IsOn = false;
                UpdateState();
            }
        }

        /// <summary>
        /// Updates visual and triggers events based on state.
        /// </summary>
        private void UpdateState()
        {
            // Update rotation target
            if (m_LeverTransform != null)
            {
                m_TargetRotation = Quaternion.Euler(m_IsOn ? m_OnRotation : m_OffRotation);
            }

            // Play sound
            if (m_AudioSource != null)
            {
                AudioClip clip = m_IsOn ? m_SwitchOnSound : m_SwitchOffSound;
                if (clip != null)
                {
                    m_AudioSource.PlayOneShot(clip);
                }
            }

            // Trigger events
            if (m_IsOn)
            {
                m_OnSwitchOn?.Invoke();
            }
            else
            {
                m_OnSwitchOff?.Invoke();
            }

            m_OnSwitchToggled?.Invoke(m_IsOn);
        }

        /// <summary>
        /// Shows locked feedback.
        /// </summary>
        private void ShowLockedFeedback()
        {
            if (m_AudioSource != null && m_SwitchOffSound != null)
            {
                m_AudioSource.PlayOneShot(m_SwitchOffSound);
            }
            StartCoroutine(ShowLockedMessageCoroutine());
        }

        private System.Collections.IEnumerator ShowLockedMessageCoroutine()
        {
            m_ShowingLockedMessage = true;
            yield return new WaitForSeconds(2f);
            m_ShowingLockedMessage = false;
        }

        #endregion
    }
}
