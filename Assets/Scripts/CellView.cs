using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CellView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI valueText;
    [SerializeField]
    private Image cellImage;

    private Cell cell;

    public void Init(Cell cell)
    {
        this.cell = cell;
        cell.OnValueChanged.AddListener(UpdateValue);
        cell.OnPositionChanged.AddListener(UpdatePosition);

        UpdateValue(cell.Value);
        UpdatePosition(new Vector2(cell.X, cell.Y));
    }

    private void UpdateValue(int value)
    {
        valueText.text = value == 0 ? string.Empty : Mathf.Pow(2, value).ToString();
    }

    private void UpdatePosition(Vector2 position)
    {
        transform.localPosition = new Vector3(position.x, position.y, 0);
    }
}