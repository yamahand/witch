using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Formatters {

public sealed class VersionFormatter : MemoryPackFormatter<Version>
{
    // Serialize as [Major, Minor, Build, Revision]

    public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref Version? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteUnmanagedWithObjectHeader(4, value.Major, value.Minor, value.Build, value.Revision);
    }

    public override void Deserialize(ref MemoryPackReader reader, ref Version? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4) MemoryPackSerializationException.ThrowInvalidPropertyCount(4, count);

        reader.ReadUnmanaged(out int major, out int minor, out int build, out int revision);
        value = new Version(major, minor, build, revision);
    }
}

}