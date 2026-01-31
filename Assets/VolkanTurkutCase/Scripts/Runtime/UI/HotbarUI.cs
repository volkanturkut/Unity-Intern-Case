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

        [Header("Slot Settings")]
        [SerializeField] private HotbarSlot[] m_Slots;
        [SerializeField] private Color m_SelectedColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color m_NormalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color m_EmptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_PlayerInventory == null)
            {
                m_PlayerInventory = PlayerInventory.Instance;
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

                m_Slots[i].SetSlot(i, key, isSelected, m_SelectedColor, m_NormalColor, m_EmptySlotColor);
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

    /// <summary>
    /// Individual hotbar slot UI element.
    /// </summary>
    [System.Serializable]
    public class HotbarSlot : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Image m_Background;
        [SerializeField] private Image m_Icon;
        [SerializeField] private TextMeshProUGUI m_KeyNumberText;
        [SerializeField] private GameObject m_SelectionBorder;

        #endregion

        #region Methods

        /// <summary>
        /// Updates the slot display.
        /// </summary>
        public void SetSlot(int slotIndex, KeyData key, bool isSelected, 
            Color selectedColor, Color normalColor, Color emptyColor)
        {
            // Set key number (1-4)
            if (m_KeyNumberText != null)
            {
                m_KeyNumberText.text = (slotIndex + 1).ToString();
            }

            // Set selection border
            if (m_SelectionBorder != null)
            {
                m_SelectionBorder.SetActive(isSelected);
            }

            // Set icon and background
            if (key != null)
            {
                // Has key
                if (m_Icon != null)
                {
                    m_Icon.gameObject.SetActive(true);
                    if (key.Icon != null)
                    {
                        m_Icon.sprite = key.Icon;
                        m_Icon.color = Color.white;
                    }
                    else
                    {
                        m_Icon.sprite = null;
                        m_Icon.color = key.KeyColor;
                    }
                }

                if (m_Background != null)
                {
                    m_Background.color = isSelected ? selectedColor : normalColor;
                }
            }
            else
            {
                // Empty slot
                if (m_Icon != null)
                {
                    m_Icon.gameObject.SetActive(false);
                }

                if (m_Background != null)
                {
                    m_Background.color = isSelected ? selectedColor : emptyColor;
                }
            }
        }

        #endregion
    }
}
