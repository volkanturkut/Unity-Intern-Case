using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// Always-visible hotbar UI showing inventory slots with key selection.
    /// </summary>
    public class HotbarUI : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [SerializeField] private PlayerInventory m_PlayerInventory;
        [SerializeField] private HotbarSlot[] m_Slots;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_PlayerInventory == null)
            {
                m_PlayerInventory = PlayerInventory.Instance;
            }

            // Initialize slot indices
            for (int i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i] != null)
                {
                    m_Slots[i].Initialize(i);
                }
            }
        }

        private void OnEnable()
        {
            if (m_PlayerInventory != null)
            {
                m_PlayerInventory.OnInventoryChanged += RefreshSlots;
                m_PlayerInventory.OnSlotSelected += HandleSlotSelected;
            }

            RefreshSlots();
        }

        private void OnDisable()
        {
            if (m_PlayerInventory != null)
            {
                m_PlayerInventory.OnInventoryChanged -= RefreshSlots;
                m_PlayerInventory.OnSlotSelected -= HandleSlotSelected;
            }
        }

        private void Start()
        {
            RefreshSlots();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes all hotbar slots.
        /// </summary>
        public void RefreshSlots()
        {
            if (m_Slots == null || m_PlayerInventory == null)
            {
                return;
            }

            for (int i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i] == null) continue;

                KeyData key = m_PlayerInventory.GetKeyAtSlot(i);
                bool isSelected = i == m_PlayerInventory.SelectedSlot;

                m_Slots[i].SetSlotData(key, isSelected);
            }
        }

        /// <summary>
        /// Handles slot selection change.
        /// </summary>
        private void HandleSlotSelected(int slotIndex)
        {
            RefreshSlots();
        }

        #endregion
    }
}
