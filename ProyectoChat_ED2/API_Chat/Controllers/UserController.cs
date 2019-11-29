using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
            return Ok(_userService.GetAllUsers());
        }

        // GET: GuatChat/User/Chat/id
        [HttpGet("Perfil/{idEmisor:length(24)}")]
        public ActionResult<User> GetUser([FromRoute] string idEmisor, [FromBody]User us)
        {
            return Ok(_userService.Get(idEmisor));
        }

        // GET: GuatChat/User/Chat/idEmisor/idReceptor
        [HttpGet("Chat/{idEmisor:length(24)}/{idReceptor:length(24)}", Name = "GetUser")]
        public ActionResult<List<Message>> Get([FromRoute]string idEmisor, [FromRoute]string idReceptor)
        {
            var mensajesEnConversacion = _userService.GetMessages(idEmisor, idReceptor);

            foreach (var mensaje in mensajesEnConversacion)
            {
                var sdes = new SDES(mensaje.Contenido, 250);
                mensaje.Contenido = sdes.OperarMensaje(2);
            }

            return Ok(mensajesEnConversacion);

        }

        // POST: GuatChat/User/Create
        [AllowAnonymous]
        [HttpPost("Create")]
        public IActionResult Create([FromBody]User usuario)
        {
            var sdes = new Utilidades.SDES(usuario.Contraseña,250);
            usuario.Contraseña = sdes.OperarMensaje(1);
            var USER = _userService.Create(usuario);

            if (USER != null)
            {
                return Created("Chat/" + USER.Id, USER);
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

        // DELETE: GuatChat/User/Perfil/id
        [HttpDelete("Perfil/{id:length(24)}")]
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
            mensaje.Fecha = DateTime.Now.ToString();

            var sdes = new SDES(mensaje.Contenido,250);
            mensaje.Contenido = sdes.OperarMensaje(1);

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
            return Ok(_userService.GetConversations(id));
        }

        //***************************************************************************************************************************************************

        //POST: GuatChat/User/authenticate
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]User userParameter)
        {
            var sdes = new SDES(userParameter.Contraseña,250);
            var jwt = _userService.Authenticate(userParameter.Username, sdes.OperarMensaje(1));

            //SI DEVUELVE UN JWT NULO, QUIERE DECIR QUE HUBO UN PROBLEMA CON LA AUTENTICACION
            if(jwt == null)
            {
                return BadRequest(new { message = "USUARIO O CONTRASENA INCORRECTOS" } );
            }

            //DE LO CONTRARIO, SE DEVOLVERA UN JWT CON LA INFORMACION PERTINENTE

            return Accepted(jwt);
        }
    }
}