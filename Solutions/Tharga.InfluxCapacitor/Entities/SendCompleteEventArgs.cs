using System;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.InfluxCapacitor.Entities
{
    public class SendCompleteEventArgs : EventArgs
    {
        public enum OutputLevel
        {
            Information,
            Warning,
            Error,
        }

        private readonly ISenderConfiguration _senderConfiguration;
        private readonly string _message;
        private readonly Exception _exception;

        public SendCompleteEventArgs(ISenderConfiguration senderConfiguration, Exception exception)
        {
            _senderConfiguration = senderConfiguration;
            _exception = exception;
            Level = OutputLevel.Error;
        }

        public SendCompleteEventArgs(ISenderConfiguration senderConfiguration, string message, int count, OutputLevel outputLevel)
        {
            _senderConfiguration = senderConfiguration;
            _message = message;
            Level = outputLevel;
        }

        public OutputLevel Level { get; }
        public string Message => _exception?.Message ?? _message;
        //public string Message { get { return "Database " + _databaseConfig.Url + "/" + _databaseConfig.Name + ". " + (_exception != null ? _exception.Message : _message); } }
    }
}