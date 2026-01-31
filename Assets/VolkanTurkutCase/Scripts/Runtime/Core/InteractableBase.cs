using System;
using UnityEngine;

namespace VolkanTurkutCase.Runtime.Core
{
    /// <summary>
    /// Abstract base class for all interactable objects.
    /// Provides common functionality and enforces Ludu Arts coding standards.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        #region Fields

        /// <summary>
        /// Default hold duration for Hold type interactions.
        /// </summary>
        private const float k_DefaultHoldDuration = 2f;

        [Header("Interaction Settings")]
        [SerializeField] private InteractionType m_InteractionType = InteractionType.Instant;
        [SerializeField] private float m_HoldDuration = k_DefaultHoldDuration;
        [SerializeField] private string m_PromptMessage = "Press E to Interact";
        [SerializeField] private string m_CannotInteractMessage = "Cannot interact";

        #endregion

        #region Events

        /// <summary>
        /// Invoked when interaction is completed successfully.
        /// </summary>
        public event Action OnInteractionComplete;

        /// <summary>
        /// Invoked when interaction fails or is cancelled.
        /// </summary>
        public event Action OnInteractionFailed;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public InteractionType InteractionType => m_InteractionType;

        /// <inheritdoc/>
        public float HoldDuration => m_HoldDuration;

        /// <summary>
        /// Gets the default prompt message.
        /// </summary>
        protected string PromptMessage => m_PromptMessage;

        /// <summary>
        /// Gets the message shown when interaction is not possible.
        /// </summary>
        protected string CannotInteractMessage => m_CannotInteractMessage;

        #endregion

        #region IInteractable Implementation

        /// <inheritdoc/>
        public abstract bool CanInteract();

        /// <inheritdoc/>
        public virtual void Interact()
        {
            if (!CanInteract())
            {
                Debug.LogWarning($"[{GetType().Name}] Cannot interact with {gameObject.name}");
                OnInteractionFailed?.Invoke();
                return;
            }

            ExecuteInteraction();
            OnInteractionComplete?.Invoke();
        }

        /// <inheritdoc/>
        public virtual void OnHoldProgress(float progress)
        {
            // Override in derived classes if needed
        }

        /// <inheritdoc/>
        public virtual void OnHoldCancelled()
        {
            // Override in derived classes if needed
        }

        /// <inheritdoc/>
        public virtual string GetPromptMessage()
        {
            return CanInteract() ? m_PromptMessage : m_CannotInteractMessage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the actual interaction logic. Must be implemented by derived classes.
        /// </summary>
        protected abstract void ExecuteInteraction();

        /// <summary>
        /// Invokes the OnInteractionComplete event. For use by derived classes.
        /// </summary>
        protected void RaiseInteractionComplete()
        {
            OnInteractionComplete?.Invoke();
        }

        /// <summary>
        /// Invokes the OnInteractionFailed event. For use by derived classes.
        /// </summary>
        protected void RaiseInteractionFailed()
        {
            OnInteractionFailed?.Invoke();
        }

        #endregion
    }
}
