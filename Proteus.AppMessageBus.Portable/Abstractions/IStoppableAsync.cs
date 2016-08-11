using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IStoppableAsync
    {
        Task Stop();
    }
}