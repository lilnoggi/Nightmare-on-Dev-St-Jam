using UnityEngine;

public interface IInteractable
{
    void Interact();
    Sprite GetPromptIcon(); // Forces all interactables to provide an icon
}
