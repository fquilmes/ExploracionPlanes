﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.Rendering.Forms;


namespace ExploracionPlanes
{
    public partial class PlantillaBlanco : Form
    {
        Plantilla plantilla;
        public PlantillaBlanco(Plantilla _plantilla)
        {
            InitializeComponent();
            plantilla = _plantilla;
            llenarDGVAnalisis();
            this.Text = plantilla.nombre;
        }

        private void llenarDGVAnalisis()
        {
            DGV_Análisis.ReadOnly = true;
            DGV_Análisis.Rows.Clear();

            DGV_Análisis.Columns[5].Width = 10;
            if (plantilla.tienePrioridades())
            {
                DGV_Análisis.Columns[1].Visible = true;
            }
            for (int i = 0; i < plantilla.listaRestricciones.Count; i++)
            {
                IRestriccion restriccion = plantilla.listaRestricciones[i];

                DGV_Análisis.Rows.Add();
                DGV_Análisis.Rows[i].Cells[0].Value = restriccion.estructura.nombre;
                DGV_Análisis.Rows[i].Cells[2].Value = restriccion.metrica();
                DGV_Análisis.Rows[i].Cells[6].Value = restriccion.nota;
                if (!string.IsNullOrEmpty(restriccion.planMod))
                {
                    DGV_Análisis.Rows[i].Cells[6].Value += " *";
                }
                if (restriccion.condicion != null && restriccion.condicion.tipo == Tipo.CondicionadaPor)
                {
                    DGV_Análisis.Rows[i].Cells[0].Value = "(" + Estructura.nombreEnDiccionario(restriccion.estructura) + ")";
                    DGV_Análisis.Rows[i].Cells[2].Value = "(" + restriccion.metrica() + ")";
                }
                string menorOmayor;
                if (restriccion.esMenorQue)
                {
                    menorOmayor = "<";
                }
                else
                {
                    menorOmayor = ">";
                }
                string valorEsperadoString;
                if (Double.IsNaN(restriccion.valorEsperado))
                {
                    valorEsperadoString = "Reportar";
                }
                else
                {
                    valorEsperadoString = menorOmayor + restriccion.valorEsperado + restriccion.unidadValor;
                }
                if (!Double.IsNaN(restriccion.valorTolerado))
                {
                    valorEsperadoString += " (" + restriccion.valorTolerado + restriccion.unidadValor + ")";
                }
                if (restriccion.prioridad != null && restriccion.prioridad != "")
                {
                    DGV_Análisis.Rows[i].Cells[1].Value = restriccion.prioridad;
                }
                DGV_Análisis.Rows[i].Cells[5].Value = valorEsperadoString;
                DGV_Análisis.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            }
            if (plantilla.TieneRestriccionEnPlanMod())
            {
                plantilla.nota += "\r\n* Restricciones se evaluarán en plan " + plantilla.ExtensionPlanMod();
            }
        }


        #region Imprimir
        private Document reporte()
        {
            return Reporte.crearReporte("", "", "", "",plantilla.nombre, plantilla.nota, "", "","",DGV_Análisis);
        }
        private void BT_GuardarReporte_Click(object sender, EventArgs e)
        {
            Reporte.exportarAPdf("", "", "", "", plantilla.nombre, reporte());
        }

        private void BT_Imprimir_Click(object sender, EventArgs e)
        {
            MigraDoc.Rendering.Printing.MigraDocPrintDocument pd = new MigraDoc.Rendering.Printing.MigraDocPrintDocument();
            var rendered = new DocumentRenderer(reporte());
            rendered.PrepareDocument();
            pd.Renderer = rendered;
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                pd.PrinterSettings = printDialog1.PrinterSettings;
                pd.Print();
            }

        }


        #endregion
    }
}