using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VolkanTurkutCase.Runtime.Core;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// Individual hotbar slot UI element.
    /// </summary>
    public class HotbarSlot : MonoBehaviour
    {
        #region Fields

        [Header("UI References")]
        [SerializeField] private Image m_Background;
        [SerializeField] private Image m_Icon;
        [SerializeField] private TextMeshProUGUI m_KeyNumberText;
        [SerializeField] private GameObject m_SelectionBorder;

        [Header("Colors")]
        [SerializeField] private Color m_SelectedColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color m_NormalColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color m_EmptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private int m_SlotIndex;
        private KeyData m_CurrentKey;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the slot index.
        /// </summary>
        public int SlotIndex => m_SlotIndex;

        /// <summary>
        /// Gets the current key in this slot.
        /// </summary>
        public KeyData CurrentKey => m_CurrentKey;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the slot with an index.
        /// </summary>
        public void Initialize(int slotIndex)
        {
            m_SlotIndex = slotIndex;
            
            if (m_KeyNumberText != null)
            {
                m_KeyNumberText.text = (slotIndex + 1).ToString();
            }
        }

        /// <summary>
        /// Updates the slot display.
        /// </summary>
        public void SetSlotData(KeyData key, bool isSelected)
        {
            m_CurrentKey = key;

            // Set selection border
            if (m_SelectionBorder != null)
            {
                m_SelectionBorder.SetActive(isSelected);
            }

            // Set icon and background
            if (key != null)
            {
                // Has key - show icon
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
                        // No icon, show colored square
                        m_Icon.sprite = null;
                        m_Icon.color = key.KeyColor;
                    }
                }

                if (m_Background != null)
                {
                    m_Background.color = isSelected ? m_SelectedColor : m_NormalColor;
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
                    m_Background.color = isSelected ? m_SelectedColor : m_EmptySlotColor;
                }
            }
        }

        #endregion
    }
}
