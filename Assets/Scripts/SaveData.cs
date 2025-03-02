[System.Serializable]
public class SaveData
{
    // Текущий лучший счёт (рекорд)
    public int bestScore;
    
    // Размер поля (FieldSize x FieldSize), 
    // чтобы знать, как интерпретировать массив.
    public int fieldSize;
    
    // Двумерный массив значений клеток.
    // В бинарном файле будем хранить как одномерный,
    // а при загрузке снова превращать в 2D.
    public int[] cells;

    // Текущие очки
    public int points;
}
