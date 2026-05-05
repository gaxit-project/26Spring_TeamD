using UnityEngine;
using System.Collections;

public class DoorAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;
    [SerializeField] private float speed = 1.0f;

    public IEnumerator Open()
    {
        float screenWidth = GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.width;
        // 左ドアを左外へ、右ドアを右外へ
        yield return MoveDoors(new Vector2(-screenWidth / 2, 0), new Vector2(screenWidth / 2, 0));
    }

    public IEnumerator Close()
    {
        // 両方のドアを中央（0, 0）へ
        yield return MoveDoors(Vector2.zero, Vector2.zero);
    }

    private IEnumerator MoveDoors(Vector2 leftTarget, Vector2 rightTarget)
    {
        float elapsed = 0;
        Vector2 leftStart = leftDoor.anchoredPosition;
        Vector2 rightStart = rightDoor.anchoredPosition;

        while (elapsed < speed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / speed;
            leftDoor.anchoredPosition = Vector2.Lerp(leftStart, leftTarget, t);
            rightDoor.anchoredPosition = Vector2.Lerp(rightStart, rightTarget, t);
            yield return null;
        }

        // 最後に値を確定させる
        leftDoor.anchoredPosition = leftTarget;
        rightDoor.anchoredPosition = rightTarget;
    }
}