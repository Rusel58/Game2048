using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using TMPro;
using System.Reflection;

public class GameControllerTests
{
    private GameObject controllerObject;
    private GameObject fieldObject;
    private GameObject colorManagerObject;
    private GameController controller;

    [SetUp]
    public void SetUp()
    {
        // 1) Создаём GameController
        controllerObject = new GameObject("GameController");
        controller = controllerObject.AddComponent<GameController>();
        GameController.Instance = controller;

        // 2) Создаём фиктивный GameField (GameField.Instance), чтобы StartGame() не падал
        fieldObject = new GameObject("GameField");
        var gameField = fieldObject.AddComponent<GameField>();
        GameField.Instance = gameField;

        // Задаём базовые параметры поля
        gameField.FieldSize = 4;
        gameField.CellSize = 100f;
        gameField.Spacing = 10f;
        gameField.InitCellsCount = 2;

        // Назначаем RectTransform
        var rt = fieldObject.AddComponent<RectTransform>();
        typeof(GameField)
            .GetField("rt", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameField, rt);

        // Создаём dummy-префаб для клеток
        var cellPrefabGO = new GameObject("CellPrefab");
        var cellComp = cellPrefabGO.AddComponent<Cell>();

        // Назначаем Image и TextMeshProUGUI для ячеек
        var dummyImageGO = new GameObject("DummyCellImage");
        var dummyImage = dummyImageGO.AddComponent<UnityEngine.UI.Image>();
        var dummyTextGO = new GameObject("DummyCellText");
        var dummyText = dummyTextGO.AddComponent<TextMeshProUGUI>();

        typeof(Cell).GetField("image", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(cellComp, dummyImage);
        typeof(Cell).GetField("points", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(cellComp, dummyText);

        // Назначаем cellPref в GameField
        typeof(GameField)
            .GetField("cellPref", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameField, cellComp);

        // 3) Создаём TextMeshPro для gameResult
        var resultGO = new GameObject("GameResultText");
        var resultTMP = resultGO.AddComponent<TextMeshProUGUI>();
        typeof(GameController)
            .GetField("gameResult", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(controller, resultTMP);

        // 4) Создаём TextMeshPro для pointsText
        var pointsGO = new GameObject("PointsText");
        var pointsTMP = pointsGO.AddComponent<TextMeshProUGUI>();
        typeof(GameController)
            .GetField("pointsText", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(controller, pointsTMP);

        // 5) Создаём ColorManager, чтобы Cell.UpdateCell() не падал
        colorManagerObject = new GameObject("ColorManager");
        var colorManager = colorManagerObject.AddComponent<ColorManager>();
        ColorManager.Instance = colorManager;

        // Инициализируем поля ColorManager
        colorManager.CellColors = new Color[12];     // минимум 12 для Value=0..11
        colorManager.PointsDarkColor = Color.black;
        colorManager.PointsLightColor = Color.white;

        // 6) Сбрасываем очки в 0, чтобы предыдущие тесты не влияли на текущий
        GameController.ResetPoints();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(controllerObject);
        Object.DestroyImmediate(fieldObject);
        Object.DestroyImmediate(colorManagerObject);

        GameController.Instance = null;
        GameField.Instance = null;
        ColorManager.Instance = null;
    }

    [Test]
    public void StartGame_ShouldResetPointsAndSetGameStarted()
    {
        // Проверяем, что Points = 0 перед стартом
        GameController.Points.Should().Be(0, "по умолчанию очки равны 0");

        // Делаем StartGame()
        controller.StartGame();

        // Убеждаемся, что очки обнулились и игра запущена
        GameController.Points.Should().Be(0, "после старта очки сбрасываются");
        GameController.GameStarted.Should().BeTrue();
    }

    [Test]
    public void AddPoints_ShouldIncreasePoints()
    {
        // Запускаем игру
        controller.StartGame();

        // Добавляем 10 очков
        controller.AddPoints(10);

        // Проверяем, что Points увеличились
        GameController.Points.Should().Be(10);
    }

    [Test]
    public void Win_ShouldSetGameResultAndStopGame()
    {
        controller.Win();
        GameController.GameStarted.Should().BeFalse();

        var gameResult = (TextMeshProUGUI)typeof(GameController)
            .GetField("gameResult", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(controller);

        gameResult.text.Should().Be("You win!");
    }

    [Test]
    public void Lose_ShouldSetGameResultAndStopGame()
    {
        controller.Lose();
        GameController.GameStarted.Should().BeFalse();

        var gameResult = (TextMeshProUGUI)typeof(GameController)
            .GetField("gameResult", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(controller);

        gameResult.text.Should().Be("You lose!");
    }
}
