using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class SDES
    {
        //Paths, Clave y Ruta de archivo .ini
        public string NombreArchivo { get; set; }
        private string RutaAbsolutaArchivoOriginal { get; set; }
        private string RutaAbsolutaServer { get; set; }
        public string RutaAbsolutaArchivoOperado { get; set; }

        public string CadenaCifrar { get; set; }
        private int Clave { get; set; }
        private SDES_Base UtilidadeSDES { get; set; }

        private const int LargoBuffer = 100;

        /// <summary>
        /// Constructor para el objeto SDES que opera un archivo
        /// </summary>
        /// <param name="nombreArchivo">Nombre simple del archivo Ejemplo -> "archivo.txt"</param>
        /// <param name="RutaAbsArchivo">Ruta absoluta del archivo que se cargó</param>
        /// <param name="RutaAbsServer">Ruta absoluta del server, hasta la carpeta donde está guardado el archivo</param>
        /// <param name="clave">Clave para operar SDES</param>
        public SDES(string nombreArchivo, string RutaAbsArchivo, string RutaAbsServer, int clave)
        {
            NombreArchivo = nombreArchivo;
            RutaAbsolutaArchivoOriginal = RutaAbsArchivo;
            RutaAbsolutaServer = RutaAbsServer;

            if (NombreArchivo.Split('.')[1] == "huff")
            {
                RutaAbsolutaArchivoOperado = RutaAbsServer + NombreArchivo.Split('.')[0] + ".sdes";
            }
            else if (NombreArchivo.Split('.')[1] == "sdes")
            {
                RutaAbsolutaArchivoOperado = RutaAbsServer + NombreArchivo.Split('.')[0] + ".huff";
            }


            Clave = clave;

            UtilidadeSDES = new SDES_Base();
        }

        /// <summary>
        /// Opera un archivo con SDES
        /// </summary>
        /// <param name="operacion">1 para cifrar, 2 para descifrar</param>
        public void Operar(int operacion)
        {
            //RutaAbsolutaArchivoOperado = RutaAbsolutaServer + NombreArchivo + ".scif";

            var key1 = "";
            var key2 = "";

            UtilidadeSDES.GenerarLlaves(ref key1, ref key2, Clave); //Se genera las llaves Key1 y key2

            var buffer = new byte[LargoBuffer];
            var bufferEscritura = new byte[LargoBuffer];

            using (var file = new FileStream(RutaAbsolutaArchivoOriginal, FileMode.Open))
            {
                using (var reader = new BinaryReader(file, Encoding.UTF8))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        buffer = reader.ReadBytes(LargoBuffer);

                        var contBuffer = 0;

                        foreach (var caracter in buffer)
                        {
                            var caracterBits = Convert.ToString(caracter, 2); //Se convierte a binario el byte

                            if (caracterBits.Length <= 8)
                            {

                                caracterBits = caracterBits.PadLeft(8, '0'); //Se rellena de ceros a la izquierda, si son menos de 8 bits

                                UtilidadeSDES.AplicarPermutacion("PI", ref caracterBits); //Se le hace permutacion inicial

                                //RONDA 1--------------------------------------------------------------------------------------------------------------------------------
                                var bloqueIzquierdo = "";
                                var bloqueDerecho = "";
                                var bloqueDerechoOriginal = bloqueDerecho;

                                //Los 8 bits del caracter se dividen en 4
                                UtilidadeSDES.DividirCadenaBits(4, caracterBits, ref bloqueIzquierdo, ref bloqueDerecho);
                                bloqueDerechoOriginal = bloqueDerecho; //Se guarda un original o copia del bloque derecho para trabajar con uno y tener la copia para usarla al final de la ronda 1


                                UtilidadeSDES.AplicarPermutacion("ExpandirPermutar", ref bloqueDerecho); //Se le aplica Expandir y permutar al bloque derecho de 4 bits, para obtener un expandido de 8 bits

                                var binarioResultante = "";

                                if (operacion == 1)
                                {
                                    binarioResultante = UtilidadeSDES.XOR(bloqueDerecho, key1);
                                }
                                else if (operacion == 2)
                                {
                                    binarioResultante = UtilidadeSDES.XOR(bloqueDerecho, key2);
                                }



                                //Del resultante del XOR se toman los 4 bits más a la derecha para consultar SBox0 y los 4 más a la izquierda para consultar a SBox1
                                var MasIzquierdos = binarioResultante[0].ToString() + binarioResultante[1].ToString() + binarioResultante[2].ToString() + binarioResultante[3].ToString();
                                var MasDerechos = binarioResultante[4].ToString() + binarioResultante[5].ToString() + binarioResultante[6].ToString() + binarioResultante[7].ToString();

                                var fila = 0;
                                var columna = 0;

                                UtilidadeSDES.ObtenerFilaColumna(MasIzquierdos, ref fila, ref columna);

                                var bitsResultantesSBoxes = UtilidadeSDES.ObtenerBitsSBox0(fila, columna); //Se obitnene los primeros 2 bits que devulve la SBox0

                                UtilidadeSDES.ObtenerFilaColumna(MasDerechos, ref fila, ref columna); //Se vuelve a obtener filas y columnas pero ahora del bloque 2 para SBox1

                                bitsResultantesSBoxes += UtilidadeSDES.ObtenerBitsSBox1(fila, columna); //A los 2 bits de SBox0, se le concatenan los nuevos 2 bits obtenidos SBox1

                                UtilidadeSDES.AplicarPermutacion("P4", ref bitsResultantesSBoxes); //Se le aplica una permutacion de 4 bits, a la cadena resultante de ambas SBox

                                var resultadoBits = UtilidadeSDES.XOR(bitsResultantesSBoxes, bloqueIzquierdo); //Se hace un Xor de los 4 bits resultantes de las SBoxes con los 4 bits del bloque izquierdo que no se usó en la ronda 1, hasta ahora.

                                var resultadoRonda1 = resultadoBits.ToString() + bloqueDerechoOriginal.ToString(); //Resultado de la ronda 1
                                //Termina ronda 1------------------------------------------------------------------------------------------------------------------------------

                                //RONDA 2-----------------------------------------------------------------------------------------------------------------------------------
                                //Se hace exacamente lo mismo de la ronda 1, las variables ahora tienen un 2, que representa al número de ronda
                                var bloqueIzquierdo2 = "";
                                var bloqueDerecho2 = "";
                                var bloqueDerechoOriginal2 = bloqueDerecho2;

                                //Se ingresa el izquierdo en el derecho y derecho en izquierdo con el motivo de intercambiar de lados, luego se trabaja como en ronda 1
                                UtilidadeSDES.DividirCadenaBits(4, resultadoRonda1, ref bloqueDerecho2, ref bloqueIzquierdo2);
                                bloqueDerechoOriginal2 = bloqueDerecho2;

                                UtilidadeSDES.AplicarPermutacion("ExpandirPermutar", ref bloqueDerecho2);

                                var binarioResultante2 = "";

                                if (operacion == 1)
                                {
                                    binarioResultante2 = UtilidadeSDES.XOR(bloqueDerecho2, key2);
                                }
                                else if (operacion == 2)
                                {
                                    binarioResultante2 = UtilidadeSDES.XOR(bloqueDerecho2, key1);
                                }

                                //Estos serviran para luego hacer las consultas en las SBoxes
                                var MasIzquierdos2 = binarioResultante2[0].ToString() + binarioResultante2[1].ToString() + binarioResultante2[2].ToString() + binarioResultante2[3].ToString();
                                var MasDerechos2 = binarioResultante2[4].ToString() + binarioResultante2[5].ToString() + binarioResultante2[6].ToString() + binarioResultante2[7].ToString();

                                var fila2 = 0;
                                var columna2 = 0;

                                UtilidadeSDES.ObtenerFilaColumna(MasIzquierdos2, ref fila2, ref columna2);

                                var bitsResultantesSBoxes2 = UtilidadeSDES.ObtenerBitsSBox0(fila2, columna2);

                                UtilidadeSDES.ObtenerFilaColumna(MasDerechos2, ref fila2, ref columna2);

                                bitsResultantesSBoxes2 += UtilidadeSDES.ObtenerBitsSBox1(fila2, columna2);

                                UtilidadeSDES.AplicarPermutacion("P4", ref bitsResultantesSBoxes2); //Se le aplica una permutacion de 4 bits

                                var resultadoBits2 = UtilidadeSDES.XOR(bitsResultantesSBoxes2, bloqueIzquierdo2);

                                var resultadoRonda2 = resultadoBits2.ToString() + bloqueDerechoOriginal2.ToString();

                                //Termina ronda 2--------------------------------------------------------------------------------------------------------------------

                                UtilidadeSDES.AplicarPermutacion("PInversa", ref resultadoRonda2); //Al resultadoRonda2, se le aplica Permutacion Inversa y ese resultado es el que se manda a escribir

                                bufferEscritura[contBuffer] = Convert.ToByte(Convert.ToInt32(resultadoRonda2, 2));
                                contBuffer++;
                            }
                            else
                            {
                                throw new Exception("Mayor a 8 bits");
                            }
                        }
                        //Manda a escribir al archivo el buffer
                        EscribirBuffer(bufferEscritura);
                        buffer = new byte[LargoBuffer];
                        bufferEscritura = new byte[LargoBuffer];
                    }
                }
            }
            File.Delete(RutaAbsolutaArchivoOriginal);
        }

        private void EscribirBuffer(byte[] buffer)
        {
            var escribirHasta = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != 0)
                {
                    escribirHasta++;
                }
            }

            using (var file = new FileStream(RutaAbsolutaArchivoOperado, FileMode.Append))
            {
                using (var writer = new BinaryWriter(file, Encoding.UTF8))
                {
                    writer.Write(buffer, 0, escribirHasta);
                }
            }
        }

        /// <summary>
        /// Contructor para mensajes y contraseña
        /// </summary>
        /// <param name="cadenaCifrar">Cadena que se desea operar con SDES</param>
        /// <param name="clave">Clave para cifrar, descifrar</param>
        public SDES(string cadenaCifrar, int clave)
        {
            Clave = clave;
            CadenaCifrar = cadenaCifrar;
            UtilidadeSDES = new SDES_Base();
        }

        /// <summary>
        /// Método para cifrar una contraseña
        /// </summary>
        /// <returns></returns>
        public string CifrarContraseña()
        {
            var key1 = "";
            var key2 = "";

            UtilidadeSDES.GenerarLlaves(ref key1, ref key2, Clave); //Se genera las llaves Key1 y key2

            //var cifradaBytes = new byte[Encoding.UTF8.GetBytes(CadenaCifrar).Length];
            var resultado = "";

            byte[] cadenaBytes = Encoding.UTF8.GetBytes(CadenaCifrar);

            var contBuffer = 0;

            foreach (var caracter in cadenaBytes)
            {
                var caracterBits = Convert.ToString(caracter, 2); //Se convierte a binario el byte

                if (caracterBits.Length <= 8)
                {
                    caracterBits =
                        caracterBits.PadLeft(8, '0'); //Se rellena de ceros a la izquierda, si son menos de 8 bits

                    UtilidadeSDES.AplicarPermutacion("PI", ref caracterBits); //Se le hace permutacion inicial

                    //RONDA 1--------------------------------------------------------------------------------------------------------------------------------
                    var bloqueIzquierdo = "";
                    var bloqueDerecho = "";
                    var bloqueDerechoOriginal = bloqueDerecho;

                    //Los 8 bits del caracter se dividen en 4
                    UtilidadeSDES.DividirCadenaBits(4, caracterBits, ref bloqueIzquierdo, ref bloqueDerecho);
                    bloqueDerechoOriginal =
                        bloqueDerecho; //Se guarda un original o copia del bloque derecho para trabajar con uno y tener la copia para usarla al final de la ronda 1


                    UtilidadeSDES.AplicarPermutacion("ExpandirPermutar",
                        ref bloqueDerecho); //Se le aplica Expandir y permutar al bloque derecho de 4 bits, para obtener un expandido de 8 bits

                    var binarioResultante = "";

                    binarioResultante = UtilidadeSDES.XOR(bloqueDerecho, key1);

                    //Del resultante del XOR se toman los 4 bits más a la derecha para consultar SBox0 y los 4 más a la izquierda para consultar a SBox1
                    var MasIzquierdos = binarioResultante[0].ToString() + binarioResultante[1].ToString() +
                                        binarioResultante[2].ToString() + binarioResultante[3].ToString();
                    var MasDerechos = binarioResultante[4].ToString() + binarioResultante[5].ToString() +
                                      binarioResultante[6].ToString() + binarioResultante[7].ToString();

                    var fila = 0;
                    var columna = 0;

                    UtilidadeSDES.ObtenerFilaColumna(MasIzquierdos, ref fila, ref columna);

                    var bitsResultantesSBoxes =
                        UtilidadeSDES.ObtenerBitsSBox0(fila,
                            columna); //Se obitnene los primeros 2 bits que devulve la SBox0

                    UtilidadeSDES.ObtenerFilaColumna(MasDerechos, ref fila,
                        ref columna); //Se vuelve a obtener filas y columnas pero ahora del bloque 2 para SBox1

                    bitsResultantesSBoxes +=
                        UtilidadeSDES.ObtenerBitsSBox1(fila,
                            columna); //A los 2 bits de SBox0, se le concatenan los nuevos 2 bits obtenidos SBox1

                    UtilidadeSDES.AplicarPermutacion("P4",
                        ref bitsResultantesSBoxes); //Se le aplica una permutacion de 4 bits, a la cadena resultante de ambas SBox

                    var resultadoBits =
                        UtilidadeSDES.XOR(bitsResultantesSBoxes,
                            bloqueIzquierdo); //Se hace un Xor de los 4 bits resultantes de las SBoxes con los 4 bits del bloque izquierdo que no se usó en la ronda 1, hasta ahora.

                    var resultadoRonda1 =
                        resultadoBits.ToString() + bloqueDerechoOriginal.ToString(); //Resultado de la ronda 1
                                                                                     //Termina ronda 1------------------------------------------------------------------------------------------------------------------------------

                    //RONDA 2-----------------------------------------------------------------------------------------------------------------------------------
                    //Se hace exacamente lo mismo de la ronda 1, las variables ahora tienen un 2, que representa al número de ronda
                    var bloqueIzquierdo2 = "";
                    var bloqueDerecho2 = "";
                    var bloqueDerechoOriginal2 = bloqueDerecho2;

                    //Se ingresa el izquierdo en el derecho y derecho en izquierdo con el motivo de intercambiar de lados, luego se trabaja como en ronda 1
                    UtilidadeSDES.DividirCadenaBits(4, resultadoRonda1, ref bloqueDerecho2, ref bloqueIzquierdo2);
                    bloqueDerechoOriginal2 = bloqueDerecho2;

                    UtilidadeSDES.AplicarPermutacion("ExpandirPermutar", ref bloqueDerecho2);

                    var binarioResultante2 = "";

                    binarioResultante2 = UtilidadeSDES.XOR(bloqueDerecho2, key2);

                    //Estos serviran para luego hacer las consultas en las SBoxes
                    var MasIzquierdos2 = binarioResultante2[0].ToString() + binarioResultante2[1].ToString() +
                                         binarioResultante2[2].ToString() + binarioResultante2[3].ToString();
                    var MasDerechos2 = binarioResultante2[4].ToString() + binarioResultante2[5].ToString() +
                                       binarioResultante2[6].ToString() + binarioResultante2[7].ToString();

                    var fila2 = 0;
                    var columna2 = 0;

                    UtilidadeSDES.ObtenerFilaColumna(MasIzquierdos2, ref fila2, ref columna2);

                    var bitsResultantesSBoxes2 = UtilidadeSDES.ObtenerBitsSBox0(fila2, columna2);

                    UtilidadeSDES.ObtenerFilaColumna(MasDerechos2, ref fila2, ref columna2);

                    bitsResultantesSBoxes2 += UtilidadeSDES.ObtenerBitsSBox1(fila2, columna2);

                    UtilidadeSDES.AplicarPermutacion("P4",
                        ref bitsResultantesSBoxes2); //Se le aplica una permutacion de 4 bits

                    var resultadoBits2 = UtilidadeSDES.XOR(bitsResultantesSBoxes2, bloqueIzquierdo2);

                    var resultadoRonda2 = resultadoBits2.ToString() + bloqueDerechoOriginal2.ToString();

                    //Termina ronda 2--------------------------------------------------------------------------------------------------------------------

                    UtilidadeSDES.AplicarPermutacion("PInversa",
                        ref resultadoRonda2); //Al resultadoRonda2, se le aplica Permutacion Inversa y ese resultado es el que se manda a escribir

                    //cifradaBytes[contBuffer] = Convert.ToByte(Convert.ToInt32(resultadoRonda2, 2));
                    resultado += Convert.ToInt32(resultadoRonda2, 2);
                    contBuffer++;
                }
                else
                {
                    throw new Exception("Mayor a 8 bits");
                }
            }

            return resultado;
        }

        /// <summary>
        /// Método que opera con SDES una cadena
        /// </summary>
        /// <param name="operacion">1 si es cifrado, 2 si es descifrado</param>
        /// <returns>Mensaje cifrado ó descifrado</returns>
        public string OperarMensaje(int operacion)
        {
            var key1 = "";
            var key2 = "";

            UtilidadeSDES.GenerarLlaves(ref key1, ref key2, Clave); //Se genera las llaves Key1 y key2

            var cifradaBytes = new char[Encoding.UTF8.GetBytes(CadenaCifrar).Length];

            byte[] cadena1Bytes = Encoding.UTF8.GetBytes(CadenaCifrar);
            char[] cadeBytes = Encoding.UTF8.GetChars(cadena1Bytes);
            var contBuffer = 0;

            foreach (var caracter in cadeBytes)
            {
                var caracterBits = Convert.ToString(caracter, 2); //Se convierte a binario el byte

                if (caracterBits.Length <= 8)
                {
                    caracterBits =
                        caracterBits.PadLeft(8, '0'); //Se rellena de ceros a la izquierda, si son menos de 8 bits

                    UtilidadeSDES.AplicarPermutacion("PI", ref caracterBits); //Se le hace permutacion inicial

                    //RONDA 1--------------------------------------------------------------------------------------------------------------------------------
                    var bloqueIzquierdo = "";
                    var bloqueDerecho = "";
                    var bloqueDerechoOriginal = bloqueDerecho;

                    //Los 8 bits del caracter se dividen en 4
                    UtilidadeSDES.DividirCadenaBits(4, caracterBits, ref bloqueIzquierdo, ref bloqueDerecho);
                    bloqueDerechoOriginal =
                        bloqueDerecho; //Se guarda un original o copia del bloque derecho para trabajar con uno y tener la copia para usarla al final de la ronda 1


                    UtilidadeSDES.AplicarPermutacion("ExpandirPermutar",
                        ref bloqueDerecho); //Se le aplica Expandir y permutar al bloque derecho de 4 bits, para obtener un expandido de 8 bits

                    var binarioResultante = "";

                    if (operacion == 1)
                    {
                        binarioResultante = UtilidadeSDES.XOR(bloqueDerecho, key1);
                    }
                    else if (operacion == 2)
                    {
                        binarioResultante = UtilidadeSDES.XOR(bloqueDerecho, key2);
                    }

                    //Del resultante del XOR se toman los 4 bits más a la derecha para consultar SBox0 y los 4 más a la izquierda para consultar a SBox1
                    var MasIzquierdos = binarioResultante[0].ToString() + binarioResultante[1].ToString() +
                                        binarioResultante[2].ToString() + binarioResultante[3].ToString();
                    var MasDerechos = binarioResultante[4].ToString() + binarioResultante[5].ToString() +
                                      binarioResultante[6].ToString() + binarioResultante[7].ToString();

                    var fila = 0;
                    var columna = 0;

                    UtilidadeSDES.ObtenerFilaColumna(MasIzquierdos, ref fila, ref columna);

                    var bitsResultantesSBoxes =
                        UtilidadeSDES.ObtenerBitsSBox0(fila,
                            columna); //Se obitnene los primeros 2 bits que devulve la SBox0

                    UtilidadeSDES.ObtenerFilaColumna(MasDerechos, ref fila,
                        ref columna); //Se vuelve a obtener filas y columnas pero ahora del bloque 2 para SBox1

                    bitsResultantesSBoxes +=
                        UtilidadeSDES.ObtenerBitsSBox1(fila,
                            columna); //A los 2 bits de SBox0, se le concatenan los nuevos 2 bits obtenidos SBox1

                    UtilidadeSDES.AplicarPermutacion("P4",
                        ref bitsResultantesSBoxes); //Se le aplica una permutacion de 4 bits, a la cadena resultante de ambas SBox

                    var resultadoBits =
                        UtilidadeSDES.XOR(bitsResultantesSBoxes,
                            bloqueIzquierdo); //Se hace un Xor de los 4 bits resultantes de las SBoxes con los 4 bits del bloque izquierdo que no se usó en la ronda 1, hasta ahora.

                    var resultadoRonda1 =
                        resultadoBits.ToString() + bloqueDerechoOriginal.ToString(); //Resultado de la ronda 1
                                                                                     //Termina ronda 1------------------------------------------------------------------------------------------------------------------------------

                    //RONDA 2-----------------------------------------------------------------------------------------------------------------------------------
                    //Se hace exacamente lo mismo de la ronda 1, las variables ahora tienen un 2, que representa al número de ronda
                    var bloqueIzquierdo2 = "";
                    var bloqueDerecho2 = "";
                    var bloqueDerechoOriginal2 = bloqueDerecho2;

                    //Se ingresa el izquierdo en el derecho y derecho en izquierdo con el motivo de intercambiar de lados, luego se trabaja como en ronda 1
                    UtilidadeSDES.DividirCadenaBits(4, resultadoRonda1, ref bloqueDerecho2, ref bloqueIzquierdo2);
                    bloqueDerechoOriginal2 = bloqueDerecho2;

                    UtilidadeSDES.AplicarPermutacion("ExpandirPermutar", ref bloqueDerecho2);

                    var binarioResultante2 = "";

                    if (operacion == 1)
                    {
                        binarioResultante2 = UtilidadeSDES.XOR(bloqueDerecho2, key2);
                    }
                    else if (operacion == 2)
                    {
                        binarioResultante2 = UtilidadeSDES.XOR(bloqueDerecho2, key1);
                    }

                    //Estos serviran para luego hacer las consultas en las SBoxes
                    var MasIzquierdos2 = binarioResultante2[0].ToString() + binarioResultante2[1].ToString() +
                                         binarioResultante2[2].ToString() + binarioResultante2[3].ToString();
                    var MasDerechos2 = binarioResultante2[4].ToString() + binarioResultante2[5].ToString() +
                                       binarioResultante2[6].ToString() + binarioResultante2[7].ToString();

                    var fila2 = 0;
                    var columna2 = 0;

                    UtilidadeSDES.ObtenerFilaColumna(MasIzquierdos2, ref fila2, ref columna2);

                    var bitsResultantesSBoxes2 = UtilidadeSDES.ObtenerBitsSBox0(fila2, columna2);

                    UtilidadeSDES.ObtenerFilaColumna(MasDerechos2, ref fila2, ref columna2);

                    bitsResultantesSBoxes2 += UtilidadeSDES.ObtenerBitsSBox1(fila2, columna2);

                    UtilidadeSDES.AplicarPermutacion("P4",
                        ref bitsResultantesSBoxes2); //Se le aplica una permutacion de 4 bits

                    var resultadoBits2 = UtilidadeSDES.XOR(bitsResultantesSBoxes2, bloqueIzquierdo2);

                    var resultadoRonda2 = resultadoBits2.ToString() + bloqueDerechoOriginal2.ToString();

                    //Termina ronda 2--------------------------------------------------------------------------------------------------------------------

                    UtilidadeSDES.AplicarPermutacion("PInversa",
                        ref resultadoRonda2); //Al resultadoRonda2, se le aplica Permutacion Inversa y ese resultado es el que se manda a escribir

                    cifradaBytes[contBuffer] = (char)(Convert.ToInt32(resultadoRonda2, 2));
                    //resultado += Convert.ToInt32(resultadoRonda2, 2);
                    contBuffer++;
                }
                else
                {
                    throw new Exception("Mayor a 8 bits");
                }
            }

            //var resultado = Encoding.UTF8.GetString(cifradaBytes, 0, contBuffer);

            var resultado = "";

            foreach (var caracterCifrado in cifradaBytes)
            {
                if (caracterCifrado != '\0')
                {
                    resultado += caracterCifrado;
                }
            }

            return resultado;
        }
    }
}