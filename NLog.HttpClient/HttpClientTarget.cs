using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public sealed class HttpClientTarget : TargetWithLayout
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
        private Func<AsyncLogEventInfo, Dictionary<string, object>> _createDocumentDelegate;


        [RequiredParameter]
        public string Url { get; set; }


        [RequiredParameter]
        public string Auth { get; set; }

        #endregion

        public HttpClientTarget()
        {
            Fields = new List<JsonField>();
        }

        /// <summary>
        /// Writes an array of logging events to the log target. By default it iterates on all
        /// events and passes them to "Write" method. Inheriting classes can use this method to
        /// optimize batch writes.
        /// </summary>
        /// <param name="logEvents">Logging events to be written out.</param>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 0)
                return;
            try
            {
                if (_createDocumentDelegate == null)
                    _createDocumentDelegate = e => CreateObject(e.LogEvent);
                var documents = logEvents.Select(_createDocumentDelegate);


                var body = JsonConvert.SerializeObject(documents);

                SendData(body);

                for (int i = 0; i < logEvents.Count; ++i)
                    logEvents[i].Continuation(null);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Error when writing to ws {0}", ex);

                if (ex.MustBeRethrownImmediately())
                    throw;

                for (int i = 0; i < logEvents.Count; ++i)
                    logEvents[i].Continuation(ex);

                if (ex.MustBeRethrown())
                    throw;
            }
        }

        private void SendData(string jsonData)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + Auth);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
            }
            httpWebRequest.GetResponse();
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
