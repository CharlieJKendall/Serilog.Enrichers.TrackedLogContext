using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Enrichers.RequestLogContext.Tests
{
    public class RequestLogContextMiddlewareTests
    {
        private RequestLogContextMiddleware _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new RequestLogContextMiddleware();
        }
    
        [Test]
        public async Task Garbage_collects_when_execution_context_ends()
        {
            WeakReference weakReference = null!;

            await _sut.InvokeAsync(null!, _ =>
            {
                weakReference = new WeakReference(RequestLogContextEnricher.TrackedProperties.Value);
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

            await _sut.InvokeAsync(null!, (context) =>
            {
                _ = Task.Run(() =>
                {
                    weakReference = new WeakReference(RequestLogContextEnricher.TrackedProperties.Value);
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