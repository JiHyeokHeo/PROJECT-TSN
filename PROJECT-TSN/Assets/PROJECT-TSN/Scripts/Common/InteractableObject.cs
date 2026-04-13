using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TST
{
    /// <summary>
    /// 방에 배치된 상호작용 가능한 오브젝트의 기반 클래스.
    /// Collider2D 기반 마우스 입력을 사용하며 아웃라인 강조를 지원합니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        /// <summary>IsInteractable == true 상태에서 클릭 시 OnInteract() 직전에 발행됩니다.</summary>
        public event Action OnInteracted;

        // ── 아웃라인 설정 ──────────────────────────────────────────────
        private static readonly int OutlineEnabledID = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int OutlineColorID   = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthID   = Shader.PropertyToID("_OutlineWidth");

        [Header("Outline")]
        [SerializeField] private Color outlineColor = new Color(1f, 0.9f, 0.4f, 1f);
        [SerializeField] private float outlineWidth = 3f;

        // ── 상태 ──────────────────────────────────────────────────────
        [SerializeField] private bool isInteractable = true;

        public bool IsInteractable
        {
            get => isInteractable;
            set
            {
                isInteractable = value;
                // 비활성화 시 아웃라인 강제 해제
                if (!isInteractable)
                    SetOutline(false);
            }
        }

        // ── 렌더러 캐시 ──────────────────────────────────────────────
        private SpriteRenderer _spriteRenderer;
        private Renderer        _renderer;
        private MaterialPropertyBlock _mpb;

        // ─────────────────────────────────────────────────────────────

        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                _renderer = GetComponent<Renderer>();

            _mpb = new MaterialPropertyBlock();
        }

        // ── 마우스 이벤트 ────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;
            SetOutline(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetOutline(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;
            OnInteracted?.Invoke();
            OnInteract();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInteractable) return;
            OnInteracted?.Invoke();
            OnInteract();
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (!isInteractable) return;
            OnInteracted?.Invoke();
            OnInteract();
        }

        //private void OnMouseEnter()
        //{
        //    if (!isInteractable) return;
        //    SetOutline(true);
        //}

        //private void OnMouseExit()
        //{
        //    SetOutline(false);
        //}

        //private void OnMouseDown()
        //{
        //    if (!isInteractable) return;
        //    OnInteracted?.Invoke();
        //    OnInteract();
        //}

        // ── 아웃라인 제어 ────────────────────────────────────────────

        private void SetOutline(bool enabled)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(OutlineEnabledID, enabled ? 1f : 0f);
                if (enabled)
                {
                    _mpb.SetColor(OutlineColorID, outlineColor);
                    _mpb.SetFloat(OutlineWidthID, outlineWidth);
                }
                _spriteRenderer.SetPropertyBlock(_mpb);
            }
            else if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(OutlineEnabledID, enabled ? 1f : 0f);
                if (enabled)
                {
                    _mpb.SetColor(OutlineColorID, outlineColor);
                    _mpb.SetFloat(OutlineWidthID, outlineWidth);
                }
                _renderer.SetPropertyBlock(_mpb);
            }
        }

        // ── 하위 클래스 구현 요구 ────────────────────────────────────

        /// <summary>
        /// IsInteractable == true 상태에서 마우스 클릭 시 호출됩니다.
        /// </summary>
        protected abstract void OnInteract();

    }
}
