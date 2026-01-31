using UnityEngine;
using UnityEngine.Events;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Interactables
{
    /// <summary>
    /// Toggle switch that can trigger other objects when activated.
    /// </summary>
    public class Switch : InteractableBase
    {
        #region Fields

        [Header("Switch Settings")]
        [SerializeField] private bool m_IsOn;
        [SerializeField] private bool m_OneTimeUse;
        [SerializeField] private Transform m_LeverVisual;
        [SerializeField] private float m_OnRotation = 45f;
        [SerializeField] private float m_OffRotation = -45f;

        [Header("Messages")]
        [SerializeField] private string m_TurnOnMessage = "Press E to Activate";
        [SerializeField] private string m_TurnOffMessage = "Press E to Deactivate";
        [SerializeField] private string m_UsedMessage = "Already Used";

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnSwitchActivated;
        [SerializeField] private UnityEvent m_OnSwitchDeactivated;
        [SerializeField] private UnityEvent m_OnSwitchToggled;

        private bool m_HasBeenUsed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the switch is currently on.
        /// </summary>
        public bool IsOn => m_IsOn;

        /// <summary>
        /// Gets whether the switch has been used (for one-time switches).
        /// </summary>
        public bool HasBeenUsed => m_HasBeenUsed;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            UpdateVisual();
        }

        #endregion

        #region InteractableBase Implementation

        /// <inheritdoc/>
        public override bool CanInteract()
        {
            if (m_OneTimeUse && m_HasBeenUsed)
            {
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        protected override void ExecuteInteraction()
        {
            Toggle();
        }

        /// <inheritdoc/>
        public override string GetPromptMessage()
        {
            if (m_OneTimeUse && m_HasBeenUsed)
            {
                return m_UsedMessage;
            }

            return m_IsOn ? m_TurnOffMessage : m_TurnOnMessage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Toggles the switch state.
        /// </summary>
        public void Toggle()
        {
            m_IsOn = !m_IsOn;
            m_HasBeenUsed = true;

            UpdateVisual();

            m_OnSwitchToggled?.Invoke();

            if (m_IsOn)
            {
                m_OnSwitchActivated?.Invoke();
                Debug.Log($"[Switch] {gameObject.name} activated.");
            }
            else
            {
                m_OnSwitchDeactivated?.Invoke();
                Debug.Log($"[Switch] {gameObject.name} deactivated.");
            }
        }

        /// <summary>
        /// Sets the switch to on state.
        /// </summary>
        public void TurnOn()
        {
            if (!m_IsOn)
            {
                Toggle();
            }
        }

        /// <summary>
        /// Sets the switch to off state.
        /// </summary>
        public void TurnOff()
        {
            if (m_IsOn)
            {
                Toggle();
            }
        }

        /// <summary>
        /// Updates the visual representation of the switch.
        /// </summary>
        private void UpdateVisual()
        {
            if (m_LeverVisual == null)
            {
                return;
            }

            float targetRotation = m_IsOn ? m_OnRotation : m_OffRotation;
            m_LeverVisual.localRotation = Quaternion.Euler(targetRotation, 0f, 0f);
        }

        #endregion
    }
}
