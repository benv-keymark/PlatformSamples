using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VertesiaActivity.Tests
{
    /// <summary>
    /// Queues up pre-configured <see cref="HttpResponseMessage"/> instances so
    /// that unit tests can drive <see cref="System.Net.Http.HttpClient"/> without
    /// making real network calls.
    /// </summary>
    internal sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();

        public void Enqueue(HttpResponseMessage response) => _responses.Enqueue(response);

        public int RemainingResponses => _responses.Count;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
                throw new InvalidOperationException(
                    "MockHttpMessageHandler has no more queued responses.");

            return Task.FromResult(_responses.Dequeue());
        }
    }

    /// <summary>
    /// Delegates each request to a user-supplied function, allowing tests to
    /// inspect the request (e.g. capture the body) and return a custom response.
    /// </summary>
    internal sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public CapturingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
