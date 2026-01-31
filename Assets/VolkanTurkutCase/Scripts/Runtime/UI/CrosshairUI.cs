using UnityEngine;
using UnityEngine.UI;
using VolkanTurkutCase.Runtime.Core;
using VolkanTurkutCase.Runtime.Player;

namespace VolkanTurkutCase.Runtime.UI
{
    /// <summary>
    /// Simple crosshair/cursor UI that changes when looking at interactables.
    /// </summary>
    public class CrosshairUI : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [SerializeField] private InteractionDetector m_InteractionDetector;
        [SerializeField] private Image m_CrosshairImage;

        [Header("Crosshair Sprites")]
        [SerializeField] private Sprite m_DefaultSprite;
        [SerializeField] private Sprite m_InteractableSprite;

        [Header("Colors")]
        [SerializeField] private Color m_DefaultColor = Color.white;
        [SerializeField] private Color m_InteractableColor = Color.green;
        [SerializeField] private Color m_CannotInteractColor = Color.red;

        [Header("Size")]
        [SerializeField] private float m_DefaultSize = 20f;
        [SerializeField] private float m_InteractableSize = 30f;

        [Header("Settings")]
        [SerializeField] private bool m_ChangeCrosshairOnInteractable = true;

        private RectTransform m_RectTransform;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (m_InteractionDetector == null)
            {
                m_InteractionDetector = FindFirstObjectByType<InteractionDetector>();
            }

            if (m_CrosshairImage != null)
            {
                m_RectTransform = m_CrosshairImage.GetComponent<RectTransform>();
            }
        }

        private void OnEnable()
        {
            if (m_InteractionDetector != null)
            {
                m_InteractionDetector.OnInteractableDetected += HandleInteractableDetected;
                m_InteractionDetector.OnInteractableLost += HandleInteractableLost;
            }
        }

        private void OnDisable()
        {
            if (m_InteractionDetector != null)
            {
                m_InteractionDetector.OnInteractableDetected -= HandleInteractableDetected;
                m_InteractionDetector.OnInteractableLost -= HandleInteractableLost;
            }
        }

        private void Update()
        {
            UpdateCrosshairState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates crosshair based on current target state.
        /// </summary>
        private void UpdateCrosshairState()
        {
            if (m_CrosshairImage == null || m_InteractionDetector == null)
            {
                return;
            }

            // If crosshair changes are disabled, keep default state
            if (!m_ChangeCrosshairOnInteractable)
            {
                return;
            }

            var target = m_InteractionDetector.CurrentTarget;

            if (target != null)
            {
                // Looking at interactable
                bool canInteract = target.CanInteract();
                m_CrosshairImage.color = canInteract ? m_InteractableColor : m_CannotInteractColor;

                if (m_InteractableSprite != null)
                {
                    m_CrosshairImage.sprite = m_InteractableSprite;
                }

                SetSize(m_InteractableSize);
            }
            else
            {
                // Default state
                m_CrosshairImage.color = m_DefaultColor;

                if (m_DefaultSprite != null)
                {
                    m_CrosshairImage.sprite = m_DefaultSprite;
                }

                SetSize(m_DefaultSize);
            }
        }

        /// <summary>
        /// Called when an interactable is detected.
        /// </summary>
        private void HandleInteractableDetected(IInteractable interactable)
        {
            UpdateCrosshairState();
        }

        /// <summary>
        /// Called when interactable is lost.
        /// </summary>
        private void HandleInteractableLost()
        {
            UpdateCrosshairState();
        }

        /// <summary>
        /// Sets the crosshair size.
        /// </summary>
        private void SetSize(float size)
        {
            if (m_RectTransform != null)
            {
                m_RectTransform.sizeDelta = new Vector2(size, size);
            }
        }

        #endregion
    }
}
