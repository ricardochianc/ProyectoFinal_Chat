using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using MVC_Chat.Models;

namespace MVC_Chat.Models
{
    public class User
    {
        
        public string Id { get; set; }

        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "DEBE INGRESAR SU NOMBRE")]
        public string Nombre { get; set; }

        [Display(Name = "Apellido")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "DEBE INGRESAR SU APELLIDO")]
        public string Apellido { get; set; }

        [Display(Name = "Usuario")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "DEBE INGRESAR UN NOMBRE DE USUARIO")]
        public string Username { get; set; }

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "DEBE INGRESAR UNA CONTRASENA")]
        public string Contraseña { get; set; }

        [Display(Name = "Telefono")]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "DEBE INGRESAR SU NUMERO DE TELEFONO")]
        public int Telefono { get; set; }
                
        public List<Message> Mensajes { get; set; }
                
        public List<string> Conversaciones { get; set; }
    }
}