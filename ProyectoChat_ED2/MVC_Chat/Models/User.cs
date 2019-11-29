using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MVC_Chat.Models;

namespace MVC_Chat.Models
{
    public class User
    {
        public string Id { get; set; }

        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Debe de ingresar su nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Appellido")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Debe de ingresar su apellido")]
        public string Apellido { get; set; }

        [Display(Name = "Usuario")]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Debe de ingresar un usuario")]
        public string Username { get; set; }

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Debe de ingresar una contraseña")]
        public string Contraseña { get; set; }

        [Display(Name = "Teléfono")]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Debe de ingresar un número de teléfono")]
        public int Telefono { get; set; }
                
        public List<Message> Mensajes { get; set; }
                
        public List<string> Conversaciones { get; set; }

        public User()
        {
            Id = "";
            Nombre = "";
            Apellido = "";
            Username = "";
            Contraseña = "";
            Telefono = 0;
            Mensajes = new List<Message>();
            Conversaciones = new List<string>();
        }
    }
}