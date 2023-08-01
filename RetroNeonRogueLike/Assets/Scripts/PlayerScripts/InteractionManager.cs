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

        private Interactable.Interactable selectedInteractable;

        private void Update()
        {
            ItemDetection();
        }

        void ItemDetection()
        {
            Physics.Raycast(cameraParentTrans.position, cameraParentTrans.forward, out var hit, itemPickupRange);

            if (hit.collider == null)
            {
                if (selectedInteractable != null)
                {
                    selectedInteractable.HideInteractionHint();
                    selectedInteractable = null;
                }
                return;
            }

            if (selectedInteractable == null || selectedInteractable.gameObject != hit.collider.gameObject)
            {
                if (selectedInteractable != null)
                    selectedInteractable.HideInteractionHint();

                if(hit.collider.CompareTag("Interactable"))
                    selectedInteractable = hit.collider.gameObject.GetComponent<Interactable.Interactable>();
            }
                
            if (selectedInteractable != null && selectedInteractable.CompareTag("Interactable"))
                AwaitInteraction();
        }

        void AwaitInteraction()
        {
            if(selectedInteractable == null)
                return;
            
            if(selectedInteractable.awaitInput)
                selectedInteractable.ShowInteractionHint();

            bool shouldUse = !selectedInteractable.awaitInput ||
                             selectedInteractable.awaitInput && IM.Interacting;
            if (shouldUse)
            {
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
        }

        void ProcessItem(Item item)
        {
            if(item.isPickable && !PlayerManager.ItemsManager.PickingUpItem && PlayerManager.ItemsManager.CanPickupItem)
                PlayerManager.ItemsManager.PickUpItem(item.gameObject);
        }

        void ProcessPopup(BasicPopupInteraction basicPopupInteraction)
        {
            if(!basicPopupInteraction.IsUsable)
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
            if(interactable.IsUsable)
                interactable.Use();
        }

    }
}
