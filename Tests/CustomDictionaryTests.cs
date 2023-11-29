namespace DotnetProject.Tests;
using System;
using System.Collections.Generic;
using Xunit;
using CustomDictionaryLibrary;

public class CustomDictionaryTests
{
    [Fact]
    public void AddItem_ShouldIncreaseCount()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();

        dictionary.Add(1, "One");

        Assert.Equal(1, dictionary.Count);
    }

    [Fact]
    public void AddItem_ShouldRaiseItemAddedEvent()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();
        bool eventRaised = false;
        dictionary.ItemAdded += (sender, args) => eventRaised = true;

        dictionary.Add(1, "One");

        Assert.True(eventRaised);
    }

    [Fact]
    public void GetValueByKey_ShouldReturnCorrectValue()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();
        dictionary.Add(1, "One");
        dictionary.Add(2, "Two");

        string value = dictionary[1];

        Assert.Equal("One", value);
    }

    [Fact]
    public void GetValueByInvalidKey_ShouldThrowKeyNotFoundException()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();

        Assert.Throws<KeyNotFoundException>(() => { var value = dictionary[1]; });
    }

    [Fact]
    public void RemoveItem_ShouldDecreaseCount()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();
        dictionary.Add(1, "One");

        dictionary.Remove(1);

        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void RemoveItem_ShouldReturnTrueIfRemoved()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();
        dictionary.Add(1, "One");

        bool result = dictionary.Remove(1);

        Assert.True(result);
    }

    [Fact]
    public void RemoveItem_ShouldReturnFalseIfKeyNotFound()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();

        bool result = dictionary.Remove(1);

        Assert.False(result);
    }

    [Fact]
    public void ContainsKey_ShouldReturnTrueIfKeyExists()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();
        dictionary.Add(1, "One");

        bool result = dictionary.ContainsKey(1);

        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_ShouldReturnFalseIfKeyNotExists()
    {
        CustomDictionary<int, string> dictionary = new CustomDictionary<int, string>();

        bool result = dictionary.ContainsKey(1);

        Assert.False(result);
    }
}
