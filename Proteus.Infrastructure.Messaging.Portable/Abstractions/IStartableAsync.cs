using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IStartableAsync
    {
        Task Start();
    }
}