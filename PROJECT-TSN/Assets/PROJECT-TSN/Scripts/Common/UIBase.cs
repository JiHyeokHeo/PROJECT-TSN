using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public class UIBase : MonoBehaviour
    {
        public virtual bool IsVisibleCursor { get; set; } = false;

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
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
