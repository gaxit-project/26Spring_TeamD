using UnityEngine;
using System.Collections;

public class DoorAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;
    [SerializeField] private float speed = 1.0f;

    // アニメーション多重起動を防ぐロック
    private bool isAnimating = false;

    public IEnumerator Open()
    {
        if (isAnimating)
        {
            Debug.LogWarning("[DoorAnimation] Open called while animating. Ignored.");
            yield break;
        }

        float screenWidth = GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;

        // 左ドアを左外へ、右ドアを右外へ
        yield return MoveDoors(
            new Vector2(-screenWidth / 2f, 0f),
            new Vector2(screenWidth / 2f, 0f)
        );
    }

    public IEnumerator Close()
    {
        if (isAnimating)
        {
            Debug.LogWarning("[DoorAnimation] Close called while animating. Ignored.");
            yield break;
        }

        // 両方のドアを中央（0, 0）へ
        yield return MoveDoors(Vector2.zero, Vector2.zero);
    }

    private IEnumerator MoveDoors(Vector2 leftTarget, Vector2 rightTarget)
    {
        isAnimating = true;

        float elapsed = 0f;
        Vector2 leftStart = leftDoor.anchoredPosition;
        Vector2 rightStart = rightDoor.anchoredPosition;

        while (elapsed < speed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / speed);

            leftDoor.anchoredPosition = Vector2.Lerp(leftStart, leftTarget, t);
            rightDoor.anchoredPosition = Vector2.Lerp(rightStart, rightTarget, t);

            yield return null;
        }

        // 終端値を確定
        leftDoor.anchoredPosition = leftTarget;
        rightDoor.anchoredPosition = rightTarget;

        isAnimating = false;
    }
}