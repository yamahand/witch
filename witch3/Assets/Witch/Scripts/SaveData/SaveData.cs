using System;
using MessagePack;

namespace Witch.SaveData
{
    /// <summary>
    /// セーブデータ
    /// </summary>
    [MessagePackObject]
    [Serializable]
    public partial class SaveData
    {
        /// <summary>
        /// ID
        /// </summary>
        /// <value></value>
        [Key(0)]
        public int Id { get; set; } = default;

        /// <summary>
        /// 保存日時
        /// </summary>
        /// <value></value>
        [Key(1)]
        public DateTimeOffset Timestamp { get; set; } = default;
        
        [Key(2)]
        public string Comment { get; set; }

        /// <summary>
        /// 空データフラグ
        /// </summary>
        [IgnoreMember]
        public bool IsEmpty => this.Id == default;

        public SaveData()
        {
        }

        [SerializationConstructor]
        public SaveData(int id, DateTimeOffset timestamp, string comment)
        {
            this.Id = id;
            this.Timestamp = timestamp;
            this.Comment = comment;
        }
    }
}