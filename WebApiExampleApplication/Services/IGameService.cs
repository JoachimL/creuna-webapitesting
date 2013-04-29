using WebApiExampleApplication.Models;

namespace WebApiExampleApplication.Services
{
    public interface IGameService
    {
        /// <summary>
        /// Creates a new game in the store.
        /// </summary>
        /// <param name="userName">The name of the user attemping the create operation.</param>
        /// <param name="game">Model of the game to create.</param>
        /// <returns>The unique id of the newly created game.</returns>
        int CreateNewGame(string userName, Game game);
    }
}