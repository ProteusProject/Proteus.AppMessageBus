using System.Threading.Tasks;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IStoppableAsync
    {
        Task Stop();
    }
}