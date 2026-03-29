using UnityEngine;
using UnityEngine.UI;

public class RespownController : MonoBehaviour
{
    public Canvas respownCanvas;
    public Button respownButton;

    void Start()
    {
        respownCanvas.gameObject.SetActive(false);

        respownButton.onClick.RemoveAllListeners();
        respownButton.onClick.AddListener(() => GameManager.Instance.RespawnPlayer());
    }
}
