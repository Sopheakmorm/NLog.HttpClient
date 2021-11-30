using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.HttpClient
{



    /// <summary>
    /// NLog message target for HttpClient.
    /// </summary>
    [Target("HttpClient")]
    public sealed class HttpClientTarget : AsyncTaskTarget
    {

        #region Property

        /// <summary>
        /// Gets the fields collection.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        [ArrayParameter(typeof(JsonField), "field")]
        public IList<JsonField> Fields { get; set; }
        private Func<LogEventInfo, Dictionary<string, object>> _createDocumentDelegate;


        [RequiredParameter]
        public string Url { get; set; }


        [RequiredParameter]
        public string Auth { get; set; }

        private static System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();
        #endregion

        public HttpClientTarget()
        {
            Fields = new List<JsonField>();
        }



        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            if (_createDocumentDelegate == null)
                _createDocumentDelegate = e => CreateObject(e);

            var documents = _createDocumentDelegate(logEvent);

            var body = JsonConvert.SerializeObject(documents);

            await SendData(body);
        }

        protected override async void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (_createDocumentDelegate == null)
                _createDocumentDelegate = e => CreateObject(e);
            var documents = logEvents.Select(info => _createDocumentDelegate);

            var body = JsonConvert.SerializeObject(documents);

            await SendData(body);
        }

        /*
        protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            if (logEvents.Count == 0)
                return;

            try
            {
                if (_createDocumentDelegate == null)
                    _createDocumentDelegate = e => CreateObject(e);
                var documents = logEvents.Select(_createDocumentDelegate);

                var body = JsonConvert.SerializeObject(documents);

                await SendData(body);

            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when writing to ws {0}", ex);

                if (ex.MustBeRethrownImmediately())
                    throw;

                if (ex.MustBeRethrown())
                    throw;
            }
        }*/

        private async Task SendData(string jsonData)
        {
            var reqMsg = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(jsonData,Encoding.UTF8, "application/json")
            };
            reqMsg.Headers.Add("Authorization", "Bearer " + Auth);
            await _client.SendAsync(reqMsg);
        }
        private Dictionary<string, object> CreateObject(LogEventInfo logEvent)
        {
            var document = new Dictionary<string, object>();

            // extra fields
            for (int i = 0; i < Fields.Count; ++i)
            {
                var value = GetValue(Fields[i], logEvent);
                if (value != null)
                    document[Fields[i].Name] = value;
            }
            
            return document;
        }
        private object GetValue(JsonField field, LogEventInfo logEvent)
        {
            var value = (field.Layout != null ? RenderLogEvent(field.Layout, logEvent) : string.Empty).Trim();
            if (string.IsNullOrEmpty(value))
                return null;
            switch (field.HttpContentTypeCode)
            {
                case TypeCode.Boolean:
                    bool.TryParse(value, out var boolValue);
                    return boolValue;
                case TypeCode.DateTime:
                    DateTime.TryParse(value, out var dateTimeValue);
                    return dateTimeValue;
                case TypeCode.Double:
                    double.TryParse(value, out var doubleValue);
                    return doubleValue;
                case TypeCode.Int32:
                case TypeCode.Int64:
                    int.TryParse(value, out var intValue);
                    return intValue;
                default:
                    return value;
            }
        }

    }
}
