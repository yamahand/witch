using System;

public static class ArrayExtension
{
    private const int INVALID_INDEX = -1;   //!< 不正なインデックス

    /// <summary>
    /// 有効なインデックスか判定
    /// </summary>
    /// <typeparam name="T">配列の要素の型</typeparam>
    /// <param name="array">配列</param>
    /// <param name="index">インデックス</param>
    /// <returns></returns>
    public static bool IsValidIndex<T>(this T[] array, int index)
    {
        if(array == null)
        {
            return false;
        }

        return index < array.Length && index >= 0;
    }

    public static bool IsValidIndex(this Array array, int index)
    {
        if (array == null)
        {
            return false;
        }

        return index < array.Length && index >= 0;
    }

    public static bool Swap<T>(this T[] array, int lhs, int rhs)
    {
        if(array == null)
        {
            return false;
        }

        if (!array.IsValidIndex(lhs) || !array.IsValidIndex(rhs))
            return false;

        var tmp = array[lhs];
        array[lhs] = array[rhs];
        array[rhs] = tmp;

        return true;
    }
}