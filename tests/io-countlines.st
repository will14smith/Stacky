main () -> () { write-file open-read 0 { over is-eof not } { over read-line drop 1 + } while swap close print }

write-file () -> str { 
  "test" dup 
  
  open-write 
  dup "line1" write-line
  dup "line2" write-line
  dup "line3" write-line
  dup "line4" write-str
  close
}