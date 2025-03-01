using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public int Value { get; private set; }
    public int Points => IsEmpty ? 0 : (int)Mathf.Pow(2, Value);

    public bool HasMerged { get; private set; }
    public bool IsEmpty => Value == 0;

    public const int MaxValue = 11;

    public UnityEvent<int> OnValueChanged = new UnityEvent<int>();
    public UnityEvent<Vector2> OnPositionChanged = new UnityEvent<Vector2>();

    [SerializeField]
    private Image image;
    [SerializeField]
    private TextMeshProUGUI points;

    public void SetValue(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;

        OnValueChanged.Invoke(Value);
        UpdateCell();
    }

    public void IncreaseValue()
    {
        Value++;
        HasMerged = true;

        OnValueChanged.Invoke(Value);
        GameController.Instance.AddPoints(Points);

        UpdateCell();
    }

    public void ResetFlags()
    {
        HasMerged = false;
    }

    public void MergeWithCell(Cell otherCell)
    {
        otherCell.IncreaseValue();
        SetValue(X, Y, 0);

        UpdateCell();
    }

    public void MoveToCell(Cell target)
    {
        target.SetValue(target.X, target.Y, Value);
        SetValue(X, Y, 0);

        OnPositionChanged.Invoke(new Vector2(target.X, target.Y));
        UpdateCell();
    }

    public void UpdateCell()
    {
        points.text = IsEmpty ? string.Empty : Points.ToString();
        points.color = Value <= 2 ? ColorManager.Instance.PointsDarkColor
            : ColorManager.Instance.PointsLightColor;

        var color = points.color;
        color.a = 1f; // Alpha = 1 (полностью видно)
        points.color = color;

        image.color = ColorManager.Instance.CellColors[Value];
    }
}