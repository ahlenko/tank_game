using UnityEngine;
using UnityEngine.UI;

public class EnemiesController : MonoBehaviour
{
    public Button twoButton;
    public Button threeButton;
    public Button fourButton;

    void Start()
    {
        twoButton.onClick.RemoveAllListeners();
        twoButton.onClick.AddListener(() => GameManager.Instance.StartNewGame(2));

        threeButton.onClick.RemoveAllListeners();
        threeButton.onClick.AddListener(() => GameManager.Instance.StartNewGame(3));

        fourButton.onClick.RemoveAllListeners();
        fourButton.onClick.AddListener(() => GameManager.Instance.StartNewGame(4));
    }
}