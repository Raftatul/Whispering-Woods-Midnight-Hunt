using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class DataParser
{
    public static Action<Dictionary<string, string>> OnChatMessageReceived;


   public static Dictionary<string, string> ParseData(IntPtr data, int size)
   {
        byte[] managedArray = new byte[size];
        Marshal.Copy(data, managedArray, 0, size);
        var str = System.Text.Encoding.Default.GetString(managedArray);
        return OwnJsonParser.Deserialize(str);
   }

   public static void ProcessData(IntPtr data, int size)
   {
        var dataDictionnary = ParseData(data, size);
        switch (dataDictionnary["DataType"])
        {
            case "ChatMessage":
                OnChatMessageReceived.Invoke(dataDictionnary);
                break;
            default:
                break;
        }
   }

}