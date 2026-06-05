using CrimeGame.Core.Models;

namespace CrimeGame.Core.Commands
{
    public interface ICommand
    {
        void Execute(WorldState state);
    }
}
