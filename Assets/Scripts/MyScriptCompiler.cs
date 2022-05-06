using RoslynCSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class MyScriptCompiler : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField updateField;
    [SerializeField]
    private TMP_InputField methodField;
    [SerializeField]
    private TMP_Dropdown variableDropdown;
    [SerializeField]
    private TMP_InputField newVariableName;
    [SerializeField]
    private TMP_InputField newVariableValue;
    [SerializeField]
    private TMP_Text variablesListText;

    [Header("Assembly references")]
    public List<AssemblyReferenceAsset> assemblyAssets;

    // Our script domain reference
    private ScriptDomain domain = null;
    private ScriptType type = null;
    private ScriptProxy proxy = null;

    List<MyVariable> variables;

    // Called by Unity
    void Start()
    {
        // Should we initialize the compiler?
        bool initCompiler = true;

        // Create the domain
        domain = ScriptDomain.CreateDomain("MyDomain",initCompiler);
        assemblyAssets.ForEach(asset =>
        {
            domain.RoslynCompilerService.ReferenceAssemblies.Add(asset);
        });


        variables = new List<MyVariable>();

        //Load dropdown
        variableDropdown.options.Clear();
        foreach (VariableType option in VariableType.GetValues())
        {
            variableDropdown.options.Add(new TMP_Dropdown.OptionData(option.ShowValue));
        }
        variableDropdown.value = 0;
        variableDropdown.RefreshShownValue();
    }

    private string GetVariableDefinition(MyVariable v)
    {
        string varValue = "";
        if(v.value.Length > 0)
        {
            if (v.type == VariableType._string)
            {
                varValue = "\""+v.value+"\"";
            } else
            {
                varValue = v.value;
            }
        }
        Debug.Log(v.type);
        return "private " + v.type.CodeValue + " " + v.name + " = "+varValue+";";
    }

    public void CompileCode()
    {
        var varString = string.Join("\n",variables.Select(v => GetVariableDefinition(v)));
        Debug.Log(varString);
        string source =
     "using UnityEngine;" +
     "class Test : MonoBehaviour" +
     "{" +
     varString+
     "private int counter = 0;" +
     " void Update()" +
     " {" + updateField.text +
     " }" +
     " void SomeMethod()" +
     " {" + methodField.text +
     " }" +
     "}";

        Debug.Log(source);
        type = domain.CompileAndLoadMainSourceInterpreted(source);
        if (proxy != null && !proxy.IsDisposed)
            proxy.Dispose();
        proxy = type.CreateInstance(gameObject);
        //proxy.Call("Update");
    }

    public void StopCode()
    {
        if (proxy != null && !proxy.IsDisposed)
        {
            proxy.Dispose();
            variablesListText.text = "Variables:\n" + string.Join("\n", variables.Select(v => v.name + ": " + v.value));
        }
    }

    public void CallSomeMethod()
    {
        if (proxy != null && !proxy.IsDisposed)
            proxy.Call("SomeMethod");
    }

    public void AddVariable()
    {
        if (newVariableName.text.Length > 1 && !variables.Select(v => v.name).Contains(newVariableName.text))
        {
            int optionIndex = variableDropdown.value;
            string option = variableDropdown.options[optionIndex].text;
            VariableType type = VariableType.GetByValue(option);

            variables.Add(new MyVariable(newVariableName.text, type, newVariableValue.text));
        }
        variablesListText.text = "Variables:\n" + string.Join("\n", variables.Select(v => v.name + ": " + v.value));
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            CompileCode();
        }

        if(proxy == null || proxy.IsDisposed)
        {
            return;
        }

        variablesListText.text = "Variables:\n" + string.Join("\n", variables.Select(v => v.name + ": " + proxy.Fields[v.name]));

    }
}
