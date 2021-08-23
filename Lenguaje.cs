using System;
using System.Collections.Generic;
using System.Text;

namespace Lenguaje2
{
    class Lenguaje : Sintaxis
    {
        public Lenguaje()
        {
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre) : base(nombre)
        {
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
        }

        // Libreria -> (#include <identificador.h> Libreria) ?
        private void Libreria()
        {
            if (getContenido() == "")
            {

            }
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(Token.clasificaciones.identificador);

                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }

                match(">");

                Libreria();
            }
        }

        // Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");

            BloqueInstrucciones();
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones()
        {
            match(clasificaciones.inicioBloque);

            Instrucciones();

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (,Lista_IDs)? 
        private void Lista_IDs()
        {
            match(clasificaciones.identificador);
            //operaciones en la declaración de una variable
            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);
                Expresion();
            }
            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs();
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables()
        {
            match(clasificaciones.tipoDato);
            Lista_IDs();
            match(clasificaciones.finSentencia);
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion()
        {
            if (getContenido() == "if")
            {
                If();
            }
            else if (getContenido() == "for")
            {
                For();
            }
            else if (getContenido() == "while")
            {
                While();
            }
            else if (getContenido() == "do")
            {
                DoWhile();
            }
            else if (getContenido() == "cin")
            {
                match("cin");
                match(">>");
                match(clasificaciones.identificador);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida();
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante();
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables();
            }
            else if (getClasificacion() == clasificaciones.identificador)
            {
                match(clasificaciones.identificador);
                match(clasificaciones.asignacion);

                if (getClasificacion() == clasificaciones.cadena)
                {
                    match(clasificaciones.cadena);
                }
                else
                {
                    Expresion();
                }

                match(clasificaciones.finSentencia);
            }
            else
            {
                errorSintactico(linea, caracter, clasificaciones.finBloque);
            }

        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones()
        {
            Instruccion();

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones();
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante()
        {
            match("const");
            match(clasificaciones.tipoDato);
            match(clasificaciones.identificador);
            match(clasificaciones.asignacion);

            if (getClasificacion() == clasificaciones.numero)
            {
                match(clasificaciones.numero);
            }
            else
            {
                match(clasificaciones.cadena);
            }

            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida()
        {
            match("<<");

            if (getClasificacion() == clasificaciones.numero)
            {
                match(clasificaciones.numero);
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {
                match(clasificaciones.cadena);
            }
            else
            {
                match(clasificaciones.identificador);
            }

            if (getContenido() == "<<")
            {
                ListaFlujoSalida();
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If()
        {
            match("if");
            match("(");
            Condicion();
            match(")");
            BloqueInstrucciones();

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones();
            }
        }

        // Condicion -> identificador operadorRelacional identificador
        private void Condicion()
        {
            Expresion();
            match(clasificaciones.operadorRelacional);
            Expresion();
        }

        // x26 = (3+5)*8-(10-4)/2;
        // Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                match(clasificaciones.operadorTermino);
                Termino();
            }
        }
        // Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                match(clasificaciones.operadorFactor);
                Factor();
            }
        }
        // Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                match(clasificaciones.identificador);
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                match(clasificaciones.numero);
            }
            else
            {
                match("(");
                Expresion();
                match(")");
            }
        }
        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For()
        {
            match("for");
            match("(");
            //¿Se puede declarar la variable aqui?
            match(clasificaciones.identificador);
            match("=");
            Expresion();
            match(clasificaciones.finSentencia);
            Condicion();
            match(clasificaciones.finSentencia);
            match(clasificaciones.identificador);
            match(clasificaciones.incrementoTermino);
            match(")");
            BloqueInstrucciones();
        }
        // While -> while (Condicion) BloqueInstrucciones
        private void While()
        {
            match("while");
            match("(");
            Condicion();
            match(")");
            BloqueInstrucciones();
        }
        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile()
        {
            match("do");
            BloqueInstrucciones();
            match("while");
            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }
    }
}