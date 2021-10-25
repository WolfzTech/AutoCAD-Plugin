(defun c:sxx (/ fi_point  se_point  my_set )
(setvar "cmdecho" 0)
(setvar "osmode" 15359)
(prompt "\nStretch follow the way X.")
  
  (setq my_set (ssget))
  
  (setq fi_point (getpoint "\nSpecify base point or displacement: "))
  (setq se_point (getpoint "\nSpecify second point: "))
  (command ".stretch" my_set "" fi_point ".x" se_point "@" )

  )