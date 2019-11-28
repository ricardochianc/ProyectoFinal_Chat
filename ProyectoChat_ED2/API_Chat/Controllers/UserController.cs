using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Chat.Models;
using API_Chat.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Utilidades;

namespace API_Chat.Controllers
{
    [Authorize]
    [Route("GuatChat/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("AllUsers")]
        public ActionResult<List<User>> GetAllUsers()
        {
            return _userService.GetAllUsers();
        }

        // GET: GuatChat/User/Chat/id
        [HttpGet("Perfil/{idEmisor:length(24)}")]
        public ActionResult<User> GetUser([FromRoute] string idEmisor)
        {
            return _userService.Get(idEmisor);
        }

        // GET: GuatChat/User/Chat/idEmisor/idReceptor
        [HttpGet("Chat/{idEmisor:length(24)}/{idReceptor:length(24)}", Name = "GetUser")]
        public ActionResult<List<Message>> Get([FromRoute]string idEmisor, [FromRoute]string idReceptor)
        {
            return _userService.GetMessages(idEmisor, idReceptor);
        }

        // POST: GuatChat/User/Create
        [HttpPost("Create")]
        public IActionResult Create([FromBody]User usuario)
        {
            var sdes = new Utilidades.SDES(usuario.Contraseña,250);
            usuario.Contraseña = sdes.CifrarContraseña();
            var USER = _userService.Create(usuario);

            if (USER != null)
            {
                return Created("GuatChat/User/Create/Conversaciones/" + USER.Id, USER);
            }

            return Conflict();
        }

        // PUT: GuatChat/User/Perfil/{id}
        //Sirve para actualizar la información del usuario, solo para actualizar contraseñas o nombre
        [HttpPut("Perfil/{id:length(24)}")]
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

        // DELETE: GuatChat/User/Perfil/Eliminar/id
        [HttpDelete("Perfil/Eliminar/{id:length(24)}")]
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

        // PUT: GuatChat/User/Chat/idEmisor/idReceptor
        [HttpPut("Chat/{idEmisor:length(24)}/{idReceptor:length(24)}", Name = "PutMessage")]
        public ActionResult UpdateMessage([FromRoute]string idEmisor,[FromRoute] string idReceptor, [FromBody] Message mensaje)
        {
            mensaje.Emisor = idEmisor;
            mensaje.Receptor = idReceptor;

            //Username compuesto se refiere a la conbinación del username y el id del objeto de mongo
            var usernameCompuesto = _userService.UpdateMessageEmisor(mensaje.Emisor, mensaje);

            var correct = _userService.UpdateMessageReceptor(mensaje.Receptor, usernameCompuesto, mensaje);

            if (correct)
            {
                return Ok(); //Se actualizaron emisor y receptor
            }

            return NoContent(); //Solo se actulizó emisor
        }

        [HttpGet("Conversaciones/{id:length(24)}")]
        public ActionResult<List<string>> GetConversations([FromRoute]string id)
        {
            return _userService.GetConversations(id);
        }

        //***************************************************************************************************************************************************

        //POST: GuatChat/authenticate
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]User userParameter)
        {
            var sdes = new SDES(userParameter.Contraseña,250);
            var jwt = _userService.Authenticate(userParameter.Username, sdes.CifrarContraseña());

            //SI DEVUELVE UN JWT NULO, QUIERE DECIR QUE HUBO UN PROBLEMA CON LA AUTENTICACION
            if(jwt == null)
            {
                return BadRequest(new { message = "USUARIO O CONTRASENA INCORRECTOS" } );
            }

            //DE LO CONTRARIO, SE DEVOLVERA UN JWT CON LA INFORMACION PERTINENTE

            return Ok(jwt);
        }
        

    }
}