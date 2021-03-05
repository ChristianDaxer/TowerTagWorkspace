using System;
using System.Collections.Generic;

/// <summary>
/// Extension for <see cref="IEnumerable{T}"/>.
///
/// </summary>
/// <author>Ole Jürgensen</author>
public static class EnumerableExtension {
    /// <summary>
    /// Applies the given action to each element of the enumerable.
    /// </summary>
    /// <param name="enumerable">The enumerable that enumerates the alements to apply the action to.</param>
    /// <param name="action">The action to be applied to every element of the enumerable.</param>
    /// <typeparam name="T">The type of the enumerated elements.</typeparam>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
        foreach (T t in enumerable) {
            action(t);
        }
    }

    /// <summary>
    /// Lazily applies the given action to each element of the enuemrable and returns the original enumerable.
    /// Lazily means that the action is only applied and evaluated when needed for some output.
    /// That means you have to actually use the result of this method before the action will be applied.
    /// Allows for concatenated applications of multiple actions.
    /// </summary>
    /// <param name="enumerable">The enumerable that enumerates the alements to apply the action to.</param>
    /// <param name="action">The action to be applied to every element of the enumerable.</param>
    /// <typeparam name="T">The type of the enumerated elements.</typeparam>
    /// <returns>The input enumerable.</returns>
    public static IEnumerable<T> Apply<T>(this IEnumerable<T> enumerable, Action<T> action) {
        foreach (T t in enumerable) {
            action(t);
            yield return t;
        }
    }
}