(defun C:ddc ()
  (setq ss_set (ssget))
  (setq ss_set (ssget "p" '((0 . "DIMENSION"))))
  (setq i_bb 0)
  (setq leng_dim 0)
  (if (null old_leng_dim)
    (setq old_leng_dim leng_dim)
  )
  (prompt (strcat "\nEnter value of extension line length <"
		  (rtos old_leng_dim 2 2)
		  "> :"
	  )
  )
  (setq leng_dim (getreal))
  (if (null leng_dim)
    (setq leng_dim old_leng_dim)
  )
  (while (< i_bb (sslength ss_set))
    (setq dimlfac_0 (getvar "dimlfac"))
;;;    (setq dimscale_0 (getvar "dimscale"))
    (setq ss_set_i (ssname ss_set i_bb))
    (setq ss_set_i_val (entget ss_set_i))
    (setq poi_a (cdr (assoc 10 ss_set_i_val)))
					;toa do diem tren dg kich thuoc
    (setq poi_a_x (car poi_a))		;toa do x
    (setq poi_a_y (cadr poi_a))		;toa do y

    (setq poi_b_sub (assoc 13 ss_set_i_val))
    (setq poi_b (cdr poi_b_sub))	;diem chan cua dg giong thu 1
    (setq poi_b_x (car poi_b))		;toa do x
    (setq poi_b_y (cadr poi_b))		;toa do y

    (setq poi_c_sub (assoc 14 ss_set_i_val))
    (setq poi_c (cdr poi_c_sub))	;diem chan cua dg giong thu 2
    (setq poi_c_x (car poi_c))		;toa do x
    (setq poi_c_y (cadr poi_c))		;toa do y

    (setq dis_val (cdr (assoc 42 ss_set_i_val)))
    (setq dis_x_b_c (abs (- poi_b_x poi_c_x)))
    (setq dis_y_b_c (abs (- poi_b_y poi_c_y)))

					;tim toa do 2 dau dg kich thuoc
    (setq poi_01 (list poi_b_x poi_a_y 0.0)) ;truong hop phuong x
    (setq poi_02 (list poi_c_x poi_a_y 0.0)) ;truong hop phuong x
    (setq poi_01a (list poi_a_x poi_b_y 0.0)) ;truong hop phuong y
    (setq poi_02a (list poi_a_x poi_c_y 0.0)) ;truong hop phuong y

    (if	(or (= dis_val (* dimlfac_0 dis_x_b_c))
	    (= dis_val (* dimlfac_0 dis_y_b_c))
	)
      (progn
	(cond
	  ((= dis_val (* dimlfac_0 dis_x_b_c))
					;neu do theo phuong x
	   (setq poi_03 (polar poi_01 (angle poi_01 poi_b) leng_dim))
	   (setq poi_04 (polar poi_02 (angle poi_02 poi_c) leng_dim))
	   (setq poi_b_sub_new (cons 13 poi_03))
	   (setq poi_c_sub_new (cons 14 poi_04))
					;thay gia tri trong ...
	   (setq ss_set_i_val
		  (subst poi_b_sub_new poi_b_sub ss_set_i_val)
	   )
	   (setq ss_set_i_val
		  (subst poi_c_sub_new poi_c_sub ss_set_i_val)
	   )
	   (entmod ss_set_i_val)
	   (entupd ss_set_i)
	  )
	  ((= dis_val (* dimlfac_0 dis_y_b_c)) ;neu do theo phuong y
	   (setq poi_03 (polar poi_01a (angle poi_01a poi_b) leng_dim))
	   (setq poi_04 (polar poi_02a (angle poi_02a poi_c) leng_dim))
	   (setq poi_b_sub_new (cons 13 poi_03))
	   (setq poi_c_sub_new (cons 14 poi_04))
					;thay gia tri trong ...
	   (setq ss_set_i_val
		  (subst poi_b_sub_new poi_b_sub ss_set_i_val)
	   )
	   (setq ss_set_i_val
		  (subst poi_c_sub_new poi_c_sub ss_set_i_val)
	   )
	   (entmod ss_set_i_val)
	   (entupd ss_set_i)
	  )
	)
      )
      (progn
	(setq ang_b_C (angle poi_b poi_c))
	(setq poi_b_1 (polar poi_b (+ (/ pi 2) ang_b_C) leng_dim))
	(setq poi_c_1 (polar poi_c (+ (/ pi 2) ang_b_C) leng_dim))
	(setq poi_a_1 (polar poi_a ang_b_C leng_dim))
	(setq poi_01b (inters poi_b poi_b_1 poi_a poi_a_1 nil))
	(setq poi_02b (inters poi_c poi_c_1 poi_a poi_a_1 nil))

	(setq poi_03 (polar poi_01b (angle poi_01b poi_b) leng_dim))
	(setq poi_04 (polar poi_02b (angle poi_02b poi_c) leng_dim))
	(setq poi_b_sub_new (cons 13 poi_03))
	(setq poi_c_sub_new (cons 14 poi_04))
					;thay gia tri trong ...
	(setq ss_set_i_val (subst poi_b_sub_new poi_b_sub ss_set_i_val))
	(setq ss_set_i_val (subst poi_c_sub_new poi_c_sub ss_set_i_val))
	(entmod ss_set_i_val)
	(entupd ss_set_i)
      )
    )
    (setq i_bb (1+ i_bb))
  )
  (setq old_leng_dim leng_dim)
  (princ)
)

