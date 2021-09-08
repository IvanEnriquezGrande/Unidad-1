using System;
using System.Collections.Generic;
using System.Text;

// Requerimiento 1: Implementar las secuencias de escape: \n, \t cuando se imprime una cadena y 
//                  eliminar las dobles comillas.
// Requerimiento 2: Levantar excepciones en la clase Stack. ERROR EN EL STACK UNDERFLOW
// Requerimiento 3: Agregar el tipo de dato en el Inserta de ListaVariables. LISTO
// Requerimiento 4: Validar existencia o duplicidad de variables. Mensaje de error: LISTO
//                  "Error de sintaxis: La variable (x26) no ha sido declarada." LISTO
//                  "Error de sintaxis: La variables (x26) está duplicada."  LISTO

namespace Lenguaje2
{
    class Lenguaje: Sintaxis
    {
        Stack s;
        ListaVariables l;
        public Lenguaje()
        {            
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre): base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        // Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {            
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);

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

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(string tipo)
        {          
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad

            if (!l.Existe(nombre))
            {
                switch(tipo)
                {
                    case "int":
                        l.Inserta(nombre, Variable.tipo.INT);
                        break;
                    case "float":
                        l.Inserta(nombre, Variable.tipo.FLOAT);
                        break;
                    case "char":
                        l.Inserta(nombre, Variable.tipo.CHAR);
                        break;
                    case "string":
                        l.Inserta(nombre, Variable.tipo.STRING);
                        break;
                }        
            }
            else
            {
                // Levantar excepción
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }                

            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);
                Expresion();
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(tipo);
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables()
        {   
            //inicia modificacion
            string tipo = getContenido(); //guarda el tipo de dato
            Console.WriteLine("\nTipo dato normal:" + tipo);
            match(clasificaciones.tipoDato);
            Lista_IDs(tipo);
            match(clasificaciones.finSentencia);           
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion()
        {
            if (getContenido() == "do")
            {
                DoWhile();
            }
            else if (getContenido() == "while")
            {
                While();
            }
            else if (getContenido() == "for")
            {
                For();
            }
            else if (getContenido() == "if")
            {
                If();
            }
            else if (getContenido() == "cin")
            {
                match("cin");
                match(clasificaciones.flujoEntrada);
                
                //Comprobar que existe 
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia
                if (!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no ha sido declarda  (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                match(clasificaciones.finSentencia);
                //fin de la modificacion
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
                //Console.WriteLine("Tipo dato normal:" + getContenido());
                Variables();
            }            
            else
            {   
                //Modificacion 
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia
                if(!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no ha sido declarda  (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                
                match(clasificaciones.asignacion);

                if (getClasificacion() == clasificaciones.cadena)
                {
                    match(clasificaciones.cadena);
                }
                else
                {
                    Expresion();
                    Console.WriteLine(s.pop(bitacora));
                }                

                match(clasificaciones.finSentencia);
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
            //inicia modificacion
            string tipo = getContenido();
            Console.WriteLine("Const Iipo: " + tipo);
            match(clasificaciones.tipoDato);
            string nombre = getContenido();         
            match(clasificaciones.identificador); // Validar duplicidad
            if (!l.Existe(nombre))
            {
                switch(tipo)
                {
                    case "int":
                        l.Inserta(nombre, Variable.tipo.INT);
                        break;
                    case "float":
                        l.Inserta(nombre, Variable.tipo.FLOAT);
                        break;
                    case "char":
                        l.Inserta(nombre, Variable.tipo.CHAR);
                        break;
                    case "string":
                        l.Inserta(nombre, Variable.tipo.STRING);
                        break;
                }
            }
            else
            {
                // Levantar excepción
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            } 
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
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                Console.Write(getContenido());
                match(clasificaciones.numero); 
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {                                
                Console.Write(getContenido());
                match(clasificaciones.cadena);
            }
            else
            {
                //Modificacion
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia
                if(!l.Existe(nombre)){
                    throw new Error(bitacora, "Error de sintaxis: Variable no ha sido declarda  (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                }
                //fin modificacion
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
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

        // Condicion -> Expresion operadorRelacional Expresion
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
                string operador = getContenido();                              
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora), e2 = s.pop(bitacora);  
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "+":
                        s.push(e2+e1, bitacora);
                        break;
                    case "-":
                        s.push(e2-e1, bitacora);
                        break;                    
                }

                s.display(bitacora);
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
                string operador = getContenido();                
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora), e2 = s.pop(bitacora); 
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "*":
                        s.push(e2*e1, bitacora);                        
                        break;
                    case "/":
                        s.push(e2/e1, bitacora);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        // Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                Console.Write(getContenido() + " ");
                //inicia modificacion
                string nombre = getContenido();
                match(clasificaciones.identificador); // Validar existencia
                if(!l.Existe(nombre))
                {
                    throw new Error(bitacora, "Error de sintaxis: Variable no ha sido declarda  (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
                } //termina modificacion
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                // Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora);
                s.display(bitacora);
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
            //inicia modificacion
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar existencia
            if(!l.Existe(nombre))
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no ha sido declarda  (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            //termina modificacion
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);

            Condicion();
            match(clasificaciones.finSentencia);
            //inicia modificacion
            nombre = getContenido();
            match(clasificaciones.identificador); // Validar existencia
            if(!l.Existe(nombre))
            {
                throw new Error(bitacora, "Error de sintaxis: Variable no ha sido declarda  (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            //fin de modificacion
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

        // x26 = (3 + 5) * 8 - (10 - 4) / 2
        // x26 = 3 + 5 * 8 - 10 - 4 / 2
        // x26 = 3 5 + 8 * 10 4 - 2 / -
    }
}