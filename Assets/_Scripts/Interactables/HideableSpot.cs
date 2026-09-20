using UnityEngine;

public class HideableSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite _hideIcon; // The UI prompt for your UIManager
    [SerializeField] private Transform _hidePoint; // Create an empty GameObject inside the wardrobe and assign it here

    public void Interact()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.StartHiding(this);
        }
    }

    public Sprite GetPromptIcon()
    {
        return _hideIcon;
    }

    public Transform GetHidePoint()
    {
        return _hidePoint;
    }
}