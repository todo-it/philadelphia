using System;

namespace Philadelphia.Tests.Client.Model {
    [AttributeUsage(AttributeTargets.Method)]
    public class FactAttribute : Attribute
    {
        /// <summary>
        /// When <c>true</c>, test is considerred 'passed' if <see cref="AssertionException"/> is thrown.
        /// Note that any other exception is still considered failure.
        /// </summary>
        public bool ExpectAssertionException { get; set; } = false;
    }
}