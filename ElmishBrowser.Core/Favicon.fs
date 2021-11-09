[<RequireQualifiedAccess>]
module Favicon
let parse (uri:string) = 
    let hd, tl = 
        let res = uri.Split '/'
        res.[0], res.[2]
    hd + "//" + tl + "/favicon.ico"