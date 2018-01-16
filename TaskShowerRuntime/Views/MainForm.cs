using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ConsPlus.TaskShowerModel;
using Ifx.FoundationHelpers.General;
using System.Collections.Specialized;

namespace ConsPlus.TaskShowerRuntime.Views
{
    public partial class MainForm: Form, IShowerView
    {
        sealed class TreeItem
        {
            public FileSysItem Item;
            public bool Loaded;
        };

        readonly IAbstractLogger _logger;
        readonly List<IDescriptorBase> _requestedDirDescriptors = new List<IDescriptorBase>();
        TreeNode _ndLastExpanded;
        string _currentSelectedDetailsPath;
        

        public MainForm(IAbstractLogger logger)
        {
            _logger = logger;     
       
            InitializeComponent();

            treeDirs.Sorted = true;
            treeDirs.TreeViewNodeSorter = new TreeNodeDirectoryComparer();

            lstCurDir.Sorting = SortOrder.Ascending;
            lstCurDir.ListViewItemSorter = new ListViewDirectoryComparer(true);
        }

        string currentSelectedDetailsPath
        {
            get { return _currentSelectedDetailsPath; }
            set
            {
                _currentSelectedDetailsPath = value;
                lblCurDir.Text = value;
            }
        }        

        #region IShowerView
        public event RequestDirHandler RequestDir;
        public event RequestDirHandler RequestDetails;
        public event ItemPickedHandler ItemPicked;


        public void SetDirDescriptor(IDescriptorBase descr)
        {
            TreeNode nd = findParentNode(descr);
            
            treeDirs.BeginUpdate();
            TreeNodeCollection nodes = nd == null ? treeDirs.Nodes : nd.Nodes;
            try {
                clearTree(nodes);
                foreach (FileSysItem item in descr)
                    addTreeDir(nodes, item);
            }
            finally {
                treeDirs.EndUpdate();
            }
            
            subscribeForUpdates(descr);
        }

