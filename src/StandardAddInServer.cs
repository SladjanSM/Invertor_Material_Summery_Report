using Inventor;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SM.MaterialSummary
{
    [ComVisible(true)]
    [Guid("CE2E4C3C-1A1B-4F7C-9A0E-9F6D4E3D1B77")]
    public class StandardAddInServer : ApplicationAddInServer
    {
        private Inventor.Application _invApp;
        private ButtonDefinition _btnOpen;
        private string _clientId = "{CE2E4C3C-1A1B-4F7C-9A0E-9F6D4E3D1B77}";
        private MaterialSummaryForm _form;

        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            _invApp = addInSiteObject.Application;

            var cmdMgr = _invApp.CommandManager;
            _btnOpen = cmdMgr.ControlDefinitions.AddButtonDefinition(
                "Material Summary",
                "SM.MaterialSummary.Open",
                CommandTypesEnum.kNonShapeEditCmdType,
                _clientId,
                "Open Material Summary window",
                "Open Material Summary window",
                Type.Missing, Type.Missing,
                ButtonDisplayEnum.kDisplayTextInLearningMode);

            _btnOpen.OnExecute += BtnOpen_OnExecute;

            AddToRibbon("Part");
            AddToRibbon("Assembly");
            AddToRibbon("Drawing");
            AddToRibbon("ZeroDoc");
        }

        private void AddToRibbon(string ribbonName)
        {
            try
            {
                var ui = _invApp.UserInterfaceManager;
                var ribbon = ui.Ribbons[ribbonName];
                var toolsTab = ribbon.RibbonTabs["id_TabTools"];

                RibbonPanel panel = null;
                try { panel = toolsTab.RibbonPanels["SMFurnitureAI.Panel"]; } catch { /* not found */ }

                if (panel == null)
                    panel = toolsTab.RibbonPanels.Add("SM Furniture AI", "SMFurnitureAI.Panel", _clientId);

                bool exists = false;
                foreach (CommandControl cc in panel.CommandControls)
                    if (cc.Definition != null && cc.Definition.InternalName == _btnOpen.InternalName)
                        exists = true;

                if (!exists)
                    panel.CommandControls.AddButton(_btnOpen, true);
            }
            catch
            {
                // ignore when ribbon/tab not available
            }
        }

        private void BtnOpen_OnExecute(NameValueMap Context)
        {
            try
            {
                if (_form == null || _form.IsDisposed)
                {
                    _form = new MaterialSummaryForm(_invApp);
                    _form.StartPosition = FormStartPosition.CenterScreen;
                }
                _form.Show();
                _form.BringToFront();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Material Summary: " + ex.Message);
            }
        }

        public void Deactivate()
        {
            try
            {
                if (_btnOpen != null) _btnOpen.OnExecute -= BtnOpen_OnExecute;
                _btnOpen = null;
                _form?.Dispose();
                _form = null;
                _invApp = null;
            }
            catch { }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int CommandID) { }
        public object Automation => null;
    }
}