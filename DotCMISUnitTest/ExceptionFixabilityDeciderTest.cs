namespace DotCMISUnitTest
{
    using System.Net;
    using System.Runtime.Serialization;
    using NUnit.Framework;
    using DotCMIS.Binding.Impl;

    [TestFixture]
    public class ExceptionFixabilityDeciderTest
    {
        class TestResponse : WebResponse {
        }

        [Test]
        public void ExceptionWithoutResponseCanBeRetried()
        {
            var we = new WebException();
            Assert.True(ExceptionFixabilityDecider.CanExceptionBeFixedByRetry(we));
        }

        [Test]
        public void ExceptionWithStatusCodeNotFoundCanNotBeRetried()
        {
            Assert.False(ExceptionFixabilityDecider.CanExceptionStatusCodeBeFixedByRetry(HttpStatusCode.NotFound));
        }

        [Test]
        public void ExceptionWithStatusCodeForbiddenCanNotBeRetried()
        {
            Assert.False(ExceptionFixabilityDecider.CanExceptionStatusCodeBeFixedByRetry(HttpStatusCode.Forbidden));
        }

        [Test]
        public void ExceptionWithStatusCodeRequestTimeOutCanBeRetried()
        {
            Assert.True(ExceptionFixabilityDecider.CanExceptionStatusCodeBeFixedByRetry(HttpStatusCode.RequestTimeout));
        }
    }
}

