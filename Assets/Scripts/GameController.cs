using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static int Points { get; private set; }
    public static bool GameStarted { get; private set; }
    private int bestScore; // Переменная для рекорда

    [SerializeField]
    private TextMeshProUGUI gameResult;
    [SerializeField]
    private TextMeshProUGUI pointsText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
            Instance = this;

        StartGame();
        //// Пытаемся загрузить из файла
        //var loadedData = SaveSystem.LoadGame();
        //if (loadedData != null)
        //{
        //    if (bestScore != loadedData.bestScore)
        //    {

        //        // Устанавливаем лучший счёт из файла
        //        bestScore = loadedData.bestScore;

        //        // Если размер поля совпадает с сохранённым – восстанавливаем
        //        if (loadedData.fieldSize == GameField.Instance.FieldSize)
        //        {
        //            GameField.Instance.GenerateField();
        //            GameField.Instance.SetAllCellValues(loadedData.cells);
        //        }
        //        else
        //        {
        //            // Если размер не совпал – создаём новое поле
        //            GameField.Instance.GenerateField();
        //        }
        //        GameStarted = true;
        //    }
        //}
        //else
        //{
        //    StartGame();
        //}
    }

    //void OnApplicationQuit()
    //{
    //    Debug.Log("Saving game before exit...");
    //    SaveData data = new SaveData
    //    {
    //        bestScore = bestScore,
    //        fieldSize = GameField.Instance.FieldSize,
    //        cells = GameField.Instance.GetAllCellValues(),
    //        points = Points // Сохраняем текущие очки
    //    };

    //    SaveSystem.SaveGame(data);
    //}

    public void Win()
    {
        GameStarted = false;
        gameResult.text = "You win!";

        //// Обновляем рекорд
        //if (Points > bestScore)
        //    bestScore = Points;

        //SaveData data = new SaveData
        //{
        //    bestScore = bestScore,
        //    fieldSize = GameField.Instance.FieldSize,
        //    cells = GameField.Instance.GetAllCellValues()
        //};
        //SaveSystem.SaveGame(data);
    }

    public void Lose()
    {
        GameStarted = false;
        gameResult.text = "You lose!";

    //    // Сравниваем текущие очки с рекордом
    //    if (Points > bestScore)
    //    {
    //        bestScore = Points;
    //    }

    //    // Формируем SaveData и сохраняем
    //    SaveData data = new SaveData
    //    {
    //        bestScore = bestScore,
    //        fieldSize = GameField.Instance.FieldSize,
    //        cells = GameField.Instance.GetAllCellValues()
    //    };
    //    SaveSystem.SaveGame(data);
    }

    public void StartGame()
    {
        gameResult.text = "";

        SetPoints(0);
        GameStarted = true;

        GameField.Instance.GenerateField();
    }

    public void AddPoints(int points)
    {
        SetPoints(Points + points);
    }

    private void SetPoints(int points)
    {
        Points = points;
        pointsText.text = Points.ToString();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
