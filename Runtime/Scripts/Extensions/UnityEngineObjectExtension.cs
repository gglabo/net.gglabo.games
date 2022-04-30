namespace GGLabo.Games.Runtime.Extensions
{
    public static class UnityEngineObjectExtension
    {
        /// <summary>
        /// Cast null in UnityEngine.Object to null in System.Object.
        /// Convenient to use null operators. (??, ?., ?[])
        /// <a href="https://qiita.com/up-hash/items/91a4ea63ccfa7a1323c5">link</a>
        /// </summary>
        /// <param name="obj">Object to cast.</param>
        /// <typeparam name="T">Type to cast.</typeparam>
        /// <returns>Null in System.Object when obj is null.</returns>
        public static T NullCast<T>(this T obj)
            where T : UnityEngine.Object
        {
            return (obj != null) ? obj : null;
        }
    }
}