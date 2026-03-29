using UnityEngine;
using UnityEngine.UI;

public class EnemiesController : MonoBehaviour
{
    public Canvas enemiesCanvas;
    public Button twoButton;
    public Button threeButton;
    public Button fourButton;

    void Start()
    {
        enemiesCanvas.gameObject.SetActive(false);

        twoButton.onClick.RemoveAllListeners();
        twoButton.onClick.AddListener(() => GameManager.Instance.StartNewGame(2));

        threeButton.onClick.RemoveAllListeners();
        threeButton.onClick.AddListener(() => GameManager.Instance.StartNewGame(3));

        fourButton.onClick.RemoveAllListeners();
        fourButton.onClick.AddListener(() => GameManager.Instance.StartNewGame(4));
    }
}