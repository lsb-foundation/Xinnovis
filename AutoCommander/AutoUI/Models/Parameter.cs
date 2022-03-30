using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoCommander.AutoUI.Models;

public class Parameter : IAutoBuild<DependencyObject[]>
{
    #region Fields
    [XmlIgnore]
    private DependencyObject _control;
    [XmlIgnore]
    private string _value;
    #endregion

    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Description { get; set; }

    [XmlAttribute]
    public string Type { get; set; }    //int,float,string,excel,select

    [XmlAttribute]
    public string DefaultValue { get; set; }

    [XmlAttribute]
    public string DataRange { get; set; }  //读取文件的范围

    [XmlAttribute]
    public string Seperator { get; set; }   //读物文件使用的分割符

    [XmlIgnore]
    public string Value
    {
        get => _value;
        set => _value = value;
    }

    [XmlArray]
    public List<Option> Options { get; set; }

    [XmlIgnore]
    public Command Parent { get; set; }

    public DependencyObject[] Build()
    {
        Label label = new() { Content = Description };
        if (Type.Trim().ToLower() == "select")
        {
            _control = CreateSelectBox();
        }
        else
        {
            _control = CreateTextBox();
            Value = DefaultValue;
        }

        return new DependencyObject[] { label, _control };
    }

    public void SetTextBox(string value)
    {
        if (_control is not TextBox tbox) return;
        Value = value;
        tbox.Text = value;
    }

    private TextBox CreateTextBox()
    {
        TextBox textBox = new()
        {
            Text = DefaultValue,
            Tag = this
        };
        textBox.TextChanged += TextBox_TextChanged;
        return textBox;
    }

    private ComboBox CreateSelectBox()
    {
        ComboBox comboBox = new()
        {
            DisplayMemberPath = "Name",
            SelectedValuePath = "Value",
            Tag = this
        };
        comboBox.SelectionChanged += ComboBox_SelectionChanged;

        if (Options == null) return comboBox;

        foreach (var option in Options)
        {
            comboBox.Items.Add(option);
            if (option.Value == DefaultValue)
            {
                comboBox.SelectedItem = option;
            }
        }
        return comboBox;
    }

    public bool TryParse(out object value)
    {
        bool canParse;
        switch (Type.Trim().ToLower())
        {
            case "int":
                canParse = int.TryParse(Value, out int v);
                value = v;
                break;
            case "float":
                canParse = float.TryParse(Value, out float f);
                value = f;
                break;
            case "excel":
            case "select":
            case "string":
                canParse = true;
                value = Value;
                break;
            default:
                canParse = false;
                value = null;
                break;
        }
        return canParse;
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { Text: string text, Tag: Parameter parameter })
        {
            parameter.Value = text;
        }
        e.Handled = true;
    }

    private static void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: Option opt, Tag: Parameter parameter })
        {
            parameter.Value = opt.Value;
        }
        e.Handled = true;
    }
}
