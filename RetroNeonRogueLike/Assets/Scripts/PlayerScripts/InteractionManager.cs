using Interactable;
using Interactable.Items;
using TMPro;
using UnityEngine;
using IM = InputManagerData;

namespace PlayerScripts
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private Transform cameraParentTrans;
        [SerializeField] private ItemManager itemManager;
        [Space]
        [SerializeField] private float itemPickupRange = 3f;

        private GameObject selectedObj;

        private void Update()
        {
            ItemDetection();
        }

        void ItemDetection() 
        {
            if (Physics.Raycast(cameraParentTrans.position, cameraParentTrans.forward, out var hit, itemPickupRange))
            {
                selectedObj = hit.collider.gameObject;

                if (selectedObj.CompareTag("Interactable"))
                    AwaitInteraction();
            }
        }

        void AwaitInteraction()
        {
            var interactable = selectedObj.GetComponent<Interactable.Interactable>();
            if(interactable == null) return;
            
            if(!interactable.isUsable)
                return;
            
            ShowInteractionHint(interactable);

            bool shouldUse = !interactable.awaitInput ||
                             interactable.awaitInput && IM.Interacting;
            switch (interactable)
            {
                case Item item :
                    if(shouldUse) ProcessItem(item);
                    break;
                
                case TextFieldInteraction textFieldInteraction :
                    if(shouldUse) ProcessTextField(textFieldInteraction);
                    break;
            }

            
        }

        void ShowInteractionHint(Interactable.Interactable interactable)
        {
            if(string.IsNullOrEmpty(interactable.interactionMessage))
                return;

            var textObj = ObjectPool.Instance.SpawnFromPoolUI("TextDisplay");
            var text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = interactable.interactionMessage;
        }

        void ProcessItem(Item item)
        {
            if(item.isUsable && !item.pickedUp 
                             && !itemManager.PickingUpItem && itemManager.CanPickupItem)
                itemManager.PickUpItem(item.gameObject);
        }

        void ProcessTextField(TextFieldInteraction textFieldInteraction)
        {
            if(textFieldInteraction.isUsable)
                if(!textFieldInteraction.awaitInput)
                    textFieldInteraction.Use();
                else if(IM.Interacting)
                    textFieldInteraction.Use();
        }

        void ProcessButton()
        {
            
        }

    }
}
