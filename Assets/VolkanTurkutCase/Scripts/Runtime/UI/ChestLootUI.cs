using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Interactables;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// UI panel that displays chest contents when opened.
    /// Shows item icon, name, description, and collect button.
    /// </summary>
    public class ChestLootUI : MonoBehaviour
    {
        #region Fields

        [Header("Panel References")]
        [SerializeField] private GameObject m_LootPanel;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Header("Item Display")]
        [SerializeField] private Image m_ItemIcon;
        [SerializeField] private TextMeshProUGUI m_ItemName;
        [SerializeField] private TextMeshProUGUI m_ItemDescription;

        [Header("Navigation")]
        [SerializeField] private Button m_CollectButton;
        [SerializeField] private Button m_NextButton;
        [SerializeField] private Button m_PreviousButton;
        [SerializeField] private TextMeshProUGUI m_ItemCountText;

        [Header("Settings")]
        [SerializeField] private bool m_PauseGameWhenOpen = true;
        [SerializeField] private bool m_UnlockCursorWhenOpen = true;
        [SerializeField] private bool m_DisablePlayerInput = true;

        private List<KeyData> m_CurrentItems = new List<KeyData>();
        private int m_CurrentIndex;
        private Chest m_CurrentChest;
        private bool m_IsOpen;
        private MonoBehaviour m_PlayerController;

        #endregion

        #region Properties

        public static ChestLootUI Instance { get; private set; }

        public bool IsOpen => m_IsOpen;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Instance = this;
            Debug.Log("[ChestLootUI] Instance set in Awake");

            if (m_LootPanel != null)
            {
                m_LootPanel.SetActive(false);
            }

            SetupButtons();
        }

        private void OnEnable()
        {
            // Ensure instance is set even if Awake didn't run
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("[ChestLootUI] Instance set in OnEnable");
            }
            SetupButtons();
        }

        private void SetupButtons()
        {
            // Setup button listeners
            if (m_CollectButton != null)
            {
                m_CollectButton.onClick.RemoveAllListeners();
                m_CollectButton.onClick.AddListener(CollectCurrentItem);
            }

            if (m_NextButton != null)
            {
                m_NextButton.onClick.RemoveAllListeners();
                m_NextButton.onClick.AddListener(ShowNextItem);
            }

            if (m_PreviousButton != null)
            {
                m_PreviousButton.onClick.RemoveAllListeners();
                m_PreviousButton.onClick.AddListener(ShowPreviousItem);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (m_IsOpen)
            {
                // Close on Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Close();
                }

                // Navigate with arrow keys
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    ShowNextItem();
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    ShowPreviousItem();
                }

                // Collect with E or Enter
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
                {
                    CollectCurrentItem();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens the loot panel for a chest with given items.
        /// </summary>
        public void Open(Chest chest, KeyData[] items)
        {
            if (items == null || items.Length == 0)
            {
                return;
            }

            m_CurrentChest = chest;
            m_CurrentItems.Clear();
            m_CurrentItems.AddRange(items);
            m_CurrentIndex = 0;

            if (m_LootPanel != null)
            {
                m_LootPanel.SetActive(true);
            }

            m_IsOpen = true;

            // Pause game and unlock cursor
            if (m_PauseGameWhenOpen)
            {
                Time.timeScale = 0f;
            }

            if (m_UnlockCursorWhenOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Disable player controller to stop camera movement
            if (m_DisablePlayerInput)
            {
                DisablePlayerController();
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Closes the loot panel.
        /// </summary>
        public void Close()
        {
            if (m_LootPanel != null)
            {
                m_LootPanel.SetActive(false);
            }

            m_IsOpen = false;
            m_CurrentChest = null;
            m_CurrentItems.Clear();

            // Resume game and lock cursor
            if (m_PauseGameWhenOpen)
            {
                Time.timeScale = 1f;
            }

            if (m_UnlockCursorWhenOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Re-enable player controller
            if (m_DisablePlayerInput)
            {
                EnablePlayerController();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the display with current item.
        /// </summary>
        private void UpdateDisplay()
        {
            if (m_CurrentItems.Count == 0)
            {
                Close();
                return;
            }

            // Clamp index
            m_CurrentIndex = Mathf.Clamp(m_CurrentIndex, 0, m_CurrentItems.Count - 1);

            KeyData currentItem = m_CurrentItems[m_CurrentIndex];

            // Update icon
            if (m_ItemIcon != null)
            {
                if (currentItem.Icon != null)
                {
                    m_ItemIcon.sprite = currentItem.Icon;
                    m_ItemIcon.color = Color.white;
                }
                else
                {
                    // Use solid color if no icon
                    m_ItemIcon.sprite = null;
                    m_ItemIcon.color = currentItem.KeyColor;
                }
            }

            // Update name
            if (m_ItemName != null)
            {
                m_ItemName.text = currentItem.ItemName;
            }

            // Update description
            if (m_ItemDescription != null)
            {
                m_ItemDescription.text = currentItem.ItemDescription;
            }

            // Update item count
            if (m_ItemCountText != null)
            {
                m_ItemCountText.text = $"{m_CurrentIndex + 1} / {m_CurrentItems.Count}";
            }

            // Update navigation buttons
            if (m_PreviousButton != null)
            {
                m_PreviousButton.interactable = m_CurrentIndex > 0;
            }

            if (m_NextButton != null)
            {
                m_NextButton.interactable = m_CurrentIndex < m_CurrentItems.Count - 1;
            }
        }

        /// <summary>
        /// Shows the next item.
        /// </summary>
        private void ShowNextItem()
        {
            if (m_CurrentIndex < m_CurrentItems.Count - 1)
            {
                m_CurrentIndex++;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Shows the previous item.
        /// </summary>
        private void ShowPreviousItem()
        {
            if (m_CurrentIndex > 0)
            {
                m_CurrentIndex--;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Collects the current item and adds to inventory.
        /// </summary>
        private void CollectCurrentItem()
        {
            if (m_CurrentItems.Count == 0)
            {
                return;
            }

            KeyData item = m_CurrentItems[m_CurrentIndex];

            // Add to inventory
            var inventory = Player.PlayerInventory.Instance;
            if (inventory != null)
            {
                inventory.AddKey(item);
                Debug.Log($"[ChestLootUI] Collected: {item.ItemName}");
            }

            // Remove from list
            m_CurrentItems.RemoveAt(m_CurrentIndex);

            // Adjust index
            if (m_CurrentIndex >= m_CurrentItems.Count && m_CurrentItems.Count > 0)
            {
                m_CurrentIndex = m_CurrentItems.Count - 1;
            }

            // Update display or close if empty
            if (m_CurrentItems.Count > 0)
            {
                UpdateDisplay();
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Finds and disables the player controller.
        /// </summary>
        private void DisablePlayerController()
        {
            // Find FirstPersonController by type name to avoid direct reference
            if (m_PlayerController == null)
            {
                // Try to find FirstPersonController
                var controllers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var controller in controllers)
                {
                    if (controller.GetType().Name == "FirstPersonController")
                    {
                        m_PlayerController = controller;
                        break;
                    }
                }
            }

            if (m_PlayerController != null)
            {
                m_PlayerController.enabled = false;
                Debug.Log("[ChestLootUI] Player controller disabled");
            }
        }

        /// <summary>
        /// Re-enables the player controller.
        /// </summary>
        private void EnablePlayerController()
        {
            if (m_PlayerController != null)
            {
                m_PlayerController.enabled = true;
                Debug.Log("[ChestLootUI] Player controller enabled");
            }
        }

        #endregion
    }
}
