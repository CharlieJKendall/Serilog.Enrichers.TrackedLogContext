using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Serilog
{
    /// <summary>
    /// The <see cref="TrackedLogContext"/> allows all logs for the remainder of an async
    /// context (e.g. an HTTP request) to be enriched with tracked properties
    /// </summary>
    public static class TrackedLogContext
    {
        private static readonly AsyncLocal<ConcurrentBag<TrackedLogContextProperty>?> _trackedProperties =
            new AsyncLocal<ConcurrentBag<TrackedLogContextProperty>?>();

        /// <summary>Gets the properties being tracked for the current async context</summary>
        /// <returns>The properties being tracked for the current async context</returns>
        public static IReadOnlyCollection<TrackedLogContextProperty> GetTrackedProperties()
        {
            var trackedProperties = _trackedProperties.Value;
            if (trackedProperties is null)
            {
                return Array.Empty<TrackedLogContextProperty>();
            }

            return trackedProperties;
        }

        /// <summary>
        /// Should be called once per async call stack before all calls to <see cref="PushProperty(string, object?)"/>
        /// </summary>
        public static void Initialise()
        {
            _trackedProperties.Value = new ConcurrentBag<TrackedLogContextProperty>();
        }

        /// <summary>
        /// May be called once per async call stack when no further calls to <see cref="PushProperty(string, object?)"/> are
        /// going to be made
        /// </summary>
        /// 
        /// <remarks>
        /// If <see cref="PushProperty(string, object?)"/> is called after <see cref="CleanUp"/>, logs will not be enriched with
        /// the properties pushed
        /// </remarks>
        public static void CleanUp()
        {
            _trackedProperties.Value = null!;
        }

        /// <summary>
        /// Push a property and value onto the <see cref="TrackedLogContext"/> so that all
        /// subsequent logs emitted during the async scope are enriched with it
        /// </summary>
        /// 
        /// <param name="name">The name of the property</param>
        /// <param name="value">The value of the property</param>
        /// 
        /// <exception cref="System.ArgumentNullException">
        ///     <paramref name="name"/> is <code>null</code>
        /// </exception>
        /// 
        /// <exception cref="System.ArgumentException">
        ///     <paramref name="name"/> only consists of white space characters
        /// </exception>
        /// 
        /// <exception cref="System.InvalidOperationException">
        ///     <see cref="PushProperty(string, object?)"/> is called before the
        ///     <see cref="Initialise()"/> has been called
        /// </exception>
        public static void PushProperty(string name, object? value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Property name must not only contain white space characters",
                    nameof(name));
            }

            var tracker = _trackedProperties.Value;
            if (tracker == null)
            {
                throw new InvalidOperationException(
                    $"Can not push property onto RequestLogContext before {nameof(Initialise)} or after {nameof(CleanUp)}");
            }

            tracker.Add(new TrackedLogContextProperty(name, value));
        }
    }
}