using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploracionPlanes
{
    public static class DesdeCSV
    {
        //public static void LeerTabla(string path)
        //public static List<IRestriccion> LeerTabla()
        public static void LeerTabla()
        {
            string[] Archivo = File.ReadAllLines(@"C:\Users\Varian\Downloads\constrains SBRT_Edit - LISTA FINAL 1 FX.csv");
            int numFx = Convert.ToInt32(Archivo[0].Split(' ').First());
            BindingList<IRestriccion> Restricciones = new BindingList<IRestriccion>();
            bool TieneUK = false;
            Condicion condicion = Condicion.crear(Tipo.NumFx, Operador.igual_a, numFx);
            if (Archivo[1].Contains("UK"))
            {
                TieneUK = true;
            }
            string EstructuraAnt = "";

            for (int i = 3; i < Archivo.Length; i++)
            {
                Estructura estructura = new Estructura();
                string[] Linea = Archivo[i].Split(',');
                if (Linea.Contains("Critical"))
                {

                }
                else
                {
                    if (Linea[0] == "")
                    {
                        estructura.nombre = EstructuraAnt;
                    }
                    else
                    {
                        estructura.nombre = Linea[0];
                    }
                    if (Linea[1] != "")
                    {
                        string nota = "[1]";
                        double valorCorrespndiente = Convert.ToDouble(Linea[1].Replace("<", "").Replace("M", "").Replace("H", "").Replace("%", ""));
                        double valorEsperado = Convert.ToDouble(Linea[2]);
                        string unidadCorrespondiente = "cm3";
                        if (Linea[1].Contains("%"))
                        {
                            unidadCorrespondiente = "%";
                        }
                        if (Linea[1].Contains("H"))
                        {
                            nota += (" [H]");
                        }
                        else if (Linea[1].Contains("M"))
                        {
                            nota += (" [M]");
                        }
                        IRestriccion restriccionDTimmerman = new RestriccionDosis().crear(estructura, "Gy", unidadCorrespondiente, true, valorEsperado, double.NaN, valorCorrespndiente, nota, condicion);
                        Restricciones.Add(restriccionDTimmerman);
                        if (Linea[3] != "" && !Linea[3].Contains("<"))
                        {
                            double valorEsperadoMax = Convert.ToDouble(Linea[3]);
                            IRestriccion restriccionDmaxTimmerman = new RestriccionDosisMax().crear(estructura, "Gy", unidadCorrespondiente, true, valorEsperadoMax, double.NaN, double.NaN, nota, condicion);
                            Restricciones.Add(restriccionDmaxTimmerman);
                        }
                    }
                    if (TieneUK && Linea[4].Contains("V"))
                    {

                        double valorCorrespondiente = Convert.ToDouble(Linea[4].Replace("V", "").Replace("Gy", "").Replace("*", "").Trim());
                        string unidadCorrespondiente = "Gy";
                        string unidadValor = "";
                        if (Linea[5].Contains("cc"))
                        {
                            unidadValor = "cm3";
                        }
                        else
                        {
                            unidadValor = "%";
                        }

                        IRestriccion restriccionOptimaConsortiumVol = RestriccionConsortium(new RestriccionVolumen(), estructura, Linea, unidadValor, unidadCorrespondiente, valorCorrespondiente, condicion);
                        Restricciones.Add(restriccionOptimaConsortiumVol);

                    }
                    else if (Linea[4].ToLower().Contains("mean") || Linea[4].ToLower().Contains("med"))
                    {
                        string unidadCorrespondiente = "cm3";
                        string unidadValor = "";
                        if (Linea[5].Contains("Gy"))
                        {
                            unidadValor = "Gy";
                        }
                        else
                        {
                            unidadValor = "%";
                        }
                        IRestriccion restriccionOptimaConsortiumMed = RestriccionConsortium(new RestriccionDosisMedia(), estructura, Linea, unidadValor, unidadCorrespondiente, double.NaN, condicion);
                        Restricciones.Add(restriccionOptimaConsortiumMed);
                    }

                    else if (Linea[4].ToLower().Contains("cc"))
                    {
                        double valorCorrespondiente = Convert.ToDouble(Linea[4].Replace("cc", "").Replace("D", "").Replace("*", "").Trim());
                        string unidadCorrespondiente = "cm3";
                        string unidadValor = "";
                        if (Linea[5].Contains("Gy"))
                        {
                            unidadValor = "Gy";
                        }
                        else
                        {
                            unidadValor = "%";
                        }
                            IRestriccion restriccionOptimaConsortiumMed = RestriccionConsortium(new RestriccionDosis(), estructura, Linea, unidadValor, unidadCorrespondiente, valorCorrespondiente, condicion);
                        Restricciones.Add(restriccionOptimaConsortiumMed);
                        }

                    EstructuraAnt = estructura.nombre;

                }
            }
            string notaPlantilla = "[1] Timmerman\r\n[2] UK Consortium\r\n[H] Hombre\r\n [M] Mujer\r\n* Optimal";
            Plantilla plantilla = Plantilla.crear("Prueba SBRT 1fx", false, Restricciones, notaPlantilla);
            plantilla.guardar(false);
        }
        public static IRestriccion RestriccionConsortium(IRestriccion restriccion, Estructura estructura, string[] Linea, string unidadValor, string unidadCorrespondiente, double valorCorrespondiente, Condicion condicion)
        {
            double valorEsperado = double.NaN;
            double valorTolerado = double.NaN;
            string esperado = "";
            string tolerado = "";

            string nota = "[2]";
            if (Linea[5] != "")
            {

                esperado = Linea[5].Replace("Gy", "").Replace("%", "").Replace("<", "").Replace("cc", "").Trim();
                tolerado = "";

                if (Linea[6] != "")
                {
                    tolerado = Linea[6].Replace("Gy", "").Replace("%", "").Replace("<", "").Replace("cc", "").Trim();
                    valorEsperado = Convert.ToDouble(esperado);
                    valorTolerado = Convert.ToDouble(tolerado);
                }
                else if (Linea[5].Contains("-"))
                {
                    string[] aux = esperado.Split('-');
                    valorEsperado = Convert.ToDouble(aux[0]);
                    valorTolerado = Convert.ToDouble(aux[1]);
                    nota += " *";
                }
                else
                {
                    valorEsperado = Convert.ToDouble(esperado);
                }
            }
            else if (Linea[6] != "")
            {
                if (Linea[6].Contains("Report"))
                {
                    valorEsperado = double.NaN;
                }
                else
                {
                    esperado = Linea[6].Replace("Gy", "").Replace("%", "").Replace("<", "").Replace("cc", "").Trim();
                    valorEsperado = Convert.ToDouble(esperado);
                }
}

            restriccion = restriccion.crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado, valorCorrespondiente, nota, condicion, "", "");
            return restriccion;
        }

    }
}
