using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TST
{
    /// <summary>
    /// 망원경 업그레이드 팝업 (Popup_TelescopeUpgrade).
    /// Show() 호출 시 TelescopeData 의 파츠 정보를 읽어 행을 생성합니다.
    /// 각 행: 파츠명 | 현재 레벨 | 업그레이드 비용 | 업그레이드 버튼
    ///
    /// 업그레이드 버튼 비활성 조건:
    ///   - 현재 레벨 >= 5 (최대)
    ///   - PlayerParameters.Singleton.Funds &lt; 업그레이드 비용
    ///
    /// Inspector wiring:
    ///   - partsContainer : 파츠 행 부모 Transform (ScrollView content)
    ///   - partRowPrefab  : 파츠 한 행 프리팹 (PartRowItem 컴포넌트 또는 TMP+Button 구조)
    ///   - backBtn        : 돌아가기 버튼
    /// </summary>
    public class UniversityController : UIBase
    {
        // ----------------------------------------------------------------
        //  Inspector
        // ----------------------------------------------------------------
        [Header("Parts List")]
        [SerializeField] private Transform partsContainer;
        [SerializeField] private GameObject partRowPrefab;

        [Header("Back Button")]
        [SerializeField] private Button backBtn;

        // ----------------------------------------------------------------
        //  Constants
        // ----------------------------------------------------------------
        private const int MaxLevel = 5;

        /// <summary>파츠별 레벨당 업그레이드 비용 테이블 (레벨 → 다음 레벨 비용).</summary>
        private static readonly Dictionary<TelescopePartType, double[]> UpgradeCostTable =
            new Dictionary<TelescopePartType, double[]>
        {
            // index 0 = lv1→2, index 1 = lv2→3, index 2 = lv3→4, index 3 = lv4→5
            { TelescopePartType.Lens,            new double[] { 100, 250, 500,  1000 } },
            { TelescopePartType.Filter,          new double[] { 80,  200, 400,  800  } },
            { TelescopePartType.Handle,          new double[] { 60,  150, 300,  600  } },
            { TelescopePartType.OpticalAdjuster, new double[] { 120, 300, 600,  1200 } },
            { TelescopePartType.FocusTracker,    new double[] { 120, 300, 600,  1200 } },
            { TelescopePartType.SignalAmplifier, new double[] { 150, 350, 700,  1500 } },
            { TelescopePartType.RecordingDevice, new double[] { 150, 350, 700,  1500 } },
        };

        // ----------------------------------------------------------------
        //  UIBase override
        // ----------------------------------------------------------------
        public override void Show()
        {
            base.Show();
            RefreshList();
            WireButtons();
        }

        // ----------------------------------------------------------------
        //  List building
        // ----------------------------------------------------------------
        private void RefreshList()
        {
            ClearList();

            foreach (TelescopePartType part in Enum.GetValues(typeof(TelescopePartType)))
            {
                SpawnPartRow(part);
            }
        }

        private void ClearList()
        {
            if (partsContainer == null) return;

            for (int i = partsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(partsContainer.GetChild(i).gameObject);
            }
        }

        private void SpawnPartRow(TelescopePartType part)
        {
            if (partRowPrefab == null || partsContainer == null) return;

            GameObject go = Instantiate(partRowPrefab, partsContainer);

            PartRowItem rowItem = go.GetComponent<PartRowItem>();
            if (rowItem != null)
            {
                int     currentLevel = TelescopeData.Singleton.GetLevel(part);
                double  cost         = GetUpgradeCost(part, currentLevel);
                bool    canUpgrade   = currentLevel < MaxLevel && PlayerParameters.Singleton.Funds >= cost;

                rowItem.Setup(part, currentLevel, cost, canUpgrade, () => OnUpgradeClicked(part));
                return;
            }

            // Fallback: 프리팹에 PartRowItem 없을 때 TMP 텍스트 + Button 으로 채움
            TextMeshProUGUI[] labels = go.GetComponentsInChildren<TextMeshProUGUI>();
            int currentLv = TelescopeData.Singleton.GetLevel(part);
            double upgCost = GetUpgradeCost(part, currentLv);

            if (labels.Length >= 1) labels[0].text = part.ToString();
            if (labels.Length >= 2) labels[1].text = $"Lv.{currentLv}";
            if (labels.Length >= 3) labels[2].text = currentLv >= MaxLevel ? "MAX" : $"{upgCost:N0}G";

            Button btn = go.GetComponentInChildren<Button>();
            if (btn != null)
            {
                bool canUpgr = currentLv < MaxLevel && PlayerParameters.Singleton.Funds >= upgCost;
                btn.interactable = canUpgr;

                TelescopePartType captured = part;
                btn.onClick.AddListener(() => OnUpgradeClicked(captured));
            }
        }

        // ----------------------------------------------------------------
        //  Upgrade
        // ----------------------------------------------------------------
        private void OnUpgradeClicked(TelescopePartType part)
        {
            int    currentLevel = TelescopeData.Singleton.GetLevel(part);
            double cost         = GetUpgradeCost(part, currentLevel);

            bool success = TelescopeData.Singleton.TryUpgrade(part, cost);
            if (success)
            {
                // 업그레이드 후 목록 갱신
                RefreshList();
            }
        }

        // ----------------------------------------------------------------
        //  Buttons
        // ----------------------------------------------------------------
        private void WireButtons()
        {
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnBackClicked()
        {
            UIManager.Hide<UniversityController>(UIList.Popup_TelescopeUpgrade);

            if (DayCityController.Singleton != null)
                DayCityController.Singleton.HideSubLocation();
        }

        // ----------------------------------------------------------------
        //  Cost helper
        // ----------------------------------------------------------------
        private static double GetUpgradeCost(TelescopePartType part, int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0.0;

            int index = currentLevel - 1; // lv1 → index 0
            if (UpgradeCostTable.TryGetValue(part, out double[] costs))
            {
                if (index >= 0 && index < costs.Length)
                    return costs[index];
            }

            // fallback: 일반 공식
            return 100.0 * Math.Pow(2.5, currentLevel - 1);
        }
    }

    // ====================================================================
    //  PartRowItem — 파츠 행 프리팹 컴포넌트
    // ====================================================================
    /// <summary>
    /// 업그레이드 팝업 내 파츠 행 한 줄 컴포넌트.
    ///
    /// Inspector wiring:
    ///   - nameLabel    : 파츠명 텍스트
    ///   - levelLabel   : 현재 레벨 텍스트
    ///   - costLabel    : 업그레이드 비용 텍스트
    ///   - upgradeBtn   : 업그레이드 버튼
    /// </summary>
    public class PartRowItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private TextMeshProUGUI costLabel;
        [SerializeField] private Button          upgradeBtn;

        public void Setup(TelescopePartType part, int currentLevel, double cost, bool canUpgrade, Action onUpgrade)
        {
            if (nameLabel  != null) nameLabel.text  = part.ToString();
            if (levelLabel != null) levelLabel.text  = $"Lv.{currentLevel}";
            if (costLabel  != null) costLabel.text   = currentLevel >= 5 ? "MAX" : $"{cost:N0}G";

            if (upgradeBtn != null)
            {
                upgradeBtn.interactable = canUpgrade;
                upgradeBtn.onClick.RemoveAllListeners();
                upgradeBtn.onClick.AddListener(() => onUpgrade?.Invoke());
            }
        }
    }
}
