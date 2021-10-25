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
[assembly: CommandClass(typeof(AutoCAD_CSharp_plug_in.MyCommands))]

namespace AutoCAD_CSharp_plug_in
{
    public class MyCommands
    {
        [CommandMethod("COU")]
        public void CountText()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            PromptIntegerOptions startNumberPIO = new PromptIntegerOptions("Enter starter number: ");
            PromptIntegerResult startNumberPR = ed.GetInteger(startNumberPIO);
            if ((startNumberPR.Status == PromptStatus.OK))
            {
                PromptIntegerOptions shiftNumberPIO = new PromptIntegerOptions("Enter shift number: ");
                PromptIntegerResult shiftNumberPR = ed.GetInteger(shiftNumberPIO);
                if ((shiftNumberPR.Status == PromptStatus.OK))
                {
                    int startNumber = startNumberPR.Value;
                    int shiftNunber = shiftNumberPR.Value;
                    string prefix = string.Empty;
                    string suffix = string.Empty;
                    while (true)
                    {
                        PromptEntityOptions promptEntityOptions = new PromptEntityOptions("\nSelect text");
                        promptEntityOptions.AllowNone = true;
                        promptEntityOptions.AllowObjectOnLockedLayer = true;
                        promptEntityOptions.SetRejectMessage("\nSelect a text");
                        promptEntityOptions.AddAllowedClass(typeof(MText), true);
                        promptEntityOptions.Keywords.Add("Prefix");
                        promptEntityOptions.Keywords.Add("Suffix");
                        PromptEntityResult promptEntityResult = ed.GetEntity(promptEntityOptions);
                        if (promptEntityResult.Status == PromptStatus.Keyword)
                        {
                            PromptStringOptions fixTextPSO = new PromptStringOptions("Enter " + promptEntityResult.StringResult + ": ");
                            fixTextPSO.AllowSpaces = true;
                            PromptResult fixTextPR = ed.GetString(fixTextPSO);
                            if (fixTextPR.Status == PromptStatus.OK)
                            {
                                switch (promptEntityResult.StringResult)
                                {
                                    case "Prefix":
                                        prefix = fixTextPR.StringResult;
                                        break;
                                    case "Suffix":
                                        suffix = fixTextPR.StringResult;
                                        break;
                                }
                            }
                        }
                        else if (promptEntityResult.Status == PromptStatus.OK)
                        {
                            Transaction trans = db.TransactionManager.StartTransaction();
                            MText mText = trans.GetObject(promptEntityResult.ObjectId, OpenMode.ForWrite) as MText;
                            mText.Contents = prefix + startNumber.ToString() + suffix;
                            trans.Commit();
                            startNumber += shiftNunber;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
        [CommandMethod("XX", CommandFlags.UsePickSet)]
        public void MoveHorizontally()
        {
            MoveStraight("Horizontally");
        }
        [CommandMethod("YY", CommandFlags.UsePickSet)]
        public void MoveVertically()
        {
            MoveStraight("Vertically");
        }
        public void MoveStraight(string Mode)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            var _selAll = ed.SelectImplied();
            SelectionSet selectionSet;
            if (_selAll.Status == PromptStatus.OK)
            {
                selectionSet = _selAll.Value;
            }
            else
            {
                PromptSelectionOptions promptSelectionOptions = new PromptSelectionOptions();
                promptSelectionOptions.MessageForAdding = "\nSelect Objects";
                PromptSelectionResult promptSelectionResult = ed.GetSelection(promptSelectionOptions);
                if (promptSelectionResult.Status != PromptStatus.OK) return;
                selectionSet = promptSelectionResult.Value;
            }
            PromptPointOptions pointOption = new PromptPointOptions("\nSpecify base point");
            pointOption.AllowArbitraryInput = false;
            pointOption.AllowNone = true;

            PromptPointResult basePointResult = ed.GetPoint(pointOption);
            if (basePointResult.Status != PromptStatus.OK) return;

            pointOption.BasePoint = basePointResult.Value;
            pointOption.UseBasePoint = true;
            pointOption.Message = "\nSpecify second point";

            PromptPointResult secondPointResult = ed.GetPoint(pointOption);
            if (secondPointResult.Status != PromptStatus.OK) return;

            Point3d secondPoint = new Point3d();
            if (Mode == "Horizontally")
            {
                secondPoint = new Point3d(secondPointResult.Value.X, basePointResult.Value.Y, secondPointResult.Value.Z);
            }
            else
            {
                secondPoint = new Point3d(basePointResult.Value.X, secondPointResult.Value.Y, secondPointResult.Value.Z);
            }

            Vector3d acVec3d = basePointResult.Value.GetVectorTo(secondPoint);
            ObjectId blockId = db.CurrentSpaceId;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord currentSpace = trans.GetObject(blockId, OpenMode.ForWrite) as BlockTableRecord;
                foreach (SelectedObject selected in selectionSet)
                {
                    try
                    {
                        Entity entity = trans.GetObject(selected.ObjectId, OpenMode.ForWrite) as Entity;
                        entity.TransformBy(Matrix3d.Displacement(acVec3d));
                    }
                    catch (System.Exception)
                    {
                    }

                }
                trans.Commit();
            }
        }
        [CommandMethod("STT", CommandFlags.UsePickSet)]
        public void SortText()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            var _selAll = ed.SelectImplied();
            SelectionSet selectionSet;
            if (_selAll.Status == PromptStatus.OK)
            {
                selectionSet = _selAll.Value;
            }
            else
            {
                PromptSelectionOptions promptSelectionOptions = new PromptSelectionOptions();
                promptSelectionOptions.MessageForAdding = "\nSelect Objects";

                TypedValue[] typedValues = new TypedValue[1];
                typedValues[0] = new TypedValue((int)DxfCode.Start, "MTEXT");
                SelectionFilter selectionFilter = new SelectionFilter(typedValues);

                PromptSelectionResult promptSelectionResult = ed.GetSelection(promptSelectionOptions, selectionFilter);
                if (promptSelectionResult.Status != PromptStatus.OK) return;
                selectionSet = promptSelectionResult.Value;
            }
            PromptIntegerOptions startNumberPIO = new PromptIntegerOptions("Enter starter number: ");
            PromptIntegerResult startNumberPR = ed.GetInteger(startNumberPIO);
            if ((startNumberPR.Status == PromptStatus.OK))
            {
                PromptIntegerOptions shiftNumberPIO = new PromptIntegerOptions("Enter shift number: ");
                PromptIntegerResult shiftNumberPR = ed.GetInteger(shiftNumberPIO);
                if ((shiftNumberPR.Status == PromptStatus.OK))
                {
                    int startNumber = startNumberPR.Value;
                    int shiftNunber = shiftNumberPR.Value;
                    string prefix = string.Empty;
                    string suffix = string.Empty;
                    SortMode sortMode = SortMode.X;
                    bool tempBool = true;
                    while (tempBool)
                    {
                        PromptKeywordOptions keywordPKO = new PromptKeywordOptions("Enter [X/-X/Y/-Y/Prefix/Suffix]: ", "X -X Y -Y Prefix Suffix");
                        keywordPKO.AllowArbitraryInput = true;
                        PromptResult getWhichEntityResult = ed.GetKeywords(keywordPKO);
                        if (getWhichEntityResult.Status == PromptStatus.OK)
                        {
                            PromptStringOptions fixTextPSO = new PromptStringOptions("Enter " + getWhichEntityResult.StringResult + ": ");
                            fixTextPSO.AllowSpaces = true;

                            switch (getWhichEntityResult.StringResult)
                            {
                                case "X":
                                    sortMode = SortMode.X;
                                    tempBool = false;
                                    break;
                                case "-X":
                                    sortMode = SortMode.NX;
                                    tempBool = false;
                                    break;
                                case "Y":
                                    sortMode = SortMode.Y;
                                    tempBool = false;
                                    break;
                                case "-Y":
                                    sortMode = SortMode.NY;
                                    tempBool = false;
                                    break;
                                case "Prefix":
                                    prefix = ed.GetString(fixTextPSO).StringResult;
                                    break;
                                case "Suffix":
                                    suffix = ed.GetString(fixTextPSO).StringResult;
                                    break;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    Transaction trans = db.TransactionManager.StartTransaction();
                    List<MText> mTexts = new List<MText>();
                    foreach (SelectedObject selectedObject in selectionSet)
                    {
                        if (trans.GetObject(selectedObject.ObjectId, OpenMode.ForRead) is MText)
                        {
                            mTexts.Add(trans.GetObject(selectedObject.ObjectId, OpenMode.ForWrite) as MText);
                        }
                    }
                    switch (sortMode)
                    {
                        case SortMode.X:
                            mTexts = mTexts.OrderBy(x => x.Location.X).ToList();
                            break;
                        case SortMode.NX:
                            mTexts = mTexts.OrderBy(x => -x.Location.X).ToList();
                            break;
                        case SortMode.Y:
                            mTexts = mTexts.OrderBy(x => x.Location.Y).ToList();
                            break;
                        case SortMode.NY:
                            mTexts = mTexts.OrderBy(x => -x.Location.Y).ToList();
                            break;
                    }

                    foreach (MText mText1 in mTexts)
                    {
                        mText1.Contents = prefix + startNumber.ToString() + suffix;
                        startNumber += shiftNunber;
                    }
                    trans.Commit();
                }
            }
        }
        enum SortMode
        {
            X,
            NX,
            Y,
            NY
        }
    }
}


