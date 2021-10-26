					; 
;;;    ACAD2004.LSP Version 1.0 for AutoCAD 2004
;;;
;;;    Copyright (C) 1994-2002 by Autodesk, Inc.
;;;
;;;
;;;
;;;


(defun S::Startup ()

					;Structures-Applications

(command "netload" "D:/PTT/03. Dev/AutoCAD-Plugin/NetReload/bin/Debug/NetReload.dll")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/cut_dim (DDC).lsp")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/cutdim.fas")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/cutdim2.fas")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/Mirror [X,Y] (MMX-MMY).lsp")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/Stretch_X (SXX).lsp")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/Stretch_Y (SYY).lsp")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/Switch_model_layout (SW).lsp")
(load "D:/PTT/03. Dev/AutoCAD-Plugin/Lisp/XLINE-VXV[XV-XH].lsp")
  ;; Silent load.
  (princ)
  ;;
)