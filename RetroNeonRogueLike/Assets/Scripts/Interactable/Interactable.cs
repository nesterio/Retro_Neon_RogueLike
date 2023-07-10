using Interface;
using UnityEngine;

namespace Interactable
{
    public abstract class Interactable : MonoBehaviour
    {
        public bool isUsable = true;
        public bool awaitInput = true;
        [TextArea] public string viewHint = "Use";

        public abstract void Use();

        public virtual void ShowInteractionHint()
        {
            if(OnScreenMessage.Instance == null)
                return;
            
            if(OnScreenMessage.IsShowingMessage)
                OnScreenMessage.Instance.HideMessage();
            
            OnScreenMessage.Instance.ShowMessage(viewHint);
        }

        public virtual void HideInteractionHint()
        {
            if(OnScreenMessage.Instance == null)
                return;
            
            OnScreenMessage.Instance.HideMessage();
        }
    }
}
