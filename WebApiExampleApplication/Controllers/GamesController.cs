using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiExampleApplication.Models;
using WebApiExampleApplication.Services;

namespace WebApiExampleApplication.Controllers
{
    public class GamesController : ApiController
    {
        private readonly IGameService _gameService;

        public GamesController()
        {
        }

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost]
        public HttpResponseMessage CreateNewGame(Game game)
        {
            try
            {
                var newGameId = _gameService.CreateNewGame(User.Identity.Name, game);
                return CreateSuccessResponseWithLinkToNewGame(newGameId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized user.", ex);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something went wrong.", ex);
            }
        }

        private HttpResponseMessage CreateSuccessResponseWithLinkToNewGame(int newGameId)
        {
            var response = Request.CreateResponse(HttpStatusCode.Created);
            var link = Url.Link("DefaultApi", new {controller = "Games", id = newGameId});
            response.Headers.Location = new Uri(link);
            return response;
        }

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.NotImplemented);
        }
    }
}