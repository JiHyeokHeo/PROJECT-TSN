using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public abstract class UIBase : MonoBehaviour
    {
        protected bool isVisibleCursor = false;
        public virtual bool IsVisibleCursor { get => isVisibleCursor; set => isVisibleCursor = value; }

        public virtual bool UseScreenSpaceCamera
        {
            get => useScreenSpaceCamera;
            set
            {
                useScreenSpaceCamera = value;
                if (useScreenSpaceCamera)
                {
                    GetComponent<Canvas>().worldCamera = UIManager.Singleton.UICamera;
                }
}
        }
        private bool useScreenSpaceCamera = false;

        public virtual void Show()
        {
            bool wasVisible = gameObject.activeSelf;
            gameObject.SetActive(true);

            if (!wasVisible)
            {
                UIManager.NotifyVisibilityChangedFromUIBase(this, true);
            }
        }

        public virtual void Hide()
        {
            bool wasVisible = gameObject.activeSelf;
            gameObject.SetActive(false);

            if (wasVisible)
            {
                UIManager.NotifyVisibilityChangedFromUIBase(this, false);
            }
        }
    }
}
