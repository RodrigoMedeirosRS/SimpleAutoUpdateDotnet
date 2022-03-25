using System;

namespace RedCell.Diagnostics.Update.Interface
{
    public interface ILogEventArgs
    {
        DateTime TimeStamp { get; }
        string Message { get; }
    }
}