using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PlayerExtension.Log
{
    /// <summary>
    /// This is an advanced useage, where you want to intercept the logging messages and devert them somewhere
    /// besides ETW.
    /// </summary>
    public sealed class LoggerEventListener : EventListener
    {
        /// <summary>
        /// Storage file to be used to write logs
        /// </summary>
        private StorageFile mStorageFile = null;

        /// <summary>
        /// Name of the current event listener
        /// </summary>
        private string mName;

        /// <summary>
        /// The format to be used by logging.
        /// </summary>
        private string mFormat = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:ffff}\tType: {1}\tId: {2}\tMessage: '{3}'";

        private SemaphoreSlim mSemaphoreSlim = new SemaphoreSlim(1);

        public LoggerEventListener(string name)
        {
            this.mName = name;

            Debug.WriteLine("StorageFileEventListener for {0} has name {1}", GetHashCode(), name);

            AssignLocalFile();
        }

        private async void AssignLocalFile()
        {
            mStorageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(mName.Replace(" ", "_") + ".log", CreationCollisionOption.OpenIfExists);
        }

        private async void WriteToFile(IEnumerable<string> lines)
        {
            await mSemaphoreSlim.WaitAsync();

            await Task.Run(async () =>
                            {
                                try
                                {
                                    await FileIO.AppendLinesAsync(mStorageFile, lines);
                                }
                                catch (Exception ex)
                                {
                                    // TODO:
                                }
                                finally
                                {
                                    mSemaphoreSlim.Release();
                                }
                            });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (mStorageFile == null) return;

            var lines = new List<string>();

            var newFormatedLine = string.Format(mFormat, DateTime.Now, eventData.Level, eventData.EventId, eventData.Payload[0]);
            
            Debug.WriteLine(newFormatedLine);

            lines.Add(newFormatedLine);

            WriteToFile(lines);
        }
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Debug.WriteLine("OnEventSourceCreated for Listener {0} - {1} got eventSource {2}", GetHashCode(), mName, eventSource.Name);
        }
    }
}