using UnityEngine;

namespace Witch
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_initialized)
                return;
            MessagePackInitialize();
            _initialized = true;
        }

        private static bool _initialized = false;

        private static void MessagePackInitialize()
        {
            MessagePack.Resolvers.StaticCompositeResolver.Instance.Register(
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Unity.UnityResolver.Instance,
                MessagePack.Unity.Extension.UnityBlitWithPrimitiveArrayResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance
            );

            var option = MessagePack.MessagePackSerializerOptions.Standard
                .WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray)
                .WithResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
            MessagePack.MessagePackSerializer.DefaultOptions = option;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            Initialize();
        }
#endif
    }
}
