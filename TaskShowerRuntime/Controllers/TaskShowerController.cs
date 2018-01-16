using ConsPlus.TaskShowerModel;

namespace ConsPlus.TaskShowerRuntime.Controllers
{
    public sealed class TaskShowerController : ITaskShowerController
    {
        IShowerView _view;        
        IFileSystemModel _fsModel;
        IXmlProcessor _xmlProc;
        bool _disposed;

        public TaskShowerController(IShowerView view, IFileSystemModel fsModel, IXmlProcessor xmlProc)
        {
            _view = view;            
            _fsModel = fsModel;
            _xmlProc = xmlProc;

            _view.RequestDir += view_RequestDir;            
            _view.RequestDetails += view_RequestDetails;
            _view.ItemPicked += view_ItemPicked;
        }


        #region ITaskShowerController
        public IShowerView View
        {
            get { return _view; }
        }
        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _view.RequestDir -= view_RequestDir;                
                _view.RequestDetails -= view_RequestDetails;
                _view.ItemPicked -= view_ItemPicked;

                _fsModel.Dispose();

                _fsModel = null;
                _view = null;
                _xmlProc = null;
            }
        }

        void view_RequestDir (IShowerView sender, RequestDirArgs args)
        {
            _view.SetDirDescriptor(_fsModel.GetDir(args.Path, true));
        }
        
        void view_RequestDetails (IShowerView sender, RequestDirArgs args)
        {
            if (args.Path == null)
                _view.SetDirDetails(null);
            else
                _view.SetDirDetails(_fsModel.GetDir(args.Path, true));
        }
        void view_ItemPicked (IShowerView sender, ItemPickedArgs args)
        {
            if (args.Item.ItemType == ItemType.Dir)        
                _view.SetDirDetails(_fsModel.GetDir(args.Item.ItemPath, true));
            else if (args.Item.IsXml)
            {
                //_view.ClearDocument();
                var validationRes = _xmlProc.ValidateSchema(args.Item.ItemPath, "<br/>" );
                if (!validationRes.Item1)
                {                
                    _view.ShowDocumentMsg(string.Format("XML '{0}' doesn't match to the schema:<br/>{1}", args.Item.Sysname, validationRes.Item2));
                    return;
                }
                _view.ShowHtml(_xmlProc.RenderXml(args.Item.ItemPath));
            }
            else
            {
                _view.ShowDocumentMsg(string.Format("The file '{0}' isn't an XML", args.Item.Sysname));
            }
        }
    }
}
