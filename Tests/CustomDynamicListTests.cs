namespace DotnetProject.Tests;
using System;
using Xunit;
using CustomDynamicListLibrary;
public class CustomDynamicListTests
{
    [Fact]
    public void Add_ShouldIncreaseCountAndRaiseEvent()
    {
        // Arrange
        var customList = new CustomDynamicList<int>();
        int itemAddedCount = 0;

        customList.ItemAdded += (sender, e) =>
        {
            itemAddedCount++;
            Assert.Equal(42, e.Item);
        };

        // Act
        customList.Add(42);

        // Assert
        Assert.Equal(1, customList.Count);
        Assert.Equal(1, itemAddedCount);
    }

    [Fact]
    public void Add_MultipleItems_ShouldResizeArray()
    {
        // Arrange
        var customList = new CustomDynamicList<int>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            customList.Add(i);
        }

        // Assert
        Assert.Equal(10, customList.Count);
        Assert.Equal(9, customList[9]);
    }

    [Fact]
    public void RemoveAt_ShouldRemoveItemAtSpecifiedIndex()
    {
        // Arrange
        var customList = new CustomDynamicList<int> { 10, 20, 30, 40 };

        // Act
        customList.RemoveAt(1);

        // Assert
        Assert.Equal(3, customList.Count);
        Assert.Equal(10, customList[0]);
        Assert.Equal(30, customList[1]);
        Assert.Equal(40, customList[2]);
    }

    [Fact]
    public void RemoveAt_InvalidIndex_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var customList = new CustomDynamicList<int>();

        // Act/Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => customList.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => customList.RemoveAt(0));
    }

    // Add more tests for other methods as needed
}
