using UnityEngine;
using System.Collections.Generic;

public class GameField : MonoBehaviour
{
    public static GameField Instance;

    [Header("Field Properties")]
    public float CellSize; // Размер клетки
    public float Spacing;  // Расстояние между клетками
    public int FieldSize;  // Размер поля (FieldSize x FieldSize)
    public int InitCellsCount; // Количество клеток, которые создаются при старте игры

    [Space(10)]
    [SerializeField]
    private Cell cellPref; // Префаб клетки
    [SerializeField]
    private RectTransform rt; // RectTransform игрового поля

    private Cell[,] field; // Двумерный массив для хранения клеток

    private bool anyCellMoved; // Флаг, указывающий, были ли клетки перемещены

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        GenerateField();
    }

    // Создание игрового поля
    private void CreateField()
    {
        field = new Cell[FieldSize, FieldSize];

        // Вычисление ширины поля
        float fieldWidth = FieldSize * (CellSize + Spacing) + Spacing;
        rt.sizeDelta = new Vector2(fieldWidth, fieldWidth);

        // Начальные координаты для первой клетки
        float startX = -(fieldWidth / 2) + (CellSize / 2) + Spacing;
        float startY = (fieldWidth / 2) - (CellSize / 2) - Spacing;

        // Создание клеток на поле
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                var cell = Instantiate(cellPref, transform, false);
                var position = new Vector2(startX + (x * (CellSize + Spacing)), startY - (y * (CellSize + Spacing)));
                cell.transform.localPosition = position;

                field[x, y] = cell;

                // Инициализация клетки с нулевым значением
                cell.SetValue(x, y, 0);
            }
        }
    }

    // Генерация поля (создание поля и начальных клеток)
    public void GenerateField()
    {
        if (field == null)
            CreateField();

        // Очистка всех клеток
        for (int x = 0; x < FieldSize; x++)
            for (int y = 0; y < FieldSize; y++)
                field[x, y].SetValue(x, y, 0);

        // Создание начальных клеток
        for (int i = 0; i < InitCellsCount; i++)
            CreateCell();
    }

    // Генерация случайной клетки на поле во время игры(20% на 4)
    private void GenerateRandomCell()
    {
        var emptyCells = new List<Cell>();

        // Поиск всех пустых клеток
        for (int x = 0; x < FieldSize; x++)
            for (int y = 0; y < FieldSize; y++)
                if (field[x, y].IsEmpty)
                    emptyCells.Add(field[x, y]);

        if (emptyCells.Count == 0)
            throw new System.Exception("There is no any empty cell!");

        // 20% вероятность для значения 4, 80% для значения 2
        int value = Random.Range(0, 100) < 20 ? 2 : 1;

        // Выбор случайной пустой клетки и установка значения
        var cell = emptyCells[Random.Range(0, emptyCells.Count)];
        cell.SetValue(cell.X, cell.Y, value);
    }

    // Получение позиции случайной пустой клетки
    public Vector2 GetEmptyPosition()
    {
        var emptyCells = new List<Vector2>();

        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (field[x, y].IsEmpty)
                {
                    emptyCells.Add(new Vector2(x, y));
                }
            }
        }

        if (emptyCells.Count == 0)
        {
            throw new System.Exception("There is no any empty cell!");
        }

        return emptyCells[Random.Range(0, emptyCells.Count)];
    }

    // Генерация случайной клетки на поле в начале игры(вероятность 10% на 4)
    public void CreateCell()
    {
        var position = GetEmptyPosition();
        int value = Random.Range(0, 10) == 0 ? 2 : 1;

        var cell = field[(int)position.x, (int)position.y];
        cell.SetValue((int)position.x, (int)position.y, value);
    }

    // Обработка ввода (движение клеток)
    public void OnInput(Vector2 direction)
    {
        if (!GameController.GameStarted)
        {
            return;
        }

        anyCellMoved = false;
        ResetCellsFlags();

        Move(direction);

        if (anyCellMoved)
        {
            GenerateRandomCell();
            CheckGameResult();
        }
    }

    // Движение клеток в указанном направлении
    private void Move(Vector2 direction)
    {
        int startXY = direction.x > 0 || direction.y < 0 ? FieldSize - 1 : 0;
        int dir = direction.x != 0 ? (int)direction.x : -(int)direction.y;

        for (int i = 0; i < FieldSize; i++)
        {
            for (int k = startXY; k >= 0 && k < FieldSize; k -= dir)
            {
                var cell = direction.x != 0 ? field[k, i] : field[i, k];

                if (cell.IsEmpty) continue;

                var cellToMerge = FindCellToMerge(cell, direction);
                if (cellToMerge != null)
                {
                    cell.MergeWithCell(cellToMerge);
                    anyCellMoved = true;

                    continue;
                }

                var emptyCell = FindEmptyCell(cell, direction);
                if (emptyCell != null)
                {
                    cell.MoveToCell(emptyCell);
                    anyCellMoved = true;
                }
            }
        }
    }

    // Поиск клетки для слияния
    private Cell FindCellToMerge(Cell cell, Vector2 direction)
    {
        int startX = cell.X + (int)direction.x;
        int startY = cell.Y - (int)direction.y;

        for (int x = startX, y = startY; x >= 0 && x < FieldSize && y >= 0 &&
            y < FieldSize; x += (int)direction.x, y -= (int)direction.y)
        {
            if (field[x, y].IsEmpty)
                continue;
            if (field[x, y].Value == cell.Value && !field[x, y].HasMerged)
                return field[x, y];
            break;
        }
        return null;
    }

    // Поиск пустой клетки для перемещения
    private Cell FindEmptyCell(Cell cell, Vector2 direction)
    {
        Cell emptyCell = null;
        int startX = cell.X + (int)direction.x;
        int startY = cell.Y - (int)direction.y;
        for (int x = startX, y = startY; x >= 0 && x < FieldSize && y >= 0 &&
            y < FieldSize; x += (int)direction.x, y -= (int)direction.y)
        {
            if (field[x, y].IsEmpty)
                emptyCell = field[x, y];
            else
                break;
        }
        return emptyCell;
    }

    // Проверка результата игры (победа или поражение)
    private void CheckGameResult()
    {
        bool lose = true;
        for (int x = 0; x < FieldSize; x++)
        {
            for (int y = 0; y < FieldSize; y++)
            {
                if (field[x, y].Value == Cell.MaxValue)
                {
                    GameController.Instance.Win();
                    return;
                }

                if (lose && field[x, y].IsEmpty ||
                    FindCellToMerge(field[x, y], Vector2.left) ||
                    FindCellToMerge(field[x, y], Vector2.right) ||
                    FindCellToMerge(field[x, y], Vector2.up) ||
                    FindCellToMerge(field[x, y], Vector2.down))
                {
                    lose = false;
                }
            }
        }
        if (lose)
            GameController.Instance.Lose();
    }

    // Сброс флагов слияния у всех клеток
    private void ResetCellsFlags()
    {
        for (int x = 0; x < FieldSize; x++)
            for (int y = 0; y < FieldSize; y++)
                field[x, y].ResetFlags();
    }

    //// Получаем текущие значения всех клеток в одномерном массиве
    //public int[] GetAllCellValues()
    //{
    //    int[] cellsFlat = new int[FieldSize * FieldSize];
    //    int index = 0;
    //    for (int x = 0; x < FieldSize; x++)
    //    {
    //        for (int y = 0; y < FieldSize; y++)
    //        {
    //            cellsFlat[index] = field[x, y].Value;
    //            index++;
    //        }
    //    }
    //    return cellsFlat;
    //}

    //// Восстанавливаем значения клеток из массива
    //public void SetAllCellValues(int[] cellsFlat)
    //{
    //    if (cellsFlat == null || cellsFlat.Length != FieldSize * FieldSize) return;

    //    int index = 0;
    //    for (int x = 0; x < FieldSize; x++)
    //    {
    //        for (int y = 0; y < FieldSize; y++)
    //        {
    //            field[x, y].SetValue(x, y, cellsFlat[index]);
    //            index++;
    //        }
    //    }
    //}

}