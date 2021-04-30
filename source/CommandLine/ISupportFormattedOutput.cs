using System.Threading.Tasks;

namespace Octopus.CommandLine
{
    public interface ISupportFormattedOutput
    {
        Task Request();

        void PrintDefaultOutput();

        void PrintJsonOutput();
    }
}