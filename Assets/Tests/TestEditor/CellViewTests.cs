using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class CellViewTests
{
    private GameObject controllerObject;
    private GameObject colorManagerObject;

    private GameObject viewObject;
    private CellView cellView;
    private Cell cell;

    [SetUp]
    public void SetUp()
    {
        // 1) Создаём GameController
        controllerObject = new GameObject("GameController");
        var gc = controllerObject.AddComponent<GameController>();
        GameController.Instance = gc;

        // 2) Создаём ColorManager
        colorManagerObject = new GameObject("ColorManager");
        var cm = colorManagerObject.AddComponent<ColorManager>();
        ColorManager.Instance = cm;
        cm.CellColors = new Color[12];

        // 3) Создаём CellView
        viewObject = new GameObject("CellView");
        cellView = viewObject.AddComponent<CellView>();

        // 4) Создаём TMP и Image для CellView
        var textGO = new GameObject("ValueText");
        textGO.transform.SetParent(viewObject.transform);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();

        var imageGO = new GameObject("CellImage");
        imageGO.transform.SetParent(viewObject.transform);
        var img = imageGO.AddComponent<Image>();

        // Назначаем приватные поля через Reflection
        typeof(CellView).GetField("valueText", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(cellView, tmp);
        typeof(CellView).GetField("cellImage", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(cellView, img);

        // 5) Создаём Cell
        var cellObj = new GameObject("TestCell");
        cell = cellObj.AddComponent<Cell>();

        // Назначаем image и points в Cell
        var cellImgGO = new GameObject("CellImageInCell");
        var cellImg = cellImgGO.AddComponent<Image>();
        typeof(Cell).GetField("image", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(cell, cellImg);

        var cellTextGO = new GameObject("PointsTextInCell");
        var cellTmp = cellTextGO.AddComponent<TextMeshProUGUI>();
        typeof(Cell).GetField("points", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(cell, cellTmp);

        cell.SetValue(0, 0, 1); // 2^1 = 2
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(viewObject);
        GameController.Instance = null;
        ColorManager.Instance = null;
        Object.DestroyImmediate(controllerObject);
        Object.DestroyImmediate(colorManagerObject);
    }

    [Test]
    public void Init_ShouldInitializeAndSetValueAndPosition()
    {
        // Act
        cellView.Init(cell);

        // Assert
        var tmp = (TextMeshProUGUI)typeof(CellView)
            .GetField("valueText", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(cellView);

        tmp.text.Should().Be("2", "Value=1 => отображаем 2^1 = 2");

        // Проверяем позицию
        cellView.transform.localPosition.x.Should().Be(cell.X);
        cellView.transform.localPosition.y.Should().Be(cell.Y);
    }

    [Test]
    public void UpdateValue_ShouldDisplayPowerOfTwoOrEmpty()
    {
        cellView.Init(cell);
        // Изначально Value=1 => "2"

        // Меняем значение на 0 => пустая клетка
        cell.SetValue(0, 0, 0);

        var tmp = (TextMeshProUGUI)typeof(CellView)
            .GetField("valueText", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(cellView);

        tmp.text.Should().BeEmpty("ячейка пустая => текст пустой");
    }
}
