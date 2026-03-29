using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public class InputSystem : SingletonBase<InputSystem>
    {
        public bool IsActiveCursorVisible => Cursor.visible;

        public System.Action OnInput_Jump;
        public System.Action OnInput_HelpPopupToggle;
        public System.Action OnInput_MainWeapon;
        public System.Action OnInput_SubWeapon;
        public System.Action OnInput_ToggleFpsRightButtonTransition;
        public System.Action OnInput_MaintainZoom;
        public System.Action OnInput_ReturnToTps;
        public System.Action OnInput_Roll;
        public System.Action OnInput_Interact;
        public System.Action OnInput_PlayerThirdViewRightLeftChange;
        public System.Action OnInput_Shoot;
        public System.Action OnInput_ShootFinish;
        public System.Action OnInput_Reload;
        public System.Action OnInput_Crouch;
        public System.Action OnInput_ShortCutItemUse;
        public System.Func<bool> OnInput_InventoryToggle;
        public System.Func<bool> OnInput_EquipmentToggle;

        public System.Action OnInput_WorldMap;


        private float aimStartTime = 0f;
        private float threshold = 0.25f;
        private bool shoulderZoom = false;
        private bool scopeZoom = false;

        private void Start()
        {
            SetCursorVisible(false);
        }

        private static void SetCursorVisible(bool isVisible)
        {
            if (isVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnInput_HelpPopupToggle?.Invoke();
            }

            if (Input.GetMouseButton(0))
            {
                OnInput_Shoot?.Invoke();
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnInput_ShootFinish?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnInput_Reload?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                OnInput_Crouch?.Invoke();
            }

            if (Input.GetMouseButtonDown(1))
            {
                aimStartTime = Time.time;
                shoulderZoom = false;
            }

            if (Input.GetMouseButton(1))
            {
                if (shoulderZoom == false && Time.time - aimStartTime >= threshold)
                {
                    shoulderZoom = true;
                    // 견착
                    OnInput_MaintainZoom?.Invoke();
                }
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                OnInput_InventoryToggle?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInput_EquipmentToggle?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                OnInput_Interact?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                OnInput_Roll?.Invoke();
            }

            if (Input.GetMouseButtonUp(1))
            {
                // 만약 견착 모드라면 해제
                if (shoulderZoom)
                {
                    OnInput_ReturnToTps?.Invoke();
                    shoulderZoom = false;
                }
                else // 짧게 누른 상태라면 스코프모드
                {
                    if (scopeZoom)
                    {
                        OnInput_ToggleFpsRightButtonTransition?.Invoke();
                        scopeZoom = false;
                    }
                    else
                    {
                        OnInput_ToggleFpsRightButtonTransition?.Invoke();
                        scopeZoom = true;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnInput_Jump?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnInput_MainWeapon?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                OnInput_SubWeapon?.Invoke();
            }

       

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                ChangeCursorVisibility(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                ChangeCursorVisibility(false);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {

            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                OnInput_ShortCutItemUse?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                OnInput_WorldMap?.Invoke();
            }
        }

        public void ChangeCursorVisibility(bool isVisible)
        {
            if (isVisible == false)
            {
                if (UIManager.Singleton.ActiveCursorVisibleUIsCount <= 0)
                {
                    SetCursorVisible(false);
                }
            }
            else
            {
                SetCursorVisible(true);
            }

        }
    }
}
