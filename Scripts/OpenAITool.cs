using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

public enum parmType
{
    Integer,
    Number,
    String,
    Boolean,
}


public class OpenAITool : ScriptableObject
{
    [System.Serializable]
    public class ToolParameter
    {
        public string name;
        public string description;
        public parmType type;
        public bool required;
    }

    public string Name;
    public string Description;
    public List<ToolParameter> Parameters;

    public JObject ToJson()
    {
        // Create the properties and required parameters objects
        JObject properties = new JObject();
        JArray requiredParameters = new JArray();

        foreach (var param in Parameters)
        {
            properties[param.name] = new JObject
            {
                ["type"] = param.type.ToString().ToLower(),
                ["description"] = param.description
            };
            if (param.required)
                requiredParameters.Add(param.name);
        }

        // Construct the parameters object
        JObject parameters = new JObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = requiredParameters
        };

        // Construct the function object
        JObject functionObject = new JObject
        {
            ["name"] = Name,
            ["description"] = Description,
            ["parameters"] = parameters
        };

        // Construct the final tool object
        JObject toolObject = new JObject
        {
            ["type"] = "function",
            ["function"] = functionObject
        };

        return toolObject;
    }


}
