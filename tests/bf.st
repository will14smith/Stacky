struct State {
    input str
    input-offset i64

    buf str
    ptr i64
}

main () -> () { 
    "++++++++[>++++[>++>+++>+++>+<<<<-]>+>+>->>+[<]<-]>>.>---.+++++++..+++.>>.<-.<.+++.------.--------.>>+.>++."
    state-init

    { state-eof? not }
    { state-consume state-exec }    
    while

    drop
}

state-init str -> State { 
  @State
  swap ~input
  0 ~input-offset
  30000 dup !@ swap buffer-clear ~buf
  0 ~ptr
}

state-read State -> State u8 {
  dup #buf
  over #ptr
  !
}
state-write State u8 -> State {
  (state value) {
    state #buf // buffer
    value // buffer value
    state #ptr // buffer value index
    !~ drop
    state
  }
}

state-consume State -> State u8 {
  dup #input
  over #input-offset
  ! swap
  dup #input-offset
  1 +
  ~input-offset
  swap
}

state-consume-rev State -> State u8 {
  dup #input
  swap dup #input-offset 1 -
  ~input-offset
  swap over #input-offset !
}

state-eof? State -> State bool {
  dup #input-offset over #input length >=
}

state-exec State u8 -> State {
  (state command) {
    state 
    command 60 == { state-exec< } if
    command 62 == { state-exec> } if
    command 43 == { state-exec+ } if
    command 45 == { state-exec- } if
    command 46 == { state-exec. } if
    command 44 == { state-exec, } if
    command 91 == { state-exec[ } if
    command 93 == { state-exec] } if
  }
}

state-exec< State -> State { dup #ptr 1 - ~ptr }
state-exec> State -> State { dup #ptr 1 + ~ptr }
state-exec+ State -> State { 
  dup #buf over #ptr
  (buf ptr) {
    buf ptr 
    buf ptr ! 1 +    
    swap !~ drop
  }
}
state-exec- State -> State { 
  dup #buf over #ptr
  (buf ptr) {
    buf ptr 
    buf ptr ! 1 -
    swap !~ drop
  }
}
state-exec. State -> State { dup #buf over #ptr ! char print }
state-exec, State -> State { "TODO handle," print }
state-exec[ State -> State { 
  dup #buf over #ptr ! 0 ==
  {    
    0 0
    { dup 0 == }
    { 
      (depth _) {
        state-consume

        dup 91 ==
        { drop depth 1 + 0 }
        { 
          93 ==
          { 
            depth 0 ==
            { depth 1 }
            { depth 1 - 0 } 
            if-else            
          }
          { depth 0 }
          if-else
        }
        if-else
      }
    } while
    drop drop
  } if
}
state-exec] State -> State {
  dup #buf over #ptr ! 0 == not
  {
    state-consume-rev drop
    0 0
    { dup 0 == }
    { 
      (depth _) {
        state-consume-rev

        dup 93 ==
        { drop depth 1 + 0 }
        { 
          91 ==
          { 
            depth 0 ==
            { depth 1 }
            { depth 1 - 0 } 
            if-else
          }
          { depth 0 }
          if-else
        }
        if-else
      }
    } while
    drop drop
  } if
}

state-print State -> () { 
  dup #input-offset string "input-offset " swap concat print
  dup #ptr string "ptr          " swap concat print

  drop
}

buffer-clear str i64 -> str {
  0
  { over over swap < }
  { (buffer length offset) { buffer 0 offset !~ length offset 1 + } }
  while
  drop drop
}