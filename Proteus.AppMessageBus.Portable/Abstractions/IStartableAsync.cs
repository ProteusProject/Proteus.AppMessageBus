using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface IStartableAsync
    {
        Task Start();
    }
}