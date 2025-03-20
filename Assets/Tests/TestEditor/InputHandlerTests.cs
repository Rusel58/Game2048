using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using System.Reflection;

public class InputHandlerTests
{
    private GameObject handlerObject;
    private InputHandler inputHandler;

    private MethodInfo convertMethod;
    private MethodInfo updateMethod;

    [SetUp]
    public void SetUp()
    {
        // Создаём объект и добавляем InputHandler
        handlerObject = new GameObject("InputHandler");
        inputHandler = handlerObject.AddComponent<InputHandler>();

        // Получаем приватный метод ConvertToCardinalDirection
        convertMethod = typeof(InputHandler)
            .GetMethod("ConvertToCardinalDirection", BindingFlags.NonPublic | BindingFlags.Instance);

        // Дополнительно получаем ссылку на Update() через Reflection
        // (Если вы хотите просто «вызвать» Update в тесте.)
        updateMethod = typeof(InputHandler)
            .GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        // Уничтожаем созданный объект
        Object.DestroyImmediate(handlerObject);
    }

    // Тестируем ConvertToCardinalDirection на 4 направления
    [TestCase(1f, 0.5f, 1f, 0f, TestName = "Right")]
    [TestCase(-0.6f, 0.2f, -1f, 0f, TestName = "Left")]
    [TestCase(0.1f, 1f, 0f, 1f, TestName = "Up")]
    [TestCase(0.2f, -0.9f, 0f, -1f, TestName = "Down")]
    public void ConvertToCardinalDirection_ShouldReturnCorrectAxis(
        float inputX, float inputY, float expectedX, float expectedY)
    {
        // Act
        var inputVector = new Vector2(inputX, inputY);
        var result = (Vector2)convertMethod.Invoke(inputHandler, new object[] { inputVector });

        // Assert
        result.x.Should().BeApproximately(expectedX, 0.001f);
        result.y.Should().BeApproximately(expectedY, 0.001f);
    }

    // Пример теста, который вызывает Update() в EditMode, 
    // чтобы убедиться, что метод не выбрасывает исключений.
    // (Он не проверяет реальное поведение Input, так как замокать старый Input сложно.)
    [Test]
    public void Update_ShouldNotThrowExceptions_WhenCalledInEditMode()
    {
        // Принудительно вызываем приватный Update через Reflection
        // Проверяем, что не возникает исключений
        updateMethod.Invoke(inputHandler, null);

        // Если дошли до этого места — значит исключений не было
        Assert.Pass("Update() called without throwing exceptions.");
    }
}
