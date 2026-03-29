using UnityEngine;
using UnityEngine.UI;

public class EndGameController : MonoBehaviour
{
    public Button goToMenuButton;

    void Start()
    {
        goToMenuButton.onClick.RemoveAllListeners();
        goToMenuButton.onClick.AddListener(() => GameManager.Instance.EndGame());
    }
}
