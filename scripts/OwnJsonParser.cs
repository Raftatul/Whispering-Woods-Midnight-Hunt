using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


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

        //godot dictionary
        Godot.Collections.Dictionary<string, string> godotDictionary = new Godot.Collections.Dictionary<string, string>();
        foreach (var item in data)
        {
            godotDictionary.Add(item.Key, item.Value);
        }
        GD.Print(Json.Stringify(godotDictionary));
        return Json.Stringify(godotDictionary);
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

        Godot.Collections.Dictionary<string, string> godotDictionary;
        Json res = new Json();
        res.Parse(json);

        godotDictionary = res.Data.AsGodotDictionary<string, string>();
        GD.Print(godotDictionary);
        Dictionary<string, string> godotDictionary2 = new Dictionary<string, string>();
        foreach (var item in godotDictionary)
        {
            godotDictionary2.Add(item.Key, item.Value);
        }
        return godotDictionary2;
    }
}