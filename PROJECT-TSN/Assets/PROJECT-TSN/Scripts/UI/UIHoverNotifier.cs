using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// Forwards UI pointer enter/exit events to HUD_Parameters so each
    /// parameter panel can be toggled by hover without polling mouse position.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverNotifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private HUD_Parameters owner;
        private ParameterType parameterType;

        public void Configure(HUD_Parameters hudOwner, ParameterType type)
        {
            owner = hudOwner;
            parameterType = type;
            EnsureRaycastTarget();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            owner?.SetHoverState(parameterType, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            owner?.SetHoverState(parameterType, false);
        }

        private void EnsureRaycastTarget()
        {
            // If there is no Graphic on hover area, add a transparent Image
            // so pointer events can be received reliably.
            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = true;
                return;
            }

            Image image = gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
        }
    }
}
