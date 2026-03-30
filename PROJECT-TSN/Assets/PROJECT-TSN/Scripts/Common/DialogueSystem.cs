using System;
using System.Collections;
using UnityEngine;

namespace TST
{
    /// <summary>
    /// StoryData를 받아 MainLayoutController의 DialogueBox를 구동하는 타이핑 애니메이션 시스템.
    ///
    /// 사용법:
    ///   DialogueSystem.Singleton.PlayStory(data, OnComplete);
    ///   DialogueSystem.Singleton.Skip();
    ///
    /// 생명주기:
    ///   - PlayStory 호출 → 줄별로 타이핑 코루틴 실행
    ///   - 타이핑 중 Skip() → 즉시 전체 텍스트 표시
    ///   - 대기 중 Skip() → 다음 줄 진행
    ///   - 마지막 줄 대기 중 Skip() → onComplete 호출
    ///
    /// Inspector:
    ///   typingSpeed — 글자 하나를 표시하는 데 걸리는 시간(초). 기본값 0.03f.
    /// </summary>
    public class DialogueSystem : SingletonBase<DialogueSystem>
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private float typingSpeed = 0.03f;

        // ── 상태 ─────────────────────────────────────────────────────
        public bool IsPlaying { get; private set; }

        private StoryData       _currentData;
        private int             _currentLineIndex;
        private Action          _onComplete;
        private bool            _isTyping;      // 현재 타이핑 진행 중
        private bool            _waitingForNext;// 타이핑 완료 후 입력 대기 중
        private Coroutine       _typingCoroutine;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>새 스토리를 재생합니다. 이미 재생 중이면 강제 중단 후 새로 시작합니다.</summary>
        public void PlayStory(StoryData data, Action onComplete)
        {
            if (data == null || data.lines == null || data.lines.Length == 0)
            {
                Debug.LogWarning("[DialogueSystem] PlayStory: StoryData가 null이거나 줄이 없습니다.");
                onComplete?.Invoke();
                return;
            }

            StopCurrentStory();

            _currentData      = data;
            _currentLineIndex = 0;
            _onComplete       = onComplete;
            IsPlaying         = true;

            ShowLine(_currentLineIndex);
        }

        /// <summary>
        /// 타이핑 중: 즉시 전체 텍스트 표시.
        /// 대기 중: 다음 줄로 진행.
        /// 마지막 줄 대기 중: onComplete 호출.
        /// </summary>
        public void Skip()
        {
            if (!IsPlaying) return;

            if (_isTyping)
            {
                // 타이핑 코루틴 중단 → 현재 줄 전체를 즉시 표시
                StopTypingCoroutine();
                ShowLineInstant(_currentLineIndex);
            }
            else if (_waitingForNext)
            {
                AdvanceLine();
            }
        }

        // ── 내부 ─────────────────────────────────────────────────────

        private void ShowLine(int index)
        {
            StoryData.DialogueLine line = _currentData.lines[index];

            // 배경 CG 갱신
            if (line.backgroundCg != null)
            {
                var layout = GetMainLayout();
                layout?.SetLeftContent(line.backgroundCg);
            }

            // 초상화 갱신
            if (line.portrait != null)
            {
                var layout = GetMainLayout();
                layout?.SetRightContent(line.portrait);
            }

            // 화자 이름 먼저 표시 (텍스트는 타이핑으로)
            GetMainLayout()?.ShowDialogue(line.speakerName, string.Empty);

            StopTypingCoroutine();
            _typingCoroutine = StartCoroutine(TypeLine(line));
        }

        private void ShowLineInstant(int index)
        {
            StoryData.DialogueLine line = _currentData.lines[index];
            GetMainLayout()?.ShowDialogue(line.speakerName, line.text);
            _isTyping        = false;
            _waitingForNext  = true;
        }

        private IEnumerator TypeLine(StoryData.DialogueLine line)
        {
            _isTyping       = true;
            _waitingForNext = false;

            string fullText   = line.text ?? string.Empty;
            string displayed  = string.Empty;

            foreach (char c in fullText)
            {
                displayed += c;
                GetMainLayout()?.ShowDialogue(line.speakerName, displayed);
                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping       = false;
            _waitingForNext = true;
            _typingCoroutine = null;
        }

        private void AdvanceLine()
        {
            _waitingForNext = false;
            _currentLineIndex++;

            if (_currentLineIndex < _currentData.lines.Length)
            {
                ShowLine(_currentLineIndex);
            }
            else
            {
                // 모든 줄 완료
                FinishStory();
            }
        }

        private void FinishStory()
        {
            IsPlaying       = false;
            _isTyping       = false;
            _waitingForNext = false;

            GetMainLayout()?.HideDialogue();

            Action callback = _onComplete;
            _onComplete     = null;
            _currentData    = null;

            callback?.Invoke();
        }

        private void StopCurrentStory()
        {
            StopTypingCoroutine();
            IsPlaying       = false;
            _isTyping       = false;
            _waitingForNext = false;
            _onComplete     = null;
            _currentData    = null;
        }

        private void StopTypingCoroutine()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
            _isTyping = false;
        }

        private MainLayoutController GetMainLayout()
        {
            var layout = UIManager.Singleton.GetUI<UIBase>(UIList.MainLayout);
            if (layout == null) return null;
            return layout.GetComponent<MainLayoutController>();
        }
    }
}
