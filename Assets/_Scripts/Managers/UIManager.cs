using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction HUD")]
    [SerializeField] private Image _promptImage;

    // ---------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Hide prompt when game starts
        HidePrompt();
    }

    public void ShowPrompt(Sprite promptSprite)
    {
        if (promptSprite != null)
        {
            _promptImage.sprite = promptSprite;
            _promptImage.gameObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        _promptImage.gameObject.SetActive(false);
    }
}
