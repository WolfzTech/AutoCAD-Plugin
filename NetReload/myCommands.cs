// (C) Copyright 2021 by  
//
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(NetReload.MyCommands))]

namespace NetReload
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        // Modal Command with localized name
        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            if (doc != null)
            {
                ed = doc.Editor;
                ed.WriteMessage("Hello, this is your first command.");

            }
        }

        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            PromptSelectionResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                // There are selected entities
                // Put your command using pickfirst set code here
            }
            else
            {
                // There are no selected entities
                // Put your command code here
            }
        }

        // Application Session Command with localized name
        [CommandMethod("MyGroup", "MySessionCmd", "MySessionCmdLocal", CommandFlags.Modal | CommandFlags.Session)]
        public void MySessionCmd() // This method can have any name
        {
            // Put your command code here
        }

        // LispFunction is similar to CommandMethod but it creates a lisp 
        // callable function. Many return types are supported not just string
        // or integer.
        [LispFunction("MyLispFunction", "MyLispFunctionLocal")]
        public int MyLispFunction(ResultBuffer args) // This method can have any name
        {
            // Put your command code here

            // Return a value to the AutoCAD Lisp Interpreter
            return 1;
        }
        #region // Sources.

        //	1.	Get running Visual Studio instances and corresponding _DTE objects:
        //		http://www.christophdebaene.com/blog/2006/11/01/get-running-visual-studio-instances-and-corresponding-_dte-objects/

        #endregion

        // Default keyword application-level variable.
        private static string defaultKeyword = "";

        // NETRELOAD command.
        [CommandMethod("NRL")]
        public static void NRL()
        {

            try
            {

                // Current AutoCAD Document, Database and Editor.
                Autodesk.AutoCAD.ApplicationServices.Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                // Get running Visual Studio instances (using helper class).
                IDictionary<string, _DTE> vsInstances = RunningObjectTable.GetRunningVSIDETable();

                // Check for no Visual Studio instances.
                if (vsInstances.Count == 0)
                {
                    ed.WriteMessage("\nNo running Visual Studio instances were found. *Cancel*");
                    return;
                }

                // Create list of solution names.
                List<string> solNames = new List<string>();
                foreach (KeyValuePair<string, _DTE> item in vsInstances)
                {
                    solNames.Add(Path.GetFileNameWithoutExtension(item.Value.Solution.FullName));
                }

                // Check if all solution names equal "".
                // i.e. no solutions loaded in any of the Visual Studio instances.
                bool allSolNamesEmpty = true;
                foreach (string name in solNames)
                {
                    if (name != "")
                    {
                        allSolNamesEmpty = false;
                        break;
                    }
                }
                if (allSolNamesEmpty == true)
                {
                    ed.WriteMessage("\nNo active Visual Studio solutions were found. *Cancel*");
                    return;
                }

                // Prompt user to select solution.
                PromptKeywordOptions pko = new PromptKeywordOptions("\nSelect Visual Studio instance to increment:");
                pko.AllowNone = false;
                foreach (string name in solNames)
                {
                    if (name != "")
                    {
                        pko.Keywords.Add(name);
                    }
                }
                if (defaultKeyword == "" || solNames.Contains(defaultKeyword) == false)
                {
                    int index = 0;
                    while (solNames[index] == "")
                    {
                        index++;
                    }
                    pko.Keywords.Default = solNames[index];
                }
                else
                {
                    pko.Keywords.Default = defaultKeyword;
                }
                PromptResult pr = ed.GetKeywords(pko);
                if (pr.Status != PromptStatus.OK)
                {
                    return;
                }
                defaultKeyword = pr.StringResult;

                // Use prompt result to set Visual Studio instance variable.
                _DTE dte = vsInstances.ElementAt(solNames.IndexOf(pr.StringResult)).Value;

                // Use custom WaitCursor class for long operation.
                using (WaitCursor wc = new WaitCursor())
                {

                    // Active Visual Studio Document.
                    EnvDTE.Document vsDoc = dte.ActiveDocument;
                    if (vsDoc == null)
                    {
                        ed.WriteMessage(String.Format("\nNo active document found for the '{0}' solution. *Cancel*",
                            pr.StringResult));
                        return;
                    }

                    // Active Visual Studio Project.
                    Project prj = vsDoc.ProjectItem.ContainingProject;

                    // Debug directory - i.e. \bin\Debug.
                    string debugDir = prj.FullName;
                    debugDir = Path.GetDirectoryName(debugDir);
                    debugDir = Path.Combine(debugDir, @"bin\Debug");

                    // NetReload directory - i.e. \bin\Debug\NetReload.
                    string netReloadDir = Path.Combine(debugDir, "NetReload");

                    // Create NetReload directory if it doens't exist.
                    if (Directory.Exists(netReloadDir) == false)
                    {
                        Directory.CreateDirectory(netReloadDir);
                    }

                    // Temporary random assembly file name (check it doesn't already exist).
                    string tempAssemblyName;
                    do
                    {
                        tempAssemblyName = Path.GetRandomFileName();
                    } while (File.Exists(Path.Combine(netReloadDir, tempAssemblyName + ".dll")));

                    // Project's initial "AssemblyName" property setting.
                    string initAssemblyName = prj.Properties.Item("AssemblyName").Value as string;

                    // Set project's "AssemblyName" property to temp value.
                    prj.Properties.Item("AssemblyName").Value = tempAssemblyName;

                    // Build solution.
                    SolutionBuild solBuild = dte.Solution.SolutionBuild;
                    solBuild.Build(true);

                    // Re-set project's "AssemblyName" property back to initial value.
                    prj.Properties.Item("AssemblyName").Value = initAssemblyName;

                    // Check if build was successful.
                    // # Note: LastBuildInfo property reports number of projects in the solution that failed to build.
                    if (solBuild.LastBuildInfo != 0)
                    {
                        ed.WriteMessage(String.Format("\nBuild failed for the '{0}' solution. *Cancel*",
                            pr.StringResult));
                        return;
                    }

                    // Move new assembly (.dll) from Debug directory to NetReload directory.
                    File.Move(
                        Path.Combine(debugDir, tempAssemblyName + ".dll"),
                        Path.Combine(netReloadDir, tempAssemblyName + ".dll")
                        );

                    // Move new .pdb file from Debug directory to NetReload directory.
                    File.Move(
                        Path.Combine(debugDir, tempAssemblyName + ".pdb"),
                        Path.Combine(netReloadDir, tempAssemblyName + ".pdb")
                        );

                    // NETLOAD new assembly file.
                    System.Reflection.Assembly.LoadFrom(Path.Combine(netReloadDir, tempAssemblyName + ".dll"));

                    // Output summary.
                    ed.WriteMessage("\nNETRELOAD complete for {0}.dll.", initAssemblyName);
                }

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {

                // Catch AutoCAD exception.
                Application.ShowAlertDialog(String.Format("ERROR" +
                    "\nMessage: {0}\nErrorStatus: {1}", ex.Message, ex.ErrorStatus));

            }
            catch (System.Exception ex)
            {

                // Catch Windows exception.
                Application.ShowAlertDialog(String.Format("ERROR" +
                    "\nMessage: {0}", ex.Message));
            }
        }

        // RunningObjectTable helper class.
        class RunningObjectTable
        {

            const uint S_OK = 0;

            [DllImport("ole32.dll")]
            public static extern uint GetRunningObjectTable(uint reserved, out IRunningObjectTable ROT);

            [DllImport("ole32.dll")]
            public static extern uint CreateBindCtx(uint reserved, out IBindCtx ctx);

            static IDictionary<string, object> GetRunningObjectTable()
            {
                IDictionary<string, object> rotTable = new Dictionary<string, object>();

                IRunningObjectTable runningObjectTable;
                IEnumMoniker monikerEnumerator;
                IMoniker[] monikers = new IMoniker[1];

                GetRunningObjectTable(0, out runningObjectTable);
                runningObjectTable.EnumRunning(out monikerEnumerator);
                monikerEnumerator.Reset();

                IntPtr numberFetched = IntPtr.Zero;

                while (monikerEnumerator.Next(1, monikers, numberFetched) == 0)
                {
                    IBindCtx ctx;
                    CreateBindCtx(0, out ctx);

                    string runningObjectName;
                    monikers[0].GetDisplayName(ctx, null, out runningObjectName);
                    Marshal.ReleaseComObject(ctx);

                    object runningObjectValue;
                    runningObjectTable.GetObject(monikers[0], out runningObjectValue);

                    if (!rotTable.ContainsKey(runningObjectName))
                        rotTable.Add(runningObjectName, runningObjectValue);
                }

                return rotTable;
            }

            public static IDictionary<string, _DTE> GetRunningVSIDETable()
            {
                IDictionary<string, object> runningObjects = GetRunningObjectTable();
                IDictionary<string, _DTE> runningDTEObjects = new Dictionary<string, _DTE>();

                foreach (string objectName in runningObjects.Keys)
                {
                    if (!objectName.StartsWith("!VisualStudio.DTE"))
                        continue;

                    _DTE ide = runningObjects[objectName] as _DTE;
                    if (ide == null)
                        continue;

                    runningDTEObjects.Add(objectName, ide);
                }

                return runningDTEObjects;
            }
        }

        // WaitCursor helper class.
        // # Note: Add project reference to System.Windows.Forms.dll.
        private class WaitCursor : IDisposable
        {

            // Field.
            System.Windows.Forms.Cursor savedCursor;

            // Constructor.
            public WaitCursor()
            {
                this.savedCursor = System.Windows.Forms.Cursor.Current;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            }

            // IDisposable implementation.
            public void Dispose()
            {
                System.Windows.Forms.Cursor.Current = this.savedCursor;
            }
        }
    }

}
