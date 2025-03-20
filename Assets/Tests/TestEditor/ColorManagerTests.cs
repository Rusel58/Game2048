using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using System.Reflection;

public class ColorManagerTests
{
    private GameObject colorManagerGO;
    private ColorManager colorManager;

    [SetUp]
    public void SetUp()
    {
        colorManagerGO = new GameObject("ColorManager");
        colorManager = colorManagerGO.AddComponent<ColorManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(colorManagerGO);
        ColorManager.Instance = null;
    }

    [Test]
    public void Awake_ShouldSetInstance()
    {
        ColorManager.Instance.Should().Be(colorManager);
    }

    [Test]
    public void CellColors_ShouldBeAccessible()
    {
        colorManager.CellColors = new Color[12];
        colorManager.CellColors.Length.Should().Be(12);
    }
}
