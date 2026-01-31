using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Player
{
    /// <summary>
    /// Enhanced inventory system with hotbar selection and key dropping.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Fields

        private const int k_MaxHotbarSlots = 4;

        private static PlayerInventory s_Instance;

        [Header("Settings")]
        [SerializeField] private int m_MaxSlots = k_MaxHotbarSlots;

        [Header("Drop Settings")]
        [SerializeField] private float m_DropDistance = 2f;
        [SerializeField] private float m_DropHeight = 0.5f;

        [Header("Key Prefabs")]
        [Tooltip("Prefab to spawn when dropping keys")]
        [SerializeField] private GameObject m_KeyPickupPrefab;

        private List<KeyData> m_CollectedKeys = new List<KeyData>();
        private int m_SelectedSlot = 0;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a key is added to the inventory.
        /// </summary>
        public event Action<KeyData> OnKeyAdded;

        /// <summary>
        /// Invoked when a key is removed from the inventory.
        /// </summary>
        public event Action<KeyData> OnKeyRemoved;

        /// <summary>
        /// Invoked when the inventory changes.
        /// </summary>
        public event Action OnInventoryChanged;

        /// <summary>
        /// Invoked when the selected slot changes.
        /// </summary>
        public event Action<int> OnSlotSelected;

        /// <summary>
        /// Invoked when a key is dropped.
        /// </summary>
        public event Action<KeyData, Vector3> OnKeyDropped;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance of the player inventory.
        /// </summary>
        public static PlayerInventory Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<PlayerInventory>();
                    if (s_Instance == null)
                    {
                        Debug.LogError("[PlayerInventory] No PlayerInventory found in scene!");
                    }
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Gets a read-only list of collected keys.
        /// </summary>
        public IReadOnlyList<KeyData> CollectedKeys => m_CollectedKeys;

        /// <summary>
        /// Gets the currently selected slot index.
        /// </summary>
        public int SelectedSlot => m_SelectedSlot;

        /// <summary>
        /// Gets the key in the selected slot, if any.
        /// </summary>
        public KeyData SelectedKey => m_SelectedSlot < m_CollectedKeys.Count ? m_CollectedKeys[m_SelectedSlot] : null;

        /// <summary>
        /// Gets the maximum number of hotbar slots.
        /// </summary>
        public int MaxSlots => m_MaxSlots;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("[PlayerInventory] Multiple instances detected, destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
        }

        private void Update()
        {
            HandleHotbarInput();
            HandleDropInput();
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles keyboard input for slot selection (1-4 keys).
        /// </summary>
        private void HandleHotbarInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SelectSlot(0);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SelectSlot(1);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                SelectSlot(2);
            }
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                SelectSlot(3);
            }
        }

        /// <summary>
        /// Handles drop input (Q key).
        /// </summary>
        private void HandleDropInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                DropSelectedKey();
            }
        }

        /// <summary>
        /// Selects a hotbar slot.
        /// </summary>
        /// <param name="slotIndex">The slot index (0-3).</param>
        public void SelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= m_MaxSlots)
            {
                return;
            }

            m_SelectedSlot = slotIndex;
            OnSlotSelected?.Invoke(slotIndex);
            Debug.Log($"[PlayerInventory] Selected slot {slotIndex + 1}");
        }

        /// <summary>
        /// Drops the currently selected key.
        /// </summary>
        public void DropSelectedKey()
        {
            if (SelectedKey == null)
            {
                Debug.Log("[PlayerInventory] No key to drop in selected slot.");
                return;
            }

            KeyData keyToDrop = SelectedKey;
            Vector3 dropPosition = CalculateDropPosition();

            // Remove from inventory
            m_CollectedKeys.RemoveAt(m_SelectedSlot);

            // Spawn the key pickup prefab
            SpawnDroppedKey(keyToDrop, dropPosition);

            // Adjust selected slot if needed
            if (m_SelectedSlot >= m_CollectedKeys.Count && m_CollectedKeys.Count > 0)
            {
                m_SelectedSlot = m_CollectedKeys.Count - 1;
            }

            OnKeyRemoved?.Invoke(keyToDrop);
            OnKeyDropped?.Invoke(keyToDrop, dropPosition);
            OnInventoryChanged?.Invoke();

            Debug.Log($"[PlayerInventory] Dropped key: {keyToDrop.ItemName}");
        }

        /// <summary>
        /// Calculates the position to drop the key.
        /// </summary>
        private Vector3 CalculateDropPosition()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                return cam.transform.position + cam.transform.forward * m_DropDistance + Vector3.down * m_DropHeight;
            }
            return transform.position + transform.forward * m_DropDistance;
        }

        /// <summary>
        /// Spawns a dropped key in the world.
        /// </summary>
        private void SpawnDroppedKey(KeyData keyData, Vector3 position)
        {
            if (m_KeyPickupPrefab == null)
            {
                Debug.LogWarning("[PlayerInventory] Key pickup prefab not assigned. Cannot spawn dropped key.");
                return;
            }

            GameObject droppedKey = Instantiate(m_KeyPickupPrefab, position, Quaternion.identity);

            // Set the key data on the spawned pickup
            var keyPickup = droppedKey.GetComponent<Interactables.KeyPickup>();
            if (keyPickup != null)
            {
                keyPickup.SetKeyData(keyData);
            }

            // Add some physics if there's a rigidbody
            var rb = droppedKey.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * 2f + Camera.main.transform.forward * 3f, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Adds a key to the inventory.
        /// </summary>
        /// <param name="key">The key data to add.</param>
        public void AddKey(KeyData key)
        {
            if (key == null)
            {
                Debug.LogError("[PlayerInventory] Cannot add null key!");
                return;
            }

            if (m_CollectedKeys.Count >= m_MaxSlots)
            {
                Debug.LogWarning("[PlayerInventory] Inventory full! Cannot add more keys.");
                return;
            }

            m_CollectedKeys.Add(key);
            Debug.Log($"[PlayerInventory] Added key: {key.ItemName}");

            OnKeyAdded?.Invoke(key);
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Checks if the inventory contains a specific key.
        /// </summary>
        /// <param name="key">The key data to check for.</param>
        /// <returns>True if the key is in the inventory.</returns>
        public bool HasKey(KeyData key)
        {
            if (key == null)
            {
                return false;
            }

            return m_CollectedKeys.Exists(k => k.KeyId == key.KeyId);
        }

        /// <summary>
        /// Removes a key from the inventory.
        /// </summary>
        /// <param name="key">The key data to remove.</param>
        /// <returns>True if the key was removed.</returns>
        public bool RemoveKey(KeyData key)
        {
            if (key == null)
            {
                Debug.LogError("[PlayerInventory] Cannot remove null key!");
                return false;
            }

            var foundKey = m_CollectedKeys.Find(k => k.KeyId == key.KeyId);
            if (foundKey != null)
            {
                m_CollectedKeys.Remove(foundKey);
                Debug.Log($"[PlayerInventory] Removed key: {key.ItemName}");

                OnKeyRemoved?.Invoke(key);
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the key at a specific slot.
        /// </summary>
        /// <param name="slotIndex">The slot index.</param>
        /// <returns>The key data, or null if slot is empty.</returns>
        public KeyData GetKeyAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= m_CollectedKeys.Count)
            {
                return null;
            }
            return m_CollectedKeys[slotIndex];
        }

        /// <summary>
        /// Clears all keys from the inventory.
        /// </summary>
        public void ClearInventory()
        {
            m_CollectedKeys.Clear();
            m_SelectedSlot = 0;
            OnInventoryChanged?.Invoke();
        }

        #endregion
    }
}
