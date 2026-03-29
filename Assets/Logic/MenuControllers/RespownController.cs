using UnityEngine;
using UnityEngine.UI;

public class RespownController : MonoBehaviour
{
    public Button respownButton;

    void Start()
    {
        respownButton.onClick.RemoveAllListeners();
        respownButton.onClick.AddListener(() => GameManager.Instance.RespawnPlayer());
    }
}
