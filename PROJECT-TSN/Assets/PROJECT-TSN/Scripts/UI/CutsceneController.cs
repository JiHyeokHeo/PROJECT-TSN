using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 전체화면 컷신 패널.
    /// UIList: Panel_Cutscene
    /// Prefab 경로: Resources/UI/Prefabs/UI.Panel_Cutscene
    ///
    /// 흐름:
    ///   PlayCutscene(data, onComplete)
    ///     → 페이드인 → DialogueSystem.PlayStory 시작
    ///     → 마지막 줄 완료 → 페이드아웃 → onComplete 호출
    ///
    /// 건너뛰기 버튼:
    ///   타이핑 중 → 즉시 텍스트 완성
    ///   대기 중   → 다음 줄
    ///   마지막 줄 → onComplete로 즉시 이동
    ///
    /// Inspector 와이어링:
    ///   skipBtn   — Button (건너뛰기)
    ///   fadeGroup — CanvasGroup (전체 패널에 연결)
    ///   fadeDuration — 페이드 시간(초), 기본 0.5f
    /// </summary>
    public class CutsceneController : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private Button      skipBtn;
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float       fadeDuration = 0.5f;

        // ── 런타임 상태 ──────────────────────────────────────────────
        private Action    _onComplete;
        private Coroutine _fadeCoroutine;

        // ── Unity 생명주기 ───────────────────────────────────────────

        private void Awake()
        {
            if (skipBtn != null)
                skipBtn.onClick.AddListener(OnSkipClicked);
        }

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>컷신을 재생합니다. 완료 시 onComplete를 호출합니다.</summary>
        public void PlayCutscene(StoryData data, Action onComplete)
        {
            if (data == null)
            {
                Debug.LogWarning("[CutsceneController] PlayCutscene: StoryData가 null입니다.");
                onComplete?.Invoke();
                return;
            }

            _onComplete = onComplete;

            // 패널이 이미 활성화되어 있지 않으면 Show
            if (!gameObject.activeSelf)
                Show();

            StopFade();
            _fadeCoroutine = StartCoroutine(FadeInThenPlay(data));
        }

        // ── UIBase 오버라이드 ─────────────────────────────────────────

        public override void Show()
        {
            if (fadeGroup != null) fadeGroup.alpha = 0f;
            base.Show();
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private IEnumerator FadeInThenPlay(StoryData data)
        {
            yield return FadeTo(1f);
            DialogueSystem.Singleton.PlayStory(data, OnStoryComplete);
        }

        private void OnStoryComplete()
        {
            StopFade();
            _fadeCoroutine = StartCoroutine(FadeOutThenFinish());
        }

        private IEnumerator FadeOutThenFinish()
        {
            yield return FadeTo(0f);
            Hide();

            Action callback = _onComplete;
            _onComplete = null;
            callback?.Invoke();
        }

        private IEnumerator FadeTo(float targetAlpha)
        {
            if (fadeGroup == null)
            {
                yield break;
            }

            float startAlpha = fadeGroup.alpha;
            float elapsed    = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed          += Time.deltaTime;
                fadeGroup.alpha   = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            fadeGroup.alpha = targetAlpha;
        }

        private void StopFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        private void OnSkipClicked()
        {
            DialogueSystem.Singleton.Skip();
        }
    }
}
