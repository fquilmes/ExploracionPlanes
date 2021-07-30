using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExploracionPlanes
{
    public class EQD2
    {
        public static double Dosis2Gy(double DosisFxAlt, double alfaBeta, int numeroFracciones)
        {
            return DosisFxAlt * (DosisFxAlt / numeroFracciones + alfaBeta) / (2 + alfaBeta);
        }

        public static double DosisFxAlt(double Dosis2Gy, double alfaBeta, int numeroFracciones)
        {
            //return numeroFracciones/2*(Math.Sqrt(Math.Pow(alfaBeta,2)+4/numeroFracciones*(2+alfaBeta))+alfaBeta);
            return (Math.Sqrt(Math.Pow(alfaBeta / 2, 2) + Dosis2Gy * (2 + alfaBeta) / numeroFracciones) - alfaBeta / 2)*numeroFracciones;
        }
    }
}
