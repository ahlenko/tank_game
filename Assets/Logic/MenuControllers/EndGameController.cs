using UnityEngine;
using UnityEngine.UI;

public class EndGameController : MonoBehaviour
{
    public Canvas endGameCanvas;
    public Button goToMenuButton;

    void Start()
    {
        endGameCanvas.gameObject.SetActive(false);

        goToMenuButton.onClick.RemoveAllListeners();
        goToMenuButton.onClick.AddListener(() => GameManager.Instance.GoToMenu());
    }
}
