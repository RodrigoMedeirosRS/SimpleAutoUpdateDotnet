namespace RedCell.Diagnostics.Update.Interface
{
    public interface IUpdater
    {
        int DefaultCheckInterval { get; }
        int FirstCheckDelay { get; }
        string WorkPath { get; }
        void StartMonitoring();
        void StopMonitoring();
    }
}