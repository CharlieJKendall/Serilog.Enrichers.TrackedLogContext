using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Enrichers.TrackedLogContext.AzureFunctions.Tests
{
    public class TrackedLogContextMiddlewareTests
    {
        private TrackedLogContextMiddleware _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TrackedLogContextMiddleware();
        }

        [Test]
        public async Task Garbage_collects_when_execution_context_ends()
        {
            WeakReference weakReference = null!;

            await _sut.Invoke(null!, _ =>
            {
                weakReference = new WeakReference(Serilog.TrackedLogContext.GetTrackedProperties());
                Assert.That(weakReference.IsAlive, Is.True);
                return Task.CompletedTask;
            });

            GC.Collect(1);

            Assert.That(weakReference.IsAlive, Is.False);
        }

        [Test]
        public async Task Execution_context_captured_garbage_collects_when_inner_context_ends()
        {
            WeakReference weakReference = null!;
            using var mres1 = new ManualResetEventSlim();
            using var mres2 = new ManualResetEventSlim();

            await _sut.Invoke(null!, (context) =>
            {
                _ = Task.Run(() =>
                {
                    weakReference = new WeakReference(Serilog.TrackedLogContext.GetTrackedProperties());
                    Assert.That(weakReference.IsAlive, Is.True);

                    mres1.Set();
                    mres2.Wait();
                });

                return Task.CompletedTask;
            });

            mres1.Wait();

            GC.Collect();
            Assert.That(weakReference.IsAlive, Is.True);

            mres2.Set();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.That(weakReference.IsAlive, Is.False);
        }
    }
}
