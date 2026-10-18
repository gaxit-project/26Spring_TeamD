using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINavigationGuard : MonoBehaviour
{
    [Tooltip("フォーカスが外れたときに強制的に再選択させたいデフォルトのボタン")]
    [SerializeField] private Button defaultSelectedButton;

    private void Update()
    {
        // ポーズ中で、かつ現在何にもフォーカスが当たっていない（SelectedGameObjectがnull）場合
        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPaused)
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            {
                if (defaultSelectedButton != null && defaultSelectedButton.gameObject.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
                }
            }
        }
    }
}