using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Chat.Models;
using API_Chat.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Chat.Controllers
{
    [Route("GuatChat/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET: GuatChat/User/
        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult Get(string id)
        {
            var user = _userService.GetMessages(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST: GuatChat/User
        [HttpPost]
        public IActionResult Create([FromBody]User usuario)
        {
            
            var USER =_userService.Create(usuario);

            if (USER != null)
            {
                return Created("GuatChat/user/" + usuario.Id,usuario);
            }

            return Conflict();
        }

        // PUT: api/User/5
        [HttpPut("{id:length(24)}")]
        public ActionResult Update(string id, [FromBody] User usuario)
        {
            var Usuario = _userService.Get(id);

            if (Usuario == null)
            {
                return NotFound();
            }

            _userService.UpdateUser(id, usuario);
            return Ok();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id:length(24)}")]
        public ActionResult Delete(string id)
        {
            var usuario = _userService.Get(id);

            if (usuario == null)
            {
                return NotFound();
            }

            _userService.Remove(id);
            return Ok(NoContent());
        }

        [HttpPut]
        public ActionResult Update([FromBody] Message mensaje)
        {
            _userService.UpdateMessage(mensaje.Emisor,mensaje);

            return NoContent();
        }

    }
}
