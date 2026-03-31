using System;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public enum GamePhase
    {
        DayAttic,
        DayCity,
        NightA,
        Fishing,
        NightB,
        Dream
    }

    public class PhaseManager : SingletonBase<PhaseManager>
    {
        // ----------------------------------------------------------------
        //  Events
        // ----------------------------------------------------------------
        /// <summary>Fired after a phase transition completes. Args: (oldPhase, newPhase)</summary>
        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        // ----------------------------------------------------------------
        //  Runtime state
        // ----------------------------------------------------------------
        [field: SerializeField]
        public GamePhase CurrentPhase { get; private set; } = GamePhase.DayAttic;
        [field: SerializeField]
        public int CurrentDay         { get; private set; } = 1;

        // ----------------------------------------------------------------
        //  Valid transitions table
        // ----------------------------------------------------------------
        private static readonly Dictionary<GamePhase, HashSet<GamePhase>> ValidTransitions =
            new Dictionary<GamePhase, HashSet<GamePhase>>
        {
            {
                GamePhase.DayAttic, new HashSet<GamePhase>
                {
                    GamePhase.DayCity,
                    GamePhase.NightA
                }
            },
            {
                GamePhase.DayCity, new HashSet<GamePhase>
                {
                    GamePhase.DayAttic
                }
            },
            {
                GamePhase.NightA, new HashSet<GamePhase>
                {
                    GamePhase.Fishing,
                    GamePhase.Dream,
                    GamePhase.DayAttic   // skip night — go straight to next day
                }
            },
            {
                GamePhase.Fishing, new HashSet<GamePhase>
                {
                    GamePhase.NightB
                }
            },
            {
                GamePhase.NightB, new HashSet<GamePhase>
                {
                    GamePhase.Dream,
                    GamePhase.DayAttic
                }
            },
            {
                GamePhase.Dream, new HashSet<GamePhase>
                {
                    GamePhase.DayAttic
                }
            }
        };

        // ----------------------------------------------------------------
        //  Transition
        // ----------------------------------------------------------------
        /// <summary>
        /// Transitions to nextPhase if the move is valid.
        /// When transitioning into DayAttic from Dream (or any end-of-night path),
        /// CurrentDay is incremented.
        /// </summary>
        public bool TransitionTo(GamePhase nextPhase)
        {
            if (!IsValidTransition(CurrentPhase, nextPhase))
            {
                Debug.LogWarningFormat(
                    "[PhaseManager] Invalid transition: {0} -> {1}",
                    CurrentPhase, nextPhase);
                return false;
            }

            GamePhase oldPhase = CurrentPhase;
            CurrentPhase = nextPhase;

            // Advance day when re-entering DayAttic from a night-side phase
            if (nextPhase == GamePhase.DayAttic && IsNightSidePhase(oldPhase))
            {
                CurrentDay++;
                Debug.LogFormat("[PhaseManager] Day advanced to {0}.", CurrentDay);
            }

            Debug.LogFormat("[PhaseManager] Phase: {0} -> {1}  (Day {2})", oldPhase, nextPhase, CurrentDay);
            OnPhaseChanged?.Invoke(oldPhase, nextPhase);
            return true;
        }

        // ----------------------------------------------------------------
        //  Save/load support
        // ----------------------------------------------------------------
        /// <summary>
        /// Restores CurrentDay directly from a save file without going through a phase transition.
        /// Does not fire OnPhaseChanged.
        /// </summary>
        public void ForceSetDay(int day)
        {
            CurrentDay = Mathf.Max(1, day);
            Debug.LogFormat("[PhaseManager] Day restored to {0} from save data.", CurrentDay);
        }

        /// <summary>
        /// Restores CurrentPhase directly from a save file. Does not fire OnPhaseChanged.
        /// 로드 후 UI 갱신이 필요하면 ForceTransitionTo를 사용하십시오.
        /// </summary>
        public void ForceSetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            Debug.LogFormat("[PhaseManager] Phase restored to {0} from save data.", phase);
        }

        /// <summary>
        /// 페이즈를 강제 전환하고 OnPhaseChanged를 발동합니다.
        /// 유효성 검사를 우회하므로 로드 직후 UI 동기화 용도로만 사용하십시오.
        /// </summary>
        public void ForceTransitionTo(GamePhase phase)
        {
            GamePhase oldPhase = CurrentPhase;
            CurrentPhase = phase;
            Debug.LogFormat("[PhaseManager] Force transition: {0} -> {1}  (Day {2})", oldPhase, phase, CurrentDay);
            OnPhaseChanged?.Invoke(oldPhase, phase);
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------
        public bool IsValidTransition(GamePhase from, GamePhase to)
        {
            return ValidTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
        }

        /// <summary>Returns true for phases that belong to the night loop (not day).</summary>
        private static bool IsNightSidePhase(GamePhase phase)
        {
            return phase == GamePhase.NightA
                || phase == GamePhase.Fishing
                || phase == GamePhase.NightB
                || phase == GamePhase.Dream;
        }
    }
}
