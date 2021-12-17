using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.HttpClient.Abstract
{



    /// <summary>
    /// NLog message target for HttpClient.
    /// </summary>
    [Target("HttpClientAbstract")]
    public abstract class HttpClientAbstract : AsyncTaskTarget
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
        
        #endregion

        public HttpClientAbstract()
        {
            Fields = new List<JsonField>();
        }
        
        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            if (_createDocumentDelegate == null)
                _createDocumentDelegate = e => CreateObject(e);

            var documents = _createDocumentDelegate(logEvent);
            await SendObjectAsync(documents);
        }
        protected override async void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (_createDocumentDelegate == null)
                _createDocumentDelegate = e => CreateObject(e);
            var documents = logEvents.Select(info => _createDocumentDelegate);
            await SendCollectionAsync(documents);
        }

        protected abstract Task SendObjectAsync(object data);
        protected abstract Task SendCollectionAsync(IEnumerable<object> data);
        /*{
            var reqMsg = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(jsonData,Encoding.UTF8, "application/json")
            };
            reqMsg.Headers.Add("Authorization", "Bearer " + Auth);
            await _client.SendAsync(reqMsg);
        }*/
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
