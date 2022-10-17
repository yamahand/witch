using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Formatters {

public sealed class KeyValuePairFormatter<TKey, TValue> : MemoryPackFormatter<KeyValuePair<TKey?, TValue?>>
{
    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref KeyValuePair<TKey?, TValue?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Key);
        writer.WriteValue(value.Value);
    }

    public override void Deserialize(ref MemoryPackReader reader, ref KeyValuePair<TKey?, TValue?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new KeyValuePair<TKey?, TValue?>(
            reader.ReadValue<TKey>(),
            reader.ReadValue<TValue>()
        );
    }
}

}