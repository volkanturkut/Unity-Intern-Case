using UnityEngine;
using UnityEngine.Events;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.Interactables
{
    /// <summary>
    /// Interactive door that can be opened/closed and optionally locked.
    /// For proper pivot rotation, create an empty parent at the hinge point and assign it as Rotating Part.
    /// </summary>
    public class Door : InteractableBase
    {
        #region Fields

        private const float k_DefaultRotationAngle = 90f;
        private const float k_DefaultRotationSpeed = 5f;

        [Header("Door Settings")]
        [SerializeField] private bool m_IsLocked;
        [SerializeField] private KeyData m_RequiredKey;
        [SerializeField] private bool m_ConsumeKeyOnUse;

        [Header("Rotation Settings")]
        [Tooltip("Create an empty GameObject at the hinge/edge position and assign it here for proper door pivot.")]
        [SerializeField] private Transform m_RotatingPart;
        [SerializeField] private float m_OpenAngle = k_DefaultRotationAngle;
        [SerializeField] private float m_RotationSpeed = k_DefaultRotationSpeed;

        [Header("Audio")]
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip m_OpenSound;
        [SerializeField] private AudioClip m_CloseSound;
        [SerializeField] private AudioClip m_LockedSound;
        [SerializeField] private AudioClip m_UnlockSound;

        [Header("Messages")]
        [SerializeField] private string m_OpenMessage = "Press E to Open";
        [SerializeField] private string m_CloseMessage = "Press E to Close";
        [SerializeField] private string m_LockedMessage = "Locked - Key Required";

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnDoorOpened;
        [SerializeField] private UnityEvent m_OnDoorClosed;
        [SerializeField] private UnityEvent m_OnDoorUnlocked;
        [SerializeField] private UnityEvent m_OnDoorLocked;

        private bool m_IsOpen;
        private Quaternion m_ClosedRotation;
        private Quaternion m_OpenRotation;
        private Quaternion m_TargetRotation;
        private bool m_ShowingLockedMessage;
        private Coroutine m_LockedMessageCoroutine;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the door is currently open.
        /// </summary>
        public bool IsOpen => m_IsOpen;

        /// <summary>
        /// Gets whether the door is locked.
        /// </summary>
        public bool IsLocked => m_IsLocked;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_RotatingPart == null)
            {
                m_RotatingPart = transform;
            }

            if (m_AudioSource == null)
            {
                m_AudioSource = GetComponent<AudioSource>();
            }

            m_ClosedRotation = m_RotatingPart.localRotation;
            m_OpenRotation = m_ClosedRotation * Quaternion.Euler(0f, m_OpenAngle, 0f);
            m_TargetRotation = m_ClosedRotation;
        }

        private void Update()
        {
            if (m_RotatingPart.localRotation != m_TargetRotation)
            {
                m_RotatingPart.localRotation = Quaternion.Slerp(
                    m_RotatingPart.localRotation,
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
            // Always allow interaction attempt - we show "locked" message in ExecuteInteraction
            return true;
        }

        /// <inheritdoc/>
        protected override void ExecuteInteraction()
        {
            // Check if door is locked
            if (m_IsLocked && m_RequiredKey != null)
            {
                var inventory = PlayerInventory.Instance;
                if (inventory == null)
                {
                    ShowLockedFeedback();
                    return;
                }

                // Check if holding the correct key
                KeyData selectedKey = inventory.SelectedKey;
                bool isHoldingCorrectKey = selectedKey != null && selectedKey.KeyId == m_RequiredKey.KeyId;

                if (!isHoldingCorrectKey)
                {
                    // Show locked feedback
                    ShowLockedFeedback();
                    return;
                }

                // Has correct key - unlock
                Unlock();
            }

            ToggleDoor();
        }

        /// <summary>
        /// Shows feedback when door is locked.
        /// </summary>
        private void ShowLockedFeedback()
        {
            PlaySound(m_LockedSound);

            // Show locked message temporarily
            if (m_LockedMessageCoroutine != null)
            {
                StopCoroutine(m_LockedMessageCoroutine);
            }
            m_LockedMessageCoroutine = StartCoroutine(ShowLockedMessageCoroutine());
        }

        /// <summary>
        /// Coroutine to show locked message temporarily.
        /// </summary>
        private System.Collections.IEnumerator ShowLockedMessageCoroutine()
        {
            m_ShowingLockedMessage = true;
            yield return new WaitForSeconds(2f);
            m_ShowingLockedMessage = false;
            m_LockedMessageCoroutine = null;
        }

        /// <inheritdoc/>
        public override string GetPromptMessage()
        {
            // Show locked message temporarily after trying without key
            if (m_ShowingLockedMessage)
            {
                return m_LockedMessage;
            }

            // Normal open/close message
            return m_IsOpen ? m_CloseMessage : m_OpenMessage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Toggles the door open/closed state.
        /// </summary>
        public void ToggleDoor()
        {
            m_IsOpen = !m_IsOpen;
            m_TargetRotation = m_IsOpen ? m_OpenRotation : m_ClosedRotation;

            if (m_IsOpen)
            {
                PlaySound(m_OpenSound);
                m_OnDoorOpened?.Invoke();
            }
            else
            {
                PlaySound(m_CloseSound);
                m_OnDoorClosed?.Invoke();
            }
        }

        /// <summary>
        /// Opens the door (can be called externally, e.g., by a switch).
        /// </summary>
        public void Open()
        {
            if (!m_IsOpen)
            {
                ToggleDoor();
            }
        }

        /// <summary>
        /// Closes the door.
        /// </summary>
        public void Close()
        {
            if (m_IsOpen)
            {
                ToggleDoor();
            }
        }

        /// <summary>
        /// Unlocks the door.
        /// </summary>
        public void Unlock()
        {
            if (!m_IsLocked)
            {
                return;
            }

            if (m_ConsumeKeyOnUse && m_RequiredKey != null)
            {
                var inventory = PlayerInventory.Instance;
                if (inventory != null)
                {
                    inventory.RemoveKey(m_RequiredKey);
                }
            }

            m_IsLocked = false;
            PlaySound(m_UnlockSound);
            m_OnDoorUnlocked?.Invoke();
        }

        /// <summary>
        /// Locks the door.
        /// </summary>
        public void Lock()
        {
            m_IsLocked = true;
            m_OnDoorLocked?.Invoke();
        }

        /// <summary>
        /// Plays an audio clip if available.
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip == null || m_AudioSource == null)
            {
                return;
            }

            m_AudioSource.PlayOneShot(clip);
        }

        #endregion
    }
}
