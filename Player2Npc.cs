namespace player2_sdk
{


    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using JetBrains.Annotations;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Networking;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using Newtonsoft.Json;
    using Unity.VisualScripting;

    [Serializable]
    public class SpawnNpc
    {
        public string short_name;
        public string name;
        public string character_description;
        public string system_prompt;
        [CanBeNull] public string voice_id;
        public List<SerializableFunction> commands;
    }

    [Serializable]
    public class ChatRequest
    {
        public string sender_name;
        public string sender_message;
        [CanBeNull] public string game_state_info;
        [CanBeNull] public TTS? tts;
    }

    [Serializable]
    public enum TTS
    {
        local_client,
        server
    }


    [Serializable]
    public class NpcSpawnedEvent : UnityEvent<string>
    {
    }

    public class Player2Npc : MonoBehaviour
    {
        [Header("State Config")] [SerializeField]
        private NpcManager npcManager;

        [Header("NPC Configuration")] [SerializeField]
        private string shortName = "Victor";

        [SerializeField] private string fullName = "Victor J. Johnson";
        [SerializeField] private string characterDescription = "A crazed scientist on the hunt for gold";
        [SerializeField] private string systemPrompt = "You are a mad scientist obsessed with finding gold.";
        [SerializeField] private bool persistent = false;

        [Header("Events")] [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI outputMessage;

        private string _npcID = null;

        private string _gameID() => npcManager.gameId;
        private string _baseUrl() => NpcManager.GetBaseUrl();

        private void Start()
        {
            Debug.Log("Starting Player2Npc with NPC: " + fullName);

            inputField.onEndEdit.AddListener(OnChatMessageSubmitted);
            inputField.onEndEdit.AddListener(_ => inputField.text = string.Empty);

            OnSpawnTriggered();
        }

        private void OnSpawnTriggered()
        {
            // Fire and forget async operation with proper error handling
            _ = SpawnNpcAsync();
        }

        private void OnChatMessageSubmitted(string message)
        {
            _ = SendChatMessageAsync(message);
        }

        private async Awaitable SpawnNpcAsync()
        {
            try
            {
                var spawnData = new SpawnNpc
                {
                    short_name = shortName,
                    name = fullName,
                    character_description = characterDescription,
                    system_prompt = systemPrompt,
                    voice_id = "test",
                    commands = npcManager.GetSerializableFunctions()
                };

                string url = $"{_baseUrl()}/npc/games/{_gameID()}/npcs/spawn";
                Debug.Log($"Spawning NPC at URL: {url}");

                string json = JsonConvert.SerializeObject(spawnData, npcManager.JsonSerializerSettings);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

                using var request = new UnityWebRequest(url, "POST");
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                // Use Unity's native Awaitable async method
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    _npcID = request.downloadHandler.text.Trim('"');
                    Debug.Log($"NPC spawned successfully with ID: {_npcID}");
                    npcManager.RegisterNpc(_npcID, outputMessage);
                }
                else
                {
                    string error = $"Failed to spawn NPC: {request.error} - Response: {request.downloadHandler.text}";
                    Debug.LogError(error);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("NPC spawn operation was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error during NPC spawn: {ex.Message}");
            }
        }

        private async Awaitable SendChatMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                Debug.Log("Sending message to NPC: " + message);

                if (string.IsNullOrEmpty(_npcID))
                {
                    Debug.LogWarning("NPC ID is not set! Cannot send message.");
                    return;
                }

                var chatRequest = new ChatRequest
                {
                    sender_name = fullName,
                    sender_message = message,
                    tts = null
                };

                await SendChatRequestAsync(chatRequest);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Chat message send operation was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error sending chat message: {ex.Message}");
            }
        }

        private async Awaitable SendChatRequestAsync(ChatRequest chatRequest)
        {
            string url = $"{_baseUrl()}/npc/games/{_gameID()}/npcs/{_npcID}/chat";
            string json = JsonConvert.SerializeObject(chatRequest, npcManager.JsonSerializerSettings);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Use Unity's native Awaitable async method
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Message sent successfully to NPC {_npcID}");
            }
            else
            {
                string error = $"Failed to send message: {request.error} - Response: {request.downloadHandler.text}";
                Debug.LogError(error);
            }
        }
    }
}