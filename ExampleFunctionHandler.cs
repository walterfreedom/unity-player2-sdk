using System;
using Newtonsoft.Json.Linq;
using player2_sdk;
using UnityEngine;


[Serializable]
public class ExampleFunctionHandler: MonoBehaviour
{
    public BasicAgentsManager AgentsManager;
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
                        Debug.Log("correct target");
                        aImovement.follow(col.gameObject.transform);
                        break;
                    }
                }
            }
        }
        if (functionCall.name == "StopFollow")
        {
            var aImovement = functionCall.aiObject.GetComponent<AImovement>();
            aImovement.StopFollow();
        }

        if (functionCall.name == "travelTo")
        {
            var aImovement = functionCall.aiObject.GetComponent<AImovement>();
            var args = (JObject)functionCall.arguments;
            Debug.Log("I got called");
            LocationEntry loc; 
            if (AgentsManager.TryGetLocation((string)args["location"], out loc))
            {
                aImovement.goTo(loc.value.position);
            }
        }

        Player2Npc p2 = functionCall.aiObject.GetComponent<Player2Npc>();
        p2.SendChatMessageAsync("System: Tool executed successfully");
        //we need to query the AI again after tool call, to form a basic event chain
    }
}


