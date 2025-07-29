namespace player2_sdk
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ExampleFunctionHandler: MonoBehaviour
    {
        public void HandleFunctionCall(FunctionCall functionCall)
        {
            Debug.Log($"Handling function call: {functionCall.name}");
        
            // Example: Just log the arguments
            foreach (var arg in functionCall.arguments)
            {
                Debug.Log($"Argument: {arg.Key} = {arg.Value}");
            }
        
            //optional: use npcObject to access the name of the NPC that called the function. prints the name of the gameobject.
            // try
            // {
            //    Debug.Log(functionCall.aiObject.name);
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine(e);
            //     throw;
            // }
        

            // Here you would implement your actual function logic
            // For example, if this is a chat function, you might send a message to the NPC
        }
    }
}
