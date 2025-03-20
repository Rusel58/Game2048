using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class CellTests
{
    private GameObject controllerObject;
    private GameObject colorManagerObject;
    private GameObject cellObject;

    private GameController gameController;
    private ColorManager colorManager;
    private Cell cell;

    [SetUp]
    public void SetUp()
    {
        // 1) Создаём GameController
        controllerObject = new GameObject("GameController");
        gameController = controllerObject.AddComponent<GameController>();
        GameController.Instance = gameController;

        // Присваиваем TextMeshPro для pointsText, чтобы SetPoints() не вызывал NullReference
        var pointsGO = new GameObject("PointsText");
        var pointsTMP = pointsGO.AddComponent<TextMeshProUGUI>();
        typeof(GameController)
            .GetField("pointsText", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(gameController, pointsTMP);

        // Присваиваем TextMeshPro для gameResult (если нужно)
        var resultGO = new GameObject("GameResultText");
        var resultTMP = resultGO.AddComponent<TextMeshProUGUI>();
        typeof(GameController)
            .GetField("gameResult", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(gameController, resultTMP);

        // 2) Создаём ColorManager
        colorManagerObject = new GameObject("ColorManager");
        colorManager = colorManagerObject.AddComponent<ColorManager>();
        ColorManager.Instance = colorManager;
        // Заполним массив CellColors (для Value от 0 до 11)
        colorManager.CellColors = new Color[12];

        // 3) Создаём объект Cell
        cellObject = new GameObject("TestCell");
        cell = cellObject.AddComponent<Cell>();

        // Назначаем image и points (оба [SerializeField] private)
        var imgGO = new GameObject("CellImage");
        var img = imgGO.AddComponent<Image>();
        typeof(Cell).GetField("image", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(cell, img);

        var txtGO = new GameObject("CellPoints");
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        typeof(Cell).GetField("points", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(cell, tmp);

        // Инициализируем ячейку
        cell.SetValue(0, 0, 1);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(cellObject);
        Object.DestroyImmediate(controllerObject);
        Object.DestroyImmediate(colorManagerObject);

        GameController.Instance = null;
        ColorManager.Instance = null;
    }

    [Test]
    public void SetValue_ShouldSetCoordinatesAndValue()
    {
        cell.SetValue(2, 3, 4);
        cell.X.Should().Be(2);
        cell.Y.Should().Be(3);
        cell.Value.Should().Be(4);
    }

    [Test]
    public void IncreaseValue_ShouldIncreaseValueByOneAndAddPoints()
    {
        int oldValue = cell.Value;
        cell.IncreaseValue();

        cell.Value.Should().Be(oldValue + 1);
        cell.Points.Should().Be((int)Mathf.Pow(2, cell.Value));
        cell.HasMerged.Should().BeTrue();
        // Проверим, что GameController.Points тоже увеличился
        GameController.Points.Should().BeGreaterThan(0, "очки должны прибавиться при IncreaseValue");
    }

    [Test]
    public void ResetFlags_ShouldResetHasMerged()
    {
        cell.IncreaseValue();
        cell.HasMerged.Should().BeTrue();

        cell.ResetFlags();
        cell.HasMerged.Should().BeFalse();
    }

    [Test]
    public void MergeWithCell_ShouldIncreaseTargetValueAndResetSource()
    {
        // Создаём другую ячейку для слияния
        var otherObject = new GameObject("OtherCell");
        var targetCell = otherObject.AddComponent<Cell>();

        // Назначаем image/points targetCell
        var tImgGO = new GameObject("TargetCellImage");
        var tImg = tImgGO.AddComponent<Image>();
        typeof(Cell).GetField("image", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(targetCell, tImg);

        var tTxtGO = new GameObject("TargetCellPoints");
        var tTmp = tTxtGO.AddComponent<TextMeshProUGUI>();
        typeof(Cell).GetField("points", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(targetCell, tTmp);

        // Инициализируем targetCell тем же значением, что и у cell
        targetCell.SetValue(1, 1, cell.Value);

        // Act
        cell.MergeWithCell(targetCell);

        // Assert
        targetCell.Value.Should().Be(2, "значение увеличивается на 1");
        cell.Value.Should().Be(0, "исходная клетка обнуляется");

        Object.DestroyImmediate(otherObject);
    }

    [Test]
    public void MoveToCell_ShouldTransferValueAndResetSource()
    {
        var targetObj = new GameObject("TargetCell");
        var targetCell = targetObj.AddComponent<Cell>();

        // Назначаем image/points
        var tImgGO = new GameObject("TargetImage");
        var tImg = tImgGO.AddComponent<Image>();
        typeof(Cell).GetField("image", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(targetCell, tImg);

        var tTxtGO = new GameObject("TargetPoints");
        var tTmp = tTxtGO.AddComponent<TextMeshProUGUI>();
        typeof(Cell).GetField("points", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(targetCell, tTmp);

        targetCell.SetValue(1, 1, 0);

        cell.MoveToCell(targetCell);

        targetCell.Value.Should().Be(1);
        cell.Value.Should().Be(0);

        Object.DestroyImmediate(targetObj);
    }

    [Test]
    public void IsEmpty_ShouldReturnTrueWhenValueIsZero()
    {
        cell.SetValue(0, 0, 0);
        cell.IsEmpty.Should().BeTrue();
    }
}
