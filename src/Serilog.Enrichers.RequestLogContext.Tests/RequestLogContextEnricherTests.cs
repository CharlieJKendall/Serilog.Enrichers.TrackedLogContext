using NUnit.Framework;
using Serilog.Enrichers.RequestLogContext.Tests.Helpers;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Enrichers.RequestLogContext.Tests
{
    public abstract class RequestLogContextEnricherTests
    {
        private RequestLogContextEnricher _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new RequestLogContextEnricher();
        }

        class Enrich : RequestLogContextEnricherTests
        {
            [Test]
            public void Throws_when_log_event_null()
            {
                // Arrange
                var propertyFactory = new ScalarLogEventPropertyFactory();

                // Act / Assert
                var exception = Assert.Throws<ArgumentNullException>(() => _sut.Enrich(null!, propertyFactory));
                Assert.That(exception.ParamName, Is.EqualTo("logEvent"));
            }
            
            [Test]
            public void Throws_when_log_factory_null()
            {
                // Arrange
                var logEvent = LogEventFactory.CreateEmpty();

                // Act / Assert
                var exception = Assert.Throws<ArgumentNullException>(() => _sut.Enrich(logEvent, null!));
                Assert.That(exception.ParamName, Is.EqualTo("propertyFactory"));
            }     
            
            [Test]
            public void Does_not_enrich_when_tracker_null()
            {
                // Arrange
                var logEvent = LogEventFactory.CreateEmpty();
                var propertyFactory = new ScalarLogEventPropertyFactory();

                RequestLogContextEnricher.Initialise();
                RequestLogContextEnricher.PushProperty("property-name", "property-value");
                RequestLogContextEnricher.CleanUp();

                // Act
                _sut.Enrich(logEvent, propertyFactory);

                // Assert
                Assert.That(logEvent.Properties, Has.Count.EqualTo(0));
            }
            
            [Test]
            public void Pushed_property_does_not_overwrite_existing_property()
            {
                // Arrange
                var name = "property-name";
                var firstValue = "first-value";
                var secondValue = "second-value";
                var logEvent = LogEventFactory.CreateWithScalarValue(name, firstValue);
                var propertyFactory = new ScalarLogEventPropertyFactory();

                RequestLogContextEnricher.Initialise();
                RequestLogContextEnricher.PushProperty(name, secondValue);

                // Act
                _sut.Enrich(logEvent, propertyFactory);

                // Assert
                Assert.That(logEvent.Properties, Has.Count.EqualTo(1));
                Assert.That(logEvent.Properties.First().Key, Is.EqualTo(name));
                Assert.That(logEvent.Properties.First().Value, Is.TypeOf<ScalarValue>());
                Assert.That(((ScalarValue)logEvent.Properties.First().Value).Value, Is.EqualTo(firstValue));
            }

            [Test]
            public void Logs_emitted_with_correct_properties_on_captured_context()
            {
                // Arrange
                var factory = new ScalarLogEventPropertyFactory();

                var logEvent1 = LogEventFactory.CreateEmpty();
                var logEvent2 = LogEventFactory.CreateEmpty();
                var logEvent3 = LogEventFactory.CreateEmpty();
                var logEvent4 = LogEventFactory.CreateEmpty();
                var logEvent5 = LogEventFactory.CreateEmpty();

                using var mres1 = new ManualResetEventSlim();
                using var mres2 = new ManualResetEventSlim();

                RequestLogContextEnricher.Initialise();

                // Act
                Serilog.RequestLogContext.PushProperty("one", 1);
                _sut.Enrich(logEvent1, factory);

                _ = Task.Run(() =>
                {
                    mres1.Wait();

                    _sut.Enrich(logEvent2, factory);
                    Serilog.RequestLogContext.PushProperty("two", 2);
                    _sut.Enrich(logEvent3, factory);

                    mres2.Set();
                });

                _sut.Enrich(logEvent4, factory);
                Serilog.RequestLogContext.PushProperty("three", 3);

                mres1.Set();
                mres2.Wait();

                _sut.Enrich(logEvent5, factory);

                RequestLogContextEnricher.CleanUp();

                // Assert
                Assert.That(logEvent1.Properties, Has.Count.EqualTo(1));
                _AssertProperty(logEvent1.Properties, "one", 1);

                Assert.That(logEvent2.Properties, Has.Count.EqualTo(2));
                _AssertProperty(logEvent2.Properties, "one", 1);
                _AssertProperty(logEvent2.Properties, "three", 3);

                Assert.That(logEvent3.Properties, Has.Count.EqualTo(3));
                _AssertProperty(logEvent3.Properties, "one", 1);
                _AssertProperty(logEvent3.Properties, "two", 2);
                _AssertProperty(logEvent3.Properties, "three", 3);

                Assert.That(logEvent4.Properties, Has.Count.EqualTo(1));
                _AssertProperty(logEvent4.Properties, "one", 1);

                Assert.That(logEvent5.Properties, Has.Count.EqualTo(3));
                _AssertProperty(logEvent5.Properties, "one", 1);
                _AssertProperty(logEvent5.Properties, "two", 2);
                _AssertProperty(logEvent5.Properties, "three", 3);
            }

            private static void _AssertProperty(IReadOnlyDictionary<string, LogEventPropertyValue> properties, string key, int value)
            {
                Assert.That(properties[key], Is.TypeOf<ScalarValue>());
                Assert.That(((ScalarValue)properties[key]).Value, Is.EqualTo(value));
            }
        }

        class PushProperty : RequestLogContextEnricherTests
        {
            [Test]
            public void Name_is_null_throws()
            {
                var exception = Assert.Throws<ArgumentNullException>(() => RequestLogContextEnricher.PushProperty(null!, null));
                Assert.That(exception.ParamName, Is.EqualTo("name"));
            }
            
            [Test]
            public void Tracker_has_not_been_initialized_throws()
            {
                Assert.Throws<InvalidOperationException>(() => RequestLogContextEnricher.PushProperty("valid", null));
            }

            [Test]
            public void Tracker_has_been_cleaned_up_throws()
            {
                // Arrange
                TestDelegate pushProperty = () => RequestLogContextEnricher.PushProperty("valid", null);

                RequestLogContextEnricher.Initialise();

                // Act / Assert
                Assert.DoesNotThrow(pushProperty);

                RequestLogContextEnricher.CleanUp();
                Assert.Throws<InvalidOperationException>(pushProperty);
            }

            [Test]
            public void Tracker_has_been_initialised_adds_property()
            {
                // Arrange
                var name = "property-name";
                var value = "property-value";

                RequestLogContextEnricher.Initialise();

                // Act
                RequestLogContextEnricher.PushProperty(name, value);

                // Assert
                Assert.That(RequestLogContextEnricher.TrackedProperties.Value, Has.Count.EqualTo(1));
                Assert.That(RequestLogContextEnricher.TrackedProperties.Value.First().Name, Is.EqualTo(name));
                Assert.That(RequestLogContextEnricher.TrackedProperties.Value.First().Value, Is.EqualTo(value));
            }
            
            [TestCase("")]
            [TestCase(" ")]
            [TestCase("\r")]
            [TestCase("\n")]
            [TestCase("\t")]
            public void Name_is_white_space_throws(string name)
            {
                var exception = Assert.Throws<ArgumentException>(() => RequestLogContextEnricher.PushProperty(name, null));
                Assert.That(exception.ParamName, Is.EqualTo("name"));
            }
        }
    }
}