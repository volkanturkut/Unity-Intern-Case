using UnityEngine;
using UnityEngine.Events;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Interactables
{
    /// <summary>
    /// Container that requires holding the interaction key to open.
    /// </summary>
    public class Chest : InteractableBase
    {
        #region Fields

        private const float k_DefaultOpenAngle = -110f;
        private const float k_DefaultRotationSpeed = 3f;

        [Header("Chest Settings")]
        [SerializeField] private bool m_IsOpen;
        [SerializeField] private bool m_CanReopen;
        [SerializeField] private Transform m_LidTransform;
        [SerializeField] private float m_OpenAngle = k_DefaultOpenAngle;
        [SerializeField] private float m_RotationSpeed = k_DefaultRotationSpeed;

        [Header("Contents")]
        [SerializeField] private ItemData[] m_Contents;
        [SerializeField] private GameObject m_ContentsVisual;

        [Header("Messages")]
        [SerializeField] private string m_HoldMessage = "Hold E to Open";
        [SerializeField] private string m_OpenedMessage = "Empty";

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnChestOpened;
        [SerializeField] private UnityEvent m_OnChestClosed;
        [SerializeField] private UnityEvent<ItemData[]> m_OnContentsRevealed;

        private bool m_HasBeenOpened;
        private Quaternion m_ClosedRotation;
        private Quaternion m_OpenRotation;
        private Quaternion m_TargetRotation;
        private float m_CurrentHoldProgress;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the chest is currently open.
        /// </summary>
        public bool IsOpen => m_IsOpen;

        /// <summary>
        /// Gets whether the chest has been opened before.
        /// </summary>
        public bool HasBeenOpened => m_HasBeenOpened;

        /// <summary>
        /// Gets the contents of the chest.
        /// </summary>
        public ItemData[] Contents => m_Contents;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_LidTransform == null)
            {
                Debug.LogWarning($"[Chest] Lid transform not assigned on {gameObject.name}");
                m_LidTransform = transform;
            }

            m_ClosedRotation = m_LidTransform.localRotation;
            m_OpenRotation = m_ClosedRotation * Quaternion.Euler(m_OpenAngle, 0f, 0f);
            m_TargetRotation = m_IsOpen ? m_OpenRotation : m_ClosedRotation;

            if (m_ContentsVisual != null)
            {
                m_ContentsVisual.SetActive(m_IsOpen && !m_HasBeenOpened);
            }
        }

        private void Update()
        {
            if (m_LidTransform.localRotation != m_TargetRotation)
            {
                m_LidTransform.localRotation = Quaternion.Slerp(
                    m_LidTransform.localRotation,
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
            if (m_HasBeenOpened && !m_CanReopen)
            {
                return false;
            }
            return !m_IsOpen;
        }

        /// <inheritdoc/>
        protected override void ExecuteInteraction()
        {
            OpenChest();
        }

        /// <inheritdoc/>
        public override void OnHoldProgress(float progress)
        {
            m_CurrentHoldProgress = progress;
        }

        /// <inheritdoc/>
        public override void OnHoldCancelled()
        {
            m_CurrentHoldProgress = 0f;
        }

        /// <inheritdoc/>
        public override string GetPromptMessage()
        {
            if (m_HasBeenOpened && !m_CanReopen)
            {
                return m_OpenedMessage;
            }

            if (m_IsOpen)
            {
                return m_OpenedMessage;
            }

            return m_HoldMessage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens the chest and reveals contents.
        /// </summary>
        public void OpenChest()
        {
            if (m_IsOpen)
            {
                return;
            }

            m_IsOpen = true;
            m_TargetRotation = m_OpenRotation;

            if (!m_HasBeenOpened)
            {
                m_HasBeenOpened = true;
                RevealContents();
            }

            m_OnChestOpened?.Invoke();
            Debug.Log($"[Chest] {gameObject.name} opened.");
        }

        /// <summary>
        /// Closes the chest (if reopening is allowed).
        /// </summary>
        public void CloseChest()
        {
            if (!m_IsOpen || !m_CanReopen)
            {
                return;
            }

            m_IsOpen = false;
            m_TargetRotation = m_ClosedRotation;

            m_OnChestClosed?.Invoke();
            Debug.Log($"[Chest] {gameObject.name} closed.");
        }

        /// <summary>
        /// Reveals the contents of the chest.
        /// </summary>
        private void RevealContents()
        {
            if (m_ContentsVisual != null)
            {
                m_ContentsVisual.SetActive(true);
            }

            if (m_Contents != null && m_Contents.Length > 0)
            {
                m_OnContentsRevealed?.Invoke(m_Contents);
                
                foreach (var item in m_Contents)
                {
                    if (item != null)
                    {
                        Debug.Log($"[Chest] Contains: {item.ItemName}");
                    }
                }
            }
        }

        #endregion
    }
}
