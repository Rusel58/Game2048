using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System;

public class GameFieldTests
{
    private GameObject controllerObject;
    private GameObject colorManagerObject;
    private GameObject fieldObject;
    private GameController gameController;
    private ColorManager colorManager;
    private GameField gameField;

    [SetUp]
    public void SetUp()
    {
        // 1) GameController
        controllerObject = new GameObject("GameController");
        gameController = controllerObject.AddComponent<GameController>();
        GameController.Instance = gameController;

        // Чтобы не получить NullReference при AddPoints/SetPoints
        var pointsGO = new GameObject("PointsText");
        var pointsTMP = pointsGO.AddComponent<TextMeshProUGUI>();
        typeof(GameController)
            .GetField("pointsText", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameController, pointsTMP);

        var resultGO = new GameObject("GameResultText");
        var resultTMP = resultGO.AddComponent<TextMeshProUGUI>();
        typeof(GameController)
            .GetField("gameResult", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameController, resultTMP);

        // 2) ColorManager
        colorManagerObject = new GameObject("ColorManager");
        colorManager = colorManagerObject.AddComponent<ColorManager>();
        ColorManager.Instance = colorManager;
        colorManager.CellColors = new Color[12]; // Для Value=0..11
        colorManager.PointsDarkColor = Color.black;
        colorManager.PointsLightColor = Color.white;

        // 3) Создаём GameField
        fieldObject = new GameObject("GameField");
        gameField = fieldObject.AddComponent<GameField>();

        // Задаём базовые параметры
        gameField.FieldSize = 4;
        gameField.CellSize = 100f;
        gameField.Spacing = 10f;
        gameField.InitCellsCount = 2;

        // RectTransform
        var rt = fieldObject.AddComponent<RectTransform>();
        typeof(GameField)
            .GetField("rt", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameField, rt);

        // Префаб Cell
        var cellPrefabGO = new GameObject("CellPrefab");
        var cellComp = cellPrefabGO.AddComponent<Cell>();

        // Назначим image и points
        var cellImgGO = new GameObject("PrefabCellImage");
        var cellImg = cellImgGO.AddComponent<Image>();
        typeof(Cell).GetField("image", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(cellComp, cellImg);

        var cellTxtGO = new GameObject("PrefabCellPoints");
        var cellTmp = cellTxtGO.AddComponent<TextMeshProUGUI>();
        typeof(Cell).GetField("points", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(cellComp, cellTmp);

        typeof(GameField)
            .GetField("cellPref", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gameField, cellComp);
    }

    [TearDown]
    public void TearDown()
    {
        // Уничтожаем объекты, сбрасываем синглтоны
        UnityEngine.Object.DestroyImmediate(fieldObject);
        UnityEngine.Object.DestroyImmediate(controllerObject);
        UnityEngine.Object.DestroyImmediate(colorManagerObject);
        GameController.Instance = null;
        ColorManager.Instance = null;
    }

    [Test]
    public void GenerateField_ShouldCreateAllCells()
    {
        gameField.GenerateField();
        // Проверим, что реально создалось FieldSize * FieldSize клеток
        fieldObject.transform.childCount.Should().Be(4 * 4);
    }

    [Test]
    public void GenerateField_ShouldClearAllCellsAndCreateInitCells()
    {
        gameField.GenerateField();
        fieldObject.transform.childCount.Should().Be(16);

        int notEmpty = CountNotEmptyCells();
        notEmpty.Should().Be(gameField.InitCellsCount, "должно быть ровно InitCellsCount непустых клеток");
    }

    [Test]
    public void OnInput_WithNoGameStarted_ShouldDoNothing()
    {
        // Генерируем поле
        gameField.GenerateField();

        // Отключаем игру
        gameController.StopGame();

        int before = CountNotEmptyCells();
        gameField.OnInput(Vector2.up);
        int after = CountNotEmptyCells();

        after.Should().Be(before, "поле не должно меняться, если игра не запущена");
    }

    [Test]
    public void OnInput_ShouldMoveAndMergeCells()
    {
        GameController.SetGameStarted(true);
        gameField.GenerateField();

        // Очищаем поле для стабильного сценария
        var fieldArray = GetFieldArray();
        for (int x = 0; x < gameField.FieldSize; x++)
        {
            for (int y = 0; y < gameField.FieldSize; y++)
            {
                fieldArray[x, y].SetValue(x, y, 0);
            }
        }

        // Устанавливаем две клетки со значением 1 в правой части, например [2,0] и [3,0]
        fieldArray[2, 0].SetValue(2, 0, 1);
        fieldArray[3, 0].SetValue(3, 0, 1);

        // Вызываем движение вправо
        gameField.OnInput(Vector2.right);

        // Ожидаем, что клетка [2,0] станет пустой, а клетка [3,0] станет 2
        fieldArray[2, 0].Value.Should().Be(0);
        fieldArray[3, 0].Value.Should().Be(2);
    }


    [Test]
    public void OnInput_ShouldGenerateRandomCellIfAnyCellMoved()
    {
        // Запускаем игру
        GameController.SetGameStarted(true);
        gameField.GenerateField();

        // Очищаем поле: устанавливаем все клетки в 0
        var fieldArray = GetFieldArray();
        for (int x = 0; x < gameField.FieldSize; x++)
        {
            for (int y = 0; y < gameField.FieldSize; y++)
            {
                fieldArray[x, y].SetValue(x, y, 0);
            }
        }

        // Устанавливаем ровно одну клетку ненулевой, например в [0,0]
        fieldArray[0, 0].SetValue(0, 0, 1);
        int before = CountNotEmptyCells(); // Должно быть 1

        // Вызываем OnInput() – направление выбираем так, чтобы клетка точно переместилась
        gameField.OnInput(Vector2.down);

        int after = CountNotEmptyCells();
        // Если движение произошло, то GenerateRandomCell() добавляет ровно 1 новую клетку,
        // и итоговое число должно быть 2.
        after.Should().Be(before + 1, "должна добавиться одна новая клетка");
    }

    [Test]
    public void CheckGameResult_ShouldTriggerWinIfCellReachedMaxValue()
    {
        GameController.SetGameStarted(true);
        gameField.GenerateField();

        // Принудительно ставим клетке Value = Cell.MaxValue (обычно 11)
        var fieldArray = GetFieldArray();
        fieldArray[0, 0].SetValue(0, 0, Cell.MaxValue);

        // Вызываем OnInput (или напрямую CheckGameResult через Reflect) 
        // чтобы игра проверила победу
        gameField.OnInput(Vector2.left);

        // Должен вызваться GameController.Instance.Win()
        GameController.GameStarted.Should().BeFalse("игра должна остановиться при победе");
    }

    [Test]
    public void CheckGameResult_ShouldTriggerLoseIfNoMovesLeft()
    {
        GameController.SetGameStarted(true);
        gameField.GenerateField();

        // Полностью заполняем поле чередующимися значениями (1,2), чтобы не было ходов
        FillNoMovesLeftBoard();

        // Любой ход теперь не изменит поле, значит => lose
        gameField.OnInput(Vector2.up);

        // Проверяем, что GameController.GameStarted = false => lose
        GameController.GameStarted.Should().BeFalse("должно быть поражение, если нет ходов");
    }

    [Test]
    public void GenerateRandomCell_ShouldThrowExceptionIfNoEmptyCell()
    {
        gameField.GenerateField();
        FillNoMovesLeftBoard(); // Полностью заполненное поле

        Action act = () =>
        {
            var method = typeof(GameField).GetMethod("GenerateRandomCell", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(gameField, null);
        };

        act.Should().Throw<TargetInvocationException>()
           .WithInnerException<Exception>()
           .WithMessage("There is no any empty cell!");
    }

    private int CountNotEmptyCells()
    {
        int count = 0;
        var fieldArray = GetFieldArray();
        for (int x = 0; x < gameField.FieldSize; x++)
        {
            for (int y = 0; y < gameField.FieldSize; y++)
            {
                if (!fieldArray[x, y].IsEmpty) count++;
            }
        }
        return count;
    }

    private Cell[,] GetFieldArray()
    {
        // Достаём приватное поле "field" (Cell[,]) через Reflection
        return (Cell[,])typeof(GameField)
            .GetField("field", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(gameField);
    }

    private void FillNoMovesLeftBoard()
    {
        var fieldArray = GetFieldArray();
        int size = gameField.FieldSize;
        // Заполним чередующимися 1 и 2, чтобы не было слияний
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int val = ((x + y) % 2 == 0) ? 1 : 2;
                fieldArray[x, y].SetValue(x, y, val);
            }
        }
    }
}
