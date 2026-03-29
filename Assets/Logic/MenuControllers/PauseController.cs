using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    public Canvas pauseCanvas;
    public Button continueGameButton;
    public Button saveGameButton;

    void Start()
    {
        pauseCanvas.gameObject.SetActive(false);

        continueGameButton.onClick.RemoveAllListeners();
        continueGameButton.onClick.AddListener(() => GameManager.Instance.ContinueGame());

        saveGameButton.onClick.RemoveAllListeners();
        saveGameButton.onClick.AddListener(() => GameManager.Instance.SaveGame());
    }
}
