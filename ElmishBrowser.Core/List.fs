[<RequireQualifiedAccess>]
module List
/// Update the first element that satisfies the predicate. 
let updateAti mapping predicate l = 
    let rec ua ind mapping predicate l cont = 
        match l with 
        | [] -> cont []
        | hd::tl -> 
            if not (predicate ind hd)
            then ua (ind + 1) mapping predicate tl (fun x -> cont(hd :: x))
            else cont (mapping ind hd :: tl)
    ua 0 mapping predicate l id 
/// Remove the first element that satisfies the predicate. 
let removeAti predicate l = 
    let rec ra ind predicate (l:list<'t>) cont = 
        match l with 
        | [] -> cont ([]:list<'t>)
        | hd::tl -> 
            if not (predicate ind hd)
            then ra (ind + 1) predicate tl (fun x -> cont (hd :: x))
            else cont (tl)
    ra 0 predicate l id