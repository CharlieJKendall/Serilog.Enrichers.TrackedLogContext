using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog.Enrichers.TrackedLogContext.AspNetCore.Tests.Helpers;
using Serilog.Events;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Enrichers.TrackedLogContext.AspNetCore.Tests
{
    internal class TrackedLogContextMiddlewareTests
    {
        [Test]
        public async Task Middleware_runs_correctly_as_web_application()
        {
            // Arrange
            var inMemoryLogEventSink = new InMemoryLogSink();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Sink(inMemoryLogEventSink)
                .Enrich.FromTrackedLogContext()
                .CreateLogger();

            var hostUrl = "http://localhost:5001/";

            _ = Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.Configure(applicationBuilder =>
                    {
                        applicationBuilder.UseTrackedLogContext();
                        applicationBuilder.UseRouting();

                        applicationBuilder.Use(async (context, next) =>
                        {
                            Log.Information("Log enriched without property from endpoint");
                            await next();
                            Log.Information("Log enriched with property from endpoint");
                        });

                        applicationBuilder.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/", async (context) =>
                            {
                                Serilog.TrackedLogContext.PushProperty("PropertyFromEndpoint", "HelloFromEndpoint");
                                await context.Response.WriteAsync("Success");
                            });
                        });
                    });

                    webHostBuilder.UseUrls(hostUrl);
                })
                .Build()
                .RunAsync();

            using var httpClient = new HttpClient();

            // Act
            var response = await httpClient.GetStringAsync(hostUrl);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response, Is.EqualTo("Success"));

                Assert.That(inMemoryLogEventSink.LogEvents[0].Properties, Has.Count.EqualTo(0));
                Assert.That(inMemoryLogEventSink.LogEvents[0].Level, Is.EqualTo(LogEventLevel.Information));
                Assert.That(inMemoryLogEventSink.LogEvents[0].MessageTemplate.Text, Is.EqualTo("Log enriched without property from endpoint"));

                Assert.That(inMemoryLogEventSink.LogEvents[1].Properties, Has.Count.EqualTo(1));
                Assert.That(inMemoryLogEventSink.LogEvents[1].Level, Is.EqualTo(LogEventLevel.Information));
                Assert.That(inMemoryLogEventSink.LogEvents[1].MessageTemplate.Text, Is.EqualTo("Log enriched with property from endpoint"));
                Assert.That(inMemoryLogEventSink.LogEvents[1].Properties["PropertyFromEndpoint"], Is.EqualTo(new ScalarValue("HelloFromEndpoint")));
            });
        }

        [Test]
        public async Task Garbage_collects_when_execution_context_ends()
        {
            // Arrange
            WeakReference weakReference = null!;

            Task next(HttpContext context)
            {
                weakReference = new WeakReference(Serilog.TrackedLogContext.GetTrackedProperties());
                Assert.That(weakReference.IsAlive, Is.True);
                return Task.CompletedTask;
            }

            var sut = new TrackedLogContextMiddleware(next);

            // Act
            await sut.InvokeAsync(null!);

            GC.Collect(1);

            // Assert
            Assert.That(weakReference.IsAlive, Is.False);
        }

        [Test]
        public async Task Execution_context_captured_garbage_collects_when_inner_context_ends()
        {
            WeakReference weakReference = null!;

            using var mres1 = new ManualResetEventSlim();
            using var mres2 = new ManualResetEventSlim();

            Task next(HttpContext context)
            {
                _ = Task.Run(() =>
                {
                    weakReference = new WeakReference(Serilog.TrackedLogContext.GetTrackedProperties());
                    Assert.That(weakReference.IsAlive, Is.True);

                    mres1.Set();
                    mres2.Wait();
                });

                return Task.CompletedTask;
            }

            var sut = new TrackedLogContextMiddleware(next);

            // Act / Assert
            await sut.InvokeAsync(null!);

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