using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExploracionPlanes
{
    class Equipos
    {
        public static Dictionary<string, string> diccionario()
        {
            Dictionary<string, string> Diccionario = new Dictionary<string, string>();
            Diccionario.Add("D-2300CD", "Equipo 4");
            Diccionario.Add("2100CMLC", "Equipo 3");
            Diccionario.Add("Equipo 2 6EX", "Equipo 2");
            Diccionario.Add("Equipo1", "Equipo 1");
            Diccionario.Add("PBA_6EX_730", "Cetro");
            Diccionario.Add("QBA_600CD_523", "Q-Equipo 1");
            Diccionario.Add("EQ2_iX_827", "Q-Equipo 2");
            return Diccionario;
        }
    }
}
