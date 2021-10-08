using System;
using System.Collections.Generic;
using System.Text;

// Requerimiento 1: Implementar el not en el IF                                     LISTO
// Requerimiento 2: Validar la asignacion de STRING en instruccion                  LISTO
// Requerimiento 3: Implementar la comparacion de tipos de datos en Lista_ID        LISTO
// Requerimiento 4: Validar los tipos de datos en la asignacion del cin             LISTO   
// Requerimiento 5: Implementar el cast                                             LISTO


namespace Lenguaje2
{
    class Lenguaje: Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo maxBytes;
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

            BloqueInstrucciones(true);            
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {
            match(clasificaciones.inicioBloque);

            Instrucciones(ejecuta);

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(string tipo, bool ejecuta)
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
                match(clasificaciones.asignacion);
                string valor = getContenido();
                if(getClasificacion() == clasificaciones.cadena)
                {
                    if(tipo == "string")
                    {
                        match(clasificaciones.cadena);
                        if(ejecuta)
                        {
                            l.setValor(nombre, valor);
                        }
                    }
                    else
                    {
                        throw new Error(bitacora, "Error de semantica: no se puede asignar un STRING a un (" + tipo + ") " + "(" + linea + ", " + caracter + ")");
                    }
                }
                else
                {
                    //requerimiento 3
                    maxBytes = Variable.tipo.CHAR;
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
        
                    if(ejecuta)
                    {
                        if(tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                        {
                            maxBytes = tipoDatoExpresion(float.Parse(valor));
                        }  
                        if(maxBytes > l.getTipoDato(nombre))
                        {
                            throw new Error(bitacora, "Error de semantica: no se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                        }   
                        l.setValor(nombre, valor);
                    }
                }
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(tipo, ejecuta);
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            //requerimento 3
            string tipo = getContenido();
           // Console.WriteLine("Tipo de dato normal: " + tipo);
            match(clasificaciones.tipoDato);
            Lista_IDs(tipo, ejecuta);
            match(clasificaciones.finSentencia);           
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DoWhile(ejecuta);
            }
            else if (getContenido() == "while")
            {
                While(ejecuta);
            }
            else if (getContenido() == "for")
            {
                For(ejecuta);
            }
            else if (getContenido() == "if")
            {
                If(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                // Requerimiento 4
                match("cin");
                match(clasificaciones.flujoEntrada);
                string nombre = getContenido();
                if(l.Existe(nombre))
                {
                    match(clasificaciones.identificador); // Validar existencia
                    if(ejecuta)
                    {
                        string valor;
                        maxBytes = l.getTipoDato(nombre);
                        //Console.WriteLine(maxBytes);
                        valor = Console.ReadLine();

                        //Console.WriteLine(tipoDatoExpresion(float.Parse(valor)));
                        //comparar maxBytes con el tipo de dato introducido
                        if(tipoDatoExpresion(float.Parse(valor)) <= maxBytes)
                        {
                            l.setValor(nombre, valor);
                        }
                        else
                        {
                            throw new Error(bitacora, "Error de semantica: no se puede asignar un " + tipoDatoExpresion(float.Parse(valor)) + " a un (" + maxBytes + ") " + "(" + linea + ", " + caracter + ")");
                        }
                    }
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
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
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
                //Requerimiento 2
                if (getClasificacion() == clasificaciones.cadena)
                {   
                    Variable.tipo comprobacion = l.getTipoDato(nombre);
                    valor = getContenido();         
                    match(clasificaciones.cadena);
                    if(comprobacion != Variable.tipo.STRING)
                    {
                        throw new Error(bitacora, "Error de semantica: no se puede asignar un STRING a un (" + comprobacion + ") " + "(" + linea + ", " + caracter + ")");
                    }                  
                }
                else
                {                
                    //Requerimiento 3 
                    maxBytes = Variable.tipo.CHAR;  
                    Expresion();
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    //Console.WriteLine(tipoDatoExpresion(float.Parse(valor)));  
                    if(tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }  
                    if(maxBytes > l.getTipoDato(nombre))
                    {
                        throw new Error(bitacora, "Error de semantica: no se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(nombre) + ") " + "(" + linea + ", " + caracter + ")");
                    }              
                }                
                
                if(ejecuta)
                {
                    l.setValor(nombre, valor); 
                }
                match(clasificaciones.finSentencia);
            }
        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {
            match("const");
            string tipo = getContenido();
            match(clasificaciones.tipoDato);
            string nombre = getContenido();
            match(clasificaciones.identificador); // Validar duplicidad
            if (!l.Existe(nombre) && ejecuta)
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
            if(ejecuta)
            {
                l.setValor(nombre, valor);
            }
            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if(ejecuta)
                {
                    Console.Write(getContenido());
                }
                match(clasificaciones.numero); 
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {                                
                string cadena = getContenido();
                FormatoString(cadena, ejecuta);
                match(clasificaciones.cadena);
            }
            else
            {
                string nombre = getContenido();
                if (l.Existe(nombre))
                {
                    if(ejecuta)
                    {
                        Console.Write(l.getValor(nombre));
                    }
                    match(clasificaciones.identificador); // Validar existencia 
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
                }
                               
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If(bool ejecuta2)
        {
            match("if");
            match("(");
            bool ejecuta;
            if(getContenido() == "!")
            {
                match("!");
                match("(");
                ejecuta = Condicion();
                match(")");
                match(")");
                BloqueInstrucciones(!ejecuta && ejecuta2);
                if (getContenido() == "else")
                {
                    match("else");
                    BloqueInstrucciones(ejecuta && ejecuta2);
                }
            }
            else
            {
                ejecuta = Condicion();
                match(")");
                BloqueInstrucciones(ejecuta && ejecuta2);
                if (getContenido() == "else")
                {
                    match("else");
                    BloqueInstrucciones(!ejecuta && ejecuta2);
                }
            }
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora, linea, caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora, linea, caracter);

            switch(operador)
            {
                case ">":
                    return n1 > n2;
                case ">=":
                    return n1 >= n2;
                case "<":
                    return n1 < n2;
                case "<=":
                    return n1 <= n2;
                case "==":
                    return n1 == n2; 
                case "!=":
                case "<>":
                    return n1 != n2;
                default:
                    return n1 != n2;
            }
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
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);  
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "+":
                        s.push(e2+e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2-e1, bitacora, linea, caracter);
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
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter); 
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "*":
                        s.push(e2*e1, bitacora, linea, caracter);                        
                        break;
                    case "/":
                        s.push(e2/e1, bitacora, linea, caracter);
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
                //Console.Write(getContenido() + " ");
                string nombre = getContenido();
                if(l.Existe(nombre))
                {
                    s.push(float.Parse(l.getValor(getContenido())), bitacora, linea, caracter);
                    s.display(bitacora);
                    match(clasificaciones.identificador); // Validar existencia
                    if(l.getTipoDato(nombre) > maxBytes)
                    {
                        maxBytes = l.getTipoDato(nombre);
                    }
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + nombre + ") no ha sido declarada " + "(" + linea + ", " + caracter + ")");
                }
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                // Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora);
                if(tipoDatoExpresion(float.Parse(getContenido())) > maxBytes)
                {
                    maxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                }
                match(clasificaciones.numero);
            }
            else
            {
                match("(");
                bool huboCast = false;

                Variable.tipo tipoDato = Variable.tipo.CHAR;
                if(getClasificacion() == clasificaciones.tipoDato)
                {
                    huboCast = true;
                    tipoDato = determinarTipoDato(getContenido());
                    match(clasificaciones.tipoDato);
                    match(")");
                    match("(");
                }
                Expresion();
                match(")");

                if(huboCast)
                {
                    //Hacer un pop y convertir ese numero a tipoDato y meterlo al Stack
                    float n1 = s.pop(bitacora, linea, caracter);
                    //para convertir un int a char necesitamos divir entre 255 y el residuo es el resultado del cast
                    //para convertir un float a int necesitamos divir entre 65535 y el residuo es el resultado del cast
                    //para convertir un float a otro tipo de datp necesitamos redondear el numero para elimnar la parte fraccional
                    //para convertir un float a char necesitamos divir entre 65535 y despues entre 256 y el residuo es el resultado del cast
                    n1 = Cast(n1, tipoDato);
                    s.push(n1, bitacora, linea, caracter);
                    
                    //Bandera de error con el cast de int a char
                    if(tipoDato == Variable.tipo.INT && n1 <= 255)
                    {
                        maxBytes = Variable.tipo.CHAR;
                    }
                    else
                    {
                        maxBytes = tipoDato;
                    }
                }
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For(bool ejecuta)
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

            BloqueInstrucciones(ejecuta);
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones(ejecuta);
        }
        
        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile(bool ejecuta)
        {
            match("do");

            BloqueInstrucciones(ejecuta);

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        private void FormatoString(string cadena , bool ejecuta)
        {
            if(ejecuta)
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
            else
            {
                return;
            }
        }

        private Variable.tipo tipoDatoExpresion(float valor)
        {
            if(valor % 1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if(valor < 255)
            {
                return Variable.tipo.CHAR;
            }
            else if(valor < 65535)
            {
                return Variable.tipo.INT;
            }
            return Variable.tipo.FLOAT;
        }

        private Variable.tipo determinarTipoDato(string tipoDato)
        {
            Variable.tipo tipoVar;

            switch(tipoDato)
            {
                case "int":
                    tipoVar = Variable.tipo.INT;
                    break;
                
                case "float":
                    tipoVar = Variable.tipo.FLOAT;
                    break;

                case "string":
                    tipoVar = Variable.tipo.STRING;
                    break;

                default:
                    tipoVar = Variable.tipo.CHAR;  
                    break;                  
            }

            return tipoVar;
        }

        private float Cast(float numero, Variable.tipo tipoDato){
            Variable.tipo tipoActual = tipoDatoExpresion(numero);
            switch(tipoActual)
            {
                case Variable.tipo.INT:
                    if(tipoDato == Variable.tipo.CHAR)
                    {
                        return numero % 255;
                    }
                    else
                    {
                        return numero;
                    }
                case Variable.tipo.FLOAT:
                    if(tipoDato == Variable.tipo.INT)
                    {
                        return (int)Math.Round(numero % 65535);
                    }
                    if(tipoDato == Variable.tipo.CHAR)
                    {
                        return (int)Math.Round((numero % 65535) % 255);
                    }
                    else
                    {
                        return numero;
                    }
                default:
                    return numero;
            }
        }
    }
}