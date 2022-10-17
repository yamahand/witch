using System;
using System.IO;
using UnityEngine;
using MemoryPack;

namespace Witch.SaveData
{
    public class MemoryDataTest : MonoBehaviour
    {
        void Start()
        {
            _person = new Person { Age = 40, Name = "John" };
            var bin = MemoryPackSerializer.Serialize(_person);
            var val = MemoryPackSerializer.Deserialize<Person>(bin);
            Debug.Log($"{_person.Age}, {_person.Name}");
        }
        
        private void OnGUI()
        {
            string path = Path.Combine(Application.persistentDataPath, "memory.dat");
            if (UnityEngine.GUILayout.Button("セーブ"))
            {
                Save(path, _person);
            }
            if (UnityEngine.GUILayout.Button("無圧縮セーブ"))
            {
                _person.Age = _person.Age + 1;
            }
            if (UnityEngine.GUILayout.Button("ロード"))
            {
                Load<Person>(path, ref _person);
                Debug.Log($"{_person.Age}, {_person.Name}");
            }
        }

        Person _person;

        private async void Save<T>(string path,T data)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                //var bin = MemoryPackSerializer.Serialize(data);
                //stream.Write(bin);
                await MemoryPackSerializer.SerializeAsync(stream, data);
            }
        }

        private void Load<T>(string path, ref T val) where T : new()
        {
            if (!File.Exists(path))
            {
                return;
            }
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] v = new byte[stream.Length];
                stream.Read(v);
                MemoryPackSerializer.Deserialize<T>(v, ref val);
            }
        }
    }
}