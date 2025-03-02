using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private const float SWIPE_THRESHOLD = 50f; // Минимальная длина свайпа (в пикселях)
    private Vector2 _startTouchPosition;       // Начальная точка свайпа

    private void Update()
    {
        // Клавиатура (wasd/arrows)
        Vector2 keyboardDirection = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            keyboardDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            keyboardDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            keyboardDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            keyboardDirection = Vector2.right;

        if (keyboardDirection != Vector2.zero)
        {
            Debug.Log($"Keyboard/Arrow Input: {keyboardDirection}");
            GameField.Instance.OnInput(keyboardDirection);
        }

        // Свайп мышью
        if (Input.GetMouseButtonDown(0))
        {
            _startTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endTouchPosition = (Vector2)Input.mousePosition;
            Vector2 delta = endTouchPosition - _startTouchPosition;

            // Проверяем, не короче ли свайп заданного порога
            if (delta.magnitude > SWIPE_THRESHOLD)
            {
                // Преобразуем дельту в одно из 4 направлений
                Vector2 swipeDirection = ConvertToCardinalDirection(delta.normalized);
                Debug.Log($"Mouse Swipe Direction: {swipeDirection}");

                GameField.Instance.OnInput(swipeDirection);
            }
        }

        // Свайп по экрану
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                Vector2 endTouchPosition = touch.position;
                Vector2 delta = endTouchPosition - _startTouchPosition;

                if (delta.magnitude > SWIPE_THRESHOLD)
                {
                    Vector2 swipeDirection = ConvertToCardinalDirection(delta.normalized);
                    Debug.Log($"Touch Swipe Direction: {swipeDirection}");

                    GameField.Instance.OnInput(swipeDirection);
                }
            }
        }
    }

    /// <summary>
    /// Превращает любой диагональный вектор в одно из 4 направлений:
    /// (1;0), (-1;0), (0;1) или (0;-1).
    /// </summary>
    private Vector2 ConvertToCardinalDirection(Vector2 input)
    {
        // Если по X модуль больше – выбираем левое/правое направление
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return (input.x > 0) ? Vector2.right : Vector2.left;
        }
        // Иначе – верх/низ
        else
        {
            return (input.y > 0) ? Vector2.up : Vector2.down;
        }
    }
}
