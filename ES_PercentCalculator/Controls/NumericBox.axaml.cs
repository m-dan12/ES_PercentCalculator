using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using System;
using System.Reactive.Linq;

namespace ES_PercentCalculator;

public class NumericBox : NumericUpDown
{
    public NumericBox()
    {
        this.AddHandler(TextBox.LostFocusEvent, OnTextBoxLostFocus, RoutingStrategies.Bubble);
    }

    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(this.Text))
        {
            Value = Minimum; // Установим значение по умолчанию, если текст пустой
        }
    }

    protected override void OnTextChanged(string? oldText, string? newText)
    {
        if (!string.IsNullOrEmpty(newText))
            if (decimal.TryParse(newText, out decimal newValue))
                Value = newValue;
        base.OnTextChanged(oldText, newText); // Вызовем базовую реализацию для обработки нормального ввода
    }
}