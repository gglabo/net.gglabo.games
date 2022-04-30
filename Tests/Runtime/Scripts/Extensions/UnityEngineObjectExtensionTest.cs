using System.Collections;
using GGLabo.Games.Runtime.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GGLabo.Games.Tests.Runtime.Extensions
{
    public class UnityEngineObjectExtensionTest
    {
        [UnityTest]
        public IEnumerator NullCastTest()
        {
            var gameObject = new GameObject("Rigidbody");
            var rigidbody = gameObject.AddComponent<Rigidbody>();

            Object.DestroyImmediate(rigidbody);
            yield return null;

            Assert.That(rigidbody.NullCast()?.velocity.x ?? 100.0f, Is.EqualTo(100.0f));
        }
    }
}