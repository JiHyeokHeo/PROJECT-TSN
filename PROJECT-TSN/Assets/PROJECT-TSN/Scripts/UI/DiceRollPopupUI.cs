using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 주사위 굴리기 팝업.
    /// UIList: Popup_DiceRoll
    /// Prefab 경로: Resources/UI/Prefabs/UI.Popup_DiceRoll
    ///
    /// 흐름:
    ///   Show(diceMax, rewindLimit, onResult)
    ///     → rollBtn 클릭 → Random.Range(1, diceMax+1) → resultLabel 표시
    ///     → 0.5 초 뒤 확정 (autoConfirmDelay)
    ///     → 되감기 횟수 남아 있으면 rewindBtn 활성 → 클릭 시 재굴림(횟수 차감)
    ///     → 확정 → onResult(result) → Hide()
    ///
    /// Inspector 와이어링:
    ///   rollBtn         — 주사위 굴리기 버튼
    ///   rewindBtn       — 되감기 버튼
    ///   resultLabel     — 결과 숫자 TMP 텍스트
    ///   rewindCountLabel — 남은 되감기 횟수 TMP 텍스트
    /// </summary>
    public class DiceRollPopupUI : UIBase
    {
        // ── 직렬화 필드 ──────────────────────────────────────────────
        [SerializeField] private Button          rollBtn;
        [SerializeField] private Button          rewindBtn;
        [SerializeField] private TextMeshProUGUI resultLabel;
        [SerializeField] private TextMeshProUGUI rewindCountLabel;
        [SerializeField] private float           autoConfirmDelay = 0.5f;

        // ── 런타임 상태 ──────────────────────────────────────────────
        private int    _diceMax;
        private int    _rewindRemaining;
        private int    _currentResult;
        private Action<int> _onResult;
        private Coroutine   _confirmCoroutine;
        private bool        _resultConfirmed;

        // ── Unity 생명주기 ───────────────────────────────────────────
        private void Awake()
        {
            if (rollBtn   != null) rollBtn.onClick.AddListener(OnRollClicked);
            if (rewindBtn != null) rewindBtn.onClick.AddListener(OnRewindClicked);
        }

        // ── 공개 API ─────────────────────────────────────────────────
        /// <summary>팝업을 열고 주사위 굴리기를 준비합니다.</summary>
        public void Show(int diceMax, int rewindLimit, Action<int> onResult)
        {
            _diceMax         = Mathf.Max(1, diceMax);
            _rewindRemaining = Mathf.Max(0, rewindLimit);
            _onResult        = onResult;
            _currentResult   = 0;
            _resultConfirmed = false;

            UpdateResultLabel("?");
            UpdateRewindLabel();

            SetRollButtonInteractable(true);
            SetRewindButtonInteractable(false);

            base.Show();
        }

        // ── 내부 ─────────────────────────────────────────────────────
        private void OnRollClicked()
        {
            if (_resultConfirmed) return;

            StopConfirmCoroutine();

            _currentResult = UnityEngine.Random.Range(1, _diceMax + 1);
            UpdateResultLabel(_currentResult.ToString());

            SetRollButtonInteractable(false);
            SetRewindButtonInteractable(_rewindRemaining > 0);

            _confirmCoroutine = StartCoroutine(AutoConfirm());
        }

        private void OnRewindClicked()
        {
            if (_rewindRemaining <= 0 || _resultConfirmed) return;

            StopConfirmCoroutine();

            _rewindRemaining--;
            _currentResult = UnityEngine.Random.Range(1, _diceMax + 1);
            UpdateResultLabel(_currentResult.ToString());

            UpdateRewindLabel();
            SetRewindButtonInteractable(_rewindRemaining > 0);

            _confirmCoroutine = StartCoroutine(AutoConfirm());
        }

        private IEnumerator AutoConfirm()
        {
            yield return new WaitForSeconds(autoConfirmDelay);
            ConfirmResult();
        }

        private void ConfirmResult()
        {
            if (_resultConfirmed) return;
            _resultConfirmed = true;

            StopConfirmCoroutine();
            SetRollButtonInteractable(false);
            SetRewindButtonInteractable(false);

            Hide();

            Action<int> callback = _onResult;
            _onResult = null;
            callback?.Invoke(_currentResult);
        }

        private void StopConfirmCoroutine()
        {
            if (_confirmCoroutine != null)
            {
                StopCoroutine(_confirmCoroutine);
                _confirmCoroutine = null;
            }
        }

        private void UpdateResultLabel(string text)
        {
            if (resultLabel != null)
                resultLabel.text = text;
        }

        private void UpdateRewindLabel()
        {
            if (rewindCountLabel != null)
                rewindCountLabel.text = _rewindRemaining.ToString();
        }

        private void SetRollButtonInteractable(bool value)
        {
            if (rollBtn != null)
                rollBtn.interactable = value;
        }

        private void SetRewindButtonInteractable(bool value)
        {
            if (rewindBtn != null)
                rewindBtn.interactable = value;
        }
    }
}
