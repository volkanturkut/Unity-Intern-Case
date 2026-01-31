using UnityEngine;
using UnityEngine.Events;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Interactables
{
    /// <summary>
    /// Chest/Container that requires holding to open and can contain items.
    /// Once opened, cannot be opened again.
    /// </summary>
    public class Chest : InteractableBase
    {
        #region Fields

        [Header("Chest Settings")]
        [SerializeField] private float m_OpenDuration = 2f;
        [SerializeField] private bool m_IsLocked;
        [SerializeField] private KeyData m_RequiredKey;

        [Header("Contents")]
        [Tooltip("Keys to give player when opened")]
        [SerializeField] private KeyData[] m_KeyContents;
        [Tooltip("Items to spawn when opened")]
        [SerializeField] private GameObject[] m_ItemPrefabs;
        [SerializeField] private Transform m_SpawnPoint;

        [Header("Lid Animation")]
        [SerializeField] private Transform m_LidTransform;
        [SerializeField] private Vector3 m_ClosedRotation = Vector3.zero;
        [SerializeField] private Vector3 m_OpenRotation = new Vector3(-110f, 0f, 0f);
        [SerializeField] private float m_LidSpeed = 3f;

        [Header("Audio")]
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip m_OpeningSound;
        [SerializeField] private AudioClip m_OpenedSound;
        [SerializeField] private AudioClip m_LockedSound;

        [Header("Messages")]
        [SerializeField] private string m_OpenMessage = "Hold E to Open";
        [SerializeField] private string m_OpeningMessage = "Opening...";
        [SerializeField] private string m_OpenedMessage = "Already Opened";
        [SerializeField] private string m_LockedMessage = "Locked - Key Required";

        [Header("Events")]
        [SerializeField] private UnityEvent m_OnChestOpened;
        [SerializeField] private UnityEvent m_OnContentsRevealed;

        private bool m_IsOpened;
        private bool m_IsOpening;
        private float m_CurrentProgress;
        private Quaternion m_TargetRotation;
        private bool m_ShowingLockedMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets whether the chest has been opened.
        /// </summary>
        public bool IsOpened => m_IsOpened;

        /// <summary>
        /// Gets the interaction type (Hold).
        /// </summary>
        public override InteractionType InteractionType => InteractionType.Hold;

        /// <summary>
        /// Gets the hold duration.
        /// </summary>
        public override float HoldDuration => m_OpenDuration;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_AudioSource == null)
            {
                m_AudioSource = GetComponent<AudioSource>();
            }

            if (m_LidTransform != null)
            {
                m_TargetRotation = Quaternion.Euler(m_ClosedRotation);
                m_LidTransform.localRotation = m_TargetRotation;
            }

            if (m_SpawnPoint == null)
            {
                m_SpawnPoint = transform;
            }
        }

        private void Update()
        {
            // Animate lid
            if (m_LidTransform != null && m_LidTransform.localRotation != m_TargetRotation)
            {
                m_LidTransform.localRotation = Quaternion.Slerp(
                    m_LidTransform.localRotation,
                    m_TargetRotation,
                    Time.deltaTime * m_LidSpeed
                );
            }
        }

        #endregion

        #region InteractableBase Implementation

        /// <inheritdoc/>
        public override bool CanInteract()
        {
            return !m_IsOpened;
        }

        /// <inheritdoc/>
        protected override void ExecuteInteraction()
        {
            if (m_IsOpened)
            {
                return;
            }

            // Check for key requirement
            if (m_IsLocked && m_RequiredKey != null)
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

                // Consume key if needed
                m_IsLocked = false;
            }

            OpenChest();
        }

        /// <inheritdoc/>
        public override void OnHoldProgress(float progress)
        {
            m_CurrentProgress = progress;
            m_IsOpening = progress > 0;

            // Play opening sound at start
            if (progress > 0 && progress < 0.1f && m_AudioSource != null && m_OpeningSound != null)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.clip = m_OpeningSound;
                    m_AudioSource.loop = true;
                    m_AudioSource.Play();
                }
            }

            // Lid stays closed during hold - only opens when completed
        }

        /// <inheritdoc/>
        public override void OnHoldCancelled()
        {
            m_IsOpening = false;
            m_CurrentProgress = 0f;

            // Stop opening sound
            if (m_AudioSource != null && m_AudioSource.isPlaying)
            {
                m_AudioSource.Stop();
                m_AudioSource.loop = false;
            }

            // Close lid back
            if (m_LidTransform != null)
            {
                m_TargetRotation = Quaternion.Euler(m_ClosedRotation);
            }
        }

        /// <inheritdoc/>
        public override string GetPromptMessage()
        {
            if (m_ShowingLockedMessage)
            {
                return m_LockedMessage;
            }

            if (m_IsOpened)
            {
                return m_OpenedMessage;
            }

            if (m_IsOpening)
            {
                return m_OpeningMessage;
            }

            return m_OpenMessage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens the chest and reveals contents.
        /// </summary>
        private void OpenChest()
        {
            if (m_IsOpened) return;

            m_IsOpened = true;
            m_IsOpening = false;

            // Stop opening sound, play opened sound
            if (m_AudioSource != null)
            {
                m_AudioSource.Stop();
                m_AudioSource.loop = false;
                if (m_OpenedSound != null)
                {
                    m_AudioSource.PlayOneShot(m_OpenedSound);
                }
            }

            // Open lid fully
            if (m_LidTransform != null)
            {
                m_TargetRotation = Quaternion.Euler(m_OpenRotation);
            }

            m_OnChestOpened?.Invoke();
            Debug.Log($"[Chest] {gameObject.name} opened!");

            // Reveal contents
            RevealContents();
        }

        /// <summary>
        /// Spawns items and opens loot UI for keys.
        /// </summary>
        private void RevealContents()
        {
            // Open loot UI for key contents
            if (m_KeyContents != null && m_KeyContents.Length > 0)
            {
                var lootUI = UI.ChestLootUI.Instance;
                if (lootUI != null)
                {
                    lootUI.Open(this, m_KeyContents);
                }
                else
                {
                    // Fallback: auto-add if no loot UI exists
                    var inventory = Player.PlayerInventory.Instance;
                    if (inventory != null)
                    {
                        foreach (var key in m_KeyContents)
                        {
                            if (key != null)
                            {
                                inventory.AddKey(key);
                                Debug.Log($"[Chest] Player received: {key.ItemName}");
                            }
                        }
                    }
                }
            }

            // Spawn item prefabs (physical items in world)
            if (m_ItemPrefabs != null)
            {
                float offset = 0f;
                foreach (var prefab in m_ItemPrefabs)
                {
                    if (prefab != null)
                    {
                        Vector3 spawnPos = m_SpawnPoint.position + Vector3.up * 0.5f + m_SpawnPoint.forward * offset;
                        var item = Instantiate(prefab, spawnPos, Quaternion.identity);

                        // Add some pop-out force
                        var rb = item.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddForce(Vector3.up * 3f + Random.insideUnitSphere * 1f, ForceMode.Impulse);
                        }

                        offset += 0.3f;
                    }
                }
            }

            m_OnContentsRevealed?.Invoke();
        }

        /// <summary>
        /// Shows locked feedback.
        /// </summary>
        private void ShowLockedFeedback()
        {
            if (m_AudioSource != null && m_LockedSound != null)
            {
                m_AudioSource.PlayOneShot(m_LockedSound);
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
