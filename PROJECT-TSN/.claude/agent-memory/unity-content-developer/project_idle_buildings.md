---
name: Idle Building System — Asset & Script Structure
description: Key facts about the idle building feature: SO layout, material creation approach, UI components, and visual layer
type: project
---

BuildingDefinitionSO extends ItemDefinitionSO, uses ResourceType enum (Gold/Mana/Wood/Stone/Food).
GameDataModel.ItemDatas is a [field: SerializeField] auto-property — its SerializedProperty name is "<ItemDatas>k__BackingField".

**Why:** Needed when editor scripts try to access ItemDatas via SerializedObject (RegisterToGameDataModel in IdleBuildingAssetCreator).
**How to apply:** When writing editor code that modifies GameDataModel.ItemDatas via reflection, always iterate SerializedProperty with iter.name.Contains("ItemDatas") as primary and FindProperty("<ItemDatas>k__BackingField") as fallback.

Material creation: MCP create_material and create_folder tools were denied; use InitializeOnLoad editor script (IdleBuildingMaterialCreator.cs) with Shader.Find("Sprites/Default") + fallback to "Universal Render Pipeline/2D/Sprite-Lit-Default".

BuildingShopUI was refactored away from inspector-ref fields to auto-discovery in Awake + runtime SpawnCard(). The BuildingShopCardUI class was extracted to its own file (BuildingShopCardUI.cs) for prefab support.

ResourceManager.OnResourceChanged signature: Action<ResourceType, double> — subscribe in OnEnable, unsubscribe in OnDisable (event-driven, no Update polling).

BuildingVisual.cs uses EnsureVisibleBlock() to create a 1x1 Texture2D at runtime when SpriteRenderer.sprite is null, so color-only blocks render correctly without an art asset.

GardenPrototype.unity scene state (as of 2026-03-29):
- 5× BuildingDefinitionSO assets exist at Assets/PROJECT-A/Data/ScriptableObject/Buildings/ and are all wired into GameDataModel.ItemDatas in the scene.
- [IdleSystem] GO hosts: ResourceManager, SpiritManager, ProductionManager, IdleBootstrapper, GameManager.
- SpiritManager.spiritPrefab = {fileID: 0} — IdlePrefabSetup.cs (Tools > TST > Setup Idle Prefabs) creates Spirit.prefab and wires it.
- UI Canvases already in scene: UI.IdleHud (IdleHudUI), UI.BuildingShop (BuildingShopUI + CardContainer), UI.BuildingInfoPanel (BuildingInfoPanelUI + all child widgets), UI.OfflineReward (OfflineRewardUI + SummaryText).
- IdleUISetup.cs (Tools > TST > Setup Idle UI) re-creates child layout for those canvases.
- MCP get_scene_hierarchy returns empty when GardenPrototype is not open in the Unity Editor — open it first for MCP scene tools to work.
