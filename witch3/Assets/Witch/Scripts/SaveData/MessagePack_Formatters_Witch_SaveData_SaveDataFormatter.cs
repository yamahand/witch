// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters.Witch.SaveData
{
    public sealed class SaveDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Witch.SaveData.SaveData>
    {
        // Id
        private static global::System.ReadOnlySpan<byte> GetSpan_Id() => new byte[1 + 2] { 162, 73, 100 };
        // Timestamp
        private static global::System.ReadOnlySpan<byte> GetSpan_Timestamp() => new byte[1 + 9] { 169, 84, 105, 109, 101, 115, 116, 97, 109, 112 };
        // Comment
        private static global::System.ReadOnlySpan<byte> GetSpan_Comment() => new byte[1 + 7] { 167, 67, 111, 109, 109, 101, 110, 116 };

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Witch.SaveData.SaveData value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNil();
                return;
            }

            var formatterResolver = options.Resolver;
            writer.WriteMapHeader(3);
            writer.WriteRaw(GetSpan_Id());
            writer.Write(value.Id);
            writer.WriteRaw(GetSpan_Timestamp());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.DateTimeOffset>(formatterResolver).Serialize(ref writer, value.Timestamp, options);
            writer.WriteRaw(GetSpan_Comment());
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.Comment, options);
        }

        public global::Witch.SaveData.SaveData Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            var formatterResolver = options.Resolver;
            var length = reader.ReadMapHeader();
            var __Id__ = default(int);
            var __Timestamp__ = default(global::System.DateTimeOffset);
            var __Comment__ = default(string);

            for (int i = 0; i < length; i++)
            {
                var stringKey = global::MessagePack.Internal.CodeGenHelpers.ReadStringSpan(ref reader);
                switch (stringKey.Length)
                {
                    default:
                    FAIL:
                      reader.Skip();
                      continue;
                    case 2:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 25673UL) { goto FAIL; }

                        __Id__ = reader.ReadInt32();
                        continue;
                    case 9:
                        if (!global::System.MemoryExtensions.SequenceEqual(stringKey, GetSpan_Timestamp().Slice(1))) { goto FAIL; }

                        __Timestamp__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.DateTimeOffset>(formatterResolver).Deserialize(ref reader, options);
                        continue;
                    case 7:
                        if (global::MessagePack.Internal.AutomataKeyGen.GetKey(ref stringKey) != 32772479205076803UL) { goto FAIL; }

                        __Comment__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        continue;

                }
            }

            var ____result = new global::Witch.SaveData.SaveData(__Id__, __Timestamp__, __Comment__);
            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
