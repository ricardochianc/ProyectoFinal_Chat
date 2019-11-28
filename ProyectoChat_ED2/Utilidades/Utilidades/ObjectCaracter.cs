using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    internal class ObjectCaracter : IComparable
    {
        public char Caracter { get; set; }
        public int CantidadRepetido { get; set; }
        public double Probabilidad { get; set; }
        public ObjectCaracter Drch { get; set; }
        public ObjectCaracter Izq { get; set; }
        public bool EsPadre { get; set; }
        public string Codigo { get; set; }

        public ObjectCaracter(char caracter, int repeticion, bool esPadre)
        {
            Caracter = caracter;
            CantidadRepetido = repeticion;
            EsPadre = esPadre;
            Codigo = String.Empty;
        }

        public int CompareTo(object obj)
        {
            var comparer = (ObjectCaracter)obj;

            return Probabilidad.CompareTo(comparer.Probabilidad);
        }

        public static Comparison<ObjectCaracter> OrdenarPorProbabilidad =
            delegate (ObjectCaracter caracter1, ObjectCaracter caracter2) { return caracter1.CompareTo(caracter2); };

        public void CalcularProbabilidad(int totalCaracteresDocumento)
        {
            Probabilidad = (float)CantidadRepetido / totalCaracteresDocumento;
        }
    }
}
