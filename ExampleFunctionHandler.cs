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
        Player2Npc p2 = functionCall.aiObject.GetComponent<Player2Npc>();
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
            bool targetFound = false;
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
                        targetFound = true;
                        break;
                    }
                }
            }
            if (!targetFound)
            {
                p2.SendChatMessageAsync("System: Tool " +functionCall.name +  " unsuccessful, reason: specified target not found");
                return;
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
            Debug.Log("Initiating Travel to the location:"+(string)args["location"] );
            LocationEntry loc; 
            if (AgentsManager.TryGetLocation((string)args["location"], out loc))
            {
                aImovement.goTo(loc.value.position);
            }
            else
            {
                p2.SendChatMessageAsync("System: Tool " +functionCall.name +  " unsuccessful, reason: specified location not found");
                return;
            }
        }

         if (functionCall.name == "addTask")
        {
            var args = (JObject)functionCall.arguments;
            string agentId = p2.shortName;
            string taskName = (string)args["TaskName"];
            string taskDesc = (string)args["TaskDesc"];
            string taskType = "default";
            string hints = (string)args["Hints"];
            string closure = (string)args["FinishClause"];

            string taskStatus = "pending";
          
            Agent agent = AgentsManager.GetAgent(agentId);
            if (agent != null)
            {
                Debug.Log("adding task");
                agent.AddTask(new AgentTask(taskName, taskDesc, taskType, taskStatus, hints, closure));
            }
            else
            {
                p2.SendChatMessageAsync("System: Tool " +functionCall.name +  " unsuccessful, reason: specified agent not found");
                return;
            }
        }

        // if (functionCall.name == "addLookingFor")
        // {   
        //     var args = (JObject)functionCall.arguments;
        //     AImovement aImovement = functionCall.aiObject.GetComponent<AImovement>();
        //     string lookingFor = (string)args["name"];
        //     aImovement.AddLookingFor(lookingFor);
        // }

        if (functionCall.name == "moveToThing")
        {
            var args = (JObject)functionCall.arguments;
            string lookingFor = (string)args["name"];
            GameObject obj = GameObject.Find(lookingFor);
            AImovement aImovement = functionCall.aiObject.GetComponent<AImovement>();
            aImovement.goTo(obj.transform.position);
        }
        if (functionCall.name == "gather")
        {
            Debug.Log("AI is trying to gather something!");
            var args = (JObject)functionCall.arguments;
            string lookingFor = (string)args["name"];
            int amount = (int)args["amount"];
            functionCall.aiObject.TryGetComponent<AIstats>( out var a);
            a.thingsToPickUp[lookingFor] = amount;
            p2.SendChatMessageAsync("System: Added "+lookingFor+" to gather list. Initiating Gather State. ");

        }

        if (functionCall.name == "drop")
        {
            Debug.Log("AI is trying to DROP something!");
            var args = (JObject)functionCall.arguments;
            string lookingFor = (string)args["name"];
            int amount = (int)args["amount"];
            functionCall.aiObject.TryGetComponent<AIstats>( out var a);
            for (int i = 0; i < amount; i++)
            {
                a.aiInventory.DropOne(lookingFor,functionCall.aiObject.transform.position);
            }
           
        }
        

        if (functionCall.name == "findNPC")
        {
            var args = (JObject)functionCall.arguments;
            AImovement aImovement = functionCall.aiObject.GetComponent<AImovement>();
            string lookingFor = (string)args["name"];
            Agent agent = AgentsManager.GetAgent(lookingFor);
            if (agent != null)
            {
                Transform agentTransform = agent.getAgentLocation();
                if (agentTransform != null)
                {
                    aImovement.follow(agentTransform);
                }
                else
                {
                    p2.SendChatMessageAsync("System: Tool " +functionCall.name +  " unsuccessful, reason: specified NPC location not found");
                    return;
                }
            }
            else
            {
                p2.SendChatMessageAsync("System: Tool " +functionCall.name +  " unsuccessful, reason: specified NPC not found");
                return;
            }
        }

        p2.SendChatMessageAsync("System: Tool " +functionCall.name +  " executed successfully. Please proceed to further tools or chat, do not call this tool again right now.");
        //we need to query the AI again after tool call, to form a basic event chain
    }
}


