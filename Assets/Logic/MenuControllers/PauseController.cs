using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    public Button continueGameButton;
    public Button saveGameButton;

    void Start()
    {
        continueGameButton.onClick.RemoveAllListeners();
        continueGameButton.onClick.AddListener(() => GameManager.Instance.ContinueGame());

        saveGameButton.onClick.RemoveAllListeners();
        saveGameButton.onClick.AddListener(() => GameManager.Instance.SaveGame());
    }
}
