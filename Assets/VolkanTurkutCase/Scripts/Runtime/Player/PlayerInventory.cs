using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.Player
{
    /// <summary>
    /// Enhanced inventory system with fixed hotbar slots and key dropping.
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

        [Header("Key Prefabs")]
        [Tooltip("Prefab to spawn when dropping keys")]
        [SerializeField] private GameObject m_KeyPickupPrefab;

        // Fixed array - slots are fixed positions, can be null (empty)
        private KeyData[] m_Slots;
        private int m_SelectedSlot = 0;

        #endregion

        #region Events

        public event Action<KeyData> OnKeyAdded;
        public event Action<KeyData> OnKeyRemoved;
        public event Action OnInventoryChanged;
        public event Action<int> OnSlotSelected;
        public event Action<KeyData, Vector3> OnKeyDropped;

        #endregion

        #region Properties

        public static PlayerInventory Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<PlayerInventory>();
                    if (s_Instance == null)
                    {
                        return null;
                    }
                }
                return s_Instance;
            }
        }

        public int SelectedSlot => m_SelectedSlot;

        /// <summary>
        /// Gets the key in the selected slot (null if empty).
        /// </summary>
        public KeyData SelectedKey => m_Slots != null && m_SelectedSlot < m_Slots.Length ? m_Slots[m_SelectedSlot] : null;

        public int MaxSlots => m_MaxSlots;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;

            // Initialize fixed slots array
            m_Slots = new KeyData[m_MaxSlots];
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

        #region Input Handling

        private void HandleHotbarInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
        }

        private void HandleDropInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                DropSelectedKey();
            }
        }

        #endregion

        #region Slot Management

        /// <summary>
        /// Selects a hotbar slot.
        /// </summary>
        public void SelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= m_MaxSlots) return;

            m_SelectedSlot = slotIndex;
            OnSlotSelected?.Invoke(slotIndex);
        }

        /// <summary>
        /// Drops the currently selected key. Slot becomes empty.
        /// </summary>
        public void DropSelectedKey()
        {
            KeyData keyToDrop = SelectedKey;
            if (keyToDrop == null)
            {
                return;
            }

            Vector3 dropPosition = CalculateDropPosition();

            // Clear the slot (don't shift, just set to null)
            m_Slots[m_SelectedSlot] = null;

            // Spawn the key pickup prefab
            SpawnDroppedKey(keyToDrop, dropPosition);

            OnKeyRemoved?.Invoke(keyToDrop);
            OnKeyDropped?.Invoke(keyToDrop, dropPosition);
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Adds a key to the first available slot.
        /// </summary>
        public void AddKey(KeyData key)
        {
            if (key == null)
            {
                return;
            }

            // Find first empty slot
            for (int i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i] == null)
                {
                    m_Slots[i] = key;
                    OnKeyAdded?.Invoke(key);
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }

        }

        /// <summary>
        /// Checks if the inventory contains a specific key.
        /// </summary>
        public bool HasKey(KeyData key)
        {
            if (key == null) return false;

            for (int i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i] != null && m_Slots[i].KeyId == key.KeyId)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a specific key from inventory.
        /// </summary>
        public bool RemoveKey(KeyData key)
        {
            if (key == null) return false;

            for (int i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i] != null && m_Slots[i].KeyId == key.KeyId)
                {
                    m_Slots[i] = null;
                    OnKeyRemoved?.Invoke(key);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the key at a specific slot (null if empty).
        /// </summary>
        public KeyData GetKeyAtSlot(int slotIndex)
        {
            if (m_Slots == null || slotIndex < 0 || slotIndex >= m_Slots.Length) return null;
            return m_Slots[slotIndex];
        }

        /// <summary>
        /// Clears all keys from the inventory.
        /// </summary>
        public void ClearInventory()
        {
            for (int i = 0; i < m_Slots.Length; i++)
            {
                m_Slots[i] = null;
            }
            OnInventoryChanged?.Invoke();
        }

        #endregion

        #region Drop Position

        private Vector3 CalculateDropPosition()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return GetDropPositionAtFeet();
            }

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, m_DropDistance))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) > 0.7f)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, hit.point);
                    if (distanceToPlayer < m_DropDistance * 1.5f)
                    {
                        return hit.point + Vector3.up * 0.2f;
                    }
                }
                return GetDropPositionAtFeet();
            }

            Vector3 dropPoint = cam.transform.position + cam.transform.forward * m_DropDistance;
            Ray downRay = new Ray(dropPoint + Vector3.up * 2f, Vector3.down);

            if (Physics.Raycast(downRay, out RaycastHit groundHit, 10f))
            {
                if (Vector3.Dot(groundHit.normal, Vector3.up) > 0.7f)
                {
                    Vector3 toDropPoint = groundHit.point - transform.position;
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, toDropPoint.normalized, toDropPoint.magnitude))
                    {
                        return groundHit.point + Vector3.up * 0.2f;
                    }
                }
            }

            return GetDropPositionAtFeet();
        }

        private Vector3 GetDropPositionAtFeet()
        {
            Ray downRay = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
            if (Physics.Raycast(downRay, out RaycastHit hit, 5f))
            {
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                return hit.point + Vector3.up * 0.2f + forward * 0.5f;
            }
            return transform.position + transform.forward * 0.5f;
        }

        #endregion

        #region Drop Spawning

        private void SpawnDroppedKey(KeyData keyData, Vector3 position)
        {
            if (m_KeyPickupPrefab == null)
            {
                return;
            }

            GameObject droppedKey = Instantiate(m_KeyPickupPrefab, position, Quaternion.Euler(270f, 0f, 0f));

            var keyPickup = droppedKey.GetComponent<Interactables.KeyPickup>();
            if (keyPickup != null)
            {
                keyPickup.SetKeyData(keyData);
                keyPickup.ApplyKeyColor();
            }

            var renderer = droppedKey.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(renderer.material);
                mat.color = keyData.KeyColor;
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", keyData.KeyColor);
                }
                renderer.material = mat;
            }

            var rb = droppedKey.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * 2f + Camera.main.transform.forward * 3f, ForceMode.Impulse);
            }
        }

        #endregion
    }
}
