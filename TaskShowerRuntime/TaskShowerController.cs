using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsPlus.TaskShowerModel;

namespace ConsPlus.TaskShowerRuntime
{
    public sealed class TaskShowerController : ITaskShowerController
    {
        readonly IShowerView _view;
        readonly ITaskShowerModel _model;

        public TaskShowerController (IShowerView view, ITaskShowerModel model)
        {
            _view = view;
            _model = model;
        }

        #region ITaskShowerController
        public IShowerView View
        {
            get { return _view; }
        }
        #endregion
    }
}
