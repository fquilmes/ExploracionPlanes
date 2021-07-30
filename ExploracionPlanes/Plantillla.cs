﻿using System;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ExploracionPlanes
{
    public class Plantilla
    {

        public static string pathDestino = Properties.Settings.Default.Path + @"\Plantillas\";
        public string IDpaciente { get; set; }
        public string plan { get; set; }
        public string nombre { get; set; }
        public string etiqueta { get; set; }
        public bool esParaExtraccion { get; set; }
        public BindingList<IRestriccion> listaRestricciones { get; set; }
        public string nota { get; set; }
        public string path { get; set; }

        public static Plantilla crear(string _nombre, bool _esParaExtraccion, BindingList<IRestriccion> _listaRestricciones, string _nota)
        {
            Plantilla plantilla = new Plantilla()
            {
                nombre = _nombre,
                etiqueta = _nombre,
                esParaExtraccion = _esParaExtraccion,
                listaRestricciones = _listaRestricciones,
                nota = _nota,
            };
            if (_esParaExtraccion)
            {
                plantilla.etiqueta += " (para Extracción)";
            }
            return plantilla;
        }

        public void guardar(bool edita, Plantilla plantillaAEditar = null)
        {
            if (!Directory.Exists(pathDestino))
            {
                Directory.CreateDirectory(pathDestino);
            }
            if (edita)
            {
                File.Delete(plantillaAEditar.path);
            }
            string fileName = IO.GetUniqueFilename(pathDestino, nombre);
            path = fileName;
            IO.writeObjectAsJson(path, this);
            MessageBox.Show("Se ha guardado la plantilla con el nombre: " + Path.GetFileName(path));
        }

        public void actualizarPath(string _nuevoPath)
        {
            path = _nuevoPath;
            IO.writeObjectAsJson(path, this);
        }

        public void duplicar(string nombreNuevo)
        {
            Plantilla duplicada = crear(nombreNuevo, this.esParaExtraccion, this.listaRestricciones, this.nota);
            duplicada.guardar(false);

        }

        public bool tieneCondicionesTipo1()
        {
            foreach (IRestriccion restriccion in listaRestricciones)
            {
                if (restriccion.condicion != null && (restriccion.condicion.tipo == Tipo.NumFx || restriccion.condicion.tipo == Tipo.VolPTV))
                {
                    return true;
                }
            }
            return false;
        }

        public bool tieneCondicionesTipo2()
        {
            foreach (IRestriccion restriccion in listaRestricciones)
            {
                if (restriccion.condicion != null && (restriccion.condicion.tipo == Tipo.CondicionadaPor))
                {
                    return true;
                }
            }
            return false;
        }

        public bool tienePrioridades()
        {
            foreach (IRestriccion restriccion in listaRestricciones)
            {
                if (restriccion.prioridad != null && restriccion.prioridad != "")
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Plantilla> leerPlantillas()
        {
            pathDestino = Properties.Settings.Default.Path + @"\Plantillas\";
            List<Plantilla> lista = new List<Plantilla>();
            if (Directory.Exists(pathDestino))
            {
                string[] plantillasPath = Directory.GetFiles(pathDestino);
                foreach (string plantillaPath in plantillasPath)
                {
                    Plantilla p = IO.readJson<Plantilla>(plantillaPath);
                    if (p.path != plantillaPath)
                    {
                        p.actualizarPath(plantillaPath);
                    }
                    lista.Add(p);
                }
            }
            else
            {
                Directory.CreateDirectory(pathDestino);
            }
            return lista;
        }

        public List<Estructura> estructuras()
        {
            List<Estructura> estructuras = new List<Estructura>();
            foreach (IRestriccion restriccion in this.listaRestricciones)
            {
                if (!estructuras.Any(e => e.nombre == restriccion.estructura.nombre))
                {
                    estructuras.Add(restriccion.estructura);
                }
            }
            return estructuras;
        }

        public List<Estructura> estructurasParaPrescribir()
        {
            List<Estructura> estructuras = new List<Estructura>();
            foreach (IRestriccion restriccion in this.listaRestricciones)
            {
                if (restriccion.dosisEstaEnPorcentaje() && !estructuras.Any(e => e.nombre == restriccion.estructura.nombre))
                {
                    estructuras.Add(restriccion.estructura);
                }
            }
            return estructuras;
        }

        public void eliminar()
        {
            if (MessageBox.Show("¿Desea eliminar la plantilla?", "Eliminar Plantilla", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                File.Delete(path);
            }
        }

        public void editar(TextBox TB_nombre, CheckBox CHB_esParaExtraer, BindingList<IRestriccion> ListaRestricciones, TextBox TB_notaPlantilla)
        {
            TB_nombre.Text = nombre;
            if (esParaExtraccion)
            {
                CHB_esParaExtraer.Checked = true;
            }
            foreach (IRestriccion restriccion in listaRestricciones)
            {
                ListaRestricciones.Add(restriccion);
            }
            TB_notaPlantilla.Text = nota;

        }

        public void editar(TextBox TB_nombre, CheckBox CHB_esParaExtraer, BindingList<IRestriccion> ListaRestricciones, TextBox TB_notaPlantilla, BindingList<Condicion> listaCondicionesNumFracc, BindingList<Condicion> listaCondicionesVolPTV)
        {
            TB_nombre.Text = nombre;
            if (esParaExtraccion)
            {
                CHB_esParaExtraer.Checked = true;
            }
            foreach (IRestriccion restriccion in listaRestricciones)
            {
                ListaRestricciones.Add(restriccion);
                if (restriccion.condicion != null && restriccion.condicion.tipo == Tipo.NumFx)
                {
                    listaCondicionesNumFracc.Add(restriccion.condicion);
                }
                else if (restriccion.condicion != null && restriccion.condicion.tipo == Tipo.VolPTV)
                {
                    listaCondicionesVolPTV.Add(restriccion.condicion);
                }
            }
            TB_notaPlantilla.Text = nota;
        }

        public void actualizarEtiquetas()
        {
            for (int i = 0; i < listaRestricciones.Count; i++)
            {
                IRestriccion restriccion = listaRestricciones[i];
                if (restriccion.condicion != null && restriccion.condicion.tipo == Tipo.CondicionadaPor)
                {
                    int ubicacion = listaRestricciones.IndexOf(restriccion);
                    listaRestricciones.Remove(restriccion);
                    restriccion.crearEtiqueta();
                    listaRestricciones.Insert(ubicacion, restriccion);
                }

            }
            guardar(true, this);
        }

        public double ContarEstructurasCoincidentes(PlanningItem plan)
        {
            double coincidencias = 0;
            foreach (Estructura estructura in estructuras())
            {
                if (Estructura.asociarConLista(estructura.nombresPosibles, Estructura.listaEstructuras(plan)) != null)
                {
                    coincidencias++;
                }
            }
            /*if (coincidencias>2)
            {
                MessageBox.Show(this.nombre + " " + coincidencias.ToString() + " " + estructuras().Count.ToString() + ((PlanSetup)plan).StructureSet.Structures.Where(s => !s.IsEmpty).Count());
            }*/
            return coincidencias*coincidencias/(estructuras().Count);
        }

        public bool sonMismaPlantilla(Plantilla otraPlantilla) //para ver si analizan lo mismo
        {
            if (nombre == otraPlantilla.nombre && listaRestricciones.Count != otraPlantilla.listaRestricciones.Count)
            {
                return false;
            }
            for (int i = 0; i < listaRestricciones.Count; i++)
            {
                if (listaRestricciones[i].etiqueta != otraPlantilla.listaRestricciones[i].etiqueta)
                {
                    return false;
                }
            }
            return true;
        }
        public static Plantilla SeleccionarAutomaticamentePlantilla(PlanningItem plan)
        {
            List<Tuple<Plantilla, double>> Coincidencias = new List<Tuple<Plantilla, double>>();
            foreach (Plantilla plantilla in Plantilla.leerPlantillas())
            {
                Coincidencias.Add(new Tuple<Plantilla, double>(plantilla, plantilla.ContarEstructurasCoincidentes(plan)));
            }
            double mayorCoincidencia = Coincidencias.OrderByDescending(c => c.Item2).First().Item2;
            List<Plantilla> plantillasMayorCoincidencia = Coincidencias.Where(c => c.Item2 == mayorCoincidencia).Select(t => t.Item1).ToList();
            if (plantillasMayorCoincidencia.Count() > 1)
            {
                PlanSetup planSetup;
                if (plan is PlanSetup)
                {
                    planSetup = (PlanSetup)plan;
                }
                else
                {
                    planSetup = ((PlanSum)plan).PlanSetups.First();
                }
                return reconocerPlantillaFino(plantillasMayorCoincidencia, planSetup);
            }
            return plantillasMayorCoincidencia.First();

        }

        private static Plantilla reconocerPlantillaFino(List<Plantilla> plantillas, PlanSetup planSetup)
        {
            IQueryable<Plantilla> query = plantillas.AsQueryable();

            if (query.Count() > 1  && query.Any(p => p.nombre.ToLower().Contains("imrt")))
            {
                if (planSetup.Beams.First().ControlPoints.Count > 20) //Es IMRT o VMAT
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("imrt")).AsQueryable();
                }
                else
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("3dc")).AsQueryable();
                }
            }
            if (query.Count()>1 && query.Any(p => p.nombre.ToLower().Contains("hipo")))
            {
                if (planSetup.UniqueFractionation.NumberOfFractions == 15) // Mama hipofraccionada
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("hipo")).AsQueryable();
                }
                else
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("normo")).AsQueryable();
                }
            }
            if (query.Count() > 1 &&  query.Any(p => p.nombre.ToLower().Contains("der"))) //mama der vs izq
            {
                if ((planSetup.Beams.First().IsocenterPosition.x - planSetup.StructureSet.Image.UserOrigin.x) < 0)
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("der")).AsQueryable();
                }
                else
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("izq")).AsQueryable();
                }
            }

            if (query.Count() > 1 && query.Any(p => p.nombre.ToLower().Contains("pros"))) //prostata vs pelvis
            {
                if (planSetup.UniqueFractionation.NumberOfFractions > 35)
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("pros")).AsQueryable();
                }
                else
                {
                    query = query.Where(p => p.nombre.ToLower().Contains("pel")).AsQueryable();
                }
            }
            if (query.Count()>0)
            {
                return query.First();
            }
            else
            {
                return plantillas.First();
            }

        }

        public bool TieneRestriccionEnPlanMod()
        {
            return listaRestricciones.Any(r=>!string.IsNullOrEmpty(r.planMod));
        }

        public string ExtensionPlanMod()
        {
            return listaRestricciones.Where(r => !string.IsNullOrEmpty(r.planMod)).First().planMod;
        }
    }
}
