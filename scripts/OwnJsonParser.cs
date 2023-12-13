using System;
using System.Collections.Generic;


public class OwnJsonParser
{
    public static string Serialize(Dictionary<string, string> data)
    {
        string json = "{";
        foreach (var item in data)
        {
            json += "\"" + item.Key + "\":\"" + item.Value + "\",";
        }
        json = json.Remove(json.Length - 1);
        json += "}";
        return json;
    }

    public static Dictionary<string, string> Deserialize(string json)
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        json = json.Replace("{", "");
        json = json.Replace("}", "");
        string[] jsonSplit = json.Split(',');
        foreach (var item in jsonSplit)
        {
            string[] itemSplit = item.Split(':');
            data.Add(itemSplit[0].Replace("\"", ""), itemSplit[1].Replace("\"", ""));
        }
        return data;
    }
}