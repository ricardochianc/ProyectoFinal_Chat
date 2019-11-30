using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class Huffman
    {
        public string RutaAbosolutaServer { get; set; }
        public string RutaAbsolutaArchivoOriginal { get; set; }
        public string NombreArchivoOriginal { get; set; }
        public string NombreArchivoOperado { get; set; }

        private int CantidadCaracteres { get; set; }
        private List<ObjectCaracter> ListadoCaracteres { get; set; }
        private Dictionary<char, int> DiccionarioApariciones { get; set; }
        private Dictionary<char, string> DiccionarioPrefijos { get; set; }

        private byte[] BufferLectura { get; set; }
        private byte[] BufferEscritura { get; set; }
        private char[] BufferEscrituraDescompresion { get; set; }
        private const int largoBuffer = 100;

        public Huffman()
        {
            RutaAbosolutaServer = string.Empty;
            RutaAbsolutaArchivoOriginal = string.Empty;
            NombreArchivoOperado = string.Empty;
            NombreArchivoOriginal = string.Empty;

            CantidadCaracteres = 0;

            ListadoCaracteres = new List<ObjectCaracter>();
            DiccionarioApariciones = new Dictionary<char, int>();
            DiccionarioPrefijos = new Dictionary<char, string>();
            BufferLectura = new byte[largoBuffer];
            BufferEscritura = new byte[largoBuffer];
            BufferEscrituraDescompresion = new char[largoBuffer];
        }

        //---------------------------COMPRESION----------------------------------------------------------------------------------
        private void ObtenerTablaAparicionesProbabilidades()
        {
            //Se lee todo el archivo y se cuentan las apariciones de cada caracter del texto
            using (var file = new FileStream(RutaAbsolutaArchivoOriginal, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(file, Encoding.GetEncoding(28591)))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        BufferLectura = reader.ReadBytes(largoBuffer);
                        var chars = Encoding.UTF8.GetChars(BufferLectura); //nuevo

                        foreach (var Caracter in chars)
                        {
                            //Predicado para buscar un caracter
                            Predicate<ObjectCaracter> BuscadorCaracter = delegate (ObjectCaracter ObjCaracter)
                            {
                                return ObjCaracter.Caracter == Caracter;

                            };

                            //Si no encuentra el caracter que está leyendo en la lista de caracteres del texto
                            if (ListadoCaracteres.Find(BuscadorCaracter) == null)
                            {
                                //Crea un objeto caracter
                                var objCaracter = new ObjectCaracter(Caracter, 0, false);
                                objCaracter.CantidadRepetido++;
                                ListadoCaracteres.Add(objCaracter);
                            }
                            else
                            {
                                //Si lo encuentra le suma otra aparición en el texto
                                ListadoCaracteres.Find(BuscadorCaracter).CantidadRepetido++;
                            }
                            CantidadCaracteres++;
                        }
                    }
                }
            }

            //Se calcula la probabilidad de apariciones de cada caracter en el texto
            foreach (var objetoCaracter in ListadoCaracteres)
            {
                objetoCaracter.CalcularProbabilidad(CantidadCaracteres);
            }

            ListadoCaracteres.Sort(ObjectCaracter.OrdenarPorProbabilidad);

            //Se agrega al diccionario de apariciones el valor del caracter y su repetición en el texto
            foreach (var objetoCaracter in ListadoCaracteres)
            {
                DiccionarioApariciones.Add(objetoCaracter.Caracter, objetoCaracter.CantidadRepetido);
            }

            var linea = "";

            //Se va contruyendo la tabla que se irá a escribir al documento *.huff
            foreach (var caracter in DiccionarioApariciones)
            {
                if (caracter.Key == '\n')
                {
                    linea += "/n " + caracter.Value + "|";
                }
                else if (caracter.Key == '\t')
                {
                    linea += "/t " + caracter.Value + "|";
                }
                else if (caracter.Key == '\r')
                {
                    linea += "/r " + caracter.Value + "|";
                }
                else if (caracter.Key == ' ')
                {
                    linea += "esp " + caracter.Value + "|";
                }
                else
                {
                    linea += caracter.Key + " " + caracter.Value + "|";
                }
            }

            linea = linea.Remove(linea.Length - 1);
            linea += Environment.NewLine;

            var bufferTabla = Encoding.UTF8.GetBytes(linea);
            var tabla = Encoding.UTF8.GetChars(bufferTabla);

            //Se escribe la tabla en el arcivo
            using (var file = new FileStream(RutaAbosolutaServer + NombreArchivoOperado, FileMode.Create))
            {
                using (var writer = new BinaryWriter(file, Encoding.UTF8))
                {
                    writer.Write(tabla);
                }
            }
        }

        private void RecorrerArbol(ObjectCaracter nodo, string prefijo)
        {
            if (nodo != null)
            {
                if (nodo.Codigo != "")
                {
                    prefijo += nodo.Codigo;

                    if (nodo.EsPadre == false)
                    {
                        DiccionarioPrefijos[nodo.Caracter] = prefijo;
                        prefijo = "";
                    }

                    RecorrerArbol(nodo.Izq, prefijo);
                    RecorrerArbol(nodo.Drch, prefijo);
                }
                else
                {
                    RecorrerArbol(nodo.Izq, prefijo);
                    RecorrerArbol(nodo.Drch, prefijo);
                }
            }
        }

        private void GenerarCodigosPrefijo()
        {
            //Se agrega al diccionario<byte, string>, la key de tipo byte sirve para cuando va leyendo el texto
            //y si la encuentra en el diccionario agrega el value que es un string que va a tener el código prefijo
            foreach (var objetoCaracter in ListadoCaracteres)
            {
                if (!DiccionarioPrefijos.ContainsKey(objetoCaracter.Caracter))
                {
                    DiccionarioPrefijos.Add(objetoCaracter.Caracter, "");
                }
            }

            //Se va construyendo el "árbol" según las proobabilidades de ocurrencia de los caracteres
            while (ListadoCaracteres.Count != 1)
            {
                var nodoIzq = ListadoCaracteres[0];
                ListadoCaracteres[0] = null;
                ListadoCaracteres.RemoveAt(0);
                nodoIzq.Codigo += "0";

                var nodoDrch = ListadoCaracteres[0];
                ListadoCaracteres[0] = null;
                ListadoCaracteres.RemoveAt(0);
                nodoDrch.Codigo += "1";

                var nodoPadre = new ObjectCaracter(' ', 0, true);

                nodoPadre.Drch = nodoDrch;
                nodoPadre.Izq = nodoIzq;

                nodoPadre.Probabilidad = nodoPadre.Drch.Probabilidad + nodoPadre.Izq.Probabilidad;

                ListadoCaracteres.Add(nodoPadre);
                ListadoCaracteres.Sort(ObjectCaracter.OrdenarPorProbabilidad);
            }

            var CodigoPrefijo = "";
            RecorrerArbol(ListadoCaracteres[0], CodigoPrefijo);
            ListadoCaracteres.RemoveAt(0);

            BufferLectura = new byte[largoBuffer];
        }

        public void Comprimir()
        {
            NombreArchivoOperado = NombreArchivoOriginal.Split('.')[0] + ".huff";

            ObtenerTablaAparicionesProbabilidades();
            GenerarCodigosPrefijo();

            BufferLectura = new byte[largoBuffer];

            var lineaEnPrefijos = "";
            var escribirEn = 0;

            using (var file = new FileStream(RutaAbsolutaArchivoOriginal, FileMode.Open))
            {
                using (var reader = new BinaryReader(file, Encoding.UTF8))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        BufferLectura = reader.ReadBytes(largoBuffer);
                        var chars = Encoding.UTF8.GetChars(BufferLectura);

                        //caracterBytes significa que adentro del buffer en cada casilla hay un byte que es el caracter leído
                        foreach (var caracter in chars) //cambiado
                        {
                            if (DiccionarioPrefijos.ContainsKey(caracter))
                            {
                                lineaEnPrefijos += DiccionarioPrefijos[caracter];
                            }
                            else
                            {
                                throw new Exception("No se ha encontrado el caracter leído en la tabla de aparaciciones");
                            }
                        }

                        while (lineaEnPrefijos.Length >= 8)
                        {
                            var ochoBits = lineaEnPrefijos.Substring(0, 8); //Obtiene los primeros 8 bits de la línea

                            lineaEnPrefijos = lineaEnPrefijos.Remove(0, 8); //Elimina los 8 obtenidos anteriormente

                            BufferEscritura[escribirEn] = (byte)Convert.ToInt32(ochoBits, 2);
                            escribirEn++;
                        }

                        if (reader.BaseStream.Position == reader.BaseStream.Length) //Si ya leyó lo último del archivo y todavía hay bits, se rellena a la derecha, no a la izquierda
                        {
                            if (lineaEnPrefijos.Length > 0)
                            {
                                lineaEnPrefijos = lineaEnPrefijos.PadRight(8, '0');
                                BufferEscritura[escribirEn] = (byte)Convert.ToInt32(lineaEnPrefijos, 2);
                                escribirEn++;
                            }
                        }

                        Escribir(escribirEn);
                        escribirEn = 0;
                    }
                }
            }
            File.Delete(RutaAbsolutaArchivoOriginal);
        }

        private void Escribir(int escribirHasta)
        {
            using (var file = new FileStream(RutaAbosolutaServer + NombreArchivoOperado, FileMode.Append, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(file, Encoding.UTF8))
                {
                    writer.Write(BufferEscritura, 0, escribirHasta);
                }
            }
            BufferEscritura = new byte[largoBuffer];
        }

        private void EscribirDecompresion(int escrbirHasta)
        {
            using (var file = new FileStream(RutaAbosolutaServer + NombreArchivoOperado, FileMode.Append, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(file, Encoding.UTF8))
                {
                    writer.Write(BufferEscrituraDescompresion, 0, escrbirHasta);
                }
            }
            BufferEscrituraDescompresion = new char[largoBuffer];
        }
        //-----------------------------------------------------------------------------------------------------------------------

        //--------------------------DESCOMPRESION--------------------------------------------------------------------------------
        private int ObtenerTabla()
        {
            BufferLectura = new byte[largoBuffer];

            var leerHasta = 0;
            var lineaTabla = "";

            using (var file = new FileStream(RutaAbsolutaArchivoOriginal, FileMode.Open))
            {
                using (var reader = new BinaryReader(file, Encoding.UTF8))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        BufferLectura = reader.ReadBytes(largoBuffer);

                        for (int i = 0; i < BufferLectura.Length; i++)
                        {
                            leerHasta++;

                            if (BufferLectura[i] == 13)
                            {
                                reader.BaseStream.Position = reader.BaseStream.Length;
                                i = largoBuffer;
                            }
                        }
                    }
                    reader.BaseStream.Position = 0;
                    lineaTabla = Encoding.UTF8.GetString(reader.ReadBytes(leerHasta - 1));
                }
            }

            var itemsTabla = lineaTabla.Split('|');

            //Este objeto caracter por el momento es un string "A 12" con caracter y repeticion
            foreach (var objetoCaracter in itemsTabla)
            {
                char caracter = ' ';

                if (objetoCaracter != "" && objetoCaracter != " ")
                {
                    var repeticion = int.Parse(objetoCaracter.Split(' ')[1]);

                    if (objetoCaracter.Split(' ')[0] == "/r")
                    {
                        caracter = '\r';
                    }
                    else if (objetoCaracter.Split(' ')[0] == "/n")
                    {
                        caracter = '\n';
                    }
                    else if (objetoCaracter.Split(' ')[0] == "/t")
                    {
                        caracter = '\t';
                    }
                    else if (objetoCaracter.Split(' ')[0] == "esp")
                    {
                        caracter = ' ';
                    }
                    else
                    {
                        caracter = Convert.ToChar(objetoCaracter.Split(' ')[0]);
                    }

                    CantidadCaracteres += repeticion;

                    var _objetoCarater = new ObjectCaracter(caracter, repeticion, false);
                    ListadoCaracteres.Add(_objetoCarater);
                }
            }

            foreach (var objectCaracter in ListadoCaracteres)
            {
                objectCaracter.CalcularProbabilidad(CantidadCaracteres);
            }

            return leerHasta;
        }

        public void Descomprimir()
        {
            NombreArchivoOperado = NombreArchivoOriginal.Split('.')[0] + ".txt";

            var leerDesde = ObtenerTabla() + 1;
            GenerarCodigosPrefijo();

            BufferLectura = new byte[largoBuffer];

            var lineaEnBits = "";
            var lineaAuxBits = "";
            var bitsCaracter = "";
            var borrarPosiciones = 0;
            var escribirEn = 0;

            using (var file = new FileStream(RutaAbsolutaArchivoOriginal, FileMode.Open))
            {
                using (var reader = new BinaryReader(file, Encoding.UTF8))
                {
                    reader.BaseStream.Position = leerDesde;

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        BufferLectura = reader.ReadBytes(largoBuffer);

                        foreach (var caracter in BufferLectura)
                        {
                            var binario = Convert.ToString(caracter, 2); //Se convierte a binario el número de byte del caracter
                            lineaEnBits += binario.PadLeft(8, '0'); //Se rellena si el binario no tiene 8 bits
                        }

                        var continuar = false;
                        bitsCaracter = "";

                        while (lineaEnBits.Length > 0 && continuar == false)
                        {
                            lineaAuxBits = lineaEnBits;

                            while (!DiccionarioPrefijos.ContainsValue(bitsCaracter) && lineaAuxBits != "")
                            {
                                bitsCaracter += lineaAuxBits.Substring(0, 1);
                                lineaAuxBits = lineaAuxBits.Remove(0, 1);
                                borrarPosiciones++;

                                if (lineaAuxBits == "" && !DiccionarioPrefijos.ContainsValue(bitsCaracter))
                                {
                                    borrarPosiciones = 0;
                                    continuar = true;
                                }

                            }

                            lineaEnBits = lineaEnBits.Remove(0, borrarPosiciones);
                            borrarPosiciones = 0;

                            char key = '\0';

                            foreach (var item in DiccionarioPrefijos)
                            {
                                if (item.Value == bitsCaracter)
                                {
                                    key = item.Key;
                                }
                            }

                            if (escribirEn < largoBuffer)
                            {
                                if (key != '\0')
                                {
                                    BufferEscrituraDescompresion[escribirEn] = key;
                                    bitsCaracter = "";
                                    escribirEn++;
                                }
                            }
                            else
                            {
                                if (key != '\0')
                                {
                                    EscribirDecompresion(escribirEn);
                                    escribirEn = 0;
                                    BufferEscrituraDescompresion[escribirEn] = key;
                                    bitsCaracter = "";
                                    escribirEn++;
                                }
                            }
                        }

                        EscribirDecompresion(escribirEn);
                        escribirEn = 0;
                    }
                }
            }
            File.Delete(RutaAbsolutaArchivoOriginal);
        }
    }
}