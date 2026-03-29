using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public Button startButton;
    public Button continueButton;
    public Button quitButton;
    public Button viewButton;

    private string savePath;

    void Start()
    {
        savePath = Application.persistentDataPath + "/save.json";
        bool hasSave = System.IO.File.Exists(savePath);

        SetButtonTransparency(continueButton, hasSave);
        SetButtonTransparency(viewButton, hasSave);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => UIManager.Instance.ShowEnemiesUI());

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => GameManager.Instance.ContinueGame());

        viewButton.onClick.RemoveAllListeners();
        viewButton.onClick.AddListener(() => GameManager.Instance.ViewSave());

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(() => Application.Quit());
    }

    void SetButtonTransparency(Button btn, bool enabled)
    {
        Color color = btn.image.color;
        color.a = enabled ? 1f : 0.5f;
        btn.image.color = color;
        btn.interactable = enabled;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}