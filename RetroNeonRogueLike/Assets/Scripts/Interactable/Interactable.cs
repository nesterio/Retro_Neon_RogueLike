using UnityEngine;

namespace Interactable
{
    public abstract class Interactable : MonoBehaviour
    {
        public bool isUsable = true;
        public bool awaitInput = true;
        [TextArea] public string interactionMessage = "Use";
    
        public abstract void Use();
    }
}
