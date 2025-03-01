using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static int Points { get; private set; }
    public static bool GameStarted { get; private set; }

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
        StartGame();
    }

    public void Win()
    {
        GameStarted = false;
        gameResult.text = "You win!";
    }

    public void Lose()
    {
        GameStarted = false;
        gameResult.text = "You lose!";
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
