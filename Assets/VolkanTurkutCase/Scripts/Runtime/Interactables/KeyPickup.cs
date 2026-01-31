using UnityEngine;
using UnityEngine.Events;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.Interactables
{
    /// <summary>
    /// Collectable key that can be picked up to unlock doors.
    /// </summary>
    public class KeyPickup : InteractableBase
    {
        #region Fields

        [Header("Key Settings")]
        [SerializeField] private KeyData m_KeyData;

        [Header("Pickup Settings")]
        [SerializeField] private bool m_DestroyOnPickup = true;
        [SerializeField] private GameObject m_VisualObject;

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnKeyPickedUp;

        private bool m_IsCollected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key data for this pickup.
        /// </summary>
        public KeyData KeyData => m_KeyData;

        /// <summary>
        /// Gets whether this key has been collected.
        /// </summary>
        public bool IsCollected => m_IsCollected;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the key data (used for dropped keys).
        /// </summary>
        public void SetKeyData(KeyData keyData)
        {
            m_KeyData = keyData;
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_VisualObject == null)
            {
                m_VisualObject = gameObject;
            }

            if (m_KeyData == null)
            {
                Debug.LogError($"[KeyPickup] KeyData is not assigned on {gameObject.name}!");
            }
        }

        #endregion

        #region InteractableBase Implementation

        /// <inheritdoc/>
        public override bool CanInteract()
        {
            return !m_IsCollected && m_KeyData != null;
        }

        /// <inheritdoc/>
        protected override void ExecuteInteraction()
        {
            var inventory = PlayerInventory.Instance;
            if (inventory == null)
            {
                Debug.LogError("[KeyPickup] PlayerInventory not found!");
                return;
            }

            inventory.AddKey(m_KeyData);
            m_IsCollected = true;

            m_OnKeyPickedUp?.Invoke();
            Debug.Log($"[KeyPickup] Collected key: {m_KeyData.ItemName}");

            if (m_DestroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                m_VisualObject.SetActive(false);
            }
        }

        /// <inheritdoc/>
        public override string GetPromptMessage()
        {
            if (m_KeyData == null)
            {
                return "Missing Key Data";
            }

            if (m_IsCollected)
            {
                return "Already Collected";
            }

            return $"Press E to pick up {m_KeyData.ItemName}";
        }

        #endregion
    }
}
