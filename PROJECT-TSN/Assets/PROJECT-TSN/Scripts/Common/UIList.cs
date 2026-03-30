using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public enum UIList
    {
        UI_PANEL_START,

        Minimap_UI,
        CrossHair_UI,
        LoadingUI,
        TitleUI,
        IngameUI,
        MainHudUI,
        ShortCutUI,
        IdleHudUI,

        // Starry Night panels
        MainLayout,
        HUD_Parameters,
        Panel_DayAttic,
        Panel_DayCity,
        Panel_NightAttic,
        Panel_Fishing,
        Panel_FishingHUD,
        Panel_FishingTimer,
        Panel_Dream,

        Panel_Title,
        Panel_Cutscene,
        Panel_SaveScreen,
        Panel_LoadScreen,
        Panel_DreamKeySelection,
        Panel_DreamVN,

        UI_PANEL_END,
        UI_POPUP_START,

        GardenShopUI,
        GardenInventoryUI,
        GardenScorePanelUI,
        CheatUI,
        BuildingShopUI,
        BuildingInfoPanelUI,
        OfflineRewardUI,

        // Starry Night popups
        Popup_Inventory,
        Popup_ObservationJournal,
        Popup_TelescopeUpgrade,
        Popup_RecordDisposal,
        Popup_SpaceMap,
        Popup_FocusMinigame,

        Popup_Menu,
        Popup_Options,
        Popup_DiceRoll,
        Popup_Choice,
        Popup_NightTelescope,
        Popup_DecorationShop,

        UI_POPUP_END,
    }
}
