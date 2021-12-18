using System.Threading;

namespace MFCSoftware.Utils
{
    /// <summary>
    /// 串口实例共享同步服务
    /// </summary>
    public class ComSharingService
    {
        public static readonly SemaphoreSlim Semaphore = new(1);
    }
}
