using ClientApp.Models;
using System.Linq;
namespace ClientApp.Services
{
    public class DownloadQueueService
    {
        private readonly List<DownloadItem> _queue = new();
        private readonly object _lock = new();
        public bool CheckDuplicate(string fileName)
        {
            lock (_lock)
            {
                return _queue.Any(i => i.FileName == fileName);
            }
        }
        public bool AddToQueue(DownloadItem item)
        {
            lock (_lock)
            {
                if (_queue.Any(i => i.FileName == item.FileName))
                {
                    return false;
                }

                item.Status = DownloadStatus.Waiting;
                _queue.Add(item);
                return true;
            }
        }
        
        public void RemoveFromQueue(string fileName)
        {
            lock (_lock)
            {
                _queue.RemoveAll(i => i.FileName == fileName);
            }
        }

        public List<DownloadItem> GetQueue()
        {
            lock (_lock)
            {
                return new List<DownloadItem>(_queue);
            }
        }
    }
}

