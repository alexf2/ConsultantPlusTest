using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConsPlus.TaskShowerModel;
using Ifx.FoundationHelpers.General;

namespace TasksShower
{
    public partial class MainForm: Form, IShowerView
    {
        readonly IAbstractLogger _logger;

        public MainForm (IAbstractLogger logger)
        {
            _logger = logger;
            InitializeComponent();
        }

        #region IShowerView
        public IDirProvider DirectoryProvider
        {
            get;
            set;
        }
        #endregion IShowerView
    }
}
