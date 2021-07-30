using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ExploracionPlanes
{
    public partial class PlanesParaComparar : Form
    {
        public PlanningItem planParaComparar = null;
        public List<PlanningItem> planesContext;
        public PlanesParaComparar(List<PlanningItem> _planesContext)
        {
            InitializeComponent();
            planesContext = _planesContext;
            LB_PlanesComparar.DataSource = planesContext.ToList();
        }

        private void BT_Selecccionar_Click(object sender, EventArgs e)
        {
            planParaComparar = (PlanningItem)LB_PlanesComparar.SelectedItem;
            this.Close();
        }
    }
}
