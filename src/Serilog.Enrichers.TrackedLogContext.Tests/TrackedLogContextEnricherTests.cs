using NUnit.Framework;
using Serilog.Enrichers.TrackedLogContext.Tests.Helpers;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Enrichers.TrackedLogContext.Tests
{
    public abstract class TrackedLogContextEnricherTests
    {
        private TrackedLogContextEnricher _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TrackedLogContextEnricher();
        }

        class Enrich : TrackedLogContextEnricherTests
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

                Serilog.TrackedLogContext.Initialise();
                Serilog.TrackedLogContext.PushProperty("property-name", "property-value");
                Serilog.TrackedLogContext.CleanUp();

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

                Serilog.TrackedLogContext.Initialise();
                Serilog.TrackedLogContext.PushProperty(name, secondValue);

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

                Serilog.TrackedLogContext.Initialise();

                // Act
                Serilog.TrackedLogContext.PushProperty("one", 1);
                _sut.Enrich(logEvent1, factory);

                _ = Task.Run(() =>
                {
                    mres1.Wait();

                    _sut.Enrich(logEvent2, factory);
                    Serilog.TrackedLogContext.PushProperty("two", 2);
                    _sut.Enrich(logEvent3, factory);

                    mres2.Set();
                });

                _sut.Enrich(logEvent4, factory);
                Serilog.TrackedLogContext.PushProperty("three", 3);

                mres1.Set();
                mres2.Wait();

                _sut.Enrich(logEvent5, factory);

                Serilog.TrackedLogContext.CleanUp();

                // Assert
                Assert.That(logEvent1.Properties, Has.Count.EqualTo(1));
                AssertProperty(logEvent1.Properties, "one", 1);

                Assert.That(logEvent2.Properties, Has.Count.EqualTo(2));
                AssertProperty(logEvent2.Properties, "one", 1);
                AssertProperty(logEvent2.Properties, "three", 3);

                Assert.That(logEvent3.Properties, Has.Count.EqualTo(3));
                AssertProperty(logEvent3.Properties, "one", 1);
                AssertProperty(logEvent3.Properties, "two", 2);
                AssertProperty(logEvent3.Properties, "three", 3);

                Assert.That(logEvent4.Properties, Has.Count.EqualTo(1));
                AssertProperty(logEvent4.Properties, "one", 1);

                Assert.That(logEvent5.Properties, Has.Count.EqualTo(3));
                AssertProperty(logEvent5.Properties, "one", 1);
                AssertProperty(logEvent5.Properties, "two", 2);
                AssertProperty(logEvent5.Properties, "three", 3);
            }

            private static void AssertProperty(IReadOnlyDictionary<string, LogEventPropertyValue> properties, string key, int value)
            {
                Assert.That(properties[key], Is.TypeOf<ScalarValue>());
                Assert.That(((ScalarValue)properties[key]).Value, Is.EqualTo(value));
            }
        }

        class PushProperty : TrackedLogContextEnricherTests
        {
            [Test]
            public void Name_is_null_throws()
            {
                var exception = Assert.Throws<ArgumentNullException>(() => Serilog.TrackedLogContext.PushProperty(null!, null));
                Assert.That(exception.ParamName, Is.EqualTo("name"));
            }
            
            [Test]
            public void Tracker_has_not_been_initialized_throws()
            {
                Assert.Throws<InvalidOperationException>(() => Serilog.TrackedLogContext.PushProperty("valid", null));
            }

            [Test]
            public void Tracker_has_been_cleaned_up_throws()
            {
                // Arrange
                TestDelegate pushProperty = () => Serilog.TrackedLogContext.PushProperty("valid", null);

                Serilog.TrackedLogContext.Initialise();

                // Act / Assert
                Assert.DoesNotThrow(pushProperty);

                Serilog.TrackedLogContext.CleanUp();
                Assert.Throws<InvalidOperationException>(pushProperty);
            }

            [Test]
            public void Tracker_has_been_initialised_adds_property()
            {
                // Arrange
                var name = "property-name";
                var value = "property-value";

                Serilog.TrackedLogContext.Initialise();

                // Act
                Serilog.TrackedLogContext.PushProperty(name, value);

                // Assert
                var property = Serilog.TrackedLogContext.GetTrackedProperties().Single();
                
                Assert.Multiple(() =>
                {
                    Assert.That(property.Name, Is.EqualTo(name));
                    Assert.That(property.Value, Is.EqualTo(value));
                });
            }

            [TestCase("")]
            [TestCase(" ")]
            [TestCase("\r")]
            [TestCase("\n")]
            [TestCase("\t")]
            public void Name_is_white_space_throws(string name)
            {
                var exception = Assert.Throws<ArgumentException>(() => Serilog.TrackedLogContext.PushProperty(name, null));
                Assert.That(exception.ParamName, Is.EqualTo("name"));
            }
        }
    }
}