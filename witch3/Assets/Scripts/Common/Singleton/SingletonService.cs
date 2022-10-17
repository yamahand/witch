
/// <summary>
/// unityに依存しないC#用のシングルトンクラス
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonService<T> where T : new()  
{
    protected static T Instance => _instance ??= new T();
    private static T _instance;
}
