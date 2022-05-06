using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableType : StringEnum
{
    private VariableType(string value, string codeValue) : base(value,codeValue) { }

    public static readonly VariableType _string = new("string","string");
    public static readonly VariableType _int = new("integer","int");
    public static readonly VariableType _float = new("float","float");

    public static VariableType GetByValue(string value)
    {
        switch(value) {
            case "string": return _string;
            case "integer": return _int;
            case "float": return _float;
            default: return null;
        }
    }

    public static List<VariableType> GetValues()
    {
        return new List<VariableType>() { _string, _int, _float };
    }
}

public class StringEnum
{
    protected StringEnum(string value, string codeValue) { ShowValue = value; CodeValue = codeValue; }
    public string ShowValue { get; }
    public string CodeValue { get; }
    public override string ToString() => ShowValue;
}

public class MyVariable
{
    public MyVariable(string name, VariableType type, string value)
    {
        this.name = name;
        this.type = type;
        this.value = value;
    }

    public string name;
    public VariableType type;
    public string value;
}
