using UnityEngine;
using Core;

namespace Demo
{
    /// <summary>
    /// Demo <see cref="IRandomProvider"/> backed by <see cref="UnityEngine.Random"/>.
    /// </summary>
    public sealed class UnityRandomProvider : IRandomProvider
    {
        public float NextFloat() => Random.value;
    }
}
