;LENH MIRROR THEO PHUONG TRUC Y

(defun c:mmy(/ ss pt1 )
(setvar "cmdecho" 0)
(prompt "Mirror to follow in the way Y")
  
 (setq ss (ssget))
  (while (/= ss nil)
    (setq pt1 (getpoint "\nEnter specify point of mirror line:"))
    (command "mirror" ss "" pt1 "@0,50000" "yes")
    (setq ss (ssget))
  );ket thuc vong lap
  
(print "Thanks a lot and back to you !")
(princ)
);end program



;================================================================
;LENH MIRROR THEO PHUONG TRUC X

(defun c:mmx(/ ss pt1)
(setvar "cmdecho" 0)
(prompt "Mirror to follow in the way X")
  
 (setq ss (ssget))
  (while (/= ss nil)
    (setq pt1 (getpoint "\nEnter specify point of mirror line:"))
    (command "mirror" ss "" pt1 "@50000,0" "yes")
    (setq ss (ssget))
  );ket thuc vong lap
  
(print "Thanks a lot and back to you !")  
(princ)
);end program

;================================================================

;===============MIRROR KHI CHUA BIET TRUNG DIEM =================

;HAM XAC DINH TRUNG DIEM
(defun MID ()

  (setq P1 (getpoint "\nSpecify first point: ")
	P2 (getpoint P1 "\nSpecify second point: ")
	old_osmode (getvar "osmode" )
  );setq
 (setvar "osmode" 0)
  
   (setq goc (angle p1 p2)
	di (/ (distance p1 p2) 2)
	)
   (setq P3 (polar p1 goc di))
 (setvar "osmode" old_osmode)
);defun MID


;================================================================
(defun c:mii (/ P1  P2  old_osmode  goc  P3  di)
(setvar "cmdecho" 0)
(setvar "orthomode" 1)
  
  (MID)
  (setq ss (ssget))
  (command "mirror" ss "" P3  pause "no")


(princ)
);defun mii