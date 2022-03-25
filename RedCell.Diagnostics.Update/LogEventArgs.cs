using System;
using RedCell.Diagnostics.Update.Interface;

namespace RedCell.Diagnostics.Update
{
    public class LogEventArgs : EventArgs, ILogEventArgs
    {
        public LogEventArgs (string message)
        {
            Message = message;
            TimeStamp = DateTime.Now;
        }
        public DateTime TimeStamp { get; private set; }
        public string Message { get; private set; }
    }
}
