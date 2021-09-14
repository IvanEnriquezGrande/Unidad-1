using System;
using System.Collections.Generic;
using System.Text;

// Requerimiento 1: Implementar las secuencias de escape: \n, \t cuando se imprime una cadena y 
//                  eliminar las dobles comillas.   (LISTO)
// Requerimiento 2: Levantar excepciones en la clase Stack.
// Requerimiento 3: Agregar el tipo de dato en el Inserta de ListaVariables.    (LISTO)
// Requerimiento 4: Validar existencia o duplicidad de variables. Mensaje de error: (LISTO)
//                  "Error de sintaxis: La variable (x26) no ha sido declarada."
//                  "Error de sintaxis: La variables (x26) está duplicada." 
// Requerimiento 5: Modificar el valor de la variable o constante al momento de su declaración. (LISTO)

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
                    case "string":
                        l.Inserta(nombre, Variable.tipo.STRING);
                        break;
                    case "char":
                        l.Inserta(nombre, Variable.tipo.CHAR);
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
                string valor;
                match(clasificaciones.asignacion);
                Expresion();
                valor = s.pop(bitacora).ToString();
                l.setValor(nombre, valor);
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
            //requerimento 3
            string tipo = getContenido();
           // Console.WriteLine("Tipo de dato normal: " + tipo);
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
                // Requerimiento 5
                match("cin");
                match(clasificaciones.flujoEntrada);
                string nombre = getContenido();
                if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
                }
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
            else
            {
                string nombre = getContenido();
                if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
                }
                match(clasificaciones.asignacion);

                string valor;

                if (getClasificacion() == clasificaciones.cadena)
                {           
                    valor = getContenido();         
                    match(clasificaciones.cadena);                    
                }
                else
                {                    
                    Expresion();
                    valor = s.pop(bitacora).ToString();                  
                }                

                l.setValor(nombre, valor);
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
            string tipo = getContenido();
            match(clasificaciones.tipoDato);
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad
            if (!l.Existe(nombre))
            {
                switch(tipo)
                {
                    case "int":
                        l.Inserta(nombre, Variable.tipo.INT, true);
                        break;
                    case "float":
                        l.Inserta(nombre, Variable.tipo.FLOAT, true);
                        break;
                    case "string":
                        l.Inserta(nombre, Variable.tipo.STRING, true);
                        break;
                    case "char":
                        l.Inserta(nombre, Variable.tipo.CHAR, true);
                        break;
                }
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Variable duplicada (" + nombre + ") " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.asignacion);
            string valor;
            valor = getContenido();
            if (getClasificacion() == clasificaciones.numero)
            {
                //valor = getContenido();
                match(clasificaciones.numero);
            }
            else
            {
                //valor = getContenido();
                match(clasificaciones.cadena);
            }
            l.setValor(nombre, valor);
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
                string cadena = getContenido();
                FormatoString(cadena);
                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    Console.Write(l.getValor(nombre));
                    match(clasificaciones.identificador); // Validar existencia 
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
                }
                               
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

                s.push(float.Parse(l.getValor(getContenido())), bitacora);
                s.display(bitacora);
                string nombre = getContenido();
                if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
                }
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
            string nombre = getContenido();
            if(l.Existe(nombre))
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);

            Condicion();
            match(clasificaciones.finSentencia);
            nombre = getContenido();
            if(l.Existe(nombre))
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
            }
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

        private void FormatoString(string cadena)
        {
            bool flag = false;
            cadena = cadena.Remove(cadena.Length - 1, 1);
            cadena = cadena.Remove(0, 1);
            foreach(char letra in cadena)
            {
                if(letra.Equals('\\')){
                    flag = true;
                    continue;
                }
                else if(letra.Equals('t') && flag)
                {
                    Console.Write("\t");
                    flag = false;
                    continue;
                }
                else if(letra.Equals('n') && flag)
                {
                    Console.Write("\n");
                    flag = false;
                    continue;
                }
                else
                {
                    Console.Write(letra);
                    flag = false;
                }
            }
        }
        // x26 = (3 + 5) * 8 - (10 - 4) / 2
        // x26 = 3 + 5 * 8 - 10 - 4 / 2
        // x26 = 3 5 + 8 * 10 4 - 2 / -
    }
}