        public void SetDirDetails(IDescriptorBase descr)
        {
            lstCurDir.BeginUpdate();
            
            try
            {
                lstCurDir.Items.Clear();

                if (descr == null)
                { 
                    lblCurDir.Text = string.Empty;
                    return;
                }
                currentSelectedDetailsPath = descr.Path;

                foreach (FileSysItem item in descr)
                {
                    addDetail(item);                    
                }
                lstCurDir.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            finally
            {
                lstCurDir.EndUpdate();
            }

            subscribeForUpdates(descr);
        }

        public void ClearDocument()
        {
            htmlViewer.DocumentText = "<html><body>&nbsp;</body></html>";            
        }
        public void ShowDocumentMsg(string msg)
        {
            htmlViewer.DocumentText = string.Format("<div style=\"text-align:center;\">{0}</div>", msg);
        }
        public void ShowHtml(string html)
        {
            htmlViewer.DocumentText = html;
        }
        public void ShowHtml(System.IO.Stream stm)
        {
            htmlViewer.DocumentStream = stm;
        }
        #endregion IShowerView


        TreeNode findParentNode (IDescriptorBase descr)
        {
            TreeNode n = _ndLastExpanded;
            _ndLastExpanded = null;
            if (n != null)
                return n;

            if (descr.Path == ".")
                return null;

            n = null;
            TreeNodeCollection coll = treeDirs.Nodes;
            foreach (string s in descr.Path.Split(new char[]{'\\'}, StringSplitOptions.RemoveEmptyEntries))
            {
                n = coll.Find(s, false).FirstOrDefault();
                if (n == null)
                    throw new Exception(string.Format("Path '{0}' can't be found in the Tree", descr.Path));
                coll = n.Nodes;
            }
            return n;
        }

        void subscribeForUpdates(IDescriptorBase descr)
        {
            if (!_requestedDirDescriptors.Contains(descr))
            {
                descr.Activatewatching(true);
                descr.CollectionChanged += descr_CollectionChanged;
                _requestedDirDescriptors.Add(descr);
            }
        }

        TreeItem addTreeDir(TreeNodeCollection coll, FileSysItem item)
        {
            if (item.ItemType == ItemType.Drive || item.ItemType == ItemType.Dir)
            {
                var tn = coll.Add(item.Sysname, ViewHelpers.makeDescription(item), 0);
                tn.Tag = new TreeItem() { Item = item };
                tn.Nodes.Add("stub", "stub");
                return (TreeItem)tn.Tag;
            }
            return null;
        }

        void removeDir (TreeNodeCollection coll, FileSysItem item)
        {
            if (item.ItemType == ItemType.Drive || item.ItemType == ItemType.Dir)
            {
                TreeNode nd = coll.Find(item.Sysname, false).FirstOrDefault();
                if (nd != null)
                    coll.Remove(nd);
            }
        }

        void removeDetail (FileSysItem item)
        {
            if (string.Compare(currentSelectedDetailsPath, item.Parent.Path, true) != 0)
                return;

            var it = lstCurDir.Items.Find(item.Sysname, false).FirstOrDefault();
            if (it != null)
                lstCurDir.Items.Remove(it);
        }
        void addDetail(FileSysItem item, bool withCheck = false)
        {
            if (withCheck)
            {
                if (string.Compare(currentSelectedDetailsPath, item.Parent.Path, true) != 0)
                    return;
            }

            var listItem = new ListViewItem(ViewHelpers.makeDescription(item));
            listItem.Name = item.Sysname;
            listItem.Tag = item;
            listItem.ImageIndex = ViewHelpers.getImageIndexByType(item);
            lstCurDir.Items.Add(listItem);
        }
        

        void onRequestDir(string path)
        {            
            RequestDirHandler h = RequestDir;
            if (h != null)
            {
                ViewHelpers.execWithWaitCursor(() =>
                {
                    h(this, new RequestDirArgs() { Path = ViewHelpers.correctPath(path) });
                });
            }
        }

        void onRequestDetails(string path)
        {
            RequestDirHandler h = RequestDetails;
            if (h != null)
            {
                ViewHelpers.execWithWaitCursor(() =>
                {
                    h(this, new RequestDirArgs() { Path = ViewHelpers.correctPath(path) });
                });
            }
        }

        void onItemPicked(FileSysItem it)
        {
            ItemPickedHandler h = ItemPicked;
            if (h != null)
            {                
                ViewHelpers.execWithWaitCursor(() =>
                {
                    h(this, new ItemPickedArgs() { Item = it });
                });                                
            }

        }

        void clearTree (TreeNodeCollection coll)
        {
            coll.Clear();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _logger.Info("Application has been started");
            onRequestDir(null);
        }

        private void treeDirs_BeforeExpand (object sender, TreeViewCancelEventArgs e)
        {
            TreeItem item = (TreeItem)e.Node.Tag;
            if (!item.Loaded)
            {
                try
                {
                    _ndLastExpanded = e.Node;
                    onRequestDir(item.Item.ItemPath);                        
                }
                catch (Exception ex)
                {
                    e.Cancel = true;
                    _ndLastExpanded = null;
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        void descr_CollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {            
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (FileSysItem it in e.OldItems)
                {
                    TreeNode nd = findParentNode(it.Parent);
                    TreeNodeCollection nodes = nd == null ? treeDirs.Nodes : nd.Nodes;
                    removeDir(nodes, it);
                    removeDetail(it);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (FileSysItem it in e.NewItems)
                {
                    TreeNode nd = findParentNode(it.Parent);
                    TreeNodeCollection nodes = nd == null ? treeDirs.Nodes : nd.Nodes;
                    addTreeDir(nodes, it);
                    addDetail(it, true);
                }
                lstCurDir.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                for (int i = 0; i < e.OldItems.Count; ++i)
                {
                    FileSysItem itOld = (FileSysItem)e.OldItems[i];
                    FileSysItem itNew = (FileSysItem)e.NewItems[i];
                    TreeNode nd = findParentNode(itOld.Parent);

                    removeDir(nd.Nodes, itOld);
                    addTreeDir(nd.Nodes, itNew);

                    removeDetail(itOld);
                    addDetail(itNew, true);
                }
                lstCurDir.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            }                        
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _requestedDirDescriptors.ForEach((d) => { d.Activatewatching(false); d.CollectionChanged -= descr_CollectionChanged; });
            _requestedDirDescriptors.Clear();
            clearTree(treeDirs.Nodes);
        }

        private void treeDirs_AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.SelectedImageIndex = e.Node.ImageIndex = 1;
        }

        private void treeDirs_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.SelectedImageIndex = e.Node.ImageIndex = 0;
        }

        private void treeDirs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                currentSelectedDetailsPath = null;
                onRequestDetails(null);
                lstCurDir.Clear();
            }
            else
            {
                TreeItem it = (TreeItem)e.Node.Tag;
                if (string.Compare(currentSelectedDetailsPath, ViewHelpers.correctPath(it.Item.ItemPath), true) != 0)
                {
                    try
                    {
                        onRequestDetails(it.Item.ItemPath);
                    }
                    catch (Exception ex)
                    {                        
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }

        private void lstCurDir_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (lstCurDir.Sorting)
            {
                case SortOrder.Ascending:
                    lstCurDir.Sorting = SortOrder.Descending;
                    lstCurDir.ListViewItemSorter = new ListViewDirectoryComparer(false);
                    break;
                case SortOrder.Descending:
                    lstCurDir.Sorting = SortOrder.None;
                    lstCurDir.ListViewItemSorter = new ListViewDirectoryComparer(true);
                    break;
            }            
        }

        
        private void lstCurDir_MouseDoubleClick(object sender, MouseEventArgs e)
        {            
            var it = lstCurDir.GetItemAt(e.Location.X, e.Location.Y);
            if (it != null)
                onItemPicked((FileSysItem)it.Tag);
        }

        private void treeDirs_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && e.Node.IsSelected)
            {
                TreeItem it = (TreeItem)e.Node.Tag;
                if (string.Compare(ViewHelpers.correctPath(it.Item.ItemPath), currentSelectedDetailsPath, true) != 0)
                {
                    try
                    {
                        onRequestDetails(it.Item.ItemPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentSelectedDetailsPath))
            {
                string[] dirs = currentSelectedDetailsPath.Split(new char[]{'\\'}, StringSplitOptions.RemoveEmptyEntries);
                if (dirs.Length > 1)
                {
                    try
                    {
                        onRequestDetails(string.Join("\\", dirs.Take(dirs.Length - 1)));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
    }

}
