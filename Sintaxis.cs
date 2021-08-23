using System;
using System.Collections.Generic;
using System.Text;

namespace Lenguaje2
{
    class Sintaxis : Lexico
    {
        protected int caracterAnterior;
        public Sintaxis()
        {
            Console.WriteLine("Iniciando analisis sintactico.");
            caracterAnterior = 0;
            NextToken();
        }

        public Sintaxis(string nombre) : base(nombre)
        {
            Console.WriteLine("Iniciando analisis sintactico.");
            caracterAnterior = 0;
            NextToken();
        }

        protected void match(string espera)
        {
            // Console.WriteLine(getContenido() + " = " + espera);
            if (espera == getContenido())
            {
                NextToken();
                caracterAnterior = caracter;
            }
            else
            {
                errorSintactico(linea, caracter, espera);
            }
        }

        protected void match(clasificaciones espera)
        {
            // Console.WriteLine(getContenido() + " = " + espera);
            if (espera == getClasificacion())
            {
                caracterAnterior = caracter;
                NextToken();
            }
            else
            {
                errorSintactico(linea, caracterAnterior, espera);
            }
        }

        protected void errorSintactico(int linea, int caracter, string espera)
        {
            string mensaje = String.Format("Error de sintaxis linea {0} caracter {1}: " +
                                               "Se espera un {2}", linea, caracter, espera);

            bitacora.WriteLine(mensaje);
            throw new Exception(mensaje);
        }

        protected void errorSintactico(int linea, int caracter, clasificaciones espera)
        {
            string mensaje = String.Format("Error de sintaxis linea {0} caracter {1}: " +
                                               "Se espera un {2}", linea, caracter, espera);

            bitacora.WriteLine(mensaje);
            throw new Exception(mensaje);
        }
    }
}