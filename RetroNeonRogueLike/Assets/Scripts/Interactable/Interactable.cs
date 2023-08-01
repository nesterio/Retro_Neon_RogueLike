using Interface;
using UnityEngine;

namespace Interactable
{
    public abstract class Interactable : MonoBehaviour
    {
        public bool IsUsable = true;
        [field:SerializeField]public bool awaitInput { get; protected set; } = true;
        [field:SerializeField]public bool showViewHint { get; protected set; } = true;
        [TextArea] public string viewHint = "Use";

        public abstract void Use();

        public virtual void ShowInteractionHint()
        {
            if(OnScreenMessage.Instance == null || !showViewHint)
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
