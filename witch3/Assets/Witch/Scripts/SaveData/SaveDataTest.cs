using System;
using System.IO;
using MessagePack;
using UnityEngine;

namespace Witch.SaveData
{
    public class SaveDataTest : MonoBehaviour
    {
        private void Start()
        {
            _saveData = new SaveData(1, timestamp:DateTimeOffset.Now, "comment");
            var serializedData = MessagePackSerializer.Serialize(_saveData);
            var deserializedData = MessagePackSerializer.Deserialize<SaveData>(serializedData);
            Debug.Log($"id {deserializedData.Id}, timestamp {deserializedData.Timestamp}");
        }

        private void OnGUI()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.dat");
            if (UnityEngine.GUILayout.Button("セーブ"))
            {
                _saveData.Timestamp = DateTimeOffset.Now;
                var option = MessagePack.MessagePackSerializerOptions.Standard
                    .WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray)
                    .WithResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
                Save(path, _saveData, option);
            }
            if (UnityEngine.GUILayout.Button("無圧縮セーブ"))
            {
                var option = MessagePack.MessagePackSerializerOptions.Standard
                    .WithResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
                _saveData.Timestamp = DateTimeOffset.Now;
                Save(path + ".txt", _saveData, option);
            }
            if (UnityEngine.GUILayout.Button("ロード"))
            {
                _saveData = Load<SaveData>(path);
                Debug.Log($"id {_saveData.Id}, timestamp {_saveData.Timestamp}");
            }
        }

        private void Save<T>(string path,T data, MessagePackSerializerOptions options = null)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                MessagePackSerializer.Serialize(stream, data, options);
            }
        }

        private T Load<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return MessagePackSerializer.Deserialize<T>(stream);
            }
        }

        private SaveData _saveData;
    }
}