using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum ParmType
{
    Integer,
    Number,
    String,
    Boolean,
    Enum
}

[CreateAssetMenu(fileName = "OpenAIFunctionDesctiption", menuName = "OpenAI/FunctionDescription", order = 1)]
public class OpenAITool : ScriptableObject
{
    [System.Serializable]
    public class ToolParameter
    {
        public string name;
        public string description;
        public ParmType type;
        public bool required;
        public string[] enumValues;
    }

    public string Name;
    public string Description;
    public List<ToolParameter> Parameters;

    public JObject ToJson()
    {
        JObject properties = new JObject();
        JArray requiredParameters = new JArray();

        foreach (var param in Parameters)
        {
            JObject propertyObject = new JObject
            {
                ["description"] = param.description
            };

            if (param.type == ParmType.Enum)
            {
                propertyObject["type"] = "string";
                propertyObject["enum"] = new JArray(param.enumValues);
            }
            else
            {
                propertyObject["type"] = param.type.ToString().ToLower();
            }

            properties[param.name] = propertyObject;

            if (param.required)
                requiredParameters.Add(param.name);
        }

        JObject parameters = new JObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = requiredParameters
        };

        JObject functionObject = new JObject
        {
            ["name"] = Name,
            ["description"] = Description,
            ["parameters"] = parameters
        };

        JObject toolObject = new JObject
        {
            ["type"] = "function",
            ["function"] = functionObject
        };

        return toolObject;
    }
}
