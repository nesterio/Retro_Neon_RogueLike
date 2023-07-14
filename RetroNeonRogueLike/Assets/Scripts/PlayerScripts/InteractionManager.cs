using Interactable;
using Interactable.Items;
using SL.Wait;
using UnityEngine;
using IM = InputManagerData;

namespace PlayerScripts
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private Transform cameraParentTrans;
        [Space]
        [SerializeField] private float itemPickupRange = 3f;

        private GameObject selectedObj;
        private Interactable.Interactable selectedInteractable;

        private void Update()
        {
            ItemDetection();
        }

        void ItemDetection() 
        {
            if (Physics.Raycast(cameraParentTrans.position, cameraParentTrans.forward, out var hit, itemPickupRange))
            {
                if (selectedObj != hit.collider.gameObject)
                {
                    if (selectedInteractable != null)
                        selectedInteractable.HideInteractionHint();

                    selectedObj = hit.collider.gameObject;
                    
                    if(hit.collider.gameObject.CompareTag("Interactable"))
                        selectedInteractable = selectedObj.GetComponent<Interactable.Interactable>();
                }
                
                if (selectedObj.CompareTag("Interactable"))
                    AwaitInteraction();
            }
        }

        void AwaitInteraction()
        {
            if(selectedInteractable == null)
                return;
            
            if(selectedInteractable.awaitInput)
                selectedInteractable.ShowInteractionHint();

            bool shouldUse = !selectedInteractable.awaitInput ||
                             selectedInteractable.awaitInput && IM.Interacting;
            if(shouldUse)
                switch (selectedInteractable)
                {
                    case Item item :
                        ProcessItem(item);
                        break;
                
                    case BasicPopupInteraction basicPopupInteraction :
                        ProcessPopup(basicPopupInteraction);
                        break;
                    
                    default:
                        ProcessInteractable(selectedInteractable);
                        break;
                }
        }

        void ProcessItem(Item item)
        {
            if(item.isPickable && !PlayerManager.ItemsManager.PickingUpItem && PlayerManager.ItemsManager.CanPickupItem)
                PlayerManager.ItemsManager.PickUpItem(item.gameObject);
        }

        void ProcessPopup(BasicPopupInteraction basicPopupInteraction)
        {
            if(!basicPopupInteraction.isUsable)
                return;

            basicPopupInteraction.OnCloseAction = () =>
            {
                var wait = Wait.Seconds(0.5f, PlayerManager.UnFreezePlayer);
                wait.Start();
            };
            basicPopupInteraction.Use();
            PlayerManager.FreezePlayer();
        }

        void ProcessInteractable(Interactable.Interactable interactable)
        {
            if(interactable.isUsable)
                interactable.Use();
        }

    }
}
