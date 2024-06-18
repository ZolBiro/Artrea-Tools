using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace ArtreaTools
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Barray : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Application
            UIApplication uiApp =commandData.Application;

            //Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uiApp.ActiveUIDocument.Document;

            BarrayWindow barrayWindow = new BarrayWindow(doc,uidoc,uiApp);
            barrayWindow.ShowDialog();

            return Result.Succeeded;
        }
    }
}