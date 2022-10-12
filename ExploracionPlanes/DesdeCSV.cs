using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploracionPlanes
{
    public static class DesdeCSV
    {
        public static Plantilla LeerTabla(string path)
        {
            string[] Archivo = File.ReadAllLines(path);
            int numFx = Convert.ToInt32(Archivo[0].First());
            bool TieneUK = false;
            Condicion condicion = Condicion.crear(Tipo.NumFx, Operador.igual_a, numFx);
            if (Archivo[1].Contains("UK"))
            {
                TieneUK = true;
            }
            string EstructuraAnt = "";
            for (int i = 3; i < Archivo.Length; i++)
            {
                List<IRestriccion> Restricciones = new List<IRestriccion>();
                Estructura estructura = new Estructura();
                string[] Linea = Archivo[i].Split(';');
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
                    if (Linea[1] != "" && !Linea[1].Contains("H"))
                    {
                        double valorCorrespndiente = Convert.ToDouble(Linea[1].Replace("<", ""));
                        double valorEsperado = Convert.ToDouble(Linea[2]);
                        IRestriccion restriccionDTimmerman = new RestriccionDosis().crear(estructura, "cm3", "Gy", true, valorEsperado, double.NaN, valorCorrespndiente, "Timmerman", condicion);
                        Restricciones.Add(restriccionDTimmerman);
                        if (!Linea[3].Contains("<"))
                        {
                            double valorEsperadoMax = Convert.ToDouble(Linea[3]);
                            IRestriccion restriccionDmaxTimmerman = new RestriccionDosisMax().crear(estructura, "cm3", "Gy", true, valorEsperadoMax, double.NaN, double.NaN, "Timmerman", condicion);
                            Restricciones.Add(restriccionDmaxTimmerman);
                        }
                    }
                    if (Linea[4].Contains("V"))
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
                        double valorEsperado = double.NaN;
                        double valorTolerado = double.NaN;
                        if (Linea[5]!="")
                        {
                            string nota = "UK - Optimal";
                            if (Linea[5].Contains("Report"))
                            {
                                nota += " Report";
                            }
                            else if (Linea[5].Contains("-"))
                            {
                                string[] aux = Linea[5].Replace("cc", "").Replace("%", "").Replace("<", "").Trim().Split('-');
                                valorEsperado = Convert.ToDouble(aux[0]);
                                valorTolerado = Convert.ToDouble(aux[1]);
                            }
                            else
                            {
                                string aux = Linea[5].Replace("cc", "").Replace("%", "").Replace("<", "").Trim();
                                valorEsperado = Convert.ToDouble(aux);
                            }
                            IRestriccion restriccionOptimaConsortiumVol = new RestriccionVolumen().crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado, valorCorrespondiente, nota, condicion);
                            Restricciones.Add(restriccionOptimaConsortiumVol);
                        }
                        
                        if (Linea[6]!="")
                        {
                            string nota = "UK - Mandatory";
                            if (Linea[6].Contains("Report"))
                            {
                                nota += " Report";
                            }
                            else if (Linea[6].Contains("-"))
                            {
                                string[] aux = Linea[6].Replace("cc", "").Replace("%", "").Replace("<", "").Trim().Split('-');
                                valorEsperado = Convert.ToDouble(aux[0]);
                                valorTolerado = Convert.ToDouble(aux[1]);
                            }
                            else
                            {
                                string aux = Linea[6].Replace("cc", "").Replace("%", "").Replace("<", "").Trim();
                                valorEsperado = Convert.ToDouble(aux);
                            }
                            IRestriccion restriccionOptimaConsortiumVol = new RestriccionVolumen().crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado, valorCorrespondiente, nota, condicion);
                            Restricciones.Add(restriccionOptimaConsortiumVol);
                        }
                    }
                    else if (Linea[4].ToLower().Contains("mean") || Linea[4].ToLower().Contains("med"))
                    {
                        //double valorCorrespondiente = Convert.ToDouble(Linea[4].Replace("V", "").Replace("Gy", "").Replace("*", "").Trim());
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
                        double valorEsperado = double.NaN;
                        double valorTolerado = double.NaN;
                        if (Linea[5] != "")
                        {
                            string nota = "UK - Optimal";
                            if (Linea[5].Contains("Report"))
                            {
                                nota += " Report";
                            }
                            else if (Linea[5].Contains("-"))
                            {
                                string[] aux = Linea[5].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim().Split('-');
                                valorEsperado = Convert.ToDouble(aux[0]);
                                valorTolerado = Convert.ToDouble(aux[1]);
                            }
                            else
                            {
                                string aux = Linea[5].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim();
                                valorEsperado = Convert.ToDouble(aux);
                            }
                            IRestriccion restriccionOptimaConsortiumMed = new RestriccionDosisMedia().crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado, double.NaN, nota, condicion);
                            Restricciones.Add(restriccionOptimaConsortiumMed);
                        }

                        if (Linea[6] != "")
                        {
                            string nota = "UK - Mandatory";
                            if (Linea[6].Contains("Report"))
                            {
                                nota += " Report";
                            }
                            else if (Linea[6].Contains("-"))
                            {
                                string[] aux = Linea[6].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim().Split('-');
                                valorEsperado = Convert.ToDouble(aux[0]);
                                valorTolerado = Convert.ToDouble(aux[1]);
                            }
                            else
                            {
                                string aux = Linea[6].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim();
                                valorEsperado = Convert.ToDouble(aux);
                            }
                            IRestriccion restriccionOptimaConsortiumMed = new RestriccionDosisMedia().crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado, double.NaN, nota, condicion);
                            Restricciones.Add(restriccionOptimaConsortiumMed);
                        }

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
                        double valorEsperado = double.NaN;
                        double valorTolerado = double.NaN;
                        if (Linea[5] != "")
                        {
                            string nota = "UK - Optimal";
                            if (Linea[5].Contains("Report"))
                            {
                                nota += " Report";
                            }
                            else if (Linea[5].Contains("-"))
                            {
                                string[] aux = Linea[5].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim().Split('-');
                                valorEsperado = Convert.ToDouble(aux[0]);
                                valorTolerado = Convert.ToDouble(aux[1]);
                            }
                            else
                            {
                                string aux = Linea[5].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim();
                                valorEsperado = Convert.ToDouble(aux);
                            }
                            IRestriccion restriccionOptimaConsortiumDosis = new RestriccionDosis().crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado,valorCorrespondiente, nota, condicion);
                            Restricciones.Add(restriccionOptimaConsortiumDosis);
                        }

                        if (Linea[6] != "")
                        {
                            string nota = "UK - Mandatory";
                            if (Linea[6].Contains("Report"))
                            {
                                nota += " Report";
                            }
                            if (Linea[6].Contains("-"))
                            {
                                string[] aux = Linea[6].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim().Split('-');
                                valorEsperado = Convert.ToDouble(aux[0]);
                                valorTolerado = Convert.ToDouble(aux[1]);
                            }
                            else
                            {
                                string aux = Linea[6].Replace("Gy", "").Replace("%", "").Replace("<", "").Trim();
                                valorEsperado = Convert.ToDouble(aux);
                            }
                            IRestriccion restriccionOptimaConsortiumMed = new RestriccionDosis().crear(estructura, unidadValor, unidadCorrespondiente, true, valorEsperado, valorTolerado, valorCorrespondiente, nota, condicion);
                            Restricciones.Add(restriccionOptimaConsortiumMed);
                        }

                    }

                }




            }


        }
    }
}
