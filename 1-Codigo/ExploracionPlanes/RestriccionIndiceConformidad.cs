﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ExploracionPlanes
{
    public class RestriccionIndiceConformidad : IRestriccion
    {
        public Condicion condicion { get; set; }
        public Estructura estructura { get; set; }
        public string unidadValor { get; set; }
        public string unidadCorrespondiente { get; set; }
        public bool esMenorQue { get; set; }
        public double valorCorrespondiente { get; set; }
        public double valorMedido { get; set; }
        public double valorEsperado { get; set; }
        public double valorTolerado { get; set; }
        public double prescripcionEstructura { get; set; }
        public string etiquetaInicio { get; set; }
        public string etiqueta { get; set; }
        public string nota { get; set; }
        public string prioridad { get; set; }
        public string planMod { get; set; }

        public int cumple()
        {
            if (esMenorQue)
            {
                if (valorMedido <= valorEsperado)
                {
                    return 0;
                }
                else if (!Double.IsNaN(valorTolerado) && valorMedido <= valorTolerado)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (valorMedido >= valorEsperado)
                {
                    return 0;
                }
                else if (!Double.IsNaN(valorTolerado) && valorMedido >= valorTolerado)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        public IRestriccion crear(Estructura _estructura, string _unidadValor, string _unidadCorrespondiente, bool _esMenorQue,
   double _valorEsperado, double _valorTolerado, double _valorCorrespondiente, string _nota, Condicion _condicion = null, string _prioridad = "", string _planMod="")

        {
            RestriccionIndiceConformidad restriccion = new RestriccionIndiceConformidad()
            {
                estructura = _estructura,
                esMenorQue = _esMenorQue,
                unidadCorrespondiente = _unidadCorrespondiente,
                valorCorrespondiente = _valorCorrespondiente, //qué isodosis busco
                valorEsperado = _valorEsperado,
                valorTolerado = _valorTolerado,
                nota =_nota,
                condicion = _condicion,
                prioridad = _prioridad,
                planMod = _planMod,
            };
            restriccion.crearEtiquetaInicio();
            restriccion.crearEtiqueta();
            return restriccion;
        }

        public void crearEtiquetaInicio()
        {
            etiquetaInicio = estructura.nombre + ": IC" + " (" + valorCorrespondiente + "%)";
        }
        public void crearEtiqueta()
        {
            etiqueta = etiquetaInicio;
            if (!string.IsNullOrEmpty(prioridad))
            {
                etiqueta += " (p=" + prioridad + ") ";
            }
            if (!Double.IsNaN(valorEsperado))
            {
                if (esMenorQue)
                {
                    etiqueta += " < ";
                }
                else
                {
                    etiqueta += " > ";
                }
                etiqueta += valorEsperado.ToString();
                if (!Double.IsNaN(valorTolerado))
                {
                    etiqueta += " (" + valorTolerado.ToString() + ") ";
                }
            }
            if (condicion != null && (condicion.tipo == Tipo.NumFx || condicion.tipo == Tipo.VolPTV))
            {
                etiqueta += " (" + condicion.id + ")";
            }
            else if (condicion != null && (condicion.tipo == Tipo.CondicionadaPor))
            {
                etiqueta += " (" + condicion.EtiquetaRestriccionAnidada + ") ";
            }
            if (!string.IsNullOrEmpty(planMod))
            {
                etiqueta += "*";
            }

        }


        public void analizarPlanEstructura(PlanningItem plan, Structure estructura)
        {
            Structure BODY;
            if (plan is PlanSetup)
            {
                BODY = ((PlanSetup)plan).StructureSet.Structures.Where(s => s.DicomType == "EXTERNAL").FirstOrDefault();
            }
            /*else if (plan.GetType() == typeof(ExternalPlanSetup))
            {
                BODY = ((ExternalPlanSetup)plan).StructureSet.Structures.Where(s => s.DicomType == "EXTERNAL").FirstOrDefault();
            }*/
            else
            {
                BODY = ((PlanSum)plan).StructureSet.Structures.Where(s => s.DicomType == "EXTERNAL").FirstOrDefault();
            }
                
            if (BODY == null)
            {
                MessageBox.Show("No se encuentra la estructura BODY. \nNo se puede analizar el I.C. de la estructura" + estructura.Id);
                valorMedido = Double.NaN;
            }
            else
            {
                double valorCorrespondienteGy = valorCorrespondiente * prescripcionEstructura / 100; //Convierto el % a Gy para extraer 
                DoseValue dosis = new DoseValue(valorCorrespondienteGy * 100, DoseValue.DoseUnit.cGy); //y acá en cGy

                if (plan is PlanSetup)
                {
                    valorMedido = Math.Round(((PlanSetup)plan).GetVolumeAtDose(BODY, dosis, VolumePresentation.AbsoluteCm3) / estructura.Volume, 3);
                }
                /*else if (plan.GetType() == typeof(ExternalPlanSetup))
                {
                    valorMedido = Math.Round(((ExternalPlanSetup)plan).GetVolumeAtDose(BODY, dosis, VolumePresentation.AbsoluteCm3) / estructura.Volume, 3);
                }*/
                else
                {
                    DVHPoint[] curveData = ((PlanSum)plan).GetDVHCumulativeData(BODY, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.01).CurveData;
                    valorMedido = Math.Round((DVHDataExtensions_ESAPIX.GetVolumeAtDose(curveData, dosis) / estructura.Volume), 3);
                }
                    
            }
        }

        public void analizarPlanEstructura(PlanningItem plan, Structure estructura, double alfaBeta, int numeroFracciones)
        {
            valorMedido = double.NaN;
        }

        public bool chequearSamplingCoverage(PlanningItem plan, Structure estructura)
        {
            if (Double.IsNaN(valorMedido))
            {
                if (plan.GetDVHCumulativeData(estructura, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.01).SamplingCoverage < 0.9)
                {
                    return true;
                }
            }
            return false;
        }

        public void agregarALista(BindingList<IRestriccion> lista)
        {
            lista.Add(this);
        }

        public bool dosisEstaEnPorcentaje()
        {
            /*if (unidadValor == "%")
            {
                return true;
            }
            else
            {
                return false;
            }*/
            return true;
        }

        public void editar(ComboBox CB_Estructura, TextBox TB_nombresAlt, ComboBox CB_TipoRestr, TextBox TB_valorCorrespondiente,
     ComboBox CB_UnidadesCorresp, ComboBox CB_EsMenorQue, TextBox TB_ValorEsperado, TextBox TB_ValorTolerado, ComboBox CB_UnidadesValor, TextBox TB_nota, ComboBox CB_Prioridad)
        {
            CB_Estructura.Text = estructura.nombre;
            for (int i = 1; i < estructura.nombresPosibles.Count; i++)
            {
                if (i > 1)
                {
                    TB_nombresAlt.Text += "\r\n";
                }
                TB_nombresAlt.Text += estructura.nombresPosibles[i];
            }
            CB_TipoRestr.SelectedIndex = 4; //cambiar en cada restriccion
            CB_Prioridad.Text = this.prioridad;
            TB_valorCorrespondiente.Text = Metodos.validarYConvertirAString(valorCorrespondiente);
            if (esMenorQue)
            {
                CB_EsMenorQue.SelectedIndex = 0;
            }
            else
            {
                CB_EsMenorQue.SelectedIndex = 1;
            }
            TB_ValorEsperado.Text = Metodos.validarYConvertirAString(valorEsperado);
            TB_ValorTolerado.Text = Metodos.validarYConvertirAString(valorTolerado);
            CB_UnidadesValor.SelectedItem = unidadValor;
            CB_UnidadesCorresp.SelectedItem = unidadCorrespondiente;
            TB_nota.Text = nota;
        }

        public void editarGrupo(List<IRestriccion> lista, DataGridView tabla, ComboBox CB_Estructura, TextBox TB_nombresAlt, ComboBox CB_TipoRestr, TextBox TB_valorCorrespondiente,
ComboBox CB_UnidadesCorresp, ComboBox CB_EsMenorQue, ComboBox CB_UnidadesValor, TextBox TB_nota, ListBox LB_TipoCondicion, ListBox LB_ListaCondiciones)
        {
            CB_Estructura.Text = estructura.nombre;
            for (int i = 1; i < estructura.nombresPosibles.Count; i++)
            {
                if (i > 1)
                {
                    TB_nombresAlt.Text += "\r\n";
                }
                TB_nombresAlt.Text += estructura.nombresPosibles[i];
            }
            CB_TipoRestr.SelectedIndex = 4; //cambiar en cada restriccion
            TB_valorCorrespondiente.Text = Metodos.validarYConvertirAString(valorCorrespondiente);
            if (esMenorQue)
            {
                CB_EsMenorQue.SelectedIndex = 0;
            }
            else
            {
                CB_EsMenorQue.SelectedIndex = 1;
            }
            foreach (IRestriccion restriccion in lista)
            {

                int indice = tabla.Columns.Add(restriccion.condicion.id, restriccion.condicion.id);
                tabla.Rows[0].Cells[indice].Value = Metodos.validarYConvertirAString(restriccion.valorEsperado);
                tabla.Rows[1].Cells[indice].Value = Metodos.validarYConvertirAString(restriccion.valorTolerado);
            }
            CB_UnidadesValor.SelectedItem = unidadValor;
            CB_UnidadesCorresp.SelectedItem = unidadCorrespondiente;
            TB_nota.Text = nota;
            LB_TipoCondicion.SelectedItem = condicion.tipo;
        }

        public string metrica()
        {
            return etiquetaInicio.Split(':')[1];
        }

        public bool cumpleCondicion(PlanningItem plan)
        {
            if (condicion == null)
            {
                return true;
            }
            else
            {
                return condicion.CumpleCondicion(plan);
            }
        }

        public override string ToString()
        {
            return etiqueta;
        }

    }
}
