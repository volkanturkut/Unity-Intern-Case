using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// UI component that displays collected keys in the inventory.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [SerializeField] private PlayerInventory m_PlayerInventory;
        [SerializeField] private Transform m_KeyContainer;
        [SerializeField] private GameObject m_KeyItemPrefab;
        [SerializeField] private GameObject m_InventoryPanel;

        [Header("Settings")]
        [SerializeField] private bool m_ShowOnKeyCollected = true;
        [SerializeField] private float m_AutoHideDelay = 3f;

        private float m_HideTimer;
        private bool m_IsVisible;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_PlayerInventory == null)
            {
                m_PlayerInventory = PlayerInventory.Instance;
            }

            if (m_InventoryPanel != null)
            {
                m_InventoryPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (m_PlayerInventory != null)
            {
                m_PlayerInventory.OnKeyAdded += HandleKeyAdded;
                m_PlayerInventory.OnKeyRemoved += HandleKeyRemoved;
                m_PlayerInventory.OnInventoryChanged += RefreshDisplay;
            }
        }

        private void OnDisable()
        {
            if (m_PlayerInventory != null)
            {
                m_PlayerInventory.OnKeyAdded -= HandleKeyAdded;
                m_PlayerInventory.OnKeyRemoved -= HandleKeyRemoved;
                m_PlayerInventory.OnInventoryChanged -= RefreshDisplay;
            }
        }

        private void Update()
        {
            if (m_IsVisible && m_HideTimer > 0)
            {
                m_HideTimer -= Time.deltaTime;
                if (m_HideTimer <= 0)
                {
                    Hide();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles when a key is added to the inventory.
        /// </summary>
        private void HandleKeyAdded(KeyData key)
        {
            if (m_ShowOnKeyCollected)
            {
                Show();
                m_HideTimer = m_AutoHideDelay;
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Handles when a key is removed from the inventory.
        /// </summary>
        private void HandleKeyRemoved(KeyData key)
        {
            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes the inventory display.
        /// </summary>
        public void RefreshDisplay()
        {
            if (m_KeyContainer == null || m_KeyItemPrefab == null)
            {
                return;
            }

            // Clear existing items
            foreach (Transform child in m_KeyContainer)
            {
                Destroy(child.gameObject);
            }

            // Create items for each key
            if (m_PlayerInventory == null)
            {
                return;
            }

            foreach (var key in m_PlayerInventory.CollectedKeys)
            {
                CreateKeyItem(key);
            }
        }

        /// <summary>
        /// Creates a UI element for a key.
        /// </summary>
        private void CreateKeyItem(KeyData key)
        {
            if (key == null || m_KeyItemPrefab == null || m_KeyContainer == null)
            {
                return;
            }

            var item = Instantiate(m_KeyItemPrefab, m_KeyContainer);
            
            // Try to set the icon
            var icon = item.GetComponentInChildren<Image>();
            if (icon != null && key.Icon != null)
            {
                icon.sprite = key.Icon;
                icon.color = key.KeyColor;
            }
            else if (icon != null)
            {
                icon.color = key.KeyColor;
            }

            // Try to set the name
            var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = key.ItemName;
            }
        }

        /// <summary>
        /// Shows the inventory panel.
        /// </summary>
        public void Show()
        {
            m_IsVisible = true;
            if (m_InventoryPanel != null)
            {
                m_InventoryPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the inventory panel.
        /// </summary>
        public void Hide()
        {
            m_IsVisible = false;
            if (m_InventoryPanel != null)
            {
                m_InventoryPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Toggles the inventory panel visibility.
        /// </summary>
        public void Toggle()
        {
            if (m_IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
                m_HideTimer = 0f; // Don't auto-hide when manually toggled
            }
        }

        #endregion
    }
}
