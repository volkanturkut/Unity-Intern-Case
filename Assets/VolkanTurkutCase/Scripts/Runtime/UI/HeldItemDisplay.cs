using UnityEngine;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// Displays the currently selected item in the player's hand.
    /// </summary>
    public class HeldItemDisplay : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [SerializeField] private PlayerInventory m_PlayerInventory;
        [SerializeField] private Transform m_HandPosition;

        [Header("Visual Settings")]
        [SerializeField] private GameObject m_KeyVisualPrefab;
        [SerializeField] private Vector3 m_HeldItemScale = new Vector3(0.3f, 0.3f, 0.3f);
        [SerializeField] private Vector3 m_HeldItemRotation = new Vector3(0f, 45f, 0f);

        private GameObject m_CurrentVisual;
        private MeshRenderer m_CurrentRenderer;
        private KeyData m_DisplayedKey;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_PlayerInventory == null)
            {
                m_PlayerInventory = PlayerInventory.Instance;
            }

            if (m_HandPosition == null)
            {
                // Default to camera child if not set
                Camera cam = Camera.main;
                if (cam != null)
                {
                    m_HandPosition = cam.transform;
                }
            }
        }

        private void OnEnable()
        {
            if (m_PlayerInventory != null)
            {
                m_PlayerInventory.OnInventoryChanged += UpdateHeldItem;
                m_PlayerInventory.OnSlotSelected += HandleSlotSelected;
            }

            UpdateHeldItem();
        }

        private void OnDisable()
        {
            if (m_PlayerInventory != null)
            {
                m_PlayerInventory.OnInventoryChanged -= UpdateHeldItem;
                m_PlayerInventory.OnSlotSelected -= HandleSlotSelected;
            }
        }

        private void Start()
        {
            UpdateHeldItem();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles slot selection change.
        /// </summary>
        private void HandleSlotSelected(int slotIndex)
        {
            UpdateHeldItem();
        }

        /// <summary>
        /// Updates the held item visual based on selected slot.
        /// </summary>
        public void UpdateHeldItem()
        {
            if (m_PlayerInventory == null)
            {
                return;
            }

            KeyData selectedKey = m_PlayerInventory.SelectedKey;

            // Same key, no update needed
            if (selectedKey == m_DisplayedKey)
            {
                return;
            }

            m_DisplayedKey = selectedKey;

            // Clear previous visual
            if (m_CurrentVisual != null)
            {
                Destroy(m_CurrentVisual);
                m_CurrentVisual = null;
                m_CurrentRenderer = null;
            }

            // No key selected
            if (selectedKey == null)
            {
                return;
            }

            // Create new visual
            CreateKeyVisual(selectedKey);
        }

        /// <summary>
        /// Creates a visual representation of the key.
        /// </summary>
        private void CreateKeyVisual(KeyData keyData)
        {
            if (m_HandPosition == null)
            {
                return;
            }

            if (m_KeyVisualPrefab != null)
            {
                // Use prefab but remove any interactable components
                m_CurrentVisual = Instantiate(m_KeyVisualPrefab, m_HandPosition);

                // Remove KeyPickup and other components that shouldn't be on held item
                var keyPickup = m_CurrentVisual.GetComponent<Interactables.KeyPickup>();
                if (keyPickup != null)
                {
                    Destroy(keyPickup);
                }

                // Remove colliders
                var colliders = m_CurrentVisual.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    Destroy(col);
                }

                // Remove rigidbody
                var rb = m_CurrentVisual.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Destroy(rb);
                }
            }
            else
            {
                // Create simple cube as placeholder
                m_CurrentVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                m_CurrentVisual.transform.SetParent(m_HandPosition);

                // Remove collider so it doesn't interfere
                var collider = m_CurrentVisual.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            // Position in hand
            m_CurrentVisual.transform.localPosition = new Vector3(0.5f, -0.3f, 0.8f);
            m_CurrentVisual.transform.localRotation = Quaternion.Euler(m_HeldItemRotation);
            m_CurrentVisual.transform.localScale = m_HeldItemScale;

            // Apply key color
            m_CurrentRenderer = m_CurrentVisual.GetComponentInChildren<MeshRenderer>();
            if (m_CurrentRenderer != null)
            {
                // Use the existing material and change its color
                // This works with both Standard and URP/Lit shaders
                Material mat = new Material(m_CurrentRenderer.sharedMaterial);
                mat.color = keyData.KeyColor;

                // Also set _BaseColor for URP compatibility
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", keyData.KeyColor);
                }

                m_CurrentRenderer.material = mat;
            }

            m_CurrentVisual.name = $"HeldKey_{keyData.ItemName}";
        }

        #endregion
    }
}
