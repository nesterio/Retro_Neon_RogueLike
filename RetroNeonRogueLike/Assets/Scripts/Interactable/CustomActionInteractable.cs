using System;
using SL.Wait;
using UnityEngine;

namespace Interactable
{
    public class CustomActionInteractable : Interactable
    {
        public Action UseAction;
    
        public static CustomActionInteractable Create(GameObject interactableObject, Action useAction, string hint = "Use", bool usable = true, bool awaitUse = true)
        {
            if (interactableObject == null)
                return null;
            
            var interactable = interactableObject.GetComponent<CustomActionInteractable>();
            interactable = interactable == null ? interactableObject.AddComponent<CustomActionInteractable>() : new CustomActionInteractable();

            interactable.isUsable = usable;
            interactable.awaitInput = awaitUse;
            interactable.viewHint = hint;
            interactable.UseAction = useAction;

            return interactable;
        }

        public override void Use()
        {
            if(!isUsable)
                return;
            
            UseAction?.Invoke();
            
            isUsable = false;
            var wait = Wait.Seconds(1f, () => isUsable = true);
            wait.Start();
        }

    }
}
