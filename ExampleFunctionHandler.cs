using System;
using Newtonsoft.Json.Linq;
using player2_sdk;
using UnityEngine;


[Serializable]
public class ExampleFunctionHandler: MonoBehaviour
{
    public void HandleFunctionCall(FunctionCall functionCall)
    {
        Debug.Log($"Handling function call: {functionCall.name}");
        if (functionCall.name == "Follow")
        {
            if (functionCall.aiObject != null)
            {
                Debug.Log("ferret");
            }

            var args = (JObject)functionCall.arguments;
            string name = (string)args["name"];
            Debug.Log(name);
            Debug.Log("otter");
            //we need to check if we can actually see the target. 
            var aImovement = functionCall.aiObject.GetComponent<AImovement>();
            var hits = Physics2D.OverlapCircleAll(functionCall.aiObject.transform.position, 10f); // ideally pass a LayerMask

            foreach (var col in hits) // col is a Collider2D (a Component), so it has TryGetComponent
            {
                Debug.Log("trying "+ col.gameObject.name);
                if (col.transform == transform) continue; // skip self if needed
                
                if (col.TryGetComponent<Stats>(out var stats))
                {
                    Debug.Log("found target");
                    if (stats.name == name)
                    {
                        aImovement.follow(col.gameObject.transform);
                        break;
                    }
                }
            }
        }
  
        
            // Example: Just log the arguments
            foreach (var arg in functionCall.arguments)
            {
                Debug.Log($"Argument: {arg.Key} = {arg.Value}");
            }
        
        //optional: use npcObject to access the name of the NPC that called the function. prints the name of the gameobject.
        try
        {
           Debug.Log(functionCall.aiObject.name);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       

            // Here you would implement your actual function logic
            // For example, if this is a chat function, you might send a message to the NPC
        }
    }


