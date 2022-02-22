main () -> () { 
    // access n-th element in string
    "abc" 1 ! print

    // set n-th element in string (88 == 'X')
    "abc" 88 1 !~ print

    // create a new string with n chars 
    5 !@
    // 97 == 'a'
    97 0 !~
    "bcd" 1 !!~
    0 4 !~
    print
    
    // create a sub-string from the n-th (inclusive) to m-th (exclusive) chars 
    "abcdef" 1 3 !! print

    // join strings
    "abc" "def" concat print
